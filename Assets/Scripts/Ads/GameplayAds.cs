using System;
using HDC.Ads;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Ads
{
    /// <summary>
    /// The ads of the gameplay scene. The bottom banner shows while the scene is open.
    /// Game code calls <see cref="ShowForceAd(Action)"/> and <see cref="ShowRewarded(Action)"/>; UI buttons can
    /// use the methods without parameters. Positions are names from ads_config.
    /// </summary>
    public sealed class GameplayAds : MonoBehaviour
    {
        [Header("Positions from ads_config")]
        [SerializeField] private string forceAdPosition = "native_gameplay";
        [SerializeField] private string rewardedPosition = "gameplay";
        [SerializeField] private string popupPosition = "popup";

        [Header("Banner")]
        [SerializeField] private bool showBannerWhileOpen = true;

        [Header("Popup")]
        [Tooltip("The popup takes this rect's place on screen.")]
        [SerializeField] private RectTransform popupArea;

        [Header("Rewarded from UI buttons")]
        [SerializeField] private UnityEvent onRewardEarned;

        public bool IsRewardedReady => HDCAds.Rewarded.CanShow;

        private void OnEnable()
        {
            if (showBannerWhileOpen)
                HDCAds.Banner.Show();
        }

        private void OnDisable()
        {
            if (showBannerWhileOpen)
                HDCAds.Banner.Hide();
        }

        /// <summary>Shows a force ad when its capping allows. <paramref name="onDone"/> always runs, ad or not.</summary>
        public void ShowForceAd(Action onDone) => HDCAds.ForceAd.Show(forceAdPosition, onDone);

        public void ShowForceAd() => ShowForceAd(null);

        /// <summary>Shows a rewarded ad. <paramref name="onRewarded"/> runs only when the player earned the reward.</summary>
        public void ShowRewarded(Action onRewarded) => HDCAds.Rewarded.Show(rewardedPosition, onRewarded);

        public void ShowRewarded() => ShowRewarded(onRewardEarned.Invoke);

        public void ShowBanner() => HDCAds.Banner.Show();

        public void HideBanner() => HDCAds.Banner.Hide();

        public void ExpandBanner() => HDCAds.Banner.Expand();

        public void ShowPopup()
        {
            if (popupArea == null)
            {
                Debug.LogWarning("[GameplayAds] Set Popup Area to show the popup.", this);
                return;
            }

            HDCAds.Popup.Move(popupPosition, popupArea);
            HDCAds.Popup.Show(popupPosition);
        }

        public void HidePopup() => HDCAds.Popup.Hide(popupPosition);
    }
}
