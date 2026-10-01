using System;
using System.Threading;
using BG_Library.NET;
using BG_Library.NET.API;
using BG_Library.Common;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AppBootstrap.Splash
{
    public class PopupAdRepository : IAdRequester
    {
        // Extra time ShowAsync waits for the impression once the show time is over.
        private const float ImpressionWaitAfterShowSeconds = 10f;

        private readonly string _groupName;
        private AdStatus _status;
        private readonly float _timeShowing;
        private bool _firstTimeCallLoad;
        private string _position;
        private string _adSourceAdapterClassName;
        private float _timeOut;
        private bool _isTracking;
        // Sticky: _status can be overwritten by later events before ShowAsync polls it.
        private bool _hasImpression;
        
        public string GroupName => _groupName;
        public AdStatus Status => _status;
        public Action<AdStatus> OnAdStatusChanged;
        public string AdSourceAdapterClassName => _adSourceAdapterClassName;

        public PopupAdRepository(string groupName, string position, float timeOut, float timeShowing, bool tracking = true)
        {
            _groupName = groupName;
            _position = position;
            _timeOut = timeOut;
            _timeShowing = timeShowing;
            _isTracking = tracking;
            _firstTimeCallLoad = true;
            
            NetEventSystem.OnRectLoaded += OnAdLoaded;
            NetEventSystem.OnRectLoadFailed += OnAdLoadFailed;
            NetEventSystem.OnRectPaid += OnAdPaidImpression;
            NetEventSystem.OnRectImpression += OnAdImpression;
            NetEventSystem.OnRectDisplayed += OnRectDisplayed;
        }

        #region CallBack

        private void OnAdLoadFailed(AdInfo adInfo, int errorCode, string errorMessage)
        {
            if (adInfo.group.Equals(_groupName))
            {
                if(_isTracking) SplashTracking.Tracking($"5_{_position}_l_s_failed");
                ChangeStatus(AdStatus.LoadFailed);
            }
        }

        private void OnAdLoaded(AdInfo adInfo)
        {
            if (adInfo.group.Equals(_groupName))
            {
                if(_isTracking) SplashTracking.Tracking($"5_{_position}_l_s_loaded");
                _adSourceAdapterClassName = adInfo.adSource;
                ChangeStatus(AdStatus.Loaded);
            }
        }
        
        private void OnAdImpression(AdInfo adInfo)
        {
            if (adInfo.group.Equals(_groupName))
            {
                _hasImpression = true;
            }
        }

        private void OnAdPaidImpression(AdInfo adInfo, AdValueInfo adValueInfo)
        {
            if (adInfo.group.Equals(_groupName))
            {
                if(_isTracking) SplashTracking.Tracking($"5_{_position}_s_s_paid");
                _hasImpression = true;
                ChangeStatus(AdStatus.AdPaidImpression);
            }
        }
        
        private void OnRectDisplayed(AdInfo adInfo)
        {
            if (adInfo.group.Equals(_groupName))
            {
                if(_isTracking) SplashTracking.Tracking($"5_{_position}_s_s_displayed");
                ChangeStatus(AdStatus.AdDisplayed);
            }
        }

        #endregion

        #region Core

        public async UniTask<AdStatus> LoadAsync(CancellationToken ct)
        {
            ChangeStatus(AdStatus.Loading);
            if (_firstTimeCallLoad)
            {
                NetCallerAPI.PU_InitManually(_groupName);
                _firstTimeCallLoad = false;
            }
            else
            {
                NetCallerAPI.PU_ForceInit(_groupName);
            }
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

        public async UniTask ShowAsync(PULayout layout,CancellationToken ct)
        {
            _hasImpression = false;
            NetCallerAPI.PU_UpdatePos(_groupName, layout);
            NetCallerAPI.PU_Show(_groupName);
            await UniTask.Delay(TimeSpan.FromSeconds(_timeShowing), DelayType.DeltaTime, cancellationToken: ct);
            if (_hasImpression) return;

            // The SDK may never record the impression (e.g. the native view could not be registered),
            // so wait a bounded time for it instead of blocking the caller forever.
            var deadline = Time.unscaledTime + ImpressionWaitAfterShowSeconds;
            await UniTask.WaitUntil(() => _hasImpression || Time.unscaledTime >= deadline, cancellationToken: ct);
            if (!_hasImpression)
                SplashLogger.Warn($"Popup {_groupName} got no impression {ImpressionWaitAfterShowSeconds:0.#}s after show time, continue");
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
