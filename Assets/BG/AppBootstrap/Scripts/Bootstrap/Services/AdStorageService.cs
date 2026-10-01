using System;
using System.Collections.Generic;
using System.Threading;
using BG_Library.Common;
using BG_Library.NET;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AppBootstrap.Splash
{
    [CreateAssetMenu(fileName = "Ad storage service", menuName = "Splash/Ad Storage Service")]
    public class AdStorageService : ScriptableObject, IAdStorage
    {
        [ShowInInspector] private readonly Dictionary<string, AdGroupDataEntry> _adData = new Dictionary<string, AdGroupDataEntry>();
        private bool _isInitialized;

        void IAdStorage.Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            _adData.Clear();
            NetEventSystem.OnFsLoadFailed += HandleFullscreenLoadFailed;
            NetEventSystem.OnRectLoadFailed += HandleRectLoadFailed;
            
            NetEventSystem.OnFsLoaded += HandleFullscreenLoaded;
            NetEventSystem.OnRectLoaded += HandleRectLoaded;
            _isInitialized = true;
        }

        AdGroupStatus IAdStorage.GetStatus(AdGroupType groupType, string groupName)
        {
            return GetStatusInternal(groupType, groupName);
        }

        void IAdStorage.SetStatus(AdGroupType groupType, string groupName, AdGroupStatus status)
        {
            SetStatusInternal(groupType, groupName, status);
        }

        UniTask<AdGroupStatus> IAdStorage.LoadAdsAsync(ISplashAdHandler handler, AdGroupType groupType, string groupName,
            float timeoutSeconds, CancellationToken cancellationToken)
        {
            return LoadAdsInternalAsync(handler, groupType, groupName, timeoutSeconds, cancellationToken);
        }

        private AdGroupStatus GetStatusInternal(AdGroupType groupType, string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName))
            {
                return AdGroupStatus.NotCalledLoadAds;
            }

            if (_adData.TryGetValue(BuildKey(groupType, groupName), out var entry))
            {
                return entry.adGroupStatus;
            }

            return AdGroupStatus.NotCalledLoadAds;
        }

        private async UniTask<AdGroupStatus> LoadAdsInternalAsync(ISplashAdHandler handler, AdGroupType groupType, string groupName,
            float timeoutSeconds, CancellationToken cancellationToken)
        {
            if (handler == null || string.IsNullOrWhiteSpace(groupName))
            {
                return AdGroupStatus.LoadAdsFailed;
            }

            var currentStatus = GetStatusInternal(groupType, groupName);
            if (currentStatus == AdGroupStatus.AdsReady || currentStatus == AdGroupStatus.WaitingLoadAds)
            {
                SplashLogger.Log($"Ad storage reuse type={groupType} group={groupName} status={currentStatus}");
                return await WaitForCompletionAsync(handler, groupType, groupName, timeoutSeconds, cancellationToken);
            }

            SetStatusInternal(groupType, groupName, AdGroupStatus.WaitingLoadAds);
            SplashLogger.Log($"Ad storage load start type={groupType} group={groupName} status={AdGroupStatus.WaitingLoadAds} timeout={timeoutSeconds}");
            handler.Initialize(groupName);

            return await WaitForCompletionAsync(handler, groupType, groupName, timeoutSeconds, cancellationToken);
        }

        private async UniTask<AdGroupStatus> WaitForCompletionAsync(ISplashAdHandler handler, AdGroupType groupType,
            string groupName, float timeoutSeconds, CancellationToken cancellationToken)
        {
            using (var linkedCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                var linkedToken = linkedCancellationSource.Token;
                var waitTask = WaitUntilCompletedAsync(handler, groupType, groupName, linkedToken);
                var timeoutTask = UniTask.Delay(TimeSpan.FromSeconds(Math.Max(0f, timeoutSeconds)), DelayType.DeltaTime,
                    cancellationToken: linkedToken);

                int completedIndex = await UniTask.WhenAny(waitTask, timeoutTask);
                linkedCancellationSource.Cancel();

                if (completedIndex == 1 && GetStatusInternal(groupType, groupName) == AdGroupStatus.WaitingLoadAds)
                {
                    SetStatusInternal(groupType, groupName, AdGroupStatus.TimeOut);
                    SplashLogger.Warn($"Ad storage timeout type={groupType} group={groupName} status={AdGroupStatus.TimeOut}");
                }
            }

            var finalStatus = GetStatusInternal(groupType, groupName);
            SplashLogger.Log($"Ad storage load end type={groupType} group={groupName} status={finalStatus}");
            return finalStatus;
        }

        private async UniTask WaitUntilCompletedAsync(ISplashAdHandler handler, AdGroupType groupType, string groupName,
            CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() =>
            {
                /*
                if (handler.IsReady(groupName))
                {
                    SetStatusInternal(groupType, groupName, AdGroupStatus.AdsReady);
                    return true;
                }
                */

                var status = GetStatusInternal(groupType, groupName);
                return status != AdGroupStatus.WaitingLoadAds;
            }, cancellationToken: cancellationToken);
        }

        private void HandleFullscreenLoadFailed(AdInfo info, string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(info.group))
            {
                return;
            }

            SetStatusInternal(AdGroupType.FA, info.group, AdGroupStatus.LoadAdsFailed);
            SplashLogger.Warn($"Ad storage event failed type={AdGroupType.FA} group={info.group} error={errorMessage}");
        }

        private void HandleRectLoadFailed(AdInfo info, int errorCode, string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(info.group))
            {
                return;
            }

            SetStatusInternal(AdGroupType.PU, info.group, AdGroupStatus.LoadAdsFailed);
            SplashLogger.Warn($"Ad storage event failed type={AdGroupType.PU} group={info.group} error={errorMessage}");
        }

        private void HandleRectLoaded(AdInfo adInfo)
        {
            if (string.IsNullOrWhiteSpace(adInfo.group))
            {
                return;
            }

            SetStatusInternal(AdGroupType.PU, adInfo.group, AdGroupStatus.AdsReady);
            SetAdInfo(AdGroupType.PU, adInfo.group, adInfo);
            SplashLogger.Log($"Ad storage event loaded type={AdGroupType.PU} group={adInfo.group} adSource={adInfo.adSource}");
        }

        private void HandleFullscreenLoaded(AdInfo adInfo)
        {
            if (string.IsNullOrWhiteSpace(adInfo.group))
            {
                return;
            }

            SetStatusInternal(AdGroupType.FA, adInfo.group, AdGroupStatus.AdsReady);
            SetAdInfo(AdGroupType.FA, adInfo.group, adInfo);
            SplashLogger.Log($"Ad storage event loaded type={AdGroupType.FA} group={adInfo.group} adSource={adInfo.adSource}");
        }
        
        private void SetStatusInternal(AdGroupType groupType, string groupName, AdGroupStatus groupStatus)
        {
            string key = BuildKey(groupType, groupName);
            if (_adData.TryGetValue(key, out var entry))
            {
                entry.adGroupStatus = groupStatus;
                return;
            }

            _adData[key] = new AdGroupDataEntry
            {
                adType = groupType,
                adGroupName = groupName,
                adGroupStatus = groupStatus
            };
        }
        
        public AdGroupDataEntry GetAdGroupDataEntry(AdGroupType groupType, string groupName)
        {
            string key = BuildKey(groupType, groupName);
            
            if (_adData.TryGetValue(key, out var entry)) 
                return entry;
            
            _adData[key] = new AdGroupDataEntry
            {
                adType = groupType,
                adGroupName = groupName
            };
            return _adData[key];
        }

        private void SetAdInfo(AdGroupType groupType, string groupName, AdInfo adInfo)
        {
            string key = BuildKey(groupType, groupName);
            if (_adData.TryGetValue(key, out var entry))
            {
                entry.adInfo = adInfo;
                return;
            }

            _adData[key] = new AdGroupDataEntry
            {
                adType = groupType,
                adGroupName = groupName,
                adInfo = adInfo
            }; 
        }

        private static string BuildKey(AdGroupType groupType, string groupName)
        {
            return $"{(int)groupType}:{groupName}";
        }
    }
}
