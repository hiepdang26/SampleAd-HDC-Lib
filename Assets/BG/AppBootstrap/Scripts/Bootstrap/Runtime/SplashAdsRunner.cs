using System;
using System.Collections.Generic;
using System.Threading;
using BG_Library.NET;
using BG_Library.NET.API;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AppBootstrap.Splash
{
    internal sealed class SplashAdsRunner : ISplashAdsRunner
    {
        private const string FirstOpenCompletedKey = "splash.first_open.completed";

        private readonly Dictionary<AdGroupType, ISplashAdHandler> _handlers;
        private readonly SplashAdShowContext _showContext;
        private readonly IAdStorage _adStorage;
        private readonly IConfigProvider _configProvider;
        private AdInfoConfig _adInfoConfig;
        private SplashConfig _splashConfig;
        private DeferredAppLauncherState _deferredAppLauncherState;

        public SplashAdsRunner(IEnumerable<ISplashAdHandler> handlers, SplashAdShowContext showContext,
            IAdStorage adStorage, IConfigProvider configProvider)
        {
            _handlers = new Dictionary<AdGroupType, ISplashAdHandler>();
            _showContext = showContext;
            _adStorage = adStorage;
            _configProvider = configProvider;

            foreach (var handler in handlers)
                _handlers[handler.GroupType] = handler;
        }

        public AdShowTriggerType PendingDeferredTrigger => _deferredAppLauncherState?.TriggerType ?? AdShowTriggerType.None;

        public async UniTask RunAsync(SplashConfig config, CancellationToken cancellationToken)
        {
            _deferredAppLauncherState = null;
            _splashConfig = config;

            if (config == null)
            {
                SplashLogger.Warn("Splash config is null in ads runner");
                return;
            }

            var tasks = BuildRunTasks(config, cancellationToken);
            if (tasks.Count == 0)
            {
                SplashLogger.Warn("Splash config has no runnable ad entries");
                return;
            }
            await UniTask.WhenAll(tasks);
        }

        public async UniTask CheckAndShowDeferredAsync(AdShowTriggerType triggerType, CancellationToken cancellationToken)
        {
            if (_deferredAppLauncherState == null)
            {
                SplashLogger.Log($"No deferred app launcher entry for trigger={triggerType}");
                return;
            }

            if (_deferredAppLauncherState.TriggerType != triggerType)
            {
                SplashLogger.Log($"Deferred app launcher trigger mismatch current={_deferredAppLauncherState.TriggerType} requested={triggerType}");
                return;
            }

            var pendingState = _deferredAppLauncherState;
            _deferredAppLauncherState = null;

            SplashLogger.Log($"Showing deferred app launcher trigger={triggerType} type={pendingState.GroupEntry.type}");
            await DelayShowIfNeededAsync(pendingState.DelayShowSeconds, cancellationToken);
            
            var currentAdStatus = _adStorage.GetStatus(pendingState.GroupEntry.type, pendingState.Result.SelectedGroupName);
            if (currentAdStatus == AdGroupStatus.WaitingLoadAds)
            {
                await UniTask.WaitUntil(() => _adStorage.GetStatus(pendingState.GroupEntry.type, pendingState.Result.SelectedGroupName) != AdGroupStatus.WaitingLoadAds, cancellationToken: cancellationToken);
            }
            
            pendingState.Handler.AdShow(pendingState.GroupEntry, _adInfoConfig, _adStorage, pendingState.Result, _showContext, cancellationToken);
            await pendingState.Handler.WaitToCompleteHandlerAsync(pendingState.GroupEntry, _adStorage, cancellationToken);
        }

        public void OnEndSceneHideAd()
        {
            if (_splashConfig.adEntryLoadings == null || _splashConfig.adEntryLoadings.Length == 0)
                return;

            if (_adInfoConfig == null)
                return;
            
            var splashEntries = ResolveSplashEntries(_splashConfig.adEntryLoadings, _adInfoConfig);
            foreach (var adGroupEntry in splashEntries)
            {
                if (_adStorage.GetStatus(adGroupEntry.type, adGroupEntry.groupName) == AdGroupStatus.AdsComplete &&
                    adGroupEntry.type == AdGroupType.PU)
                {
                    NetCallerAPI.PU_Hide(adGroupEntry.adsPosition);
                }
                
                if (_adStorage.GetStatus(adGroupEntry.type, adGroupEntry.groupNameBackup) == AdGroupStatus.AdsComplete &&
                    adGroupEntry.type == AdGroupType.PU)
                {
                    NetCallerAPI.PU_Hide(adGroupEntry.adsPositionBackup);
                }
            }
        }

        private List<UniTask> BuildRunTasks(SplashConfig config, CancellationToken cancellationToken)
        {
            var tasks = new List<UniTask>();
            _adInfoConfig = _configProvider.LoadRemoteConfig<AdInfoConfig>("ads_info_config");

            if (config.appLauncherEntry != null)
            {
                tasks.Add(InitializeAppLauncherEntryAsync(config.appLauncherEntry, cancellationToken));
            }

            if (config.adEntryLoadings == null || config.adEntryLoadings.Length == 0)
            {
                return tasks;
            }

            if (_adInfoConfig == null)
            {
                SplashLogger.Warn("ads_info_config is null while splash config requested ad entries");
                return tasks;
            }
            
            var splashEntries = ResolveSplashEntries(config.adEntryLoadings, _adInfoConfig);


            if (config.isAdsLoadAsync)
            {
                tasks.Add(RunSplashEntriesSequentiallyAsync(splashEntries, cancellationToken));
            }
            else
            {
                foreach (var entry in splashEntries)
                {
                    tasks.Add(InitializeSplashAdEntryAsync(entry, cancellationToken));
                    if (TryGetHandler(entry.type, "splash", out var handler))
                        tasks.Add(handler.WaitToCompleteHandlerAsync(entry, _adStorage, cancellationToken));
                }
            }
            return tasks;
        }
        
        private async UniTask InitializeAppLauncherEntryAsync(AdAppLauncherEntry entry, CancellationToken cancellationToken)
        {
            if (!TryGetHandler(entry.type, "app launcher", out var handler))
            {
                return;
            }

            var result = await ExecuteInitializationAsync(
                handler,
                entry.type,
                entry.groupName,
                entry.adsPosition,
                entry.loadSeconds,
                entry.isBackup,
                entry.groupNameBackup,
                entry.adsPositionBackup,
                entry.loadBackupSeconds,
                cancellationToken);

            /*if (!result.IsReady)
            {
                return;
            }*/

            var showEntry = ToSplashAdEntry(entry);
            /*if (entry.showTriggerType == AdShowTriggerType.OnLoadCompleted)
            {
                await DelayShowIfNeededAsync(entry.delayShowSeconds, cancellationToken);
                handler.AdShow(showEntry, _adStorage, result, _showContext, cancellationToken);
                await handler.WaitToCompleteHandlerAsync(showEntry, _adStorage, cancellationToken);
                return;
            }*/

            _deferredAppLauncherState = new DeferredAppLauncherState
            {
                TriggerType = entry.showTriggerType,
                DelayShowSeconds = entry.delayShowSeconds,
                Handler = handler,
                GroupEntry = showEntry,
                Result = result
            };

            SplashLogger.Log($"AppLauncher entry ready and deferred by trigger={entry.showTriggerType} type={entry.type}");
        }
        
        private List<AdGroupEntry> ResolveSplashEntries(string[] entryKeys, AdInfoConfig adConfig)
        {
            var entries = new List<AdGroupEntry>(entryKeys.Length);

            foreach (var entryKey in entryKeys)
            {
                var entry = adConfig.GetAdGroupEntry(entryKey);
                if (entry == null)
                {
                    SplashLogger.Warn($"Missing splash ad entry for key={entryKey}");
                    continue;
                }

                entries.Add(entry);
            }
            return entries;
        }

        private async UniTask RunSplashEntriesSequentiallyAsync(IReadOnlyList<AdGroupEntry> entries, CancellationToken cancellationToken)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                await InitializeSplashAdEntryAsync(entries[i], cancellationToken);
            }
            var tasks = new List<UniTask>();
            foreach (var entry in entries)
            {
                if (TryGetHandler(entry.type, "splash", out var handler))
                    tasks.Add(handler.WaitToCompleteHandlerAsync(entry, _adStorage, cancellationToken));
            }
            await tasks;
        }

        private async UniTask InitializeSplashAdEntryAsync(AdGroupEntry groupEntry, CancellationToken cancellationToken)
        {
            if (groupEntry.onlyLoadOnFirstOpen && !SplashBootstrap.IsFirstOpenSession)
            {
                SplashLogger.Log($"Skip splash ad because onlyLoadOnFirstOpen=true type={groupEntry.type} group={groupEntry.groupName}");
                return;
            }

            if (!TryGetHandler(groupEntry.type, "splash", out var handler))
            {
                return;
            }

            var result = await ExecuteInitializationAsync(
                handler,
                groupEntry.type,
                groupEntry.groupName,
                groupEntry.adsPosition,
                groupEntry.loadSeconds,
                groupEntry.isBackup,
                groupEntry.groupNameBackup,
                groupEntry.adsPositionBackup,
                groupEntry.loadBackupSeconds,
                cancellationToken);

            if (!result.IsReady)
            {
                return;
            }

            if (!groupEntry.autoShowOnLoaded)
            {
                SplashLogger.Log($"Ad loaded without auto show type={groupEntry.type} group={result.SelectedGroupName}");
                return;
            }

            handler.AdShow(groupEntry, _adInfoConfig, _adStorage, result, _showContext, cancellationToken);
        }

        private bool TryGetHandler(AdGroupType groupType, string entryKind, out ISplashAdHandler handler)
        {
            if (_handlers.TryGetValue(groupType, out handler))
            {
                return true;
            }

            SplashLogger.Warn($"Missing handler for {entryKind} ad type={groupType}");
            return false;
        }

        private static AdGroupEntry ToSplashAdEntry(AdAppLauncherEntry entry)
        {
            return new AdGroupEntry
            {
                loadSeconds = entry.loadSeconds,
                showDurationSeconds = 0f,
                onlyLoadOnFirstOpen = false,
                autoShowOnLoaded = true,
                type = entry.type,
                groupName = entry.groupName,
                adsPosition = entry.adsPosition,
                isBackup = entry.isBackup,
                loadBackupSeconds = entry.loadBackupSeconds,
                groupNameBackup = entry.groupNameBackup,
                adsPositionBackup = entry.adsPositionBackup
            };
        }

        private async UniTask<SplashAdExecutionResult> ExecuteInitializationAsync(
            ISplashAdHandler handler,
            AdGroupType groupType,
            string groupName,
            string adsPosition,
            float loadSeconds,
            bool isBackup,
            string groupNameBackup,
            string adsPositionBackup,
            float loadBackupSeconds,
            CancellationToken cancellationToken)
        {
            SplashLogger.Log(
                $"Init ad start type={groupType} group={groupName} timeout={loadSeconds} backupEnabled={isBackup} backupGroup={groupNameBackup} backupTimeout={loadBackupSeconds}");

            var primaryStatus = await _adStorage.LoadAdsAsync(handler, groupType, groupName, loadSeconds, cancellationToken);
            bool primaryReady = primaryStatus == AdGroupStatus.AdsReady;
            SplashLogger.Log($"Primary result type={groupType} group={groupName} status={primaryStatus} ready={primaryReady}");

            bool backupReady = false;
            if (!primaryReady && isBackup && !string.IsNullOrWhiteSpace(groupNameBackup))
            {
                SplashLogger.Warn($"Primary group not ready type={groupType} group={groupName} status={primaryStatus}, switching backup={groupNameBackup}");
                var backupStatus = await _adStorage.LoadAdsAsync(handler, groupType, groupNameBackup, loadBackupSeconds, cancellationToken);
                backupReady = backupStatus == AdGroupStatus.AdsReady;
                SplashLogger.Log($"Backup result type={groupType} group={groupNameBackup} status={backupStatus} ready={backupReady}");
            }

            var result = new SplashAdExecutionResult(
                primaryReady,
                backupReady,
                primaryReady ? groupName : groupNameBackup,
                primaryReady ? adsPosition : adsPositionBackup);

            SplashLogger.Log(
                $"Init ad end type={groupType} primary={groupName} primaryReady={primaryReady} backup={groupNameBackup} backupReady={backupReady} ready={result.IsReady}");

            return result;
        }

        private static async UniTask DelayShowIfNeededAsync(float delayShowSeconds, CancellationToken cancellationToken)
        {
            float delay = Mathf.Max(0f, delayShowSeconds);
            if (delay <= 0f)
            {
                return;
            }

            SplashLogger.Log($"Delay show seconds={delay}");
            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: cancellationToken);
        }
    }
}
