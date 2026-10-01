// using System.Collections.Generic;
// using BG_Library.NET.AdCore.MainAndroid;
// using BG_Library.NET.AndroidSDK;
// using UnityEngine;
// using UnityEngine.UI;

// namespace BG_Lib.AndroidMediation.Scripts.sample
// {
//     public class SampleAdWithNewIns : MonoBehaviour
//     {
//         #region Shared Config

//         private const string LogTag = "SampleAdWithNewIns";

//         #endregion

//         #region MainActivity Mirror Config

//         private const int BannerTimeReloadSeconds = 20;
//         private const int BannerTimeCountdownSeconds = 3;
//         private const string AliasFullscreenSingle = "fullscreen_single";
//         private const string AliasFullscreenMultiple = "fullscreen_multiple";
//         private const string AliasFullscreenSequence = "fullscreen_sequence";
//         private const string AliasFullscreenOverlay = "fullscreen_overlay";
//         private const string AliasFullscreenOverlayCls = "fullscreen_overlay_cls";
//         private const string AliasFullscreenOverlayNav = "fullscreen_overlay_nav";
//         private const string AliasBanner = "banner_native";
//         private const string AliasPopup = "overlay_popup";

//         private static readonly string[] AvailableNativeAdUnitIds =
//         {
//             "ca-app-pub-3940256099942544/1044960115",
//             "ca-app-pub-3940256099942544/2247696110"
//         };

//         private static readonly string[] MainFullscreenLayoutNames =
//         {
//             "fs_multi_01",
//             "fs_sequence_01",
//             "fs_single_cls_01",
//             "fs_single_cls_02",
//             "fs_single_cls_03",
//             "fs_single_nav_01",
//             "fs_single_nav_02",
//             "fs_single_nav_03",
//             "fs_single_transparent_01",
//             "fs_single_transparent_02",
//             "fs_single_transparent_03",
//             "fs_single_transparent_04",
//             "fs_single_transparent_05",
//             "fs_single_transparent_06",
//             "fs_single_transparent_07",
//             "fs_single_universal_01",
//             "fs_single_universal_02",
//             "fs_single_universal_03",
//             "fs_single_universal_04",
//             "fs_single_universal_05",
//             "fs_single_universal_06",
//             "fs_single_universal_07",
//             "fs_single_universal_08",
//             "fs_single_universal_09",
//             "fs_single_universal_10",
//             "fs_single_universal_11",
//             "fs_single_universal_12",
//             "fs_single_universal_13",
//             "fs_single_universal_14",
//             "fs_single_universal_15"
//         };

//         private static readonly string[] MainBannerLayoutNames =
//         {
//             "bn_single_transparent_01",
//             "bn_single_transparent_02",
//             "bn_single_transparent_03",
//             "bn_single_transparent_04",
//             "bn_single_transparent_05",
//             "bn_single_transparent_06",
//             "bn_single_transparent_07",
//             "bn_single_transparent_08",
//             "bn_single_transparent_09",
//             "bn_single_transparent_10",
//             "bn_single_transparent_11",
//             "bn_single_transparent_12",
//             "bn_single_transparent_13",
//             "bn_single_transparent_14",
//             "bn_single_transparent_15",
//             "bn_single_transparent_16",
//             "bn_single_transparent_17",
//             "bn_single_transparent_18",
//             "bn_single_transparent_19",
//             "bn_single_transparent_20",
//             "bn_single_transparent_21",
//             "bn_single_transparent_22",
//             "bn_single_transparent_23",
//             "bn_single_transparent_24"
//         };

//         private static readonly string[] MainPopupLayoutNames =
//         {
//             "mrec_single_manual_01",
//             "mrec_single_manual_02",
//             "mrec_single_manual_03",
//             "mrec_single_manual_04",
//             "mrec_single_manual_05",
//             "mrec_single_manual_06",
//             "mrec_single_manual_07",
//             "mrec_single_manual_08",
//             "mrec_single_manual_09",
//             "mrec_single_manual_10",
//             "mrec_single_manual_11",
//             "mrec_single_manual_12",
//             "mrec_single_manual_13",
//             "mrec_single_manual_14"
//         };

//         private static readonly string[] NativeOrientations =
//         {
//             "portrait",
//             "landscape",
//             "auto"
//         };

//         private enum NativeSampleKind
//         {
//             Fullscreen,
//             Banner,
//             Popup
//         }

//         private sealed class NativeSampleDefinition
//         {
//             public readonly string Label;
//             public readonly NativeSampleKind Kind;
//             public readonly string Alias;
//             public readonly string[] Layouts;
//             public readonly string Mode;
//             public readonly int IdCount;
//             public readonly int Duration;
//             public readonly bool PauseGameplay;
//             public readonly bool EnableAdComeback;
//             public readonly bool ShowTcd;

//             public NativeSampleDefinition(
//                 string label,
//                 NativeSampleKind kind,
//                 string alias,
//                 string[] layouts,
//                 string mode = "",
//                 int idCount = 1,
//                 int duration = 5,
//                 bool pauseGameplay = true,
//                 bool enableAdComeback = false,
//                 bool showTcd = true)
//             {
//                 Label = label;
//                 Kind = kind;
//                 Alias = alias;
//                 Layouts = layouts;
//                 Mode = mode;
//                 IdCount = idCount;
//                 Duration = duration;
//                 PauseGameplay = pauseGameplay;
//                 EnableAdComeback = enableAdComeback;
//                 ShowTcd = showTcd;
//             }
//         }

//         private static readonly NativeSampleDefinition[] NativeSamples =
//         {
//             new NativeSampleDefinition(
//                 "Fullscreen Single",
//                 NativeSampleKind.Fullscreen,
//                 AliasFullscreenSingle,
//                 MainFullscreenLayoutNames,
//                 mode: "SINGLE",
//                 duration: 5,
//                 pauseGameplay: true,
//                 enableAdComeback: true),
//             new NativeSampleDefinition(
//                 "Fullscreen Multiple",
//                 NativeSampleKind.Fullscreen,
//                 AliasFullscreenMultiple,
//                 MainFullscreenLayoutNames,
//                 mode: "MULTIPLE",
//                 idCount: 2,
//                 duration: 5,
//                 pauseGameplay: true),
//             new NativeSampleDefinition(
//                 "Fullscreen Sequence",
//                 NativeSampleKind.Fullscreen,
//                 AliasFullscreenSequence,
//                 MainFullscreenLayoutNames,
//                 mode: "SEQUENCE",
//                 idCount: 3,
//                 pauseGameplay: true),
//             new NativeSampleDefinition(
//                 "Fullscreen Overlay",
//                 NativeSampleKind.Fullscreen,
//                 AliasFullscreenOverlay,
//                 MainFullscreenLayoutNames,
//                 mode: "OVERLAY",
//                 duration: 5,
//                 pauseGameplay: false,
//                 enableAdComeback: true),
//             new NativeSampleDefinition(
//                 "Fullscreen Overlay CLS",
//                 NativeSampleKind.Fullscreen,
//                 AliasFullscreenOverlayCls,
//                 MainFullscreenLayoutNames,
//                 mode: "OVERLAY_CLS",
//                 duration: 3,
//                 pauseGameplay: false,
//                 enableAdComeback: true),
//             new NativeSampleDefinition(
//                 "Fullscreen Overlay NAV",
//                 NativeSampleKind.Fullscreen,
//                 AliasFullscreenOverlayNav,
//                 MainFullscreenLayoutNames,
//                 mode: "OVERLAY_NAV",
//                 duration: 5,
//                 pauseGameplay: false,
//                 enableAdComeback: true),
//             new NativeSampleDefinition(
//                 "Banner",
//                 NativeSampleKind.Banner,
//                 AliasBanner,
//                 MainBannerLayoutNames),
//             new NativeSampleDefinition(
//                 "Popup",
//                 NativeSampleKind.Popup,
//                 AliasPopup,
//                 MainPopupLayoutNames)
//         };

//         #endregion

//         #region MainActivity Mirror UI Bindings

//         [SerializeField] private Dropdown adUnitIdDropdown;
//         [SerializeField] private Dropdown nativeFeatureDropdown;
//         [SerializeField] private Dropdown nativeLayoutDropdown;
//         [SerializeField] private Dropdown nativeOrientationDropdown;
//         [SerializeField] private GameObject popupControlsRoot;
//         [SerializeField] private Slider popupXSlider;
//         [SerializeField] private Slider popupYSlider;
//         [SerializeField] private Slider popupWidthSlider;
//         [SerializeField] private Slider popupHeightSlider;
//         [SerializeField] private Text popupXValueText;
//         [SerializeField] private Text popupYValueText;
//         [SerializeField] private Text popupWidthValueText;
//         [SerializeField] private Text popupHeightValueText;

//         private int selectedNativeSampleIndex;
//         private string selectedNativeAdUnitId = AvailableNativeAdUnitIds[0];
//         private string selectedNativeOrientation = "portrait";
//         private float popupXDp;
//         private float popupYDp;
//         private float popupWidthDp = 1000f;
//         private float popupHeightDp = 300f;
//         private FSInstance selectedFullscreenAd;
//         private string selectedFullscreenAlias;

//         #endregion

//         #region Interstitial Ad

//         private const string InterstitialAdUnit = "ca-app-pub-3940256099942544/1033173712";

//         #endregion

//         #region Fullscreen Sequence Ad

//         private static readonly int[] FullscreenSequenceDurations =
//         {
//             3,
//             4,
//             5
//         };

//         #endregion

//         #region Runtime Instances

//         private InterstitialAndroidInstance interstitialAd;
//         private RectAdInstance bannerAd;
//         private string loadedBannerLayoutName;
//         private RectAdInstance popupAd;
//         private string loadedPopupLayoutName;

//         #endregion

//         #region App Lifecycle

//         private void Start()
//         {
//             AdsUtility.SetEnvironment("staging");
//             Log("Start - MainActivity mapped sample");
//             Initialize();
//             BindMainActivityControls();
//         }

//         private void Initialize()
//         {
//             try
//             {
//                 AdsUtility.SetEnvironment("staging");
//                 AdsUtility.InitAds();
//                 AdsUtility.StartInternetChecking();
//                 Log("Ads SDK initialized for staging");
//             }
//             catch (System.Exception exception)
//             {
//                 LogError($"Ads SDK initialize failed; SampleScene UI stays available. {exception.GetType().Name}: {exception.Message}");
//             }
//         }

//         #endregion

//         #region Utility And Diagnostics

//         private static void Log(string message)
//         {
//             Debug.Log(WithLogTag(message));
//         }

//         private static void LogWarning(string message)
//         {
//             Debug.LogWarning(WithLogTag(message));
//         }

//         private static void LogError(string message)
//         {
//             Debug.LogError(WithLogTag(message));
//         }

//         private static string WithLogTag(string message)
//         {
//             var text = message ?? string.Empty;
//             return text.StartsWith($"[{LogTag}]") ? text : $"[{LogTag}] {text}";
//         }

//         public void OpenAdInspector()
//         {
//             AdsUtility.InitAds();
//             AdsUtility.OpenAdInspector();
//         }

//         private void BindMainActivityControls()
//         {
//             SetDropdownOptions(adUnitIdDropdown, AvailableNativeAdUnitIds);
//             SetDropdownOptions(nativeFeatureDropdown, NativeSampleLabels());
//             SetDropdownOptions(nativeOrientationDropdown, NativeOrientations);

//             if (adUnitIdDropdown != null)
//             {
//                 adUnitIdDropdown.SetValueWithoutNotify(0);
//                 adUnitIdDropdown.RefreshShownValue();
//                 adUnitIdDropdown.onValueChanged.AddListener(OnNativeAdUnitChanged);
//             }

//             if (nativeFeatureDropdown != null)
//             {
//                 nativeFeatureDropdown.SetValueWithoutNotify(selectedNativeSampleIndex);
//                 nativeFeatureDropdown.RefreshShownValue();
//                 nativeFeatureDropdown.onValueChanged.AddListener(OnNativeFeatureChanged);
//             }

//             if (nativeOrientationDropdown != null)
//             {
//                 nativeOrientationDropdown.SetValueWithoutNotify(0);
//                 nativeOrientationDropdown.RefreshShownValue();
//                 nativeOrientationDropdown.onValueChanged.AddListener(OnNativeOrientationChanged);
//             }

//             BindPopupSlider(popupXSlider, OnPopupXChanged);
//             BindPopupSlider(popupYSlider, OnPopupYChanged);
//             BindPopupSlider(popupWidthSlider, OnPopupWidthChanged);
//             BindPopupSlider(popupHeightSlider, OnPopupHeightChanged);
//             RefreshNativeLayoutSelector(selectedNativeSampleIndex);
//             RefreshPopupValueLabels();
//         }

//         public void RotatePortrait()
//         {
//             Screen.orientation = ScreenOrientation.Portrait;
//             selectedNativeOrientation = "portrait";
//             SetDropdownSelection(nativeOrientationDropdown, NativeOrientations, selectedNativeOrientation);
//         }

//         public void RotateLandscape()
//         {
//             Screen.orientation = ScreenOrientation.LandscapeLeft;
//             selectedNativeOrientation = "landscape";
//             SetDropdownSelection(nativeOrientationDropdown, NativeOrientations, selectedNativeOrientation);
//         }

//         private void OnNativeAdUnitChanged(int index)
//         {
//             if (index < 0 || index >= AvailableNativeAdUnitIds.Length)
//             {
//                 return;
//             }

//             selectedNativeAdUnitId = AvailableNativeAdUnitIds[index];
//             Log($"Selected native ad unit: {selectedNativeAdUnitId}");
//         }

//         private void OnNativeFeatureChanged(int index)
//         {
//             RefreshNativeLayoutSelector(index);
//         }

//         private void OnNativeOrientationChanged(int index)
//         {
//             if (index < 0 || index >= NativeOrientations.Length)
//             {
//                 return;
//             }

//             selectedNativeOrientation = NativeOrientations[index];
//         }

//         private void OnPopupXChanged(float value)
//         {
//             popupXDp = value;
//             RefreshPopupValueLabels();
//         }

//         private void OnPopupYChanged(float value)
//         {
//             popupYDp = value;
//             RefreshPopupValueLabels();
//         }

//         private void OnPopupWidthChanged(float value)
//         {
//             popupWidthDp = value;
//             RefreshPopupValueLabels();
//         }

//         private void OnPopupHeightChanged(float value)
//         {
//             popupHeightDp = value;
//             RefreshPopupValueLabels();
//         }

//         public void LoadSelectedNativeAd()
//         {
//             var sample = SelectedNativeSample();
//             switch (sample.Kind)
//             {
//                 case NativeSampleKind.Fullscreen:
//                     LoadFullscreenNativeSample(sample);
//                     break;
//                 case NativeSampleKind.Banner:
//                     LoadBannerNativeSample();
//                     break;
//                 case NativeSampleKind.Popup:
//                     LoadPopupNativeSample();
//                     break;
//             }
//         }

//         public void ShowSelectedNativeAd()
//         {
//             var sample = SelectedNativeSample();
//             var layoutName = SelectedNativeLayoutName();
//             var orientation = SelectedNativeOrientation();

//             switch (sample.Kind)
//             {
//                 case NativeSampleKind.Fullscreen:
//                     ShowFullscreenNativeSample(sample, layoutName, orientation);
//                     break;
//                 case NativeSampleKind.Banner:
//                     ShowBannerNativeSample(layoutName);
//                     break;
//                 case NativeSampleKind.Popup:
//                     ShowPopupNativeSample(layoutName);
//                     break;
//             }
//         }

//         public void ExpandSelectedBanner()
//         {
//             ExpandBannerAd(enableClick: true);
//         }

//         #endregion

//         #region Interstitial Ad

//         public void LoadInterstitialAd()
//         {
//             Log("Loading Interstitial Ad...");
//             PrepareInterstitialAd(autoReload: false, preloadBufferSize: 1, loadBufferSize: 1);
//         }

//         public void LoadInterstitialBufferedAd()
//         {
//             Log("Loading Interstitial Ad with buffer...");
//             PrepareInterstitialAd(autoReload: false, preloadBufferSize: 2, loadBufferSize: 2);
//         }

//         public void ShowInterstitialAd()
//         {
//             if (interstitialAd == null || !interstitialAd.IsReady())
//             {
//                 LogWarning("Interstitial not ready, call load first");
//                 return;
//             }

//             Log("Showing Interstitial Ad...");
//             interstitialAd.ShowInterstitial(false);
//         }

//         #endregion

//         #region Rect Ad Actions

//         private void ExpandBannerAd(bool enableClick)
//         {
//             Log($"Expanding Banner Native Ad... enableClick={enableClick}");
//             if (bannerAd == null)
//             {
//                 LogWarning("Banner not ready, call load/show first");
//                 return;
//             }

//             var expanded = bannerAd.ExpandAd(enableClick);
//             Log($"Banner Native expand requested: {expanded} | enableClick={enableClick}");
//         }

//         public void HideBanner()
//         {
//             Log("Hiding banner...");
//             bannerAd?.HideAd();
//         }

//         public void ClosePopup()
//         {
//             Log("Closing popup...");
//             popupAd?.CloseAd();
//         }

//         #endregion

//         #region Flow Buttons

//         public void StartLanguageIntroFlow()
//         {
//             Log("Starting Language/Intro Flow...");
//         }

//         public void ExitApp()
//         {
// #if UNITY_EDITOR
//             Log("Exit requested from SampleScene. Application.Quit() is ignored in the Unity Editor.");
// #else
//             Application.Quit();
// #endif
//         }

//         #endregion

//         #region Shared Factories And Callbacks

//         private void RefreshNativeLayoutSelector(int sampleIndex)
//         {
//             selectedNativeSampleIndex = Mathf.Clamp(sampleIndex, 0, NativeSamples.Length - 1);
//             var sample = SelectedNativeSample();
//             SetDropdownOptions(nativeLayoutDropdown, sample.Layouts);
//             if (popupControlsRoot != null)
//             {
//                 popupControlsRoot.SetActive(sample.Kind == NativeSampleKind.Popup);
//             }
//         }

//         private void LoadFullscreenNativeSample(NativeSampleDefinition sample)
//         {
//             Log($"Loading {sample.Label} | layoutCount={sample.Layouts.Length} | id={selectedNativeAdUnitId}");
//             selectedFullscreenAd?.DestroyAd();
//             selectedFullscreenAd = CreateFullscreenAd(
//                 name: sample.Label,
//                 ids: SelectedNativeAdUnitIds(sample.IdCount),
//                 layouts: sample.Layouts,
//                 adTimes: ResolveNativeSampleDurations(sample),
//                 pauseGameplay: sample.PauseGameplay,
//                 enableAdComeback: sample.EnableAdComeback,
//                 orientation: SelectedNativeOrientation());
//             selectedFullscreenAlias = sample.Alias;
//             selectedFullscreenAd.LoadAd();
//             Log($"Preloading {sample.Label} ads...");
//         }

//         private void ShowFullscreenNativeSample(NativeSampleDefinition sample, string layoutName, string orientation)
//         {
//             if (selectedFullscreenAd == null || selectedFullscreenAlias != sample.Alias)
//             {
//                 LogWarning($"{sample.Label} not ready, press Load first");
//                 return;
//             }

//             Log($"Showing {sample.Label} | mode={sample.Mode} | layout={layoutName} | orientation={orientation}");
//             var selectedLayouts = new[] { layoutName };

//             switch (sample.Mode)
//             {
//                 case "MULTIPLE":
//                     selectedFullscreenAd.ShowMultipleAd(new[] { layoutName, layoutName }, sample.Duration);
//                     break;
//                 case "SEQUENCE":
//                     selectedFullscreenAd.ShowSequenceAd(selectedLayouts, FullscreenSequenceDurations);
//                     break;
//                 case "OVERLAY_CLS":
//                     selectedFullscreenAd.ShowOverlayCls(selectedLayouts, sample.Duration, orientation, sample.ShowTcd);
//                     break;
//                 case "OVERLAY_NAV":
//                     selectedFullscreenAd.ShowOverlayNav(selectedLayouts, sample.Duration, orientation, sample.ShowTcd);
//                     break;
//                 default:
//                     selectedFullscreenAd.ShowSingleAd(selectedLayouts, sample.Duration, orientation);
//                     break;
//             }
//         }

//         private void LoadBannerNativeSample()
//         {
//             var layoutName = SelectedNativeLayoutName();
//             Log($"Loading Banner X close test | layout={layoutName} | timeReload={BannerTimeReloadSeconds}s | id={selectedNativeAdUnitId}");
//             bannerAd?.DestroyAd();
//             bannerAd = CreateBannerAd(new[] { selectedNativeAdUnitId }, layoutName, new[] { layoutName }, BannerTimeReloadSeconds, BannerTimeCountdownSeconds);
//             loadedBannerLayoutName = layoutName;
//             AttachRectCallbacks("Banner", bannerAd);
//             bannerAd.LoadAd(selectedNativeAdUnitId);
//             Log($"Preloading selected banner id only: {selectedNativeAdUnitId}");
//         }

//         private void ShowBannerNativeSample(string layoutName)
//         {
//             Log($"Showing Banner native test | layout={layoutName} | timeReload={BannerTimeReloadSeconds}s | timeCountdown={BannerTimeCountdownSeconds}s");
//             if (bannerAd == null)
//             {
//                 LogWarning("Banner not ready, press Load first");
//                 return;
//             }

//             if (!SameLayout(loadedBannerLayoutName, layoutName))
//             {
//                 LogWarning($"Banner loaded with layout={loadedBannerLayoutName}. Select layout={layoutName} then press Load first.");
//                 return;
//             }

//             bannerAd.ShowAd();
//         }

//         private void LoadPopupNativeSample()
//         {
//             var layoutName = SelectedNativeLayoutName();
//             Log($"Loading Overlay Popup Ad... layout={layoutName}");
//             popupAd?.DestroyAd();
//             popupAd = new RectAdInstance(
//                 ids: new[] { selectedNativeAdUnitId },
//                 layout: layoutName,
//                 layouts: new[] { layoutName },
//                 adSourceLayouts: null,
//                 timeShow: 10000,
//                 reloadTime: 1000,
//                 isPU: true);
//             loadedPopupLayoutName = layoutName;
//             popupAd.PU_UpdatePos(popupXDp, popupYDp, popupWidthDp, popupHeightDp);
//             AttachRectCallbacks("Popup", popupAd);
//             popupAd.LoadAd(selectedNativeAdUnitId);
//             Log("Preloading popup ads...");
//         }

//         private void ShowPopupNativeSample(string layoutName)
//         {
//             Log($"Showing Popup | layout={layoutName} | x={popupXDp} | y={popupYDp} | size={popupWidthDp}x{popupHeightDp}");
//             if (popupAd == null)
//             {
//                 LogWarning("Popup not ready, press Load first");
//                 return;
//             }

//             if (!SameLayout(loadedPopupLayoutName, layoutName))
//             {
//                 LogWarning($"Popup loaded with layout={loadedPopupLayoutName}. Select layout={layoutName} then press Load first.");
//                 return;
//             }

//             popupAd.PU_UpdatePos(popupXDp, popupYDp, popupWidthDp, popupHeightDp);
//             popupAd.ShowAd();
//         }

//         private static string[] NativeSampleLabels()
//         {
//             var labels = new string[NativeSamples.Length];
//             for (var i = 0; i < NativeSamples.Length; i++)
//             {
//                 labels[i] = NativeSamples[i].Label;
//             }

//             return labels;
//         }

//         private NativeSampleDefinition SelectedNativeSample()
//         {
//             if (selectedNativeSampleIndex < 0 || selectedNativeSampleIndex >= NativeSamples.Length)
//             {
//                 selectedNativeSampleIndex = 0;
//             }

//             return NativeSamples[selectedNativeSampleIndex];
//         }

//         private string SelectedNativeLayoutName()
//         {
//             var sample = SelectedNativeSample();
//             if (nativeLayoutDropdown != null &&
//                 nativeLayoutDropdown.value >= 0 &&
//                 nativeLayoutDropdown.value < sample.Layouts.Length)
//             {
//                 return sample.Layouts[nativeLayoutDropdown.value];
//             }

//             return sample.Layouts.Length > 0 ? sample.Layouts[0] : string.Empty;
//         }

//         private string SelectedNativeOrientation()
//         {
//             return string.IsNullOrWhiteSpace(selectedNativeOrientation) ? "portrait" : selectedNativeOrientation;
//         }

//         private string[] SelectedNativeAdUnitIds(int count)
//         {
//             var resolvedCount = Mathf.Max(1, count);
//             var ids = new string[resolvedCount];
//             for (var i = 0; i < resolvedCount; i++)
//             {
//                 ids[i] = selectedNativeAdUnitId;
//             }

//             return ids;
//         }

//         private static int[] ResolveNativeSampleDurations(NativeSampleDefinition sample)
//         {
//             return sample.Mode == "SEQUENCE"
//                 ? FullscreenSequenceDurations
//                 : new[] { sample.Duration };
//         }

//         private static void SetDropdownOptions(Dropdown dropdown, string[] options)
//         {
//             if (dropdown == null)
//             {
//                 return;
//             }

//             dropdown.ClearOptions();
//             dropdown.AddOptions(new List<string>(options));
//             dropdown.SetValueWithoutNotify(0);
//             dropdown.RefreshShownValue();
//         }

//         private static void SetDropdownSelection(Dropdown dropdown, string[] options, string value)
//         {
//             if (dropdown == null || options == null)
//             {
//                 return;
//             }

//             for (var i = 0; i < options.Length; i++)
//             {
//                 if (options[i] == value)
//                 {
//                     dropdown.SetValueWithoutNotify(i);
//                     dropdown.RefreshShownValue();
//                     return;
//                 }
//             }
//         }

//         private static void BindPopupSlider(Slider slider, UnityEngine.Events.UnityAction<float> onChanged)
//         {
//             if (slider == null)
//             {
//                 return;
//             }

//             onChanged?.Invoke(slider.value);
//             slider.onValueChanged.AddListener(onChanged);
//         }

//         private void RefreshPopupValueLabels()
//         {
//             SetText(popupXValueText, $"X: {Mathf.RoundToInt(popupXDp)}dp");
//             SetText(popupYValueText, $"Y: {Mathf.RoundToInt(popupYDp)}dp");
//             SetText(popupWidthValueText, $"Width: {Mathf.RoundToInt(popupWidthDp)}dp");
//             SetText(popupHeightValueText, $"Height: {Mathf.RoundToInt(popupHeightDp)}dp");
//         }

//         private static void SetText(Text text, string value)
//         {
//             if (text != null)
//             {
//                 text.text = value;
//             }
//         }

//         private static FSInstance CreateFullscreenAd(
//             string name,
//             string[] ids,
//             string[] layouts,
//             int[] adTimes,
//             bool pauseGameplay,
//             bool enableAdComeback,
//             string orientation)
//         {
//             var layoutGroup = BuildLayoutGroupConfig(layouts, adTimes);
//             var ad = new FSInstance(
//                 ids: ids,
//                 layoutGroup: layoutGroup,
//                 pauseGameplay: pauseGameplay,
//                 enableAdComeback: enableAdComeback,
//                 orientation: orientation
//                );
//             AttachFSCallbacks(name, ad);
//             return ad;
//         }

//         private static LayoutGroupConfig BuildLayoutGroupConfig(string[] layouts, int[] adTimes)
//         {
//             if (layouts == null || layouts.Length == 0)
//             {
//                 return JsonUtility.FromJson<LayoutGroupConfig>("{\"groupName\":\"sample_runtime\",\"layouts\":[],\"adSourceGroups\":[]}");
//             }

//             var json = new System.Text.StringBuilder();
//             json.Append("{\"groupName\":\"sample_runtime\",\"layouts\":[");
//             for (var i = 0; i < layouts.Length; i++)
//             {
//                 if (i > 0)
//                     json.Append(',');

//                 json.Append("{\"layout\":\"")
//                     .Append(EscapeJson(layouts[i]))
//                     .Append("\",\"layoutTime\":")
//                     .Append(ResolveLayoutTime(adTimes, i))
//                     .Append(",\"defaultRate\":0")
//                     .Append('}');
//             }
//             json.Append("],\"adSourceGroups\":[]}");

//             return JsonUtility.FromJson<LayoutGroupConfig>(json.ToString());
//         }

//         private static int ResolveLayoutTime(int[] adTimes, int index)
//         {
//             if (adTimes == null || adTimes.Length == 0)
//             {
//                 return 0;
//             }

//             var resolvedIndex = index < adTimes.Length ? index : adTimes.Length - 1;
//             return System.Math.Max(0, adTimes[resolvedIndex]);
//         }

//         private static string EscapeJson(string value)
//         {
//             return string.IsNullOrEmpty(value)
//                 ? string.Empty
//                 : value.Replace("\\", "\\\\").Replace("\"", "\\\"");
//         }

//         private static RectAdInstance CreateBannerAd(string[] ids, string layoutName, string[] layouts, int reloadTime, int countdownSeconds)
//         {
//             return new RectAdInstance(
//                 ids: ids,
//                 layout: layoutName,
//                 layouts: layouts,
//                 adSourceLayouts: null,
//                 timeShow: 0,
//                 reloadTime: reloadTime,
//                 isPU: false,
//                 timeCountdown: countdownSeconds);
//         }

//         private static bool SameLayout(string left, string right)
//         {
//             return string.Equals(left, right, System.StringComparison.OrdinalIgnoreCase);
//         }

//         #region Interstitial Ad

//         private void BindInterstitialCallbacks(InterstitialAndroidInstance ad)
//         {
//             ad.OnInterstitialLoaded += info =>
//             {
//                 Log($"Interstitial loaded: adUnit={info?.adUnitId ?? InterstitialAdUnit} adapter={info?.mediationAdapter} source={info?.adSource}");
//                 ShowLoadSuccessToast("Interstitial");
//             };
//             ad.OnInterstitialDisplayed += info =>
//                 Log($"Interstitial displayed: adUnit={info?.adUnitId} adapter={info?.mediationAdapter} source={info?.adSource}");
//             ad.OnInterstitialOpened += info =>
//                 Log($"Interstitial opened: adUnit={info?.adUnitId} adapter={info?.mediationAdapter} source={info?.adSource}");
//             ad.OnInterstitialClosed += _ =>
//             {
//                 Log("Interstitial closed");
//                 DestroyInterstitialAd();
//             };
//             ad.OnInterstitialClicked += info =>
//                 Log($"Interstitial clicked: adUnit={info?.adUnitId} adapter={info?.mediationAdapter} source={info?.adSource}");
//             ad.OnInterstitialDisplayable += () => Log("Interstitial displayable");
//             ad.OnInterstitialFailedToLoad += (adUnit, errorCode, error) =>
//             {
//                 LogError($"Interstitial failed: {adUnit} | {error} || {errorCode}");
//                 ShowLoadFailedToast("Interstitial", adUnit, errorCode, error);
//             };
//             ad.OnInterstitialPaidImpression += (_, paid) =>
//                 Log($"Interstitial paid impression: revenue={paid.revenueMicros} currency={paid.currencyCode} adapter={paid.mediationAdapter} source={paid.adSource}");

//             ad.OnAdLoadFailedCompat += error =>
//                 LogError($"Interstitial compat load failed: {error?.GetMessage()}");
//             ad.OnAdPaid += adValue =>
//                 Log($"Interstitial compat paid: {adValue?.Value} {adValue?.CurrencyCode}");
//             ad.OnAdClicked += () => Log("Interstitial compat clicked");
//             ad.OnAdFullScreenContentOpened += () => Log("Interstitial compat opened");
//             ad.OnAdFullScreenContentClosed += () => Log("Interstitial compat closed");
//             ad.OnAdFullScreenContentFailed += error =>
//                 LogError($"Interstitial compat fullscreen failed: {error?.GetMessage()}");
//         }

//         private void DestroyInterstitialAd()
//         {
//             interstitialAd?.Clear();
//             interstitialAd = null;
//         }

//         private void PrepareInterstitialAd(bool autoReload, int preloadBufferSize, int loadBufferSize)
//         {
//             DestroyInterstitialAd();
//             interstitialAd = new InterstitialAndroidInstance(
//                 new InterstitialAndroidInstance.InterstitialAndroidConfig(
//                     ids: new[] { InterstitialAdUnit },
//                     autoReload: autoReload,
//                     preloadBufferSize: preloadBufferSize));
//             BindInterstitialCallbacks(interstitialAd);
//             interstitialAd.LoadInterstitial(loadBufferSize);
//         }

//         #endregion

//         #region Fullscreen Shared Ad Callbacks

//         private static void AttachFSCallbacks(string name, FSInstance ad)
//         {
//             ad.OnAdLoadedEvent += info =>
//             {
//                 LogFullscreenState(info?.adUnitId, "LOAD_SUCCESS");
//                 ShowLoadSuccessToast(name);
//             };
//             ad.OnAdDisplayedEvent += info =>
//                 LogFullscreenState(info?.adUnitId, "SHOW");
//             ad.OnAdHiddenEvent += info =>
//                 LogFullscreenState(info?.adUnitId, "CLOSE");
//             ad.OnAdClicked += info =>
//                 LogFullscreenState(info?.adUnitId, "CLICK");
//             ad.OnAdOpenedEvent += info =>
//                 LogFullscreenState(info?.adUnitId, "OPENED");
//             ad.OnAdDisplayableEvent += () =>
//                 LogFullscreenState("none", "DISPLAYABLE");
//             ad.OnAdLoadFailedEvent += (adUnit, errorCode, error) =>
//             {
//                 LogError($"adUnit={adUnit} | state=LOAD_FAIL");
//                 ShowLoadFailedToast(name, adUnit, errorCode, error);
//             };
//             ad.OnPaidAdImpressionEvent += (info, paid) =>
//                 LogFullscreenState(info?.adUnitId, "PAID");
//         }

//         private static void LogFullscreenState(string adUnit, string state)
//         {
//             Log($"adUnit={adUnit ?? "none"} | state={state}");
//         }

//         private static void ShowLoadSuccessToast(string name)
//         {
//             SampleAndroidToast.Show($"{name} load success");
//         }

//         private static void ShowLoadFailedToast(string name, string adUnit, int errorCode, string error)
//         {
//             var suffix = string.IsNullOrWhiteSpace(adUnit) ? string.Empty : $" | {adUnit}";
//             var reason = string.IsNullOrWhiteSpace(error) ? string.Empty : $" | {error}";
//             SampleAndroidToast.Show($"{name} load failed: {errorCode}{suffix}{reason}", longDuration: true);
//         }

//         #endregion

//         #region Rect Ad Callbacks

//         private static void AttachRectCallbacks(string name, RectAdInstance ad)
//         {
//             ad.OnAdLoadedEvent += info =>
//             {
//                 Log($"{name} loaded: {info?.adUnitId} latest native mediation adapter={ad.GetLatestNativeMediationAdapter()} ad source={ad.GetLatestNativeAdSource()}");
//                 ShowLoadSuccessToast(name);
//             };
//             ad.OnAdDisplayedEvent += info =>
//                 Log($"{name} displayed: adUnit={info?.adUnitId} adapter={info?.mediationAdapter} source={info?.adSource}");
//             ad.OnAdClosedEvent += info =>
//                 Log($"{name} closed: adUnit={info?.adUnitId} adapter={info?.mediationAdapter} source={info?.adSource}");
//             ad.OnAdClicked += info =>
//                 Log($"{name} clicked: adUnit={info?.adUnitId} adapter={info?.mediationAdapter} source={info?.adSource}");
//             ad.OnAdOpenedEvent += info =>
//                 Log($"{name} opened: adUnit={info?.adUnitId} adapter={info?.mediationAdapter} source={info?.adSource}");
//             ad.OnAdClickCloseButtonEvent += info =>
//                 Log($"{name} click close button: adUnit={info?.adUnitId} adapter={info?.mediationAdapter} source={info?.adSource}");
//             ad.OnAdDisplayableEvent += () =>
//                 Log($"{name} displayable: latest native mediation adapter={ad.GetLatestNativeMediationAdapter()} ad source={ad.GetLatestNativeAdSource()}");
//             ad.OnAdLoadFailedEvent += (adUnit, errorCode, error) =>
//             {
//                 LogError($"{name} failed: {adUnit} | {error} || {errorCode}");
//                 ShowLoadFailedToast(name, adUnit, errorCode, error);
//             };
//             ad.OnPaidAdImpressionEvent += (info, paid) =>
//                 Log($"{name} paid impression: adUnit={info?.adUnitId} revenue={paid.revenueMicros} currency={paid.currencyCode} adapter={info?.mediationAdapter} source={info?.adSource}");
//         }

//         #endregion

//         #endregion

//         #region Cleanup

//         private void OnDestroy()
//         {
//             DestroyInterstitialAd();
//             selectedFullscreenAd?.DestroyAd();
//             bannerAd?.DestroyAd();
//             popupAd?.DestroyAd();
//         }

//         #endregion
//     }
// }
