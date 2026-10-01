using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace AppBootstrap.Splash
{
    public enum InternetStatus
    {
        None, Pending, InternetNotAvailable, InternetAvailable
    }
    
    public class NoInternetTracking : MonoBehaviour
    {
        [SerializeField] private int _delayRecallTrackingInternet = 2;
        [SerializeField] private int _networkTimeoutSeconds = 3;
        [SerializeField] private float _timeRecheckInternet = 5f;
        [SerializeField] private NoInternetPopup _noInternetPopup;
        [SerializeField] private InternetStatus _internetStatus = InternetStatus.None;

        private bool _isStartTracking = false;
        private float _timeSinceTracking = 0;
        
        public InternetStatus InternetStatus => _internetStatus;

        private void Awake()
        {
            /*CheckingNetworkConnection();*/
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            // if(!_isStartTracking) return;
            if (Application.internetReachability == NetworkReachability.NotReachable)
                _internetStatus = InternetStatus.InternetNotAvailable;
            else
                _internetStatus = InternetStatus.InternetAvailable;

            if (_internetStatus == InternetStatus.InternetNotAvailable)
            {
                if (!_noInternetPopup.IsEnable())
                {
                    Time.timeScale = 0;
                    _noInternetPopup.Enable();
                }
            }
            else
            {
                if (_noInternetPopup.IsEnable())
                {
                    Time.timeScale = 1;
                    _noInternetPopup.Disable();
                }
            }
        }

        public void Tracking()
        {
            _isStartTracking = true;
        }
    }
}