using UnityEngine;

namespace AppBootstrap.Splash
{
    public class NoInternetPopup : MonoBehaviour
    {
        [SerializeField] private GameObject tryReconnectContent;
        [SerializeField] private GameObject reconnectContent;

        public void ToggleTryReconnect()
        {
            tryReconnectContent.SetActive(true);
            reconnectContent.SetActive(false);
        }

        public void ToggleReconnect()
        {
            tryReconnectContent.SetActive(false);
            reconnectContent.SetActive(true);
        }

        public bool IsEnable()
        {
            return gameObject.activeInHierarchy;
        }

        public void Disable()
        {
            gameObject.SetActive(false);
        }
        
        public void Enable()
        {
            gameObject.SetActive(true);
        }
    }
}