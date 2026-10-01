using System;
using System.Threading;
using BG_Library.NET;
using BG_Library.NET.API;
using BG_Library.Common;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AppBootstrap.Splash
{
    public class NativeForceAdRepository : IAdRequester
    {
        private readonly string _groupName;
        private AdStatus _status;
        private string _position;
        private float _timeOut;
        
        public string Position => _position;
        public string GroupName => _groupName;
        public AdStatus Status => _status;
        public Action<AdStatus> OnAdStatusChanged;

        public NativeForceAdRepository(string position, string groupName, float timeOut)
        {
            _groupName = groupName;
            _position = position;
            _timeOut = timeOut;
            
            NetEventSystem.OnFsLoaded += OnAdLoaded;
            NetEventSystem.OnFsLoadFailed += OnAdLoadFailed;
            NetEventSystem.OnFsPaid += OnAdPaidImpression;
            NetEventSystem.OnFsDisplayed += OnRectDisplayed;
            NetEventSystem.OnFsClosed += OnAdClosed;
        }

        #region CallBack

        private void OnAdLoadFailed(AdInfo adInfo, string errorMessage)
        {
            if (adInfo.group.Equals(_groupName))
            {
                SplashTracking.Tracking($"5_{_position}_l_s_failed");
                ChangeStatus(AdStatus.LoadFailed);
            }
        }

        private void OnAdLoaded(AdInfo adInfo)
        {
            if (adInfo.group.Equals(_groupName))
            {
                SplashTracking.Tracking($"5_{_position}_l_s_loaded");
                ChangeStatus(AdStatus.Loaded);
            }
        }
        
        private void OnAdPaidImpression(AdInfo adInfo, AdValueInfo adValueInfo)
        {
            if (adInfo.group.Equals(_groupName))
            {
                SplashTracking.Tracking($"5_{_position}_s_s_paid");
                ChangeStatus(AdStatus.AdPaidImpression);
            }
        }
        
        private void OnRectDisplayed(AdInfo adInfo)
        {
            if (adInfo.group.Equals(_groupName))
            {
                SplashTracking.Tracking($"5_{_position}_s_s_displayed");
                ChangeStatus(AdStatus.AdDisplayed);
            }
        }
        
        private void OnAdClosed(AdInfo adInfo)
        {
            SplashLogger.Log($"Ad Closed {adInfo.group} | {_groupName} | {_status} | {adInfo.group.Equals(_groupName)}");
            if (adInfo.group.Equals(_groupName))
            {
                SplashLogger.Log($"Ad Tracking {adInfo.group} | {_groupName}");
                SplashTracking.Tracking($"5_{_position}_s_s_closed");
                ChangeStatus(AdStatus.AdClosed);
            }
        }


        #endregion

        #region Core

        public async UniTask<AdStatus> LoadAsync(CancellationToken ct)
        {
            ChangeStatus(AdStatus.Loading);
            NetCallerAPI.FA_InitManually(_groupName);
            await UniTask.WhenAny(
                UniTask.WaitUntil(() => _status != AdStatus.Loading, cancellationToken: ct),
                UniTask.Delay(TimeSpan.FromSeconds(_timeOut), DelayType.DeltaTime, cancellationToken: ct));
            if (_status == AdStatus.Loading)
            {
                SplashTracking.Tracking($"5_{_position}_l_s_timeout");
                ChangeStatus(AdStatus.LoadTimeOut);
            }
            return _status;
        }

        public async UniTask ShowAsync(CancellationToken ct)
        {
            NetCallerAPI.FA_Show(_groupName);
            await UniTask.WaitUntil(() => _status == AdStatus.AdClosed, cancellationToken: ct);
        }

        public bool IsReady()
        {
            return _status == AdStatus.Loaded;
        }

        public void ChangeStatus(AdStatus status)
        {
            _status = status;
            OnAdStatusChanged?.Invoke(status);
        }
        #endregion

    }
}
