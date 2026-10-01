using System;
using System.Collections.Generic;
using System.Threading;
using BG_Library.NET.API;
using Cysharp.Threading.Tasks;
using Firebase.Analytics;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AppBootstrap.Splash
{
    [DefaultExecutionOrder(-1000)]
    public sealed class IntroBootstrap : MonoBehaviour
    {
        [SerializeField] private PULayout _puLayoutTemplate;
        [SerializeField] private AdStorageService _adStorageService;
        [SerializeField] private IntroLayoutView[] _introLayoutViews;

        [ReadOnly, ShowInInspector] private IntroConfig _introConfig;
        [ReadOnly, ShowInInspector] private AdInfoConfig _adInfoConfig;

        private RemoteConfigProvider _configProvider;
        private SplashAdShowContext _showContext;
        private ISplashSceneTransitionService _sceneTransitionService;
        private readonly Dictionary<IntroLayoutType, IntroLayoutView> _viewByType = new Dictionary<IntroLayoutType, IntroLayoutView>();
        private readonly Dictionary<AdGroupType, ISplashAdHandler> _handlersByType = new Dictionary<AdGroupType, ISplashAdHandler>();

        private void Awake()
        {
            Installer();
        }
        
        private async void Start()
        {
            try
            {
                await UpdateRemoteSettingsAsync();
                await RunIntroAsync();
            }
            catch (Exception ex)
            {
                SplashLogger.Error($"Intro bootstrap failed: {ex}");
                throw;
            }
        }

        private void Installer()
        {
            _configProvider = new RemoteConfigProvider();
            _showContext = new SplashAdShowContext(_puLayoutTemplate);
            _sceneTransitionService = new SplashSceneTransitionService();
            _viewByType.Clear();
            _handlersByType.Clear();

            if (_adStorageService == null)
            {
                SplashLogger.Warn("AdStorageService asset is missing in IntroBootstrap, creating runtime instance");
                _adStorageService = ScriptableObject.CreateInstance<AdStorageService>();
            }

            RegisterHandler(new SplashPUAdHandler());
            RegisterHandler(new SplashFAAdHandler());

            if (_introLayoutViews == null)
            {
                return;
            }

            foreach (var view in _introLayoutViews)
            {
                if (view == null)
                {
                    continue;
                }

                _viewByType[view.LayoutType] = view;
                view.SetVisible(false);
            }
        }

        private void RegisterHandler(ISplashAdHandler handler)
        {
            if (handler == null)
            {
                return;
            }

            _handlersByType[handler.GroupType] = handler;
            SplashLogger.Log($"Intro register handler type={handler.GroupType} handler={handler.GetType().Name}");
        }

        private async UniTask UpdateRemoteSettingsAsync()
        {
            FirebaseAnalytics.LogEvent($"ff_5_intro_init");
            
            const string introConfigKey = "intro_config";
            
            _introConfig = _configProvider.LoadIntro(introConfigKey);
            _adInfoConfig = _configProvider.LoadAdInfo();
            if (_introConfig == null)
            {
                SplashLogger.Warn("Intro config is null");
                return;
            }

            SplashLogger.Log($"Intro config parsed entries={_introConfig.IntroEntries?.Length ?? 0} nextScene={_introConfig.nextSceneName}");
            FirebaseAnalytics.LogEvent($"ff_5_intro_init_d");
            await UniTask.CompletedTask;
        }

        private async UniTask RunIntroAsync()
        {
            if (_introConfig?.IntroEntries == null || _introConfig.IntroEntries.Length == 0)
            {
                SplashLogger.Warn("Intro entries are empty");
                await LoadConfiguredNextSceneAsync();
                return;
            }

            var cancellationToken = this.GetCancellationTokenOnDestroy();
            List<ShownIntroAd> currentShownAds = null;

            for (int index = 0; index < _introConfig.IntroEntries.Length; index++)
            {
                var entry = _introConfig.IntroEntries[index];
                if (entry == null)
                {
                    continue;
                }

                HideShownAds(currentShownAds);

                if (!_viewByType.TryGetValue(entry.type, out var view) || view == null)
                {
                    SplashLogger.Warn($"Missing intro view for type={entry.type}");
                    continue;
                }
                
                ShowOnly(view);
                view.ApplyEntry(entry);

                currentShownAds = ShowReadyAds(entry, cancellationToken);
                PreloadNextIntroAds(index + 1, cancellationToken);
                
                FirebaseAnalytics.LogEvent($"ff_5_intro_{index}");
                SplashLogger.Log($"Run intro entry type={entry.type} actionType={entry.actionType} time={entry.introTime} ads={currentShownAds.Count}");
                await RunEntryCountdownAsync(view, entry.introTime, cancellationToken);
                await HandleEntryActionAsync(view, entry, cancellationToken);
                FirebaseAnalytics.LogEvent($"ff_5_intro_{index}_d");
            }

            HideShownAds(currentShownAds);
           // HideAllViews();
            SplashLogger.Log("Intro flow completed");
            await LoadConfiguredNextSceneAsync();
        }

        private async UniTask HandleEntryActionAsync(IntroLayoutView view, IntroEntry entry, CancellationToken cancellationToken)
        {
            if (entry.actionType == IntroActionType.AutoNext)
            {
                SplashLogger.Log($"Intro action auto-next type={entry.type}");
                view.ShowNextButton(false);
                return;
            }

            SplashLogger.Log($"Intro action wait-next-click type={entry.type}");
            view.ShowNextButton(true);
            await view.WaitForNextClickAsync();
            SplashLogger.Log($"Intro action next-click-received type={entry.type}");
            view.ShowNextButton(false);
        }

        private async UniTask LoadConfiguredNextSceneAsync()
        {
            FirebaseAnalytics.LogEvent($"ff_5_intro_next");
            if (string.IsNullOrWhiteSpace(_introConfig?.nextSceneName))
            {
                SplashLogger.Log("Intro next scene is empty, skip scene transition");
                return;
            }

            await _sceneTransitionService.LoadNextSceneAsync(_introConfig.nextSceneName, this.GetCancellationTokenOnDestroy());
            FirebaseAnalytics.LogEvent($"ff_5_intro_next_d");
        }

        private List<ShownIntroAd> ShowReadyAds(IntroEntry entry, CancellationToken cancellationToken)
        {
            var shownAds = new List<ShownIntroAd>();
            var adStorage = (IAdStorage)_adStorageService;
            var adEntries = ResolveIntroAdEntries(entry);
            SplashLogger.Log($"Intro show-ready-ads entryType={entry?.type} resolvedCount={adEntries.Count}");

            foreach (var adEntry in adEntries)
            {
                if (adEntry == null)
                {
                    continue;
                }

                if (!_handlersByType.TryGetValue(adEntry.type, out var handler))
                {
                    SplashLogger.Warn($"Missing intro handler for type={adEntry.type}");
                    continue;
                }

                if (!TryResolveReadyResult(adStorage, adEntry, out var result, out var debugReason))
                {
                    SplashLogger.Log(
                        $"Intro ad not ready type={adEntry.type} group={adEntry.groupName} pos={adEntry.adsPosition} " +
                        $"backupGroup={adEntry.groupNameBackup} backupPos={adEntry.adsPositionBackup} reason={debugReason}");
                    continue;
                }

                SplashLogger.Log(
                    $"Intro ad resolved type={adEntry.type} selectedGroup={result.SelectedGroupName} selectedPos={result.SelectedAdsPosition} " +
                    $"usePrimary={result.PrimaryReady} useBackup={result.BackupReady}");
                shownAds.Add(new ShownIntroAd(adEntry.type, result.SelectedGroupName, result.SelectedAdsPosition));
                ShowIntroAdAsync(handler, adEntry, result, cancellationToken).Forget();
            }

            return shownAds;
        }

        private void PreloadNextIntroAds(int nextIndex, CancellationToken cancellationToken)
        {
            if (_introConfig?.IntroEntries == null || nextIndex < 0 || nextIndex >= _introConfig.IntroEntries.Length)
            {
                SplashLogger.Log($"Intro preload-next skipped nextIndex={nextIndex} total={_introConfig?.IntroEntries?.Length ?? 0}");
                return;
            }

            SplashLogger.Log($"Intro preload-next start nextIndex={nextIndex} type={_introConfig.IntroEntries[nextIndex]?.type}");
            PreloadIntroAdsAsync(_introConfig.IntroEntries[nextIndex], cancellationToken).Forget();
        }

        private async UniTaskVoid PreloadIntroAdsAsync(IntroEntry entry, CancellationToken cancellationToken)
        {
            try
            {
                var tasks = new List<UniTask>();
                foreach (var adEntry in ResolveIntroAdEntries(entry))
                {
                    if (adEntry == null)
                    {
                        continue;
                    }

                    if (!_handlersByType.TryGetValue(adEntry.type, out var handler))
                    {
                        SplashLogger.Warn($"Missing preload handler for type={adEntry.type}");
                        continue;
                    }

                    tasks.Add(PreloadSingleAdAsync(handler, adEntry, cancellationToken));
                }

                if (tasks.Count > 0)
                {
                    SplashLogger.Log($"Preload next intro ads count={tasks.Count} entryType={entry?.type}");
                    await UniTask.WhenAll(tasks);
                }
                else
                {
                    SplashLogger.Log($"Preload next intro ads skipped entryType={entry?.type} reason=no-resolved-entries");
                }
            }
            catch (OperationCanceledException)
            {
                SplashLogger.Log("Intro preload cancelled");
            }
            catch (Exception ex)
            {
                SplashLogger.Error($"Intro preload failed: {ex}");
            }
        }

        private async UniTask PreloadSingleAdAsync(ISplashAdHandler handler, AdGroupEntry adEntry, CancellationToken cancellationToken)
        {
            var adStorage = (IAdStorage)_adStorageService;
            SplashLogger.Log(
                $"Intro preload primary start type={adEntry.type} group={adEntry.groupName} pos={adEntry.adsPosition} timeout={adEntry.loadSeconds}");
            var primaryStatus = await adStorage.LoadAdsAsync(handler, adEntry.type, adEntry.groupName, adEntry.loadSeconds, cancellationToken);
            SplashLogger.Log(
                $"Intro preload primary end type={adEntry.type} group={adEntry.groupName} pos={adEntry.adsPosition} status={primaryStatus}");
            if (primaryStatus == AdGroupStatus.AdsReady)
            {
                return;
            }

            if (!adEntry.isBackup || string.IsNullOrWhiteSpace(adEntry.groupNameBackup))
            {
                SplashLogger.Log(
                    $"Intro preload backup skipped type={adEntry.type} group={adEntry.groupName} reason=backup-disabled-or-empty");
                return;
            }

            SplashLogger.Log(
                $"Intro preload backup start type={adEntry.type} group={adEntry.groupNameBackup} pos={adEntry.adsPositionBackup} timeout={adEntry.loadBackupSeconds}");
            var backupStatus = await adStorage.LoadAdsAsync(handler, adEntry.type, adEntry.groupNameBackup, adEntry.loadBackupSeconds, cancellationToken);
            SplashLogger.Log(
                $"Intro preload backup end type={adEntry.type} group={adEntry.groupNameBackup} pos={adEntry.adsPositionBackup} status={backupStatus}");
        }

        private List<AdGroupEntry> ResolveIntroAdEntries(IntroEntry entry)
        {
            var resolvedEntries = new List<AdGroupEntry>();
            if (entry?.introAdGroupNames == null || entry.introAdGroupNames.Length == 0)
            {
                SplashLogger.Log($"Resolve intro ad entries skipped entryType={entry?.type} reason=no-group-names");
                return resolvedEntries;
            }

            foreach (var groupName in entry.introAdGroupNames)
            {
                var adEntry = _configProvider.ResolveAdEntry(groupName, _adInfoConfig);
                if (adEntry == null)
                {
                    SplashLogger.Warn($"Missing intro ad entry for group={groupName}");
                    continue;
                }

                SplashLogger.Log(
                    $"Resolve intro ad entry success requestedGroup={groupName} type={adEntry.type} group={adEntry.groupName} pos={adEntry.adsPosition} " +
                    $"backupEnabled={adEntry.isBackup} backupGroup={adEntry.groupNameBackup} backupPos={adEntry.adsPositionBackup}");
                resolvedEntries.Add(adEntry);
            }

            return resolvedEntries;
        }

        private static bool TryResolveReadyResult(IAdStorage adStorage, AdGroupEntry adEntry, out SplashAdExecutionResult result,
            out string debugReason)
        {
            var mainStatus = adStorage.GetStatus(adEntry.type, adEntry.groupName);
            var backupStatus = adEntry.isBackup && !string.IsNullOrWhiteSpace(adEntry.groupNameBackup)
                ? adStorage.GetStatus(adEntry.type, adEntry.groupNameBackup)
                : AdGroupStatus.NotCalledLoadAds;

            SplashLogger.Log(
                $"Intro resolve-ready type={adEntry.type} group={adEntry.groupName} pos={adEntry.adsPosition} mainStatus={mainStatus} " +
                $"backupEnabled={adEntry.isBackup} backupGroup={adEntry.groupNameBackup} backupPos={adEntry.adsPositionBackup} backupStatus={backupStatus}");
            if (mainStatus == AdGroupStatus.AdsReady)
            {
                result = new SplashAdExecutionResult(true, false, adEntry.groupName, adEntry.adsPosition);
                debugReason = "primary-ready";
                return true;
            }

            if (adEntry.isBackup && backupStatus == AdGroupStatus.AdsReady)
            {
                result = new SplashAdExecutionResult(false, true, adEntry.groupNameBackup, adEntry.adsPositionBackup);
                debugReason = "backup-ready";
                return true;
            }

            result = default;
            debugReason = $"primary={mainStatus};backup={(adEntry.isBackup ? backupStatus.ToString() : "disabled")}";
            return false;
        }

        private async UniTaskVoid ShowIntroAdAsync(ISplashAdHandler handler, AdGroupEntry adEntry, SplashAdExecutionResult result,
            CancellationToken cancellationToken)
        {
            try
            {
                SplashLogger.Log(
                    $"Intro ad show start type={adEntry.type} selectedGroup={result.SelectedGroupName} selectedPos={result.SelectedAdsPosition} " +
                    $"primary={result.PrimaryReady} backup={result.BackupReady} handler={handler.GetType().Name}");
                handler.AdShow(adEntry, _adInfoConfig, _adStorageService, result, _showContext, cancellationToken);
                await handler.WaitToCompleteHandlerAsync(adEntry, _adStorageService, cancellationToken);
                ((IAdStorage)_adStorageService).SetStatus(adEntry.type, result.SelectedGroupName, AdGroupStatus.AdsComplete);
                SplashLogger.Log(
                    $"Intro ad show complete type={adEntry.type} selectedGroup={result.SelectedGroupName} selectedPos={result.SelectedAdsPosition}");
            }
            catch (OperationCanceledException)
            {
                SplashLogger.Log(
                    $"Intro ad show cancelled type={adEntry.type} selectedGroup={result.SelectedGroupName} selectedPos={result.SelectedAdsPosition}");
            }
            catch (Exception ex)
            {
                SplashLogger.Error($"Intro ad show failed type={adEntry.type} group={result.SelectedGroupName} error={ex}");
            }
        }

        private static void HideShownAds(List<ShownIntroAd> shownAds)
        {
            if (shownAds == null)
            {
                return;
            }

            foreach (var shownAd in shownAds)
            {
                if (shownAd.Type == AdGroupType.PU && !string.IsNullOrWhiteSpace(shownAd.Position))
                {
                    SplashLogger.Log($"Intro hide shown ad type={shownAd.Type} group={shownAd.GroupName} pos={shownAd.Position}");
                    NetCallerAPI.PU_Hide(shownAd.Position);
                }
                else
                {
                    SplashLogger.Log($"Intro skip hide shown ad type={shownAd.Type} group={shownAd.GroupName} pos={shownAd.Position}");
                }
            }
        }

        private async UniTask RunEntryCountdownAsync(IntroLayoutView view, int introTime, CancellationToken cancellationToken)
        {
            int remainingSeconds = Mathf.Max(0, introTime);
            SplashLogger.Log($"Intro countdown start seconds={remainingSeconds}");
            view.SetCountdown(remainingSeconds);
            if (remainingSeconds <= 0)
            {
                SplashLogger.Log("Intro countdown skipped reason=non-positive-time");
                return;
            }

            while (remainingSeconds > 0)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: cancellationToken);
                remainingSeconds--;
                view.SetCountdown(remainingSeconds);
            }

            SplashLogger.Log("Intro countdown complete");
        }

        private void ShowOnly(IntroLayoutView targetView)
        {
            foreach (var pair in _viewByType)
            {
                pair.Value.SetVisible(pair.Value == targetView);
            }
        }

        private void HideAllViews()
        {
            foreach (var pair in _viewByType)
            {
                pair.Value.SetVisible(false);
            }
        }

        private readonly struct ShownIntroAd
        {
            public ShownIntroAd(AdGroupType type, string groupName, string position)
            {
                Type = type;
                GroupName = groupName;
                Position = position;
            }

            public AdGroupType Type { get; }
            public string GroupName { get; }
            public string Position { get; }
        }
    }
}
