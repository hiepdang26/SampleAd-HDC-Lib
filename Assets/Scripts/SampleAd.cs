// using BG_Library.NET;
// using BG_Library.NET.AdSystem;
// using BG_Library.NET.API;
// using BG_Library.NET.Mediation;
// using BG_Library.NET.Mediation.Admob;
// using Firebase.RemoteConfig;
// using System;
// using System.Collections;
// using UnityEngine;
// using UnityEngine.UI;

// namespace BG_Lib.AndroidMediation.Scripts.sample
// {
//     /// <summary>
//     /// SampleAd
//     /// - Quản lý toàn bộ Ad Instance
//     /// - Bind UI Buttons
//     /// - AndroidNAConfig luôn dùng array ids
//     /// </summary>
//     public class SampleAd : MonoBehaviour
//     {
//         // ───────────────── UI ─────────────────
//         [Header("Banner Buttons")]
//         public Button btnLoadBanner;
//         public Button btnShowBanner;
//         public Button btnExpandBanner;
//         public Button btnHideBanner;
//         public Button btnCheckBNNAIsReady;

//         [Header("Fullscreen Buttons")]
//         public Button btnLoadAdFullscreen;
//         public Button btnShowFullscreenSingle;
//         public Button btnShowFullscreenMultiple;
//         public Button btnShowFullscreenSequence;
//         public Button btnCheckFSNAIsReady;

//         [Header("Popup Buttons")]
//         public Button btnLoadPopup;
//         public Button btnShowPopupManual;
//         public Button btnShowPopupAuto;
//         public Button btnClosePopup;
//         public Button btnCheckPopupIsReady;

//         [Header("Collapse Buttons")]
//         public Button btnLoadCollapse;
//         public Button btnShowCollapse;
//         public Button btnCloseCollapse;
//         public Button btnCheckCollapseIsReady;

//         [Header("Collapse Reload Buttons")]
//         public Button btnLoadCollapseReload;
//         public Button btnShowCollapseReload;
//         public Button btnHideCollapseReload;
//         public Button btnStopCollapseReload;
//         public Button btnCheckCollapseReloadIsReady;

//         [Header("Utility Buttons")]
//         public Button btnOpenInspector;
//         public Button btnOpenDebugPanel;

//         // ───────────────── Ad Instances ─────────────────
//         private BNNAInstance bannerAd;
//         private FSNAInstance fullscreenAd;
//         private ONAPopupInstace popupAd;
//         private ONACollapseReloadByTimeClickInstance reloadAd;

//         // ───────────────── Lifecycle ─────────────────
//         private IEnumerator Start()
//         {
//             Debug.LogWarning("[SampleAd][Legacy] This old sample script is running. SampleScene should use SampleAdWithNewIns instead.");
//             yield return new WaitUntil(() => Admob_MediationManager.IsInitComplete);
//             AdsUtility.SetEnvironment("staging");
//             AdsUtility.StartInternetChecking();
//             //AdsUtility.OpenDebugPanel();

//             InitCallbackBanner();
//             InitCallbackFullscreen();
//             InitCallbackPopup();
//             InitCallbackReload();

//             BindUI();
//         }

//         // ───────────────── Banner ─────────────────
//         void InitCallbackBanner()
//         {
//             var config = new AndroidNAConfig(new[]
//             {
//         "ca-app-pub-3940256099942544/2247696110"
//     });

//             bannerAd = new BNNAInstance(config);

//             bannerAd.OnBannerLoaded += ad =>
//                 Debug.Log($"✅ Banner Loaded: {ad.adUnitId}");

//             bannerAd.OnBannerDisplayed += ad =>
//                 Debug.Log("📺 Banner Displayed");

//             bannerAd.OnBannerClosed += ad =>
//                 Debug.Log("❌ Banner Closed");

//             bannerAd.OnBannerClicked += ad =>
//                 Debug.Log("🖱 Banner Clicked");

//             bannerAd.OnBannerFailedToLoad += (adUnit, errorCode, error) =>
//                 Debug.LogError($"❌ Banner Failed: {adUnit} | {errorCode} | {error}");

//             bannerAd.OnBannerDisplayable += () =>
//                 Debug.Log("🎯 Banner Displayable");

//             bannerAd.OnBannerCollapsed += () =>
//                 Debug.Log("⬇️ Banner Collapsed");

//             bannerAd.OnBannerPaidImpression += (ad, paid) =>
//             {
//                 var revenue = paid.revenueMicros / 1_000_000.0;
//                 Debug.Log($"💰 Banner Revenue: {revenue} {paid.currencyCode}");
//             };
//         }


//         // ───────────────── Fullscreen ─────────────────
//         void InitCallbackFullscreen()
//         {
//             var config = new AndroidNAConfig(new[]
//             {
//         "ca-app-pub-3940256099942544/2247696110x"
//     });

//             fullscreenAd = new FSNAInstance(config);

//             fullscreenAd.OnFSNALoaded += ad =>
//                 Debug.Log($"FS Loaded: {ad.adUnitId}");

//             fullscreenAd.OnFSNADisplayed += ad =>
//                 Debug.Log("FS Displayed");

//             fullscreenAd.OnFSNAClosed += ad =>
//                 Debug.Log("FS Closed");

//             fullscreenAd.OnFSNAClicked += ad =>
//                 Debug.Log("FS Clicked");

//             fullscreenAd.OnFSNAFailedToLoad += (adUnit, errorCode, error) =>
//                 Debug.LogError($"FS Failed: {adUnit} | {error}");

//             fullscreenAd.OnFSNADisplayable += () =>
//                 Debug.Log("FS Displayable");

//             fullscreenAd.OnFSNAPaidImpression += (ad, paid) =>
//             {
//                 var revenue = paid.revenueMicros / 1_000_000.0;
//                 Debug.Log($"💰 FS Revenue: {revenue} {paid.currencyCode}");
//             };
//         }



//         // ───────────────── Popup ─────────────────
//         void InitCallbackPopup()
//         {
//             popupAd = new ONAPopupInstace(
//                 new AndroidNAConfig(new[] { "ca-app-pub-3940256099942544/2247696110x" })
//             );

//             popupAd.OnONAPopupLoaded += ad =>
//                 Debug.Log($"✅ Popup Loaded: {ad.adUnitId}");

//             popupAd.OnONAPopupDisplayed += ad =>
//                 Debug.Log("📺 Popup Displayed");

//             popupAd.OnONAPopupClosed += ad =>
//                 Debug.Log("❌ Popup Closed");

//             popupAd.OnONAPopupClicked += ad =>
//                 Debug.Log("🖱 Popup Clicked");

//             popupAd.OnONAPopupFailedToload += (adUnit, errorCode, error) =>
//                 Debug.LogError($"❌ Popup Failed: {adUnit} | {error}");

//             popupAd.OnONAPopupDisplayable += () =>
//                 Debug.Log("🎯 Popup Displayable");

//             popupAd.OnONAPopupPaidImpression += (ad, paid) =>
//             {
//                 var revenue = paid.revenueMicros / 1_000_000.0;
//                 Debug.Log($"💰 Popup Revenue: {revenue} {paid.currencyCode}");
//             };
//         }

//         // ───────────────── Reload ─────────────────
//         void InitCallbackReload()
//         {
//             reloadAd = new ONACollapseReloadByTimeClickInstance(
//                 new AndroidNAConfig(new[] { "ca-app-pub-3940256099942544/2247696110x" })
//             );

//             reloadAd.OnCollapseReloadLoaded += ad =>
//                 Debug.Log($"✅ Reload Loaded: {ad.adUnitId}");

//             reloadAd.OnCollapseReloadDisplayed += ad =>
//                 Debug.Log("📺 Reload Displayed");

//             reloadAd.OnCollapseReloadOpened += ad =>
//                 Debug.Log("🔓 Reload Opened");

//             reloadAd.OnCollapseReloadClosed += ad =>
//                 Debug.Log("❌ Reload Closed");

//             reloadAd.OnCollapseReloadClicked += ad =>
//                 Debug.Log("🖱 Reload Clicked");

//             reloadAd.OnCollapseReloadFailedToLoad += (adUnit, errorCode, error) =>
//                 Debug.LogError($"❌ Reload Failed: {adUnit}, {errorCode}, {error}");

//             reloadAd.OnCollapseReloadDisplayable += () =>
//                 Debug.Log("🎯 Reload Displayable");

//             reloadAd.OnCollapseReloadPaidImpression += (ad, paid) =>
//             {
//                 var revenue = paid.revenueMicros / 1_000_000.0;
//                 Debug.Log($"💰 Reload Revenue: {revenue} {paid.currencyCode}");
//             };
//         }


//         // ───────────────── UI Binding ─────────────────
//         void BindUI()
//         {
//             // ───────────────── BNNA ─────────────────
//             btnLoadBanner?.onClick.AddListener(() => bannerAd.PreloadAll());
//             btnShowBanner?.onClick.AddListener(() => bannerAd?.Show(new[] { "bn_single_01" }, 5));
//             //btnExpandBanner?.onClick.AddListener(() => bannerAd?.Expand(false));
//             btnHideBanner?.onClick.AddListener(() => bannerAd?.Hide());

//             // ───────────────── FSNA ─────────────────
//             btnLoadAdFullscreen?.onClick.AddListener(() => fullscreenAd.PreloadAll());

//             btnShowFullscreenSingle?.onClick.AddListener(() => fullscreenAd?.ShowSingle(new[] { "fs_single_universal_05" }, 5, "portrait"));

//             btnShowFullscreenMultiple?.onClick.AddListener(() => fullscreenAd?.ShowMultiple(new[] { "fs_multi_01" }, 5));

//             btnShowFullscreenSequence?.onClick.AddListener(() => fullscreenAd?.ShowSequence(new[] { "fs_single_universal_04" }, new[] { 3, 4, 5 }));

//             btnCheckFSNAIsReady?.onClick.AddListener(() => checkIsReady());


//             // ───────────────── ONA POPUP ─────────────────
//             btnLoadPopup?.onClick.AddListener(() => popupAd.PreloadAll());

//             btnShowPopupManual?.onClick.AddListener(() => popupAd?.ShowManualClose("mrec_single_manual_05", 3, 5, 50, 100, 300, 300));

//             btnClosePopup?.onClick.AddListener(() => popupAd?.Close());

//             // ───────────────── ONA COLLAPSE RELOAD BY TIMES CLICK (NEW) ─────────────────
//             btnLoadCollapseReload?.onClick.AddListener(() => reloadAd.PreloadAll());

//             btnShowCollapseReload?.onClick.AddListener(() => reloadAd?.ShowCollapseReloadAuto("mrec_single_01_45_bottom", 1, 5, true, 3));

//             btnHideCollapseReload?.onClick.AddListener(() => reloadAd?.Hide());

//             btnStopCollapseReload?.onClick.AddListener(() => reloadAd?.Stop());



//             // ───────────────── HELPERS FUNCTION ─────────────────
//             btnOpenInspector?.onClick.AddListener(AdsUtility.OpenAdInspector);
//             btnOpenDebugPanel?.onClick.AddListener(AdsUtility.OpenDebugPanel);
//         }

//         // ───────────────── Cleanup ─────────────────
//         void OnDestroy()
//         {
//             bannerAd?.Clear();
//             fullscreenAd?.Clear();
//             popupAd?.Clear();
//             reloadAd?.Clear();
//         }

//         void checkIsReady()
//         {
//             Debug.Log("BNNA IsReady: " + bannerAd?.IsReady().ToString());
//             Debug.Log("FSNA IsReady: " + fullscreenAd?.IsReady().ToString());
//             Debug.Log("ONA Popup IsReady: " + popupAd?.IsReady().ToString());
//             Debug.Log("ONA Collapse Reload By TimeClick IsReady: " + reloadAd?.IsReady().ToString());
//         }
//     }
// }
