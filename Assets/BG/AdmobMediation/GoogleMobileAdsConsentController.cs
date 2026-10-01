using System;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Common;
using GoogleMobileAds.Ump.Api;
using BG_Library.NET.AdSystem;

namespace BG_Library.NET.Mediation.Admob
{
	/// <summary>
	/// Helper class that implements consent using the Google User Messaging Platform (UMP) SDK.
	/// </summary>
	public class GoogleMobileAdsConsentController
    {
        /// <summary>
        /// If true, it is safe to call MobileAds.Initialize() and load Ads.
        /// </summary>
        public bool CanRequestAds => ConsentInformation.CanRequestAds();

        /// <summary>
        /// Startup method for the Google User Messaging Platform (UMP) SDK
        /// which will run all startup logic including loading any required
        /// updates and displaying any required forms.
        /// </summary>
        public void GatherConsent(Action<string> onComplete)
        {
            UnityEngine.Debug.Log("Gathering consent.");

            DebugGeography debugGeography = DebugGeography.Disabled;
            if (NetConfigsSO.Ins.Admob_TestDevice)
                debugGeography = DebugGeography.EEA;

            List<string> testDeviceHashedIds = Admob_MediationManager.GetAutoConsentTestDeviceHashedIds();

            var requestParameters = new ConsentRequestParameters
            {
                // False means users are not under age.
                TagForUnderAgeOfConsent = false,
                ConsentDebugSettings = new ConsentDebugSettings
                {
                    // For debugging consent settings by geography.
                    DebugGeography = debugGeography,
                    // https://developers.google.com/admob/unity/test-ads
                    TestDeviceHashedIds = testDeviceHashedIds,
                }
            };

            // Combine the callback with an error popup handler.
            onComplete = (onComplete == null)
                ? UpdateErrorPopup
                : onComplete + UpdateErrorPopup;

            // The Google Mobile Ads SDK provides the User Messaging Platform (Google's
            // IAB Certified consent management platform) as one solution to capture
            // consent for users in GDPR impacted countries. This is an example and
            // you can choose another consent management platform to capture consent.
            ConsentInformation.Update(requestParameters, (FormError updateError) =>
            {
                // Enable the change privacy settings button.
                UpdatePrivacyButton();

                if (updateError != null)
                {
                    onComplete(updateError.Message);
                    return;
                }

                // Determine the consent-related action to take based on the ConsentStatus.
                if (CanRequestAds)
                {
                    // Consent has already been gathered or not required.
                    // Return control back to the user.
                    onComplete(null);
                    return;
                }

                // Consent not obtained and is required.
                // Load the initial consent request form for the user.
                ConsentForm.LoadAndShowConsentFormIfRequired((FormError showError) =>
                {
                    UpdatePrivacyButton();
                    if (showError != null)
                    {
                        // Form showing failed.
                        if (onComplete != null)
                        {
                            onComplete(showError.Message);
                        }
                    }
                    // Form showing succeeded.
                    else if (onComplete != null)
                    {
                        onComplete(null);
                    }
                });
            });
        }

        /// <summary>
        /// Shows the privacy options form to the user.
        /// </summary>
        /// <remarks>
        /// Your app needs to allow the user to change their consent status at any time.
        /// Load another form and store it to allow the user to change their consent status
        /// </remarks>
        public void ShowPrivacyOptionsForm(Action<string> onComplete)
        {
            UnityEngine.Debug.Log("Showing privacy options form.");

            // combine the callback with an error popup handler.
            onComplete = (onComplete == null)
                ? UpdateErrorPopup
                : onComplete + UpdateErrorPopup;

            ConsentForm.ShowPrivacyOptionsForm((FormError showError) =>
            {
                UpdatePrivacyButton();
                if (showError != null)
                {
                    // Form showing failed.
                    if (onComplete != null)
                    {
                        onComplete(showError.Message);
                    }
                }
                // Form showing succeeded.
                else if (onComplete != null)
                {
                    onComplete(null);
                }
            });
        }

        /// <summary>
        /// Reset ConsentInformation for the user.
        /// </summary>
        public void ResetConsentInformation()
        {
            ConsentInformation.Reset();
            UpdatePrivacyButton();
        }

        void UpdatePrivacyButton()
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
			{
                UnityEngine.Debug.Log($"[Admob UMP] Update Privacy Button: " +
                    $"{ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required}");
			});
		}

        void UpdateErrorPopup(string message)
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                if (string.IsNullOrEmpty(message))
                {
                    return;
                }

                UnityEngine.Debug.LogError($"[Admob UMP] Error with message: {message}");
            });
        }
    }
}
