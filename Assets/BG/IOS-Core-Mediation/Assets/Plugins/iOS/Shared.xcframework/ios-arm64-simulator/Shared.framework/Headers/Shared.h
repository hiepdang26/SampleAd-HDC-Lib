#import <Foundation/NSArray.h>
#import <Foundation/NSDictionary.h>
#import <Foundation/NSError.h>
#import <Foundation/NSObject.h>
#import <Foundation/NSSet.h>
#import <Foundation/NSString.h>
#import <Foundation/NSValue.h>

@class SharedAdsConsoleAssetVisibilityConfig, SharedAdsConsoleClickAssetConfig, SharedAdsConsoleEnvironment, SharedAdsConsoleFeature, SharedAdsConsoleOrientation, SharedAdsConsolePopupPlacement, SharedAdsConsolePopupPlacementCompanion, SharedAdsConsoleSections, SharedAdsConsoleStatus, SharedAdsConsoleUiState, SharedAdsConsoleViewport, SharedBannerCollapseCountdownStyle, SharedBannerNativeAdComposeCallbacks, SharedBannerNativeAdComposeState, SharedBannerNativeAdConfig, SharedBannerNativeAdError, SharedBannerNativeAdInfo, SharedBannerNativeAdLayoutCatalog, SharedBannerNativeAdReloadPolicy, SharedBannerNativeAdRequest, SharedBannerNativeAdShowOptions, SharedBannerNativeAdSizing, SharedFullscreenConsoleLayoutGroup, SharedFullscreenLayoutCatalog, SharedFullscreenNativeAdCallbacks, SharedFullscreenNativeAdCloseStyle, SharedFullscreenNativeAdConfig, SharedFullscreenNativeAdControlPhase, SharedFullscreenNativeAdControlState, SharedFullscreenNativeAdError, SharedFullscreenNativeAdInfo, SharedFullscreenNativeAdLayoutCatalog, SharedFullscreenNativeAdMode, SharedFullscreenNativeAdModeCompanion, SharedFullscreenNativeAdRegistry, SharedFullscreenNativeAdRequest, SharedFullscreenNativeAdShowOptions, SharedFullscreenNativeAdShowOptionsCompanion, SharedFullscreenNativeAdState, SharedInterstitialAdConfig, SharedInterstitialAdInfo, SharedInterstitialAdPaidInfo, SharedInterstitialAdRegistry, SharedInterstitialShowOptions, SharedIosAdEvent, SharedIosAdEventCenter, SharedKotlinArray<T>, SharedKotlinEnum<E>, SharedKotlinEnumCompanion, SharedNativeAdLayoutNames, SharedNativeAdLoadResult, SharedNativeAdLoadResultCompanion, SharedNativeAdShowOptions, SharedNativeAdState, SharedNativeAssetVisibilityOptions, SharedNativeClickAssetOptions, SharedPopupNativeAdComposeCallbacks, SharedPopupNativeAdComposeState, SharedPopupNativeAdConfig, SharedPopupNativeAdError, SharedPopupNativeAdInfo, SharedPopupNativeAdLayoutCatalog, SharedPopupNativeAdRequest, UIViewController;

@protocol SharedFullscreenNativeAdCallback, SharedInterstitialAdCallback, SharedKotlinComparable, SharedKotlinIterator, SharedPlatform;

NS_ASSUME_NONNULL_BEGIN
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wunknown-warning-option"
#pragma clang diagnostic ignored "-Wincompatible-property-type"
#pragma clang diagnostic ignored "-Wnullability"

#pragma push_macro("_Nullable_result")
#if !__has_feature(nullability_nullable_result)
#undef _Nullable_result
#define _Nullable_result _Nullable
#endif

__attribute__((swift_name("KotlinBase")))
@interface SharedBase : NSObject
- (instancetype)init __attribute__((unavailable));
+ (instancetype)new __attribute__((unavailable));
+ (void)initialize __attribute__((objc_requires_super));
@end

@interface SharedBase (SharedBaseCopying) <NSCopying>
@end

__attribute__((swift_name("KotlinMutableSet")))
@interface SharedMutableSet<ObjectType> : NSMutableSet<ObjectType>
@end

__attribute__((swift_name("KotlinMutableDictionary")))
@interface SharedMutableDictionary<KeyType, ObjectType> : NSMutableDictionary<KeyType, ObjectType>
@end

@interface NSError (NSErrorSharedKotlinException)
@property (readonly) id _Nullable kotlinException;
@end

__attribute__((swift_name("KotlinNumber")))
@interface SharedNumber : NSNumber
- (instancetype)initWithChar:(char)value __attribute__((unavailable));
- (instancetype)initWithUnsignedChar:(unsigned char)value __attribute__((unavailable));
- (instancetype)initWithShort:(short)value __attribute__((unavailable));
- (instancetype)initWithUnsignedShort:(unsigned short)value __attribute__((unavailable));
- (instancetype)initWithInt:(int)value __attribute__((unavailable));
- (instancetype)initWithUnsignedInt:(unsigned int)value __attribute__((unavailable));
- (instancetype)initWithLong:(long)value __attribute__((unavailable));
- (instancetype)initWithUnsignedLong:(unsigned long)value __attribute__((unavailable));
- (instancetype)initWithLongLong:(long long)value __attribute__((unavailable));
- (instancetype)initWithUnsignedLongLong:(unsigned long long)value __attribute__((unavailable));
- (instancetype)initWithFloat:(float)value __attribute__((unavailable));
- (instancetype)initWithDouble:(double)value __attribute__((unavailable));
- (instancetype)initWithBool:(BOOL)value __attribute__((unavailable));
- (instancetype)initWithInteger:(NSInteger)value __attribute__((unavailable));
- (instancetype)initWithUnsignedInteger:(NSUInteger)value __attribute__((unavailable));
+ (instancetype)numberWithChar:(char)value __attribute__((unavailable));
+ (instancetype)numberWithUnsignedChar:(unsigned char)value __attribute__((unavailable));
+ (instancetype)numberWithShort:(short)value __attribute__((unavailable));
+ (instancetype)numberWithUnsignedShort:(unsigned short)value __attribute__((unavailable));
+ (instancetype)numberWithInt:(int)value __attribute__((unavailable));
+ (instancetype)numberWithUnsignedInt:(unsigned int)value __attribute__((unavailable));
+ (instancetype)numberWithLong:(long)value __attribute__((unavailable));
+ (instancetype)numberWithUnsignedLong:(unsigned long)value __attribute__((unavailable));
+ (instancetype)numberWithLongLong:(long long)value __attribute__((unavailable));
+ (instancetype)numberWithUnsignedLongLong:(unsigned long long)value __attribute__((unavailable));
+ (instancetype)numberWithFloat:(float)value __attribute__((unavailable));
+ (instancetype)numberWithDouble:(double)value __attribute__((unavailable));
+ (instancetype)numberWithBool:(BOOL)value __attribute__((unavailable));
+ (instancetype)numberWithInteger:(NSInteger)value __attribute__((unavailable));
+ (instancetype)numberWithUnsignedInteger:(NSUInteger)value __attribute__((unavailable));
@end

__attribute__((swift_name("KotlinByte")))
@interface SharedByte : SharedNumber
- (instancetype)initWithChar:(char)value;
+ (instancetype)numberWithChar:(char)value;
@end

__attribute__((swift_name("KotlinUByte")))
@interface SharedUByte : SharedNumber
- (instancetype)initWithUnsignedChar:(unsigned char)value;
+ (instancetype)numberWithUnsignedChar:(unsigned char)value;
@end

__attribute__((swift_name("KotlinShort")))
@interface SharedShort : SharedNumber
- (instancetype)initWithShort:(short)value;
+ (instancetype)numberWithShort:(short)value;
@end

__attribute__((swift_name("KotlinUShort")))
@interface SharedUShort : SharedNumber
- (instancetype)initWithUnsignedShort:(unsigned short)value;
+ (instancetype)numberWithUnsignedShort:(unsigned short)value;
@end

__attribute__((swift_name("KotlinInt")))
@interface SharedInt : SharedNumber
- (instancetype)initWithInt:(int)value;
+ (instancetype)numberWithInt:(int)value;
@end

__attribute__((swift_name("KotlinUInt")))
@interface SharedUInt : SharedNumber
- (instancetype)initWithUnsignedInt:(unsigned int)value;
+ (instancetype)numberWithUnsignedInt:(unsigned int)value;
@end

__attribute__((swift_name("KotlinLong")))
@interface SharedLong : SharedNumber
- (instancetype)initWithLongLong:(long long)value;
+ (instancetype)numberWithLongLong:(long long)value;
@end

__attribute__((swift_name("KotlinULong")))
@interface SharedULong : SharedNumber
- (instancetype)initWithUnsignedLongLong:(unsigned long long)value;
+ (instancetype)numberWithUnsignedLongLong:(unsigned long long)value;
@end

__attribute__((swift_name("KotlinFloat")))
@interface SharedFloat : SharedNumber
- (instancetype)initWithFloat:(float)value;
+ (instancetype)numberWithFloat:(float)value;
@end

__attribute__((swift_name("KotlinDouble")))
@interface SharedDouble : SharedNumber
- (instancetype)initWithDouble:(double)value;
+ (instancetype)numberWithDouble:(double)value;
@end

__attribute__((swift_name("KotlinBoolean")))
@interface SharedBoolean : SharedNumber
- (instancetype)initWithBool:(BOOL)value;
+ (instancetype)numberWithBool:(BOOL)value;
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("FullscreenNativeAdRequest")))
@interface SharedFullscreenNativeAdRequest : SharedBase
- (instancetype)initWithInstanceId:(NSString *)instanceId adUnitIds:(NSArray<NSString *> *)adUnitIds __attribute__((swift_name("init(instanceId:adUnitIds:)"))) __attribute__((objc_designated_initializer));
- (SharedFullscreenNativeAdRequest *)doCopyInstanceId:(NSString *)instanceId adUnitIds:(NSArray<NSString *> *)adUnitIds __attribute__((swift_name("doCopy(instanceId:adUnitIds:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) NSArray<NSString *> *adUnitIds __attribute__((swift_name("adUnitIds")));
@property (readonly) NSString *instanceId __attribute__((swift_name("instanceId")));
@end

__attribute__((swift_name("Platform")))
@protocol SharedPlatform
@required
@property (readonly) NSString *name __attribute__((swift_name("name")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("IOSPlatform")))
@interface SharedIOSPlatform : SharedBase <SharedPlatform>
- (instancetype)init __attribute__((swift_name("init()"))) __attribute__((objc_designated_initializer));
+ (instancetype)new __attribute__((availability(swift, unavailable, message="use object initializers instead")));
@property (readonly) NSString *name __attribute__((swift_name("name")));
@end


/**
 * A tracking event (impression, click, paid revenue) for one ad instance.
 * [instanceId] is the alias the host (Unity) used when it loaded the ad.
 */
__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("IosAdEvent")))
@interface SharedIosAdEvent : SharedBase

/** JSON payload whose field names match Unity's `IOSNativeAdEventData`. */
- (NSString *)toUnityJson __attribute__((swift_name("toUnityJson()")));
@property (readonly) NSString *adSourceId __attribute__((swift_name("adSourceId")));
@property (readonly) NSString *adSourceInstanceId __attribute__((swift_name("adSourceInstanceId")));
@property (readonly) NSString *adSourceInstanceName __attribute__((swift_name("adSourceInstanceName")));
@property (readonly) NSString *adSourceName __attribute__((swift_name("adSourceName")));
@property (readonly) NSString *adUnitId __attribute__((swift_name("adUnitId")));
@property (readonly) NSString *currencyCode __attribute__((swift_name("currencyCode")));
@property (readonly) NSString *eventName __attribute__((swift_name("eventName")));
@property (readonly) NSString *format __attribute__((swift_name("format")));
@property (readonly) NSString *instanceId __attribute__((swift_name("instanceId")));
@property (readonly) NSString *mediationAdapter __attribute__((swift_name("mediationAdapter")));
@property (readonly) int32_t precision __attribute__((swift_name("precision")));
@property (readonly) NSString *responseId __attribute__((swift_name("responseId")));
@property (readonly) int64_t valueMicros __attribute__((swift_name("valueMicros")));
@end


/** Forwards ad tracking events to the host app; the Unity bridge subscribes once at startup. */
__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("IosAdEventCenter")))
@interface SharedIosAdEventCenter : SharedBase
+ (instancetype)alloc __attribute__((unavailable));

/** Forwards ad tracking events to the host app; the Unity bridge subscribes once at startup. */
+ (instancetype)allocWithZone:(struct _NSZone *)zone __attribute__((unavailable));
+ (instancetype)iosAdEventCenter __attribute__((swift_name("init()")));
@property (class, readonly, getter=shared) SharedIosAdEventCenter *shared __attribute__((swift_name("shared")));
- (void)subscribeListener:(void (^ _Nullable)(SharedIosAdEvent *))listener __attribute__((swift_name("subscribe(listener:)")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("NativeAdIosBridge")))
@interface SharedNativeAdIosBridge : SharedBase
- (instancetype)init __attribute__((swift_name("init()"))) __attribute__((objc_designated_initializer));
+ (instancetype)new __attribute__((availability(swift, unavailable, message="use object initializers instead")));
- (BOOL)createType:(NSString *)type alias:(NSString *)alias configJson:(NSString *)configJson __attribute__((swift_name("create(type:alias:configJson:)")));
- (BOOL)createInterstitialAlias:(NSString *)alias configJson:(NSString *)configJson __attribute__((swift_name("createInterstitial(alias:configJson:)")));
- (void)createInterstitialAlias:(NSString *)alias adUnitIdsCsv:(NSString *)adUnitIdsCsv preloadBufferSize:(int32_t)preloadBufferSize autoReload:(BOOL)autoReload __attribute__((swift_name("createInterstitial(alias:adUnitIdsCsv:preloadBufferSize:autoReload:)")));
- (BOOL)destroyAlias:(NSString *)alias __attribute__((swift_name("destroy(alias:)")));
- (BOOL)destroyInterstitialAlias:(NSString *)alias __attribute__((swift_name("destroyInterstitial(alias:)")));
- (void)disableMetaTestMode __attribute__((swift_name("disableMetaTestMode()")));
- (BOOL)enableMetaTestModeTestDeviceIds:(NSArray<NSString *> *)testDeviceIds __attribute__((swift_name("enableMetaTestMode(testDeviceIds:)")));

/**
 * Unity entry point: registers this device, plus any hashes in [additionalDeviceHashesCsv]
 * (separated by `,` `;` or new lines), as Meta Audience Network test devices.
 * [testAdType] is the raw `FBAdTestAdType` value (0 = default creative).
 * Call before Meta ads are requested; ads already loaded are not test ads.
 */
- (BOOL)enableMetaTestModeAdditionalDeviceHashesCsv:(NSString *)additionalDeviceHashesCsv testAdType:(int32_t)testAdType __attribute__((swift_name("enableMetaTestMode(additionalDeviceHashesCsv:testAdType:)")));
- (UIViewController *)fullscreenViewControllerInstanceId:(NSString *)instanceId options:(SharedFullscreenNativeAdShowOptions *)options fallbackAdUnitId:(NSString *)fallbackAdUnitId __attribute__((swift_name("fullscreenViewController(instanceId:options:fallbackAdUnitId:)")));
- (UIViewController *)fullscreenViewControllerInstanceId:(NSString *)instanceId layoutName:(NSString *)layoutName durationSeconds:(double)durationSeconds fallbackAdUnitId:(NSString *)fallbackAdUnitId __attribute__((swift_name("fullscreenViewController(instanceId:layoutName:durationSeconds:fallbackAdUnitId:)")));
- (UIViewController *)fullscreenViewControllerForUnityInstanceId:(NSString *)instanceId layoutName:(NSString *)layoutName durationSeconds:(double)durationSeconds fallbackAdUnitId:(NSString *)fallbackAdUnitId onClosed:(void (^)(void))onClosed __attribute__((swift_name("fullscreenViewControllerForUnity(instanceId:layoutName:durationSeconds:fallbackAdUnitId:onClosed:)")));
- (UIViewController *)fullscreenViewControllerForUnityWithOptionsInstanceId:(NSString *)instanceId mode:(NSString *)mode layoutNamesCsv:(NSString *)layoutNamesCsv durationSeconds:(double)durationSeconds durationsSecondsCsv:(NSString *)durationsSecondsCsv orientation:(NSString *)orientation autoClose:(BOOL)autoClose pauseGameplay:(BOOL)pauseGameplay enableAdComeback:(BOOL)enableAdComeback showTCD:(BOOL)showTCD delaySeconds:(double)delaySeconds timeUpCSeconds:(int32_t)timeUpCSeconds fallbackAdUnitId:(NSString *)fallbackAdUnitId cta:(BOOL)cta headline:(BOOL)headline body:(BOOL)body description:(BOOL)description icon:(BOOL)icon advertiser:(BOOL)advertiser media:(BOOL)media mediaImage:(BOOL)mediaImage mediaVideo:(BOOL)mediaVideo onClosed:(void (^)(void))onClosed __attribute__((swift_name("fullscreenViewControllerForUnityWithOptions(instanceId:mode:layoutNamesCsv:durationSeconds:durationsSecondsCsv:orientation:autoClose:pauseGameplay:enableAdComeback:showTCD:delaySeconds:timeUpCSeconds:fallbackAdUnitId:cta:headline:body:description:icon:advertiser:media:mediaImage:mediaVideo:onClosed:)")));
- (UIViewController *)fullscreenViewControllerWithAutoCloseInstanceId:(NSString *)instanceId layoutName:(NSString *)layoutName durationSeconds:(double)durationSeconds fallbackAdUnitId:(NSString *)fallbackAdUnitId autoClose:(BOOL)autoClose __attribute__((swift_name("fullscreenViewControllerWithAutoClose(instanceId:layoutName:durationSeconds:fallbackAdUnitId:autoClose:)")));
- (NSString *)getCurrentMetaTestDeviceHash __attribute__((swift_name("getCurrentMetaTestDeviceHash()")));
- (BOOL)isInterstitialReadyAlias:(NSString *)alias __attribute__((swift_name("isInterstitialReady(alias:)")));
- (BOOL)isMetaTestModeEnabled __attribute__((swift_name("isMetaTestModeEnabled()")));
- (BOOL)isReadyAlias:(NSString *)alias __attribute__((swift_name("isReady(alias:)")));
- (void)loadAdUnitId:(NSString *)adUnitId rootViewController:(UIViewController *)rootViewController __attribute__((swift_name("load(adUnitId:rootViewController:)")));
- (void)loadFullscreenInstanceId:(NSString *)instanceId adUnitIdsCsv:(NSString *)adUnitIdsCsv rootViewController:(UIViewController *)rootViewController __attribute__((swift_name("loadFullscreen(instanceId:adUnitIdsCsv:rootViewController:)")));
- (void)loadFullscreenInstanceId:(NSString *)instanceId adUnitIdsCsv:(NSString *)adUnitIdsCsv rootViewController:(UIViewController *)rootViewController enableReloadAfterShow:(BOOL)enableReloadAfterShow onStateChanged:(void (^)(SharedNativeAdState *))onStateChanged __attribute__((swift_name("loadFullscreen(instanceId:adUnitIdsCsv:rootViewController:enableReloadAfterShow:onStateChanged:)")));
- (void)loadFullscreenWithReloadInstanceId:(NSString *)instanceId adUnitIdsCsv:(NSString *)adUnitIdsCsv rootViewController:(UIViewController *)rootViewController enableReloadAfterShow:(BOOL)enableReloadAfterShow onStateChanged:(void (^)(SharedNativeAdState *))onStateChanged __attribute__((swift_name("loadFullscreenWithReload(instanceId:adUnitIdsCsv:rootViewController:enableReloadAfterShow:onStateChanged:)")));
- (BOOL)loadInterstitialRootViewController:(UIViewController *)rootViewController alias:(NSString *)alias __attribute__((swift_name("loadInterstitial(rootViewController:alias:)")));
- (BOOL)loadInterstitialRootViewController:(UIViewController *)rootViewController alias:(NSString *)alias bufferSize:(int32_t)bufferSize __attribute__((swift_name("loadInterstitial(rootViewController:alias:bufferSize:)")));
- (BOOL)preloadAllRootViewController:(UIViewController *)rootViewController alias:(NSString *)alias __attribute__((swift_name("preloadAll(rootViewController:alias:)")));
- (BOOL)preloadOneRootViewController:(UIViewController *)rootViewController alias:(NSString *)alias adUnitId:(NSString *)adUnitId __attribute__((swift_name("preloadOne(rootViewController:alias:adUnitId:)")));
- (BOOL)setInterstitialCallbackAlias:(NSString *)alias callback:(id<SharedInterstitialAdCallback> _Nullable)callback __attribute__((swift_name("setInterstitialCallback(alias:callback:)")));
- (BOOL)setInterstitialCallbackRootViewController:(UIViewController *)rootViewController alias:(NSString *)alias callback:(id<SharedInterstitialAdCallback> _Nullable)callback __attribute__((swift_name("setInterstitialCallback(rootViewController:alias:callback:)")));
- (BOOL)showInterstitialRootViewController:(UIViewController *)rootViewController alias:(NSString *)alias __attribute__((swift_name("showInterstitial(rootViewController:alias:)")));
- (BOOL)showInterstitialRootViewController:(UIViewController *)rootViewController alias:(NSString *)alias optionsJson:(NSString * _Nullable)optionsJson __attribute__((swift_name("showInterstitial(rootViewController:alias:optionsJson:)")));
- (UIViewController *)viewControllerAdUnitId:(NSString *)adUnitId __attribute__((swift_name("viewController(adUnitId:)")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("NativeAdLayoutNames")))
@interface SharedNativeAdLayoutNames : SharedBase
+ (instancetype)alloc __attribute__((unavailable));
+ (instancetype)allocWithZone:(struct _NSZone *)zone __attribute__((unavailable));
+ (instancetype)nativeAdLayoutNames __attribute__((swift_name("init()")));
@property (class, readonly, getter=shared) SharedNativeAdLayoutNames *shared __attribute__((swift_name("shared")));
- (NSString *)default __attribute__((swift_name("default()")));
- (NSString *)normalizeLayoutName:(NSString * _Nullable)layoutName __attribute__((swift_name("normalize(layoutName:)")));
@property (readonly) NSArray<NSString *> *All __attribute__((swift_name("All")));
@property (readonly) NSString *Default __attribute__((swift_name("Default")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("NativeAdLoadResult")))
@interface SharedNativeAdLoadResult : SharedBase
- (instancetype)initWithState:(SharedNativeAdState *)state adUnitId:(NSString *)adUnitId code:(int32_t)code message:(NSString *)message __attribute__((swift_name("init(state:adUnitId:code:message:)"))) __attribute__((objc_designated_initializer));
@property (class, readonly, getter=companion) SharedNativeAdLoadResultCompanion *companion __attribute__((swift_name("companion")));
- (SharedNativeAdLoadResult *)doCopyState:(SharedNativeAdState *)state adUnitId:(NSString *)adUnitId code:(int32_t)code message:(NSString *)message __attribute__((swift_name("doCopy(state:adUnitId:code:message:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) NSString *adUnitId __attribute__((swift_name("adUnitId")));
@property (readonly) int32_t code __attribute__((swift_name("code")));
@property (readonly) BOOL hasErrorCode __attribute__((swift_name("hasErrorCode")));
@property (readonly) NSString *message __attribute__((swift_name("message")));
@property (readonly) SharedNativeAdState *state __attribute__((swift_name("state")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("NativeAdLoadResult.Companion")))
@interface SharedNativeAdLoadResultCompanion : SharedBase
+ (instancetype)alloc __attribute__((unavailable));
+ (instancetype)allocWithZone:(struct _NSZone *)zone __attribute__((unavailable));
+ (instancetype)companion __attribute__((swift_name("init()")));
@property (class, readonly, getter=shared) SharedNativeAdLoadResultCompanion *shared __attribute__((swift_name("shared")));
@property (readonly) int32_t NoErrorCode __attribute__((swift_name("NoErrorCode")));
@end

__attribute__((swift_name("NativeAdLoader")))
@protocol SharedNativeAdLoader
@required
- (void)loadRequest:(SharedFullscreenNativeAdRequest *)request onStateChanged:(void (^)(SharedNativeAdState *))onStateChanged __attribute__((swift_name("load(request:onStateChanged:)")));
- (void)loadAdUnitId:(NSString *)adUnitId onStateChanged:(void (^)(SharedNativeAdState *))onStateChanged __attribute__((swift_name("load(adUnitId:onStateChanged:)")));
- (void)loadWithResultRequest:(SharedFullscreenNativeAdRequest *)request onResult:(void (^)(SharedNativeAdLoadResult *))onResult __attribute__((swift_name("loadWithResult(request:onResult:)")));
- (void)loadWithResultAdUnitId:(NSString *)adUnitId onResult:(void (^)(SharedNativeAdLoadResult *))onResult __attribute__((swift_name("loadWithResult(adUnitId:onResult:)")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("NativeAdShowOptions")))
@interface SharedNativeAdShowOptions : SharedBase
- (instancetype)initWithInstanceId:(NSString *)instanceId layoutName:(NSString *)layoutName durationSeconds:(double)durationSeconds autoClose:(BOOL)autoClose pauseGameplay:(BOOL)pauseGameplay enableAdComeback:(BOOL)enableAdComeback showTCD:(BOOL)showTCD delaySeconds:(double)delaySeconds timeUpCSeconds:(int32_t)timeUpCSeconds mode:(SharedFullscreenNativeAdMode *)mode layoutNames:(NSArray<NSString *> *)layoutNames durationsSeconds:(NSArray<SharedDouble *> *)durationsSeconds clickAssets:(SharedNativeClickAssetOptions *)clickAssets assetVisibility:(SharedNativeAssetVisibilityOptions *)assetVisibility orientation:(NSString *)orientation __attribute__((swift_name("init(instanceId:layoutName:durationSeconds:autoClose:pauseGameplay:enableAdComeback:showTCD:delaySeconds:timeUpCSeconds:mode:layoutNames:durationsSeconds:clickAssets:assetVisibility:orientation:)"))) __attribute__((objc_designated_initializer));
- (SharedNativeAdShowOptions *)doCopyInstanceId:(NSString *)instanceId layoutName:(NSString *)layoutName durationSeconds:(double)durationSeconds autoClose:(BOOL)autoClose pauseGameplay:(BOOL)pauseGameplay enableAdComeback:(BOOL)enableAdComeback showTCD:(BOOL)showTCD delaySeconds:(double)delaySeconds timeUpCSeconds:(int32_t)timeUpCSeconds mode:(SharedFullscreenNativeAdMode *)mode layoutNames:(NSArray<NSString *> *)layoutNames durationsSeconds:(NSArray<SharedDouble *> *)durationsSeconds clickAssets:(SharedNativeClickAssetOptions *)clickAssets assetVisibility:(SharedNativeAssetVisibilityOptions *)assetVisibility orientation:(NSString *)orientation __attribute__((swift_name("doCopy(instanceId:layoutName:durationSeconds:autoClose:pauseGameplay:enableAdComeback:showTCD:delaySeconds:timeUpCSeconds:mode:layoutNames:durationsSeconds:clickAssets:assetVisibility:orientation:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) SharedNativeAssetVisibilityOptions *assetVisibility __attribute__((swift_name("assetVisibility")));
@property (readonly) BOOL autoClose __attribute__((swift_name("autoClose")));
@property (readonly) SharedNativeClickAssetOptions *clickAssets __attribute__((swift_name("clickAssets")));
@property (readonly) double delaySeconds __attribute__((swift_name("delaySeconds")));
@property (readonly) double durationSeconds __attribute__((swift_name("durationSeconds")));
@property (readonly) NSArray<SharedDouble *> *durationsSeconds __attribute__((swift_name("durationsSeconds")));
@property (readonly) BOOL enableAdComeback __attribute__((swift_name("enableAdComeback")));
@property (readonly) NSString *instanceId __attribute__((swift_name("instanceId")));
@property (readonly) NSString *layoutName __attribute__((swift_name("layoutName")));
@property (readonly) NSArray<NSString *> *layoutNames __attribute__((swift_name("layoutNames")));
@property (readonly) SharedFullscreenNativeAdMode *mode __attribute__((swift_name("mode")));
@property (readonly) NSString *orientation __attribute__((swift_name("orientation")));
@property (readonly) BOOL pauseGameplay __attribute__((swift_name("pauseGameplay")));
@property (readonly) BOOL showTCD __attribute__((swift_name("showTCD")));
@property (readonly) int32_t timeUpCSeconds __attribute__((swift_name("timeUpCSeconds")));
@end

__attribute__((swift_name("KotlinComparable")))
@protocol SharedKotlinComparable
@required
- (int32_t)compareToOther:(id _Nullable)other __attribute__((swift_name("compareTo(other:)")));
@end

__attribute__((swift_name("KotlinEnum")))
@interface SharedKotlinEnum<E> : SharedBase <SharedKotlinComparable>
- (instancetype)initWithName:(NSString *)name ordinal:(int32_t)ordinal __attribute__((swift_name("init(name:ordinal:)"))) __attribute__((objc_designated_initializer));
@property (class, readonly, getter=companion) SharedKotlinEnumCompanion *companion __attribute__((swift_name("companion")));
- (int32_t)compareToOther:(E)other __attribute__((swift_name("compareTo(other:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) NSString *name __attribute__((swift_name("name")));
@property (readonly) int32_t ordinal __attribute__((swift_name("ordinal")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("NativeAdState")))
@interface SharedNativeAdState : SharedKotlinEnum<SharedNativeAdState *>
+ (instancetype)alloc __attribute__((unavailable));
+ (instancetype)allocWithZone:(struct _NSZone *)zone __attribute__((unavailable));
- (instancetype)initWithName:(NSString *)name ordinal:(int32_t)ordinal __attribute__((swift_name("init(name:ordinal:)"))) __attribute__((objc_designated_initializer)) __attribute__((unavailable));
@property (class, readonly) SharedNativeAdState *idle __attribute__((swift_name("idle")));
@property (class, readonly) SharedNativeAdState *loading __attribute__((swift_name("loading")));
@property (class, readonly) SharedNativeAdState *loaded __attribute__((swift_name("loaded")));
@property (class, readonly) SharedNativeAdState *failed __attribute__((swift_name("failed")));
@property (class, readonly) SharedNativeAdState *shown __attribute__((swift_name("shown")));
@property (class, readonly) SharedNativeAdState *onclosed __attribute__((swift_name("onclosed")));
+ (SharedKotlinArray<SharedNativeAdState *> *)values __attribute__((swift_name("values()")));
@property (class, readonly) NSArray<SharedNativeAdState *> *entries __attribute__((swift_name("entries")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("BannerCollapseCountdownStyle")))
@interface SharedBannerCollapseCountdownStyle : SharedKotlinEnum<SharedBannerCollapseCountdownStyle *>
+ (instancetype)alloc __attribute__((unavailable));
+ (instancetype)allocWithZone:(struct _NSZone *)zone __attribute__((unavailable));
- (instancetype)initWithName:(NSString *)name ordinal:(int32_t)ordinal __attribute__((swift_name("init(name:ordinal:)"))) __attribute__((objc_designated_initializer)) __attribute__((unavailable));
@property (class, readonly) SharedBannerCollapseCountdownStyle *none __attribute__((swift_name("none")));
@property (class, readonly) SharedBannerCollapseCountdownStyle *ring __attribute__((swift_name("ring")));
@property (class, readonly) SharedBannerCollapseCountdownStyle *horizontal __attribute__((swift_name("horizontal")));
@property (class, readonly) SharedBannerCollapseCountdownStyle *segments __attribute__((swift_name("segments")));
+ (SharedKotlinArray<SharedBannerCollapseCountdownStyle *> *)values __attribute__((swift_name("values()")));
@property (class, readonly) NSArray<SharedBannerCollapseCountdownStyle *> *entries __attribute__((swift_name("entries")));
@end

__attribute__((swift_name("BannerNativeAdCallback")))
@protocol SharedBannerNativeAdCallback
@required
- (void)onClickedAd:(SharedBannerNativeAdInfo *)ad __attribute__((swift_name("onClicked(ad:)")));
- (void)onClosedAd:(SharedBannerNativeAdInfo *)ad __attribute__((swift_name("onClosed(ad:)")));
- (void)onDisplayableAlias:(NSString *)alias __attribute__((swift_name("onDisplayable(alias:)")));
- (void)onDisplayedAd:(SharedBannerNativeAdInfo *)ad __attribute__((swift_name("onDisplayed(ad:)")));
- (void)onFailedToLoadError:(SharedBannerNativeAdError *)error __attribute__((swift_name("onFailedToLoad(error:)")));
- (void)onLoadedAd:(SharedBannerNativeAdInfo *)ad __attribute__((swift_name("onLoaded(ad:)")));
- (void)onOpenedAd:(SharedBannerNativeAdInfo *)ad __attribute__((swift_name("onOpened(ad:)")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("BannerNativeAdConfig")))
@interface SharedBannerNativeAdConfig : SharedBase
- (instancetype)initWithIds:(NSArray<NSString *> *)ids layoutNames:(NSArray<NSString *> *)layoutNames timeReloadSeconds:(int32_t)timeReloadSeconds timeCountdownSeconds:(int32_t)timeCountdownSeconds timeCollapseSeconds:(int32_t)timeCollapseSeconds __attribute__((swift_name("init(ids:layoutNames:timeReloadSeconds:timeCountdownSeconds:timeCollapseSeconds:)"))) __attribute__((objc_designated_initializer));
- (SharedBannerNativeAdConfig *)doCopyIds:(NSArray<NSString *> *)ids layoutNames:(NSArray<NSString *> *)layoutNames timeReloadSeconds:(int32_t)timeReloadSeconds timeCountdownSeconds:(int32_t)timeCountdownSeconds timeCollapseSeconds:(int32_t)timeCollapseSeconds __attribute__((swift_name("doCopy(ids:layoutNames:timeReloadSeconds:timeCountdownSeconds:timeCollapseSeconds:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) NSArray<NSString *> *ids __attribute__((swift_name("ids")));
@property (readonly) NSArray<NSString *> *layoutNames __attribute__((swift_name("layoutNames")));
@property (readonly) int32_t timeCollapseSeconds __attribute__((swift_name("timeCollapseSeconds")));
@property (readonly) int32_t timeCountdownSeconds __attribute__((swift_name("timeCountdownSeconds")));
@property (readonly) int32_t timeReloadSeconds __attribute__((swift_name("timeReloadSeconds")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("BannerNativeAdError")))
@interface SharedBannerNativeAdError : SharedBase
- (instancetype)initWithAlias:(NSString *)alias adUnitId:(NSString *)adUnitId code:(int32_t)code message:(NSString *)message __attribute__((swift_name("init(alias:adUnitId:code:message:)"))) __attribute__((objc_designated_initializer));
- (SharedBannerNativeAdError *)doCopyAlias:(NSString *)alias adUnitId:(NSString *)adUnitId code:(int32_t)code message:(NSString *)message __attribute__((swift_name("doCopy(alias:adUnitId:code:message:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) NSString *adUnitId __attribute__((swift_name("adUnitId")));
@property (readonly) NSString *alias __attribute__((swift_name("alias")));
@property (readonly) int32_t code __attribute__((swift_name("code")));
@property (readonly) NSString *message __attribute__((swift_name("message")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("BannerNativeAdInfo")))
@interface SharedBannerNativeAdInfo : SharedBase
- (instancetype)initWithAlias:(NSString *)alias instanceId:(NSString *)instanceId adUnitId:(NSString *)adUnitId layoutName:(NSString *)layoutName __attribute__((swift_name("init(alias:instanceId:adUnitId:layoutName:)"))) __attribute__((objc_designated_initializer));
- (SharedBannerNativeAdInfo *)doCopyAlias:(NSString *)alias instanceId:(NSString *)instanceId adUnitId:(NSString *)adUnitId layoutName:(NSString *)layoutName __attribute__((swift_name("doCopy(alias:instanceId:adUnitId:layoutName:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) NSString *adUnitId __attribute__((swift_name("adUnitId")));
@property (readonly) NSString *alias __attribute__((swift_name("alias")));
@property (readonly) NSString *instanceId __attribute__((swift_name("instanceId")));
@property (readonly) NSString *layoutName __attribute__((swift_name("layoutName")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("BannerNativeAdLayoutCatalog")))
@interface SharedBannerNativeAdLayoutCatalog : SharedBase
+ (instancetype)alloc __attribute__((unavailable));
+ (instancetype)allocWithZone:(struct _NSZone *)zone __attribute__((unavailable));
+ (instancetype)bannerNativeAdLayoutCatalog __attribute__((swift_name("init()")));
@property (class, readonly, getter=shared) SharedBannerNativeAdLayoutCatalog *shared __attribute__((swift_name("shared")));
- (SharedBannerCollapseCountdownStyle *)collapseCountdownStyleLayoutName:(NSString * _Nullable)layoutName __attribute__((swift_name("collapseCountdownStyle(layoutName:)")));
- (BOOL)hasCollapseCountdownLayoutName:(NSString * _Nullable)layoutName __attribute__((swift_name("hasCollapseCountdown(layoutName:)")));
- (NSString *)normalizeLayoutName:(NSString * _Nullable)layoutName __attribute__((swift_name("normalize(layoutName:)")));
@property (readonly) NSArray<NSString *> *All __attribute__((swift_name("All")));
@property (readonly) NSString *Default __attribute__((swift_name("Default")));
@end

__attribute__((swift_name("BannerNativeAdLoader")))
@protocol SharedBannerNativeAdLoader
@required
- (void)destroyInstanceId:(NSString *)instanceId __attribute__((swift_name("destroy(instanceId:)")));
- (void)loadRequest:(SharedBannerNativeAdRequest *)request onStateChanged_:(void (^)(SharedNativeAdState *))onStateChanged __attribute__((swift_name("load(request:onStateChanged_:)")));
- (void)loadAdUnitId:(NSString *)adUnitId onStateChanged:(void (^)(SharedNativeAdState *))onStateChanged __attribute__((swift_name("load(adUnitId:onStateChanged:)")));
- (void)loadWithResultRequest:(SharedBannerNativeAdRequest *)request onResult_:(void (^)(SharedNativeAdLoadResult *))onResult __attribute__((swift_name("loadWithResult(request:onResult_:)")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("BannerNativeAdReloadPolicy")))
@interface SharedBannerNativeAdReloadPolicy : SharedBase
+ (instancetype)alloc __attribute__((unavailable));
+ (instancetype)allocWithZone:(struct _NSZone *)zone __attribute__((unavailable));
+ (instancetype)bannerNativeAdReloadPolicy __attribute__((swift_name("init()")));
@property (class, readonly, getter=shared) SharedBannerNativeAdReloadPolicy *shared __attribute__((swift_name("shared")));
- (int64_t)intervalMillisTimeReloadSeconds:(int32_t)timeReloadSeconds __attribute__((swift_name("intervalMillis(timeReloadSeconds:)")));
- (BOOL)isEnabledTimeReloadSeconds:(int32_t)timeReloadSeconds __attribute__((swift_name("isEnabled(timeReloadSeconds:)")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("BannerNativeAdRequest")))
@interface SharedBannerNativeAdRequest : SharedBase
- (instancetype)initWithInstanceId:(NSString *)instanceId adUnitIds:(NSArray<NSString *> *)adUnitIds __attribute__((swift_name("init(instanceId:adUnitIds:)"))) __attribute__((objc_designated_initializer));
- (SharedBannerNativeAdRequest *)doCopyInstanceId:(NSString *)instanceId adUnitIds:(NSArray<NSString *> *)adUnitIds __attribute__((swift_name("doCopy(instanceId:adUnitIds:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) NSArray<NSString *> *adUnitIds __attribute__((swift_name("adUnitIds")));
@property (readonly) NSString *instanceId __attribute__((swift_name("instanceId")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("BannerNativeAdShowOptions")))
@interface SharedBannerNativeAdShowOptions : SharedBase
- (instancetype)initWithLayoutName:(NSString *)layoutName timeReloadSeconds:(int32_t)timeReloadSeconds timeCountdownSeconds:(int32_t)timeCountdownSeconds timeCollapseSeconds:(int32_t)timeCollapseSeconds __attribute__((swift_name("init(layoutName:timeReloadSeconds:timeCountdownSeconds:timeCollapseSeconds:)"))) __attribute__((objc_designated_initializer));
- (SharedBannerNativeAdShowOptions *)doCopyLayoutName:(NSString *)layoutName timeReloadSeconds:(int32_t)timeReloadSeconds timeCountdownSeconds:(int32_t)timeCountdownSeconds timeCollapseSeconds:(int32_t)timeCollapseSeconds __attribute__((swift_name("doCopy(layoutName:timeReloadSeconds:timeCountdownSeconds:timeCollapseSeconds:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) NSString *layoutName __attribute__((swift_name("layoutName")));
@property (readonly) int32_t timeCollapseSeconds __attribute__((swift_name("timeCollapseSeconds")));
@property (readonly) int32_t timeCountdownSeconds __attribute__((swift_name("timeCountdownSeconds")));
@property (readonly) int32_t timeReloadSeconds __attribute__((swift_name("timeReloadSeconds")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("BannerNativeAdSizing")))
@interface SharedBannerNativeAdSizing : SharedBase
+ (instancetype)alloc __attribute__((unavailable));
+ (instancetype)allocWithZone:(struct _NSZone *)zone __attribute__((unavailable));
+ (instancetype)bannerNativeAdSizing __attribute__((swift_name("init()")));
@property (class, readonly, getter=shared) SharedBannerNativeAdSizing *shared __attribute__((swift_name("shared")));
@property (readonly) float ExpandedHeightRatioLandscape __attribute__((swift_name("ExpandedHeightRatioLandscape")));
@property (readonly) float ExpandedHeightRatioPortrait __attribute__((swift_name("ExpandedHeightRatioPortrait")));
@property (readonly) float HeightRatioLandscape __attribute__((swift_name("HeightRatioLandscape")));
@property (readonly) float HeightRatioPortrait __attribute__((swift_name("HeightRatioPortrait")));
@property (readonly) float MinExpandedHeightLandscapeDp __attribute__((swift_name("MinExpandedHeightLandscapeDp")));
@property (readonly) float MinExpandedHeightPortraitDp __attribute__((swift_name("MinExpandedHeightPortraitDp")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("IosBannerNativeAdSdk")))
@interface SharedIosBannerNativeAdSdk : SharedBase
- (instancetype)init __attribute__((swift_name("init()"))) __attribute__((objc_designated_initializer));
+ (instancetype)new __attribute__((availability(swift, unavailable, message="use object initializers instead")));
- (void)collapseAlias:(NSString *)alias __attribute__((swift_name("collapse(alias:)")));
- (void)createAlias:(NSString *)alias adUnitIdsCsv:(NSString *)adUnitIdsCsv layoutNamesCsv:(NSString *)layoutNamesCsv timeReloadSeconds:(int32_t)timeReloadSeconds timeCountdownSeconds:(int32_t)timeCountdownSeconds timeCollapseSeconds:(int32_t)timeCollapseSeconds __attribute__((swift_name("create(alias:adUnitIdsCsv:layoutNamesCsv:timeReloadSeconds:timeCountdownSeconds:timeCollapseSeconds:)")));
- (void)destroyAlias:(NSString *)alias __attribute__((swift_name("destroy(alias:)")));
- (BOOL)expandAlias:(NSString *)alias enableClick:(BOOL)enableClick __attribute__((swift_name("expand(alias:enableClick:)")));
- (void)hideAlias:(NSString *)alias __attribute__((swift_name("hide(alias:)")));
- (void)loadRootViewController:(UIViewController *)rootViewController alias:(NSString *)alias __attribute__((swift_name("load(rootViewController:alias:)")));
- (void)loadWithConfigRootViewController:(UIViewController *)rootViewController alias:(NSString *)alias adUnitIdsCsv:(NSString *)adUnitIdsCsv layoutNamesCsv:(NSString *)layoutNamesCsv timeReloadSeconds:(int32_t)timeReloadSeconds timeCountdownSeconds:(int32_t)timeCountdownSeconds timeCollapseSeconds:(int32_t)timeCollapseSeconds onStateChanged:(void (^)(SharedNativeAdState *))onStateChanged __attribute__((swift_name("loadWithConfig(rootViewController:alias:adUnitIdsCsv:layoutNamesCsv:timeReloadSeconds:timeCountdownSeconds:timeCollapseSeconds:onStateChanged:)")));
- (void)setCallbackAlias:(NSString *)alias callback:(void (^)(SharedNativeAdState *))callback __attribute__((swift_name("setCallback(alias:callback:)")));
- (void)showRootViewController:(UIViewController *)rootViewController alias:(NSString *)alias __attribute__((swift_name("show(rootViewController:alias:)")));
@end


/**
 * @note annotations
 *   androidx.compose.runtime.Immutable
*/
__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("BannerNativeAdComposeCallbacks")))
@interface SharedBannerNativeAdComposeCallbacks : SharedBase
- (instancetype)initWithOnIconClick:(void (^)(void))onIconClick onCallToActionClick:(void (^)(void))onCallToActionClick onCollapsedCloseClick:(void (^)(void))onCollapsedCloseClick onCollapseClick:(void (^)(void))onCollapseClick __attribute__((swift_name("init(onIconClick:onCallToActionClick:onCollapsedCloseClick:onCollapseClick:)"))) __attribute__((objc_designated_initializer));
- (SharedBannerNativeAdComposeCallbacks *)doCopyOnIconClick:(void (^)(void))onIconClick onCallToActionClick:(void (^)(void))onCallToActionClick onCollapsedCloseClick:(void (^)(void))onCollapsedCloseClick onCollapseClick:(void (^)(void))onCollapseClick __attribute__((swift_name("doCopy(onIconClick:onCallToActionClick:onCollapsedCloseClick:onCollapseClick:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) void (^onCallToActionClick)(void) __attribute__((swift_name("onCallToActionClick")));
@property (readonly) void (^onCollapseClick)(void) __attribute__((swift_name("onCollapseClick")));
@property (readonly) void (^onCollapsedCloseClick)(void) __attribute__((swift_name("onCollapsedCloseClick")));
@property (readonly) void (^onIconClick)(void) __attribute__((swift_name("onIconClick")));
@end


/**
 * @note annotations
 *   androidx.compose.runtime.Immutable
*/
__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("BannerNativeAdComposeState")))
@interface SharedBannerNativeAdComposeState : SharedBase
- (instancetype)initWithHeadline:(NSString *)headline body:(NSString *)body advertiser:(NSString *)advertiser callToAction:(NSString *)callToAction showMedia:(BOOL)showMedia showIcon:(BOOL)showIcon showCallToAction:(BOOL)showCallToAction showAdChoices:(BOOL)showAdChoices usesNativeAssetTouchHandling:(BOOL)usesNativeAssetTouchHandling __attribute__((swift_name("init(headline:body:advertiser:callToAction:showMedia:showIcon:showCallToAction:showAdChoices:usesNativeAssetTouchHandling:)"))) __attribute__((objc_designated_initializer));
- (SharedBannerNativeAdComposeState *)doCopyHeadline:(NSString *)headline body:(NSString *)body advertiser:(NSString *)advertiser callToAction:(NSString *)callToAction showMedia:(BOOL)showMedia showIcon:(BOOL)showIcon showCallToAction:(BOOL)showCallToAction showAdChoices:(BOOL)showAdChoices usesNativeAssetTouchHandling:(BOOL)usesNativeAssetTouchHandling __attribute__((swift_name("doCopy(headline:body:advertiser:callToAction:showMedia:showIcon:showCallToAction:showAdChoices:usesNativeAssetTouchHandling:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) NSString *advertiser __attribute__((swift_name("advertiser")));
@property (readonly) NSString *body __attribute__((swift_name("body")));
@property (readonly) NSString *callToAction __attribute__((swift_name("callToAction")));
@property (readonly) NSString *headline __attribute__((swift_name("headline")));
@property (readonly) BOOL showAdChoices __attribute__((swift_name("showAdChoices")));
@property (readonly) BOOL showCallToAction __attribute__((swift_name("showCallToAction")));
@property (readonly) BOOL showIcon __attribute__((swift_name("showIcon")));
@property (readonly) BOOL showMedia __attribute__((swift_name("showMedia")));

/** Native platform asset views must receive touches directly for SDK click tracking. */
@property (readonly) BOOL usesNativeAssetTouchHandling __attribute__((swift_name("usesNativeAssetTouchHandling")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("AdsConsoleAssetVisibilityConfig")))
@interface SharedAdsConsoleAssetVisibilityConfig : SharedBase
- (instancetype)initWithCta:(BOOL)cta mediaImage:(BOOL)mediaImage headline:(BOOL)headline body:(BOOL)body mediaVideo:(BOOL)mediaVideo description:(BOOL)description icon:(BOOL)icon advertiser:(BOOL)advertiser starRating:(BOOL)starRating store:(BOOL)store price:(BOOL)price __attribute__((swift_name("init(cta:mediaImage:headline:body:mediaVideo:description:icon:advertiser:starRating:store:price:)"))) __attribute__((objc_designated_initializer));
- (SharedAdsConsoleAssetVisibilityConfig *)doCopyCta:(BOOL)cta mediaImage:(BOOL)mediaImage headline:(BOOL)headline body:(BOOL)body mediaVideo:(BOOL)mediaVideo description:(BOOL)description icon:(BOOL)icon advertiser:(BOOL)advertiser starRating:(BOOL)starRating store:(BOOL)store price:(BOOL)price __attribute__((swift_name("doCopy(cta:mediaImage:headline:body:mediaVideo:description:icon:advertiser:starRating:store:price:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (SharedNativeAssetVisibilityOptions *)toNativeAssetVisibilityOptions __attribute__((swift_name("toNativeAssetVisibilityOptions()")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) BOOL advertiser __attribute__((swift_name("advertiser")));
@property (readonly) BOOL body __attribute__((swift_name("body")));
@property (readonly) BOOL cta __attribute__((swift_name("cta")));
@property (readonly) BOOL description_ __attribute__((swift_name("description_")));
@property (readonly) BOOL headline __attribute__((swift_name("headline")));
@property (readonly) BOOL icon __attribute__((swift_name("icon")));
@property (readonly) BOOL mediaImage __attribute__((swift_name("mediaImage")));
@property (readonly) BOOL mediaVideo __attribute__((swift_name("mediaVideo")));
@property (readonly) BOOL price __attribute__((swift_name("price")));
@property (readonly) BOOL starRating __attribute__((swift_name("starRating")));
@property (readonly) BOOL store __attribute__((swift_name("store")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("AdsConsoleClickAssetConfig")))
@interface SharedAdsConsoleClickAssetConfig : SharedBase
- (instancetype)initWithCta:(BOOL)cta mediaImage:(BOOL)mediaImage headline:(BOOL)headline body:(BOOL)body mediaVideo:(BOOL)mediaVideo description:(BOOL)description icon:(BOOL)icon advertiser:(BOOL)advertiser __attribute__((swift_name("init(cta:mediaImage:headline:body:mediaVideo:description:icon:advertiser:)"))) __attribute__((objc_designated_initializer));
- (SharedAdsConsoleClickAssetConfig *)doCopyCta:(BOOL)cta mediaImage:(BOOL)mediaImage headline:(BOOL)headline body:(BOOL)body mediaVideo:(BOOL)mediaVideo description:(BOOL)description icon:(BOOL)icon advertiser:(BOOL)advertiser __attribute__((swift_name("doCopy(cta:mediaImage:headline:body:mediaVideo:description:icon:advertiser:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (SharedNativeClickAssetOptions *)toNativeClickAssetOptions __attribute__((swift_name("toNativeClickAssetOptions()")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) BOOL advertiser __attribute__((swift_name("advertiser")));
@property (readonly) BOOL body __attribute__((swift_name("body")));
@property (readonly) BOOL cta __attribute__((swift_name("cta")));
@property (readonly) BOOL description_ __attribute__((swift_name("description_")));
@property (readonly) BOOL headline __attribute__((swift_name("headline")));
@property (readonly) BOOL icon __attribute__((swift_name("icon")));
@property (readonly) BOOL mediaImage __attribute__((swift_name("mediaImage")));
@property (readonly) BOOL mediaVideo __attribute__((swift_name("mediaVideo")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("AdsConsoleEnvironment")))
@interface SharedAdsConsoleEnvironment : SharedKotlinEnum<SharedAdsConsoleEnvironment *>
+ (instancetype)alloc __attribute__((unavailable));
+ (instancetype)allocWithZone:(struct _NSZone *)zone __attribute__((unavailable));
- (instancetype)initWithName:(NSString *)name ordinal:(int32_t)ordinal __attribute__((swift_name("init(name:ordinal:)"))) __attribute__((objc_designated_initializer)) __attribute__((unavailable));
@property (class, readonly) SharedAdsConsoleEnvironment *debug __attribute__((swift_name("debug")));
@property (class, readonly) SharedAdsConsoleEnvironment *staging __attribute__((swift_name("staging")));
@property (class, readonly) SharedAdsConsoleEnvironment *release_ __attribute__((swift_name("release_")));
+ (SharedKotlinArray<SharedAdsConsoleEnvironment *> *)values __attribute__((swift_name("values()")));
@property (class, readonly) NSArray<SharedAdsConsoleEnvironment *> *entries __attribute__((swift_name("entries")));
@property (readonly) NSString *title __attribute__((swift_name("title")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("AdsConsoleFeature")))
@interface SharedAdsConsoleFeature : SharedKotlinEnum<SharedAdsConsoleFeature *>
+ (instancetype)alloc __attribute__((unavailable));
+ (instancetype)allocWithZone:(struct _NSZone *)zone __attribute__((unavailable));
- (instancetype)initWithName:(NSString *)name ordinal:(int32_t)ordinal __attribute__((swift_name("init(name:ordinal:)"))) __attribute__((objc_designated_initializer)) __attribute__((unavailable));
@property (class, readonly) SharedAdsConsoleFeature *fullscreennative __attribute__((swift_name("fullscreennative")));
@property (class, readonly) SharedAdsConsoleFeature *bannernative __attribute__((swift_name("bannernative")));
@property (class, readonly) SharedAdsConsoleFeature *collapsebanner __attribute__((swift_name("collapsebanner")));
@property (class, readonly) SharedAdsConsoleFeature *popupnative __attribute__((swift_name("popupnative")));
@property (class, readonly) SharedAdsConsoleFeature *interstitial __attribute__((swift_name("interstitial")));
@property (class, readonly) SharedAdsConsoleFeature *rewarded __attribute__((swift_name("rewarded")));
@property (class, readonly) SharedAdsConsoleFeature *appopen __attribute__((swift_name("appopen")));
+ (SharedKotlinArray<SharedAdsConsoleFeature *> *)values __attribute__((swift_name("values()")));
@property (class, readonly) NSArray<SharedAdsConsoleFeature *> *entries __attribute__((swift_name("entries")));
@property (readonly) NSString *alias __attribute__((swift_name("alias")));
@property (readonly) NSString *mediation __attribute__((swift_name("mediation")));
@property (readonly) NSString *shortTitle __attribute__((swift_name("shortTitle")));
@property (readonly) NSString *title __attribute__((swift_name("title")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("AdsConsoleOrientation")))
@interface SharedAdsConsoleOrientation : SharedKotlinEnum<SharedAdsConsoleOrientation *>
+ (instancetype)alloc __attribute__((unavailable));
+ (instancetype)allocWithZone:(struct _NSZone *)zone __attribute__((unavailable));
- (instancetype)initWithName:(NSString *)name ordinal:(int32_t)ordinal __attribute__((swift_name("init(name:ordinal:)"))) __attribute__((objc_designated_initializer)) __attribute__((unavailable));
@property (class, readonly) SharedAdsConsoleOrientation *auto_ __attribute__((swift_name("auto_")));
@property (class, readonly) SharedAdsConsoleOrientation *portrait __attribute__((swift_name("portrait")));
@property (class, readonly) SharedAdsConsoleOrientation *landscape __attribute__((swift_name("landscape")));
+ (SharedKotlinArray<SharedAdsConsoleOrientation *> *)values __attribute__((swift_name("values()")));
@property (class, readonly) NSArray<SharedAdsConsoleOrientation *> *entries __attribute__((swift_name("entries")));
@property (readonly) NSString *sdkValue __attribute__((swift_name("sdkValue")));
@property (readonly) NSString *title __attribute__((swift_name("title")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("AdsConsolePopupPlacement")))
@interface SharedAdsConsolePopupPlacement : SharedBase
- (instancetype)initWithXDp:(float)xDp yDp:(float)yDp widthDp:(float)widthDp heightDp:(float)heightDp __attribute__((swift_name("init(xDp:yDp:widthDp:heightDp:)"))) __attribute__((objc_designated_initializer));
@property (class, readonly, getter=companion) SharedAdsConsolePopupPlacementCompanion *companion __attribute__((swift_name("companion")));
- (SharedAdsConsolePopupPlacement *)coerceForViewportViewport:(SharedAdsConsoleViewport *)viewport layoutName:(NSString *)layoutName __attribute__((swift_name("coerceForViewport(viewport:layoutName:)")));
- (SharedAdsConsolePopupPlacement *)doCopyXDp:(float)xDp yDp:(float)yDp widthDp:(float)widthDp heightDp:(float)heightDp __attribute__((swift_name("doCopy(xDp:yDp:widthDp:heightDp:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (SharedAdsConsolePopupPlacement *)stepViewport:(SharedAdsConsoleViewport *)viewport layoutName:(NSString *)layoutName deltaX:(float)deltaX deltaY:(float)deltaY deltaWidth:(float)deltaWidth deltaHeight:(float)deltaHeight __attribute__((swift_name("step(viewport:layoutName:deltaX:deltaY:deltaWidth:deltaHeight:)")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) float heightDp __attribute__((swift_name("heightDp")));
@property (readonly) float widthDp __attribute__((swift_name("widthDp")));
@property (readonly) float xDp __attribute__((swift_name("xDp")));
@property (readonly) float yDp __attribute__((swift_name("yDp")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("AdsConsolePopupPlacement.Companion")))
@interface SharedAdsConsolePopupPlacementCompanion : SharedBase
+ (instancetype)alloc __attribute__((unavailable));
+ (instancetype)allocWithZone:(struct _NSZone *)zone __attribute__((unavailable));
+ (instancetype)companion __attribute__((swift_name("init()")));
@property (class, readonly, getter=shared) SharedAdsConsolePopupPlacementCompanion *shared __attribute__((swift_name("shared")));
@property (readonly) float MaxPopupWidthDp __attribute__((swift_name("MaxPopupWidthDp")));
@property (readonly) float MinPopupHeightDp __attribute__((swift_name("MinPopupHeightDp")));
@property (readonly) float MinPopupSizeDp __attribute__((swift_name("MinPopupSizeDp")));
@property (readonly) float MinPopupWidthDp __attribute__((swift_name("MinPopupWidthDp")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("AdsConsoleSections")))
@interface SharedAdsConsoleSections : SharedBase
- (instancetype)initWithShowNativeRuntimeControls:(BOOL)showNativeRuntimeControls showBannerControls:(BOOL)showBannerControls showFullscreenControls:(BOOL)showFullscreenControls showPopupControls:(BOOL)showPopupControls showClickAssetControls:(BOOL)showClickAssetControls __attribute__((swift_name("init(showNativeRuntimeControls:showBannerControls:showFullscreenControls:showPopupControls:showClickAssetControls:)"))) __attribute__((objc_designated_initializer));
- (SharedAdsConsoleSections *)doCopyShowNativeRuntimeControls:(BOOL)showNativeRuntimeControls showBannerControls:(BOOL)showBannerControls showFullscreenControls:(BOOL)showFullscreenControls showPopupControls:(BOOL)showPopupControls showClickAssetControls:(BOOL)showClickAssetControls __attribute__((swift_name("doCopy(showNativeRuntimeControls:showBannerControls:showFullscreenControls:showPopupControls:showClickAssetControls:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) BOOL showBannerControls __attribute__((swift_name("showBannerControls")));
@property (readonly) BOOL showClickAssetControls __attribute__((swift_name("showClickAssetControls")));
@property (readonly) BOOL showFullscreenControls __attribute__((swift_name("showFullscreenControls")));
@property (readonly) BOOL showNativeRuntimeControls __attribute__((swift_name("showNativeRuntimeControls")));
@property (readonly) BOOL showPopupControls __attribute__((swift_name("showPopupControls")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("AdsConsoleStatus")))
@interface SharedAdsConsoleStatus : SharedBase
- (instancetype)initWithAlias:(NSString *)alias adUnitId:(NSString *)adUnitId layoutName:(NSString *)layoutName state:(NSString *)state mediation:(NSString *)mediation responseId:(NSString *)responseId orientation:(NSString *)orientation __attribute__((swift_name("init(alias:adUnitId:layoutName:state:mediation:responseId:orientation:)"))) __attribute__((objc_designated_initializer));
- (SharedAdsConsoleStatus *)doCopyAlias:(NSString *)alias adUnitId:(NSString *)adUnitId layoutName:(NSString *)layoutName state:(NSString *)state mediation:(NSString *)mediation responseId:(NSString *)responseId orientation:(NSString *)orientation __attribute__((swift_name("doCopy(alias:adUnitId:layoutName:state:mediation:responseId:orientation:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) NSString *adUnitId __attribute__((swift_name("adUnitId")));
@property (readonly) NSString *alias __attribute__((swift_name("alias")));
@property (readonly) NSString *layoutName __attribute__((swift_name("layoutName")));
@property (readonly) NSString *mediation __attribute__((swift_name("mediation")));
@property (readonly) NSString *orientation __attribute__((swift_name("orientation")));
@property (readonly) NSString *responseId __attribute__((swift_name("responseId")));
@property (readonly) NSString *state __attribute__((swift_name("state")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("AdsConsoleUiState")))
@interface SharedAdsConsoleUiState : SharedBase
- (instancetype)initWithSelectedFeature:(SharedAdsConsoleFeature *)selectedFeature environment:(SharedAdsConsoleEnvironment *)environment selectedAdUnitId:(NSString *)selectedAdUnitId customAdUnitId:(NSString *)customAdUnitId savedAdUnitIds:(NSArray<NSString *> *)savedAdUnitIds selectedLayoutName:(NSString *)selectedLayoutName fullscreenMode:(SharedFullscreenNativeAdMode *)fullscreenMode fullscreenLayoutGroup:(SharedFullscreenConsoleLayoutGroup *)fullscreenLayoutGroup orientation:(SharedAdsConsoleOrientation *)orientation clickAssets:(SharedAdsConsoleClickAssetConfig *)clickAssets assetVisibility:(SharedAdsConsoleAssetVisibilityConfig *)assetVisibility popupPlacement:(SharedAdsConsolePopupPlacement *)popupPlacement timeUpCSeconds:(int32_t)timeUpCSeconds countdownSeconds:(int32_t)countdownSeconds delaySeconds:(int32_t)delaySeconds pauseGameplay:(BOOL)pauseGameplay showTCD:(BOOL)showTCD enableAdComeback:(BOOL)enableAdComeback autoCapture:(BOOL)autoCapture autoClosePopup:(BOOL)autoClosePopup enablePopupCtrOverlay:(BOOL)enablePopupCtrOverlay enableClickOnBannerExpand:(BOOL)enableClickOnBannerExpand metaTestDevices:(NSString *)metaTestDevices metaTestModeEnabled:(BOOL)metaTestModeEnabled metaTestDeviceCount:(int32_t)metaTestDeviceCount __attribute__((swift_name("init(selectedFeature:environment:selectedAdUnitId:customAdUnitId:savedAdUnitIds:selectedLayoutName:fullscreenMode:fullscreenLayoutGroup:orientation:clickAssets:assetVisibility:popupPlacement:timeUpCSeconds:countdownSeconds:delaySeconds:pauseGameplay:showTCD:enableAdComeback:autoCapture:autoClosePopup:enablePopupCtrOverlay:enableClickOnBannerExpand:metaTestDevices:metaTestModeEnabled:metaTestDeviceCount:)"))) __attribute__((objc_designated_initializer));

/** Selects the typed ad unit, adds it to the dropdown and clears the input. */
- (SharedAdsConsoleUiState *)applyCustomAdUnitId __attribute__((swift_name("applyCustomAdUnitId()")));
- (SharedAdsConsoleUiState *)doCopySelectedFeature:(SharedAdsConsoleFeature *)selectedFeature environment:(SharedAdsConsoleEnvironment *)environment selectedAdUnitId:(NSString *)selectedAdUnitId customAdUnitId:(NSString *)customAdUnitId savedAdUnitIds:(NSArray<NSString *> *)savedAdUnitIds selectedLayoutName:(NSString *)selectedLayoutName fullscreenMode:(SharedFullscreenNativeAdMode *)fullscreenMode fullscreenLayoutGroup:(SharedFullscreenConsoleLayoutGroup *)fullscreenLayoutGroup orientation:(SharedAdsConsoleOrientation *)orientation clickAssets:(SharedAdsConsoleClickAssetConfig *)clickAssets assetVisibility:(SharedAdsConsoleAssetVisibilityConfig *)assetVisibility popupPlacement:(SharedAdsConsolePopupPlacement *)popupPlacement timeUpCSeconds:(int32_t)timeUpCSeconds countdownSeconds:(int32_t)countdownSeconds delaySeconds:(int32_t)delaySeconds pauseGameplay:(BOOL)pauseGameplay showTCD:(BOOL)showTCD enableAdComeback:(BOOL)enableAdComeback autoCapture:(BOOL)autoCapture autoClosePopup:(BOOL)autoClosePopup enablePopupCtrOverlay:(BOOL)enablePopupCtrOverlay enableClickOnBannerExpand:(BOOL)enableClickOnBannerExpand metaTestDevices:(NSString *)metaTestDevices metaTestModeEnabled:(BOOL)metaTestModeEnabled metaTestDeviceCount:(int32_t)metaTestDeviceCount __attribute__((swift_name("doCopy(selectedFeature:environment:selectedAdUnitId:customAdUnitId:savedAdUnitIds:selectedLayoutName:fullscreenMode:fullscreenLayoutGroup:orientation:clickAssets:assetVisibility:popupPlacement:timeUpCSeconds:countdownSeconds:delaySeconds:pauseGameplay:showTCD:enableAdComeback:autoCapture:autoClosePopup:enablePopupCtrOverlay:enableClickOnBannerExpand:metaTestDevices:metaTestModeEnabled:metaTestDeviceCount:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (SharedAdsConsoleUiState *)selectFeatureFeature:(SharedAdsConsoleFeature *)feature __attribute__((swift_name("selectFeature(feature:)")));
- (SharedAdsConsoleUiState *)selectFullscreenLayoutGroupGroup:(SharedFullscreenConsoleLayoutGroup *)group __attribute__((swift_name("selectFullscreenLayoutGroup(group:)")));
- (SharedAdsConsoleUiState *)selectFullscreenModeMode:(SharedFullscreenNativeAdMode *)mode __attribute__((swift_name("selectFullscreenMode(mode:)")));
- (SharedAdsConsoleUiState *)selectLayoutNameLayoutName:(NSString *)layoutName __attribute__((swift_name("selectLayoutName(layoutName:)")));
- (SharedAdsConsoleStatus *)statusState:(SharedNativeAdState *)state responseId:(NSString *)responseId adUnitId:(NSString *)adUnitId layoutName:(NSString *)layoutName __attribute__((swift_name("status(state:responseId:adUnitId:layoutName:)")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) SharedAdsConsoleAssetVisibilityConfig *assetVisibility __attribute__((swift_name("assetVisibility")));
@property (readonly) BOOL autoCapture __attribute__((swift_name("autoCapture")));
@property (readonly) BOOL autoClosePopup __attribute__((swift_name("autoClosePopup")));
@property (readonly) SharedAdsConsoleClickAssetConfig *clickAssets __attribute__((swift_name("clickAssets")));
@property (readonly) int32_t countdownSeconds __attribute__((swift_name("countdownSeconds")));
@property (readonly) NSString *customAdUnitId __attribute__((swift_name("customAdUnitId")));
@property (readonly) int32_t delaySeconds __attribute__((swift_name("delaySeconds")));
@property (readonly) BOOL enableAdComeback __attribute__((swift_name("enableAdComeback")));
@property (readonly) BOOL enableClickOnBannerExpand __attribute__((swift_name("enableClickOnBannerExpand")));
@property (readonly) BOOL enablePopupCtrOverlay __attribute__((swift_name("enablePopupCtrOverlay")));
@property (readonly) SharedAdsConsoleEnvironment *environment __attribute__((swift_name("environment")));
@property (readonly) SharedFullscreenConsoleLayoutGroup *fullscreenLayoutGroup __attribute__((swift_name("fullscreenLayoutGroup")));
@property (readonly) SharedFullscreenNativeAdMode *fullscreenMode __attribute__((swift_name("fullscreenMode")));
@property (readonly) NSArray<NSString *> *layoutOptions __attribute__((swift_name("layoutOptions")));
@property (readonly) int32_t metaTestDeviceCount __attribute__((swift_name("metaTestDeviceCount")));
@property (readonly) NSString *metaTestDevices __attribute__((swift_name("metaTestDevices")));
@property (readonly) BOOL metaTestModeEnabled __attribute__((swift_name("metaTestModeEnabled")));
@property (readonly) NSString *normalizedLayoutName __attribute__((swift_name("normalizedLayoutName")));
@property (readonly) SharedAdsConsoleOrientation *orientation __attribute__((swift_name("orientation")));
@property (readonly) BOOL pauseGameplay __attribute__((swift_name("pauseGameplay")));
@property (readonly) SharedAdsConsolePopupPlacement *popupPlacement __attribute__((swift_name("popupPlacement")));

/** Ad unit ids applied from the custom input, listed in the Ad Unit dropdown. */
@property (readonly) NSArray<NSString *> *savedAdUnitIds __attribute__((swift_name("savedAdUnitIds")));
@property (readonly) SharedAdsConsoleSections *sections __attribute__((swift_name("sections")));
@property (readonly) NSString *selectedAdUnitId __attribute__((swift_name("selectedAdUnitId")));
@property (readonly) SharedAdsConsoleFeature *selectedFeature __attribute__((swift_name("selectedFeature")));
@property (readonly) NSString *selectedLayoutName __attribute__((swift_name("selectedLayoutName")));
@property (readonly) BOOL showTCD __attribute__((swift_name("showTCD")));
@property (readonly) int32_t timeUpCSeconds __attribute__((swift_name("timeUpCSeconds")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("AdsConsoleViewport")))
@interface SharedAdsConsoleViewport : SharedBase
- (instancetype)initWithWidthDp:(float)widthDp heightDp:(float)heightDp __attribute__((swift_name("init(widthDp:heightDp:)"))) __attribute__((objc_designated_initializer));
- (SharedAdsConsoleViewport *)doCopyWidthDp:(float)widthDp heightDp:(float)heightDp __attribute__((swift_name("doCopy(widthDp:heightDp:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) float heightDp __attribute__((swift_name("heightDp")));
@property (readonly) float widthDp __attribute__((swift_name("widthDp")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("FullscreenConsoleLayoutGroup")))
@interface SharedFullscreenConsoleLayoutGroup : SharedKotlinEnum<SharedFullscreenConsoleLayoutGroup *>
+ (instancetype)alloc __attribute__((unavailable));
+ (instancetype)allocWithZone:(struct _NSZone *)zone __attribute__((unavailable));
- (instancetype)initWithName:(NSString *)name ordinal:(int32_t)ordinal __attribute__((swift_name("init(name:ordinal:)"))) __attribute__((objc_designated_initializer)) __attribute__((unavailable));
@property (class, readonly) SharedFullscreenConsoleLayoutGroup *universal __attribute__((swift_name("universal")));
@property (class, readonly) SharedFullscreenConsoleLayoutGroup *cls __attribute__((swift_name("cls")));
@property (class, readonly) SharedFullscreenConsoleLayoutGroup *nav __attribute__((swift_name("nav")));
@property (class, readonly) SharedFullscreenConsoleLayoutGroup *prgs __attribute__((swift_name("prgs")));
@property (class, readonly) SharedFullscreenConsoleLayoutGroup *prgso __attribute__((swift_name("prgso")));
@property (class, readonly) SharedFullscreenConsoleLayoutGroup *loop __attribute__((swift_name("loop")));
+ (SharedKotlinArray<SharedFullscreenConsoleLayoutGroup *> *)values __attribute__((swift_name("values()")));
@property (class, readonly) NSArray<SharedFullscreenConsoleLayoutGroup *> *entries __attribute__((swift_name("entries")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("FullscreenLayoutCatalog")))
@interface SharedFullscreenLayoutCatalog : SharedBase
+ (instancetype)alloc __attribute__((unavailable));
+ (instancetype)allocWithZone:(struct _NSZone *)zone __attribute__((unavailable));
+ (instancetype)fullscreenLayoutCatalog __attribute__((swift_name("init()")));
@property (class, readonly, getter=shared) SharedFullscreenLayoutCatalog *shared __attribute__((swift_name("shared")));
- (BOOL)isAndroidXmlParityLayoutNameLayoutName:(NSString * _Nullable)layoutName __attribute__((swift_name("isAndroidXmlParityLayoutName(layoutName:)")));
- (BOOL)isFullscreenLayoutNameLayoutName:(NSString * _Nullable)layoutName __attribute__((swift_name("isFullscreenLayoutName(layoutName:)")));
- (NSString *)normalizeLayoutName:(NSString * _Nullable)layoutName __attribute__((swift_name("normalize(layoutName:)")));
@property (readonly) NSArray<NSString *> *AarClsLayouts __attribute__((swift_name("AarClsLayouts")));
@property (readonly) NSArray<NSString *> *AarFullscreenLayouts __attribute__((swift_name("AarFullscreenLayouts")));
@property (readonly) NSArray<NSString *> *AarLoopLayouts __attribute__((swift_name("AarLoopLayouts")));
@property (readonly) NSArray<NSString *> *AarNavLayouts __attribute__((swift_name("AarNavLayouts")));
@property (readonly) NSArray<NSString *> *AarProgressClsLayouts __attribute__((swift_name("AarProgressClsLayouts")));
@property (readonly) NSArray<NSString *> *AarProgressLayouts __attribute__((swift_name("AarProgressLayouts")));
@property (readonly) NSArray<NSString *> *AarSingleTransparentLayouts __attribute__((swift_name("AarSingleTransparentLayouts")));
@property (readonly) NSArray<NSString *> *AarSingleUniversalLayouts __attribute__((swift_name("AarSingleUniversalLayouts")));
@property (readonly) NSArray<NSString *> *AllLayouts __attribute__((swift_name("AllLayouts")));
@property (readonly) NSArray<NSString *> *AndroidXmlParityLayouts __attribute__((swift_name("AndroidXmlParityLayouts")));
@property (readonly) NSString *LoopDefault __attribute__((swift_name("LoopDefault")));
@property (readonly) NSString *MultipleDefault __attribute__((swift_name("MultipleDefault")));
@property (readonly) NSString *Nav01 __attribute__((swift_name("Nav01")));
@property (readonly) NSString *Nav02 __attribute__((swift_name("Nav02")));
@property (readonly) NSString *Nav03 __attribute__((swift_name("Nav03")));
@property (readonly) NSString *ProgressClsDefault __attribute__((swift_name("ProgressClsDefault")));
@property (readonly) NSString *ProgressDefault __attribute__((swift_name("ProgressDefault")));
@property (readonly) NSString *ProgressOnly01 __attribute__((swift_name("ProgressOnly01")));
@property (readonly) NSString *ProgressOnly02 __attribute__((swift_name("ProgressOnly02")));
@property (readonly) NSString *ProgressOnly03 __attribute__((swift_name("ProgressOnly03")));
@property (readonly) NSString *ProgressOnly04 __attribute__((swift_name("ProgressOnly04")));
@property (readonly) NSString *SequenceDefault __attribute__((swift_name("SequenceDefault")));
@property (readonly) NSString *SingleCtr __attribute__((swift_name("SingleCtr")));
@property (readonly) NSString *SingleCtrTransparent __attribute__((swift_name("SingleCtrTransparent")));
@property (readonly) NSString *SingleDefault __attribute__((swift_name("SingleDefault")));
@property (readonly) NSArray<NSString *> *SingleLayouts __attribute__((swift_name("SingleLayouts")));
@end

__attribute__((swift_name("FullscreenNativeAdCallback")))
@protocol SharedFullscreenNativeAdCallback
@required
- (void)onClickedAd_:(SharedFullscreenNativeAdInfo *)ad __attribute__((swift_name("onClicked(ad_:)")));
- (void)onClosedAd_:(SharedFullscreenNativeAdInfo *)ad __attribute__((swift_name("onClosed(ad_:)")));
- (void)onDisplayableAlias:(NSString *)alias __attribute__((swift_name("onDisplayable(alias:)")));
- (void)onDisplayedAd_:(SharedFullscreenNativeAdInfo *)ad __attribute__((swift_name("onDisplayed(ad_:)")));
- (void)onFailedToLoadError_:(SharedFullscreenNativeAdError *)error __attribute__((swift_name("onFailedToLoad(error_:)")));
- (void)onLoadedAd_:(SharedFullscreenNativeAdInfo *)ad __attribute__((swift_name("onLoaded(ad_:)")));
- (void)onOpenedAd_:(SharedFullscreenNativeAdInfo *)ad __attribute__((swift_name("onOpened(ad_:)")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("FullscreenNativeAdCloseStyle")))
@interface SharedFullscreenNativeAdCloseStyle : SharedKotlinEnum<SharedFullscreenNativeAdCloseStyle *>
+ (instancetype)alloc __attribute__((unavailable));
+ (instancetype)allocWithZone:(struct _NSZone *)zone __attribute__((unavailable));
- (instancetype)initWithName:(NSString *)name ordinal:(int32_t)ordinal __attribute__((swift_name("init(name:ordinal:)"))) __attribute__((objc_designated_initializer)) __attribute__((unavailable));
@property (class, readonly) SharedFullscreenNativeAdCloseStyle *standard __attribute__((swift_name("standard")));
@property (class, readonly) SharedFullscreenNativeAdCloseStyle *cls __attribute__((swift_name("cls")));
@property (class, readonly) SharedFullscreenNativeAdCloseStyle *nav __attribute__((swift_name("nav")));
@property (class, readonly) SharedFullscreenNativeAdCloseStyle *pgrs __attribute__((swift_name("pgrs")));
@property (class, readonly) SharedFullscreenNativeAdCloseStyle *pgrsCls __attribute__((swift_name("pgrsCls")));
@property (class, readonly) SharedFullscreenNativeAdCloseStyle *loop __attribute__((swift_name("loop")));
+ (SharedKotlinArray<SharedFullscreenNativeAdCloseStyle *> *)values __attribute__((swift_name("values()")));
@property (class, readonly) NSArray<SharedFullscreenNativeAdCloseStyle *> *entries __attribute__((swift_name("entries")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("FullscreenNativeAdConfig")))
@interface SharedFullscreenNativeAdConfig : SharedBase
- (instancetype)initWithIds:(NSArray<NSString *> *)ids preloadBufferSize:(int32_t)preloadBufferSize enableReloadAfterShow:(BOOL)enableReloadAfterShow __attribute__((swift_name("init(ids:preloadBufferSize:enableReloadAfterShow:)"))) __attribute__((objc_designated_initializer));
- (SharedFullscreenNativeAdConfig *)doCopyIds:(NSArray<NSString *> *)ids preloadBufferSize:(int32_t)preloadBufferSize enableReloadAfterShow:(BOOL)enableReloadAfterShow __attribute__((swift_name("doCopy(ids:preloadBufferSize:enableReloadAfterShow:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) BOOL enableReloadAfterShow __attribute__((swift_name("enableReloadAfterShow")));
@property (readonly) NSArray<NSString *> *ids __attribute__((swift_name("ids")));
@property (readonly) int32_t preloadBufferSize __attribute__((swift_name("preloadBufferSize")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("FullscreenNativeAdControlPhase")))
@interface SharedFullscreenNativeAdControlPhase : SharedKotlinEnum<SharedFullscreenNativeAdControlPhase *>
+ (instancetype)alloc __attribute__((unavailable));
+ (instancetype)allocWithZone:(struct _NSZone *)zone __attribute__((unavailable));
- (instancetype)initWithName:(NSString *)name ordinal:(int32_t)ordinal __attribute__((swift_name("init(name:ordinal:)"))) __attribute__((objc_designated_initializer)) __attribute__((unavailable));
@property (class, readonly) SharedFullscreenNativeAdControlPhase *progress __attribute__((swift_name("progress")));
@property (class, readonly) SharedFullscreenNativeAdControlPhase *closeDelay __attribute__((swift_name("closeDelay")));
@property (class, readonly) SharedFullscreenNativeAdControlPhase *finished __attribute__((swift_name("finished")));
+ (SharedKotlinArray<SharedFullscreenNativeAdControlPhase *> *)values __attribute__((swift_name("values()")));
@property (class, readonly) NSArray<SharedFullscreenNativeAdControlPhase *> *entries __attribute__((swift_name("entries")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("FullscreenNativeAdControlState")))
@interface SharedFullscreenNativeAdControlState : SharedBase
- (instancetype)initWithRemainingSeconds:(int32_t)remainingSeconds canClose:(BOOL)canClose showCountdown:(BOOL)showCountdown phase:(SharedFullscreenNativeAdControlPhase *)phase closeStyle:(SharedFullscreenNativeAdCloseStyle *)closeStyle progress:(float)progress showProgress:(BOOL)showProgress showOpenStoreButton:(BOOL)showOpenStoreButton showCloseButton:(BOOL)showCloseButton closeButtonEnabled:(BOOL)closeButtonEnabled closeButtonAlpha:(float)closeButtonAlpha __attribute__((swift_name("init(remainingSeconds:canClose:showCountdown:phase:closeStyle:progress:showProgress:showOpenStoreButton:showCloseButton:closeButtonEnabled:closeButtonAlpha:)"))) __attribute__((objc_designated_initializer));
- (SharedFullscreenNativeAdControlState *)doCopyRemainingSeconds:(int32_t)remainingSeconds canClose:(BOOL)canClose showCountdown:(BOOL)showCountdown phase:(SharedFullscreenNativeAdControlPhase *)phase closeStyle:(SharedFullscreenNativeAdCloseStyle *)closeStyle progress:(float)progress showProgress:(BOOL)showProgress showOpenStoreButton:(BOOL)showOpenStoreButton showCloseButton:(BOOL)showCloseButton closeButtonEnabled:(BOOL)closeButtonEnabled closeButtonAlpha:(float)closeButtonAlpha __attribute__((swift_name("doCopy(remainingSeconds:canClose:showCountdown:phase:closeStyle:progress:showProgress:showOpenStoreButton:showCloseButton:closeButtonEnabled:closeButtonAlpha:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) BOOL canClose __attribute__((swift_name("canClose")));
@property (readonly) float closeButtonAlpha __attribute__((swift_name("closeButtonAlpha")));
@property (readonly) BOOL closeButtonEnabled __attribute__((swift_name("closeButtonEnabled")));
@property (readonly) SharedFullscreenNativeAdCloseStyle *closeStyle __attribute__((swift_name("closeStyle")));
@property (readonly) SharedFullscreenNativeAdControlPhase *phase __attribute__((swift_name("phase")));
@property (readonly) float progress __attribute__((swift_name("progress")));
@property (readonly) int32_t remainingSeconds __attribute__((swift_name("remainingSeconds")));
@property (readonly) BOOL showCloseButton __attribute__((swift_name("showCloseButton")));
@property (readonly) BOOL showCountdown __attribute__((swift_name("showCountdown")));
@property (readonly) BOOL showOpenStoreButton __attribute__((swift_name("showOpenStoreButton")));
@property (readonly) BOOL showProgress __attribute__((swift_name("showProgress")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("FullscreenNativeAdError")))
@interface SharedFullscreenNativeAdError : SharedBase
- (instancetype)initWithAlias:(NSString *)alias adUnitId:(NSString *)adUnitId code:(int32_t)code message:(NSString *)message __attribute__((swift_name("init(alias:adUnitId:code:message:)"))) __attribute__((objc_designated_initializer));
- (SharedFullscreenNativeAdError *)doCopyAlias:(NSString *)alias adUnitId:(NSString *)adUnitId code:(int32_t)code message:(NSString *)message __attribute__((swift_name("doCopy(alias:adUnitId:code:message:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) NSString *adUnitId __attribute__((swift_name("adUnitId")));
@property (readonly) NSString *alias __attribute__((swift_name("alias")));
@property (readonly) int32_t code __attribute__((swift_name("code")));
@property (readonly) NSString *message __attribute__((swift_name("message")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("FullscreenNativeAdInfo")))
@interface SharedFullscreenNativeAdInfo : SharedBase
- (instancetype)initWithAlias:(NSString *)alias adUnitId:(NSString *)adUnitId layoutName:(NSString *)layoutName __attribute__((swift_name("init(alias:adUnitId:layoutName:)"))) __attribute__((objc_designated_initializer));
- (SharedFullscreenNativeAdInfo *)doCopyAlias:(NSString *)alias adUnitId:(NSString *)adUnitId layoutName:(NSString *)layoutName __attribute__((swift_name("doCopy(alias:adUnitId:layoutName:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) NSString *adUnitId __attribute__((swift_name("adUnitId")));
@property (readonly) NSString *alias __attribute__((swift_name("alias")));
@property (readonly) NSString *layoutName __attribute__((swift_name("layoutName")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("FullscreenNativeAdMode")))
@interface SharedFullscreenNativeAdMode : SharedKotlinEnum<SharedFullscreenNativeAdMode *>
+ (instancetype)alloc __attribute__((unavailable));
+ (instancetype)allocWithZone:(struct _NSZone *)zone __attribute__((unavailable));
- (instancetype)initWithName:(NSString *)name ordinal:(int32_t)ordinal __attribute__((swift_name("init(name:ordinal:)"))) __attribute__((objc_designated_initializer)) __attribute__((unavailable));
@property (class, readonly, getter=companion) SharedFullscreenNativeAdModeCompanion *companion __attribute__((swift_name("companion")));
@property (class, readonly) SharedFullscreenNativeAdMode *single __attribute__((swift_name("single")));
@property (class, readonly) SharedFullscreenNativeAdMode *transparent __attribute__((swift_name("transparent")));
@property (class, readonly) SharedFullscreenNativeAdMode *ctr __attribute__((swift_name("ctr")));
@property (class, readonly) SharedFullscreenNativeAdMode *multiple __attribute__((swift_name("multiple")));
@property (class, readonly) SharedFullscreenNativeAdMode *sequence __attribute__((swift_name("sequence")));
@property (class, readonly) SharedFullscreenNativeAdMode *overlay __attribute__((swift_name("overlay")));
@property (class, readonly) SharedFullscreenNativeAdMode *overlayTransparent __attribute__((swift_name("overlayTransparent")));
@property (class, readonly) SharedFullscreenNativeAdMode *overlayCtr __attribute__((swift_name("overlayCtr")));
@property (class, readonly) SharedFullscreenNativeAdMode *overlayCls __attribute__((swift_name("overlayCls")));
@property (class, readonly) SharedFullscreenNativeAdMode *overlayNav __attribute__((swift_name("overlayNav")));
@property (class, readonly) SharedFullscreenNativeAdMode *overlayPgrs __attribute__((swift_name("overlayPgrs")));
@property (class, readonly) SharedFullscreenNativeAdMode *overlayPgrsCls __attribute__((swift_name("overlayPgrsCls")));
@property (class, readonly) SharedFullscreenNativeAdMode *loop __attribute__((swift_name("loop")));
+ (SharedKotlinArray<SharedFullscreenNativeAdMode *> *)values __attribute__((swift_name("values()")));
@property (class, readonly) NSArray<SharedFullscreenNativeAdMode *> *entries __attribute__((swift_name("entries")));
@property (readonly) BOOL isBgLibraryOverlay __attribute__((swift_name("isBgLibraryOverlay")));
@property (readonly) BOOL isProgressOverlay __attribute__((swift_name("isProgressOverlay")));
@property (readonly) BOOL keepsDuplicateLayouts __attribute__((swift_name("keepsDuplicateLayouts")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("FullscreenNativeAdMode.Companion")))
@interface SharedFullscreenNativeAdModeCompanion : SharedBase
+ (instancetype)alloc __attribute__((unavailable));
+ (instancetype)allocWithZone:(struct _NSZone *)zone __attribute__((unavailable));
+ (instancetype)companion __attribute__((swift_name("init()")));
@property (class, readonly, getter=shared) SharedFullscreenNativeAdModeCompanion *shared __attribute__((swift_name("shared")));
- (SharedFullscreenNativeAdMode *)fromNameValue:(NSString * _Nullable)value __attribute__((swift_name("fromName(value:)")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("FullscreenNativeAdRegistry")))
@interface SharedFullscreenNativeAdRegistry : SharedBase
+ (instancetype)alloc __attribute__((unavailable));
+ (instancetype)allocWithZone:(struct _NSZone *)zone __attribute__((unavailable));
+ (instancetype)fullscreenNativeAdRegistry __attribute__((swift_name("init()")));
@property (class, readonly, getter=shared) SharedFullscreenNativeAdRegistry *shared __attribute__((swift_name("shared")));
- (SharedFullscreenNativeAdConfig *)configForAlias:(NSString *)alias fallbackAdUnitId:(NSString *)fallbackAdUnitId __attribute__((swift_name("configFor(alias:fallbackAdUnitId:)")));
- (void)createAlias:(NSString *)alias config:(SharedFullscreenNativeAdConfig *)config __attribute__((swift_name("create(alias:config:)")));
- (void)destroyAlias:(NSString *)alias __attribute__((swift_name("destroy(alias:)")));
- (BOOL)isReadyAlias:(NSString *)alias __attribute__((swift_name("isReady(alias:)")));
- (NSString * _Nullable)loadedAdUnitIdAlias:(NSString *)alias __attribute__((swift_name("loadedAdUnitId(alias:)")));
- (void)markLoadingAlias:(NSString *)alias __attribute__((swift_name("markLoading(alias:)")));

/**
 * Takes the loaded ad out of the ready state as soon as its screen is built, so a second show
 * cannot reuse it. [notifyDisplayed] still runs once the ad is actually on screen.
 */
- (void)markShowingAlias:(NSString *)alias __attribute__((swift_name("markShowing(alias:)")));
- (void)notifyClickedAlias:(NSString *)alias __attribute__((swift_name("notifyClicked(alias:)")));
- (void)notifyClosedAlias:(NSString *)alias __attribute__((swift_name("notifyClosed(alias:)")));
- (void)notifyDisplayedAlias:(NSString *)alias adUnitId:(NSString *)adUnitId layoutName:(NSString *)layoutName __attribute__((swift_name("notifyDisplayed(alias:adUnitId:layoutName:)")));
- (void)notifyFailedAlias:(NSString *)alias adUnitId:(NSString *)adUnitId message:(NSString *)message code:(int32_t)code __attribute__((swift_name("notifyFailed(alias:adUnitId:message:code:)")));
- (void)notifyLoadedAlias:(NSString *)alias adUnitId:(NSString *)adUnitId __attribute__((swift_name("notifyLoaded(alias:adUnitId:)")));
- (void)notifyStateAlias:(NSString *)alias state:(SharedNativeAdState *)state adUnitId:(NSString * _Nullable)adUnitId errorMessage:(NSString * _Nullable)errorMessage __attribute__((swift_name("notifyState(alias:state:adUnitId:errorMessage:)")));
- (void)setCallbackAlias:(NSString *)alias callback:(id<SharedFullscreenNativeAdCallback> _Nullable)callback __attribute__((swift_name("setCallback(alias:callback:)")));
- (BOOL)shouldReloadAfterShowAlias:(NSString *)alias __attribute__((swift_name("shouldReloadAfterShow(alias:)")));
- (SharedNativeAdState *)stateForAlias:(NSString *)alias __attribute__((swift_name("stateFor(alias:)")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("FullscreenNativeAdShowOptions")))
@interface SharedFullscreenNativeAdShowOptions : SharedBase
- (instancetype)initWithLayoutName:(NSString *)layoutName durationSeconds:(double)durationSeconds autoClose:(BOOL)autoClose pauseGameplay:(BOOL)pauseGameplay mode:(SharedFullscreenNativeAdMode *)mode layoutNames:(NSArray<NSString *> *)layoutNames durationsSeconds:(NSArray<SharedDouble *> *)durationsSeconds orientation:(NSString *)orientation enableAdComeback:(BOOL)enableAdComeback showTCD:(BOOL)showTCD delaySeconds:(double)delaySeconds timeUpCSeconds:(int32_t)timeUpCSeconds clickAssets:(SharedNativeClickAssetOptions *)clickAssets assetVisibility:(SharedNativeAssetVisibilityOptions *)assetVisibility __attribute__((swift_name("init(layoutName:durationSeconds:autoClose:pauseGameplay:mode:layoutNames:durationsSeconds:orientation:enableAdComeback:showTCD:delaySeconds:timeUpCSeconds:clickAssets:assetVisibility:)"))) __attribute__((objc_designated_initializer));
@property (class, readonly, getter=companion) SharedFullscreenNativeAdShowOptionsCompanion *companion __attribute__((swift_name("companion")));
- (SharedFullscreenNativeAdShowOptions *)doCopyLayoutName:(NSString *)layoutName durationSeconds:(double)durationSeconds autoClose:(BOOL)autoClose pauseGameplay:(BOOL)pauseGameplay mode:(SharedFullscreenNativeAdMode *)mode layoutNames:(NSArray<NSString *> *)layoutNames durationsSeconds:(NSArray<SharedDouble *> *)durationsSeconds orientation:(NSString *)orientation enableAdComeback:(BOOL)enableAdComeback showTCD:(BOOL)showTCD delaySeconds:(double)delaySeconds timeUpCSeconds:(int32_t)timeUpCSeconds clickAssets:(SharedNativeClickAssetOptions *)clickAssets assetVisibility:(SharedNativeAssetVisibilityOptions *)assetVisibility __attribute__((swift_name("doCopy(layoutName:durationSeconds:autoClose:pauseGameplay:mode:layoutNames:durationsSeconds:orientation:enableAdComeback:showTCD:delaySeconds:timeUpCSeconds:clickAssets:assetVisibility:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) SharedNativeAssetVisibilityOptions *assetVisibility __attribute__((swift_name("assetVisibility")));
@property (readonly) BOOL autoClose __attribute__((swift_name("autoClose")));
@property (readonly) SharedNativeClickAssetOptions *clickAssets __attribute__((swift_name("clickAssets")));
@property (readonly) double delaySeconds __attribute__((swift_name("delaySeconds")));
@property (readonly) double durationSeconds __attribute__((swift_name("durationSeconds")));
@property (readonly) NSArray<SharedDouble *> *durationsSeconds __attribute__((swift_name("durationsSeconds")));
@property (readonly) BOOL enableAdComeback __attribute__((swift_name("enableAdComeback")));
@property (readonly) NSString *layoutName __attribute__((swift_name("layoutName")));
@property (readonly) NSArray<NSString *> *layoutNames __attribute__((swift_name("layoutNames")));
@property (readonly) SharedFullscreenNativeAdMode *mode __attribute__((swift_name("mode")));
@property (readonly) NSArray<SharedDouble *> *normalizedDurationsSeconds __attribute__((swift_name("normalizedDurationsSeconds")));
@property (readonly) NSString *normalizedLayoutName __attribute__((swift_name("normalizedLayoutName")));
@property (readonly) NSArray<NSString *> *normalizedLayoutNames __attribute__((swift_name("normalizedLayoutNames")));
@property (readonly) NSString *orientation __attribute__((swift_name("orientation")));
@property (readonly) BOOL pauseGameplay __attribute__((swift_name("pauseGameplay")));
@property (readonly) double resolvedDurationSeconds __attribute__((swift_name("resolvedDurationSeconds")));
@property (readonly) BOOL showTCD __attribute__((swift_name("showTCD")));
@property (readonly) int32_t timeUpCSeconds __attribute__((swift_name("timeUpCSeconds")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("FullscreenNativeAdShowOptions.Companion")))
@interface SharedFullscreenNativeAdShowOptionsCompanion : SharedBase
+ (instancetype)alloc __attribute__((unavailable));
+ (instancetype)allocWithZone:(struct _NSZone *)zone __attribute__((unavailable));
+ (instancetype)companion __attribute__((swift_name("init()")));
@property (class, readonly, getter=shared) SharedFullscreenNativeAdShowOptionsCompanion *shared __attribute__((swift_name("shared")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("IosFullscreenNativeAdSdk")))
@interface SharedIosFullscreenNativeAdSdk : SharedBase
- (instancetype)init __attribute__((swift_name("init()"))) __attribute__((objc_designated_initializer));
+ (instancetype)new __attribute__((availability(swift, unavailable, message="use object initializers instead")));
- (void)createAlias:(NSString *)alias adUnitIdsCsv:(NSString *)adUnitIdsCsv preloadBufferSize:(int32_t)preloadBufferSize enableReloadAfterShow:(BOOL)enableReloadAfterShow __attribute__((swift_name("create(alias:adUnitIdsCsv:preloadBufferSize:enableReloadAfterShow:)")));
- (void)destroyAlias:(NSString *)alias __attribute__((swift_name("destroy(alias:)")));
- (BOOL)isReadyAlias:(NSString *)alias __attribute__((swift_name("isReady(alias:)")));
- (void)loadRootViewController:(UIViewController *)rootViewController alias:(NSString *)alias __attribute__((swift_name("load(rootViewController:alias:)")));
- (void)loadWithIdsRootViewController:(UIViewController *)rootViewController alias:(NSString *)alias adUnitIdsCsv:(NSString *)adUnitIdsCsv enableReloadAfterShow:(BOOL)enableReloadAfterShow __attribute__((swift_name("loadWithIds(rootViewController:alias:adUnitIdsCsv:enableReloadAfterShow:)")));
- (void)setCallbackAlias:(NSString *)alias callback:(id<SharedFullscreenNativeAdCallback> _Nullable)callback __attribute__((swift_name("setCallback(alias:callback:)")));
- (UIViewController *)viewControllerAlias:(NSString *)alias options:(SharedFullscreenNativeAdShowOptions *)options __attribute__((swift_name("viewController(alias:options:)")));
- (UIViewController *)viewControllerWithLayoutNameAlias:(NSString *)alias layoutName:(NSString *)layoutName durationSeconds:(double)durationSeconds autoClose:(BOOL)autoClose __attribute__((swift_name("viewControllerWithLayoutName(alias:layoutName:durationSeconds:autoClose:)")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("NativeAssetVisibilityOptions")))
@interface SharedNativeAssetVisibilityOptions : SharedBase
- (instancetype)initWithCta:(BOOL)cta headline:(BOOL)headline body:(BOOL)body description:(BOOL)description icon:(BOOL)icon advertiser:(BOOL)advertiser media:(BOOL)media mediaImage:(BOOL)mediaImage mediaVideo:(BOOL)mediaVideo price:(BOOL)price store:(BOOL)store starRating:(BOOL)starRating __attribute__((swift_name("init(cta:headline:body:description:icon:advertiser:media:mediaImage:mediaVideo:price:store:starRating:)"))) __attribute__((objc_designated_initializer));
- (SharedNativeAssetVisibilityOptions *)doCopyCta:(BOOL)cta headline:(BOOL)headline body:(BOOL)body description:(BOOL)description icon:(BOOL)icon advertiser:(BOOL)advertiser media:(BOOL)media mediaImage:(BOOL)mediaImage mediaVideo:(BOOL)mediaVideo price:(BOOL)price store:(BOOL)store starRating:(BOOL)starRating __attribute__((swift_name("doCopy(cta:headline:body:description:icon:advertiser:media:mediaImage:mediaVideo:price:store:starRating:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) BOOL advertiser __attribute__((swift_name("advertiser")));
@property (readonly) BOOL anyMediaVisible __attribute__((swift_name("anyMediaVisible")));
@property (readonly) BOOL body __attribute__((swift_name("body")));
@property (readonly) BOOL bodyOrDescriptionVisible __attribute__((swift_name("bodyOrDescriptionVisible")));
@property (readonly) BOOL cta __attribute__((swift_name("cta")));
@property (readonly) BOOL description_ __attribute__((swift_name("description_")));
@property (readonly) BOOL headline __attribute__((swift_name("headline")));
@property (readonly) BOOL icon __attribute__((swift_name("icon")));
@property (readonly) BOOL media __attribute__((swift_name("media")));
@property (readonly) BOOL mediaImage __attribute__((swift_name("mediaImage")));
@property (readonly) BOOL mediaImageVisible __attribute__((swift_name("mediaImageVisible")));
@property (readonly) BOOL mediaVideo __attribute__((swift_name("mediaVideo")));
@property (readonly) BOOL mediaVideoVisible __attribute__((swift_name("mediaVideoVisible")));
@property (readonly) BOOL price __attribute__((swift_name("price")));
@property (readonly) BOOL starRating __attribute__((swift_name("starRating")));
@property (readonly) BOOL store __attribute__((swift_name("store")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("NativeClickAssetOptions")))
@interface SharedNativeClickAssetOptions : SharedBase
- (instancetype)initWithCta:(BOOL)cta headline:(BOOL)headline body:(BOOL)body description:(BOOL)description icon:(BOOL)icon advertiser:(BOOL)advertiser media:(BOOL)media mediaImage:(BOOL)mediaImage mediaVideo:(BOOL)mediaVideo __attribute__((swift_name("init(cta:headline:body:description:icon:advertiser:media:mediaImage:mediaVideo:)"))) __attribute__((objc_designated_initializer));
- (SharedNativeClickAssetOptions *)doCopyCta:(BOOL)cta headline:(BOOL)headline body:(BOOL)body description:(BOOL)description icon:(BOOL)icon advertiser:(BOOL)advertiser media:(BOOL)media mediaImage:(BOOL)mediaImage mediaVideo:(BOOL)mediaVideo __attribute__((swift_name("doCopy(cta:headline:body:description:icon:advertiser:media:mediaImage:mediaVideo:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) BOOL advertiser __attribute__((swift_name("advertiser")));
@property (readonly) BOOL anyMediaClickEnabled __attribute__((swift_name("anyMediaClickEnabled")));
@property (readonly) BOOL body __attribute__((swift_name("body")));
@property (readonly) BOOL bodyOrDescriptionClickEnabled __attribute__((swift_name("bodyOrDescriptionClickEnabled")));
@property (readonly) BOOL cta __attribute__((swift_name("cta")));
@property (readonly) BOOL description_ __attribute__((swift_name("description_")));
@property (readonly) BOOL headline __attribute__((swift_name("headline")));
@property (readonly) BOOL icon __attribute__((swift_name("icon")));
@property (readonly) BOOL media __attribute__((swift_name("media")));
@property (readonly) BOOL mediaImage __attribute__((swift_name("mediaImage")));
@property (readonly) BOOL mediaImageClickEnabled __attribute__((swift_name("mediaImageClickEnabled")));
@property (readonly) BOOL mediaVideo __attribute__((swift_name("mediaVideo")));
@property (readonly) BOOL mediaVideoClickEnabled __attribute__((swift_name("mediaVideoClickEnabled")));
@end


/**
 * @note annotations
 *   androidx.compose.runtime.Immutable
*/
__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("FullscreenNativeAdCallbacks")))
@interface SharedFullscreenNativeAdCallbacks : SharedBase
- (instancetype)initWithOnMediaClick:(void (^)(void))onMediaClick onIconClick:(void (^)(void))onIconClick onInfoClick:(void (^)(void))onInfoClick onCallToActionClick:(void (^)(void))onCallToActionClick onOpenStoreClick:(void (^)(void))onOpenStoreClick onCloseClick:(void (^)(void))onCloseClick __attribute__((swift_name("init(onMediaClick:onIconClick:onInfoClick:onCallToActionClick:onOpenStoreClick:onCloseClick:)"))) __attribute__((objc_designated_initializer));
- (SharedFullscreenNativeAdCallbacks *)doCopyOnMediaClick:(void (^)(void))onMediaClick onIconClick:(void (^)(void))onIconClick onInfoClick:(void (^)(void))onInfoClick onCallToActionClick:(void (^)(void))onCallToActionClick onOpenStoreClick:(void (^)(void))onOpenStoreClick onCloseClick:(void (^)(void))onCloseClick __attribute__((swift_name("doCopy(onMediaClick:onIconClick:onInfoClick:onCallToActionClick:onOpenStoreClick:onCloseClick:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) void (^onCallToActionClick)(void) __attribute__((swift_name("onCallToActionClick")));
@property (readonly) void (^onCloseClick)(void) __attribute__((swift_name("onCloseClick")));
@property (readonly) void (^onIconClick)(void) __attribute__((swift_name("onIconClick")));
@property (readonly) void (^onInfoClick)(void) __attribute__((swift_name("onInfoClick")));
@property (readonly) void (^onMediaClick)(void) __attribute__((swift_name("onMediaClick")));
@property (readonly) void (^onOpenStoreClick)(void) __attribute__((swift_name("onOpenStoreClick")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("FullscreenNativeAdLayoutCatalog")))
@interface SharedFullscreenNativeAdLayoutCatalog : SharedBase
+ (instancetype)alloc __attribute__((unavailable));
+ (instancetype)allocWithZone:(struct _NSZone *)zone __attribute__((unavailable));
+ (instancetype)fullscreenNativeAdLayoutCatalog __attribute__((swift_name("init()")));
@property (class, readonly, getter=shared) SharedFullscreenNativeAdLayoutCatalog *shared __attribute__((swift_name("shared")));
- (NSString *)normalizeLayoutName:(NSString *)layoutName __attribute__((swift_name("normalize(layoutName:)")));
- (BOOL)requiresDetachedLandscapeControlLayoutName:(NSString *)layoutName __attribute__((swift_name("requiresDetachedLandscapeControl(layoutName:)")));
@property (readonly) NSString *Cls01 __attribute__((swift_name("Cls01")));
@property (readonly) NSString *Cls02 __attribute__((swift_name("Cls02")));
@property (readonly) NSString *Cls03 __attribute__((swift_name("Cls03")));
@property (readonly) NSString *Cls04 __attribute__((swift_name("Cls04")));
@property (readonly) NSString *Cls05 __attribute__((swift_name("Cls05")));
@property (readonly) NSString *Cls06 __attribute__((swift_name("Cls06")));
@property (readonly) NSString *Cls07 __attribute__((swift_name("Cls07")));
@property (readonly) NSString *Cls08 __attribute__((swift_name("Cls08")));
@property (readonly) NSString *Cls09 __attribute__((swift_name("Cls09")));
@property (readonly) NSString *Cls10 __attribute__((swift_name("Cls10")));
@property (readonly) NSString *Cls11 __attribute__((swift_name("Cls11")));
@property (readonly) NSString *Loop01 __attribute__((swift_name("Loop01")));
@property (readonly) NSString *Nav01 __attribute__((swift_name("Nav01")));
@property (readonly) NSString *Nav02 __attribute__((swift_name("Nav02")));
@property (readonly) NSString *Nav03 __attribute__((swift_name("Nav03")));
@property (readonly) NSString *Progress01 __attribute__((swift_name("Progress01")));
@property (readonly) NSString *ProgressCls01 __attribute__((swift_name("ProgressCls01")));
@property (readonly) NSString *ProgressOnly01 __attribute__((swift_name("ProgressOnly01")));
@property (readonly) NSString *ProgressOnly02 __attribute__((swift_name("ProgressOnly02")));
@property (readonly) NSString *ProgressOnly03 __attribute__((swift_name("ProgressOnly03")));
@property (readonly) NSString *ProgressOnly04 __attribute__((swift_name("ProgressOnly04")));
@property (readonly) NSString *Universal01 __attribute__((swift_name("Universal01")));
@property (readonly) NSString *Universal02 __attribute__((swift_name("Universal02")));
@property (readonly) NSString *Universal03 __attribute__((swift_name("Universal03")));
@property (readonly) NSString *Universal04 __attribute__((swift_name("Universal04")));
@property (readonly) NSString *Universal05 __attribute__((swift_name("Universal05")));
@property (readonly) NSString *Universal06 __attribute__((swift_name("Universal06")));
@property (readonly) NSString *Universal07 __attribute__((swift_name("Universal07")));
@property (readonly) NSString *Universal08 __attribute__((swift_name("Universal08")));
@property (readonly) NSString *Universal09 __attribute__((swift_name("Universal09")));
@property (readonly) NSString *Universal10 __attribute__((swift_name("Universal10")));
@property (readonly) NSString *Universal11 __attribute__((swift_name("Universal11")));
@property (readonly) NSString *Universal12 __attribute__((swift_name("Universal12")));
@property (readonly) NSArray<NSString *> *cls __attribute__((swift_name("cls")));
@property (readonly) NSArray<NSString *> *firstClsLayouts __attribute__((swift_name("firstClsLayouts")));
@property (readonly) NSArray<NSString *> *loop __attribute__((swift_name("loop")));
@property (readonly) NSArray<NSString *> *nav __attribute__((swift_name("nav")));
@property (readonly) NSArray<NSString *> *progress __attribute__((swift_name("progress")));
@property (readonly) NSArray<NSString *> *progressCls __attribute__((swift_name("progressCls")));
@property (readonly) NSArray<NSString *> *supported __attribute__((swift_name("supported")));
@property (readonly) NSArray<NSString *> *universal __attribute__((swift_name("universal")));
@end


/**
 * @note annotations
 *   androidx.compose.runtime.Immutable
*/
__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("FullscreenNativeAdState")))
@interface SharedFullscreenNativeAdState : SharedBase
- (instancetype)initWithHeadline:(NSString *)headline body:(NSString *)body advertiser:(NSString *)advertiser callToAction:(NSString *)callToAction countdownText:(NSString *)countdownText showCountdown:(BOOL)showCountdown showCloseButton:(BOOL)showCloseButton closeButtonEnabled:(BOOL)closeButtonEnabled closeButtonAlpha:(float)closeButtonAlpha closeStyle:(SharedFullscreenNativeAdCloseStyle *)closeStyle countdownProgress:(float)countdownProgress showProgress:(BOOL)showProgress showOpenStoreButton:(BOOL)showOpenStoreButton showHeadline:(BOOL)showHeadline showBody:(BOOL)showBody showAdvertiser:(BOOL)showAdvertiser showIcon:(BOOL)showIcon showCallToAction:(BOOL)showCallToAction showMedia:(BOOL)showMedia showAdChoices:(BOOL)showAdChoices usesNativeAssetTouchHandling:(BOOL)usesNativeAssetTouchHandling renderAdvertiserText:(BOOL)renderAdvertiserText showIconSlot:(BOOL)showIconSlot __attribute__((swift_name("init(headline:body:advertiser:callToAction:countdownText:showCountdown:showCloseButton:closeButtonEnabled:closeButtonAlpha:closeStyle:countdownProgress:showProgress:showOpenStoreButton:showHeadline:showBody:showAdvertiser:showIcon:showCallToAction:showMedia:showAdChoices:usesNativeAssetTouchHandling:renderAdvertiserText:showIconSlot:)"))) __attribute__((objc_designated_initializer));
- (SharedFullscreenNativeAdState *)doCopyHeadline:(NSString *)headline body:(NSString *)body advertiser:(NSString *)advertiser callToAction:(NSString *)callToAction countdownText:(NSString *)countdownText showCountdown:(BOOL)showCountdown showCloseButton:(BOOL)showCloseButton closeButtonEnabled:(BOOL)closeButtonEnabled closeButtonAlpha:(float)closeButtonAlpha closeStyle:(SharedFullscreenNativeAdCloseStyle *)closeStyle countdownProgress:(float)countdownProgress showProgress:(BOOL)showProgress showOpenStoreButton:(BOOL)showOpenStoreButton showHeadline:(BOOL)showHeadline showBody:(BOOL)showBody showAdvertiser:(BOOL)showAdvertiser showIcon:(BOOL)showIcon showCallToAction:(BOOL)showCallToAction showMedia:(BOOL)showMedia showAdChoices:(BOOL)showAdChoices usesNativeAssetTouchHandling:(BOOL)usesNativeAssetTouchHandling renderAdvertiserText:(BOOL)renderAdvertiserText showIconSlot:(BOOL)showIconSlot __attribute__((swift_name("doCopy(headline:body:advertiser:callToAction:countdownText:showCountdown:showCloseButton:closeButtonEnabled:closeButtonAlpha:closeStyle:countdownProgress:showProgress:showOpenStoreButton:showHeadline:showBody:showAdvertiser:showIcon:showCallToAction:showMedia:showAdChoices:usesNativeAssetTouchHandling:renderAdvertiserText:showIconSlot:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) NSString *advertiser __attribute__((swift_name("advertiser")));
@property (readonly) NSString *body __attribute__((swift_name("body")));
@property (readonly) NSString *callToAction __attribute__((swift_name("callToAction")));
@property (readonly) float closeButtonAlpha __attribute__((swift_name("closeButtonAlpha")));
@property (readonly) BOOL closeButtonEnabled __attribute__((swift_name("closeButtonEnabled")));
@property (readonly) SharedFullscreenNativeAdCloseStyle *closeStyle __attribute__((swift_name("closeStyle")));
@property (readonly) float countdownProgress __attribute__((swift_name("countdownProgress")));
@property (readonly) NSString *countdownText __attribute__((swift_name("countdownText")));
@property (readonly) NSString *headline __attribute__((swift_name("headline")));

/** Android can render this text through the registered native advertiser view. */
@property (readonly) BOOL renderAdvertiserText __attribute__((swift_name("renderAdvertiserText")));
@property (readonly) BOOL showAdChoices __attribute__((swift_name("showAdChoices")));
@property (readonly) BOOL showAdvertiser __attribute__((swift_name("showAdvertiser")));
@property (readonly) BOOL showBody __attribute__((swift_name("showBody")));
@property (readonly) BOOL showCallToAction __attribute__((swift_name("showCallToAction")));
@property (readonly) BOOL showCloseButton __attribute__((swift_name("showCloseButton")));
@property (readonly) BOOL showCountdown __attribute__((swift_name("showCountdown")));
@property (readonly) BOOL showHeadline __attribute__((swift_name("showHeadline")));
@property (readonly) BOOL showIcon __attribute__((swift_name("showIcon")));

/** The icon frame stays (empty) when the ad has no icon image; only hiding the icon asset removes it. */
@property (readonly) BOOL showIconSlot __attribute__((swift_name("showIconSlot")));
@property (readonly) BOOL showMedia __attribute__((swift_name("showMedia")));
@property (readonly) BOOL showOpenStoreButton __attribute__((swift_name("showOpenStoreButton")));
@property (readonly) BOOL showProgress __attribute__((swift_name("showProgress")));

/** Native platform assets must receive the touch themselves instead of Compose consuming it. */
@property (readonly) BOOL usesNativeAssetTouchHandling __attribute__((swift_name("usesNativeAssetTouchHandling")));
@end

__attribute__((swift_name("InterstitialAdCallback")))
@protocol SharedInterstitialAdCallback
@required
- (void)onInterstitialClickedAd:(SharedInterstitialAdInfo *)ad __attribute__((swift_name("onInterstitialClicked(ad:)")));
- (void)onInterstitialClosedAd:(SharedInterstitialAdInfo *)ad __attribute__((swift_name("onInterstitialClosed(ad:)")));
- (void)onInterstitialDisplayable __attribute__((swift_name("onInterstitialDisplayable()")));
- (void)onInterstitialDisplayedAd:(SharedInterstitialAdInfo *)ad __attribute__((swift_name("onInterstitialDisplayed(ad:)")));
- (void)onInterstitialFailedToLoadAdUnit:(NSString *)adUnit errorCode:(int32_t)errorCode error:(NSString *)error __attribute__((swift_name("onInterstitialFailedToLoad(adUnit:errorCode:error:)")));
- (void)onInterstitialLoadedAd:(SharedInterstitialAdInfo *)ad __attribute__((swift_name("onInterstitialLoaded(ad:)")));
- (void)onInterstitialOpenedAd:(SharedInterstitialAdInfo *)ad __attribute__((swift_name("onInterstitialOpened(ad:)")));
- (void)onInterstitialPaidImpressionAd:(SharedInterstitialAdInfo *)ad paidInfo:(SharedInterstitialAdPaidInfo *)paidInfo __attribute__((swift_name("onInterstitialPaidImpression(ad:paidInfo:)")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("InterstitialAdConfig")))
@interface SharedInterstitialAdConfig : SharedBase
- (instancetype)initWithIds:(NSArray<NSString *> *)ids autoReload:(BOOL)autoReload preloadBufferSize:(int32_t)preloadBufferSize __attribute__((swift_name("init(ids:autoReload:preloadBufferSize:)"))) __attribute__((objc_designated_initializer));
- (SharedInterstitialAdConfig *)doCopyIds:(NSArray<NSString *> *)ids autoReload:(BOOL)autoReload preloadBufferSize:(int32_t)preloadBufferSize __attribute__((swift_name("doCopy(ids:autoReload:preloadBufferSize:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) BOOL autoReload __attribute__((swift_name("autoReload")));
@property (readonly) NSArray<NSString *> *ids __attribute__((swift_name("ids")));
@property (readonly) int32_t preloadBufferSize __attribute__((swift_name("preloadBufferSize")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("InterstitialAdInfo")))
@interface SharedInterstitialAdInfo : SharedBase
- (instancetype)initWithAdUnitId:(NSString *)adUnitId mediationAdapter:(NSString *)mediationAdapter responseId:(NSString *)responseId adSource:(NSString * _Nullable)adSource __attribute__((swift_name("init(adUnitId:mediationAdapter:responseId:adSource:)"))) __attribute__((objc_designated_initializer));
- (SharedInterstitialAdInfo *)doCopyAdUnitId:(NSString *)adUnitId mediationAdapter:(NSString *)mediationAdapter responseId:(NSString *)responseId adSource:(NSString * _Nullable)adSource __attribute__((swift_name("doCopy(adUnitId:mediationAdapter:responseId:adSource:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) NSString * _Nullable adSource __attribute__((swift_name("adSource")));
@property (readonly) NSString *adUnitId __attribute__((swift_name("adUnitId")));
@property (readonly) NSString *mediationAdapter __attribute__((swift_name("mediationAdapter")));
@property (readonly) NSString *responseId __attribute__((swift_name("responseId")));
@end

__attribute__((swift_name("InterstitialAdLoader")))
@protocol SharedInterstitialAdLoader
@required
- (void)configureAlias:(NSString *)alias config:(SharedInterstitialAdConfig *)config __attribute__((swift_name("configure(alias:config:)")));
- (void)destroyAlias:(NSString *)alias __attribute__((swift_name("destroy(alias:)")));
- (void)hideAlias:(NSString *)alias __attribute__((swift_name("hide(alias:)")));
- (BOOL)isReadyAlias:(NSString *)alias __attribute__((swift_name("isReady(alias:)")));
- (void)loadAlias:(NSString *)alias onResult:(void (^)(SharedNativeAdLoadResult *))onResult __attribute__((swift_name("load(alias:onResult:)")));
- (void)loadAlias:(NSString *)alias bufferSize:(int32_t)bufferSize onResult:(void (^)(SharedNativeAdLoadResult *))onResult __attribute__((swift_name("load(alias:bufferSize:onResult:)")));
- (void)setCallbackAlias:(NSString *)alias callback:(id<SharedInterstitialAdCallback> _Nullable)callback __attribute__((swift_name("setCallback(alias:callback:)")));
- (BOOL)showAlias:(NSString *)alias options:(SharedInterstitialShowOptions *)options __attribute__((swift_name("show(alias:options:)")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("InterstitialAdPaidInfo")))
@interface SharedInterstitialAdPaidInfo : SharedBase
- (instancetype)initWithAdUnitId:(NSString *)adUnitId revenueMicros:(int64_t)revenueMicros currencyCode:(NSString *)currencyCode mediationAdapter:(NSString *)mediationAdapter adSource:(NSString * _Nullable)adSource responseId:(NSString *)responseId __attribute__((swift_name("init(adUnitId:revenueMicros:currencyCode:mediationAdapter:adSource:responseId:)"))) __attribute__((objc_designated_initializer));
- (SharedInterstitialAdPaidInfo *)doCopyAdUnitId:(NSString *)adUnitId revenueMicros:(int64_t)revenueMicros currencyCode:(NSString *)currencyCode mediationAdapter:(NSString *)mediationAdapter adSource:(NSString * _Nullable)adSource responseId:(NSString *)responseId __attribute__((swift_name("doCopy(adUnitId:revenueMicros:currencyCode:mediationAdapter:adSource:responseId:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) NSString * _Nullable adSource __attribute__((swift_name("adSource")));
@property (readonly) NSString *adUnitId __attribute__((swift_name("adUnitId")));
@property (readonly) NSString *currencyCode __attribute__((swift_name("currencyCode")));
@property (readonly) NSString *mediationAdapter __attribute__((swift_name("mediationAdapter")));
@property (readonly) NSString *responseId __attribute__((swift_name("responseId")));
@property (readonly) int64_t revenueMicros __attribute__((swift_name("revenueMicros")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("InterstitialAdRegistry")))
@interface SharedInterstitialAdRegistry : SharedBase
+ (instancetype)alloc __attribute__((unavailable));
+ (instancetype)allocWithZone:(struct _NSZone *)zone __attribute__((unavailable));
+ (instancetype)interstitialAdRegistry __attribute__((swift_name("init()")));
@property (class, readonly, getter=shared) SharedInterstitialAdRegistry *shared __attribute__((swift_name("shared")));
- (SharedInterstitialAdConfig *)configForAlias:(NSString *)alias fallbackAdUnitId:(NSString *)fallbackAdUnitId __attribute__((swift_name("configFor(alias:fallbackAdUnitId:)")));
- (void)createAlias:(NSString *)alias config:(SharedInterstitialAdConfig *)config __attribute__((swift_name("create(alias:config:)")));
- (void)destroyAlias:(NSString *)alias __attribute__((swift_name("destroy(alias:)")));
- (BOOL)isReadyAlias:(NSString *)alias __attribute__((swift_name("isReady(alias:)")));
- (NSString * _Nullable)loadedAdUnitIdAlias:(NSString *)alias __attribute__((swift_name("loadedAdUnitId(alias:)")));
- (void)notifyClickedAlias:(NSString *)alias ad:(SharedInterstitialAdInfo *)ad __attribute__((swift_name("notifyClicked(alias:ad:)")));
- (void)notifyClosedAlias:(NSString *)alias ad:(SharedInterstitialAdInfo *)ad __attribute__((swift_name("notifyClosed(alias:ad:)")));
- (void)notifyDisplayedAlias:(NSString *)alias ad:(SharedInterstitialAdInfo *)ad __attribute__((swift_name("notifyDisplayed(alias:ad:)")));
- (void)notifyFailedToLoadAlias:(NSString *)alias adUnit:(NSString *)adUnit errorCode:(int32_t)errorCode error:(NSString *)error __attribute__((swift_name("notifyFailedToLoad(alias:adUnit:errorCode:error:)")));
- (void)notifyLoadedAlias:(NSString *)alias ad:(SharedInterstitialAdInfo *)ad __attribute__((swift_name("notifyLoaded(alias:ad:)")));
- (void)notifyLoadingAlias:(NSString *)alias __attribute__((swift_name("notifyLoading(alias:)")));
- (void)notifyOpenedAlias:(NSString *)alias ad:(SharedInterstitialAdInfo *)ad __attribute__((swift_name("notifyOpened(alias:ad:)")));
- (void)notifyPaidImpressionAlias:(NSString *)alias ad:(SharedInterstitialAdInfo *)ad paidInfo:(SharedInterstitialAdPaidInfo *)paidInfo __attribute__((swift_name("notifyPaidImpression(alias:ad:paidInfo:)")));
- (void)setCallbackAlias:(NSString *)alias callback:(id<SharedInterstitialAdCallback> _Nullable)callback __attribute__((swift_name("setCallback(alias:callback:)")));
- (SharedNativeAdState *)stateForAlias:(NSString *)alias __attribute__((swift_name("stateFor(alias:)")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("InterstitialShowOptions")))
@interface SharedInterstitialShowOptions : SharedBase
- (instancetype)initWithImmersiveMode:(BOOL)immersiveMode __attribute__((swift_name("init(immersiveMode:)"))) __attribute__((objc_designated_initializer));
- (SharedInterstitialShowOptions *)doCopyImmersiveMode:(BOOL)immersiveMode __attribute__((swift_name("doCopy(immersiveMode:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) BOOL immersiveMode __attribute__((swift_name("immersiveMode")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("IosInterstitialAdSdk")))
@interface SharedIosInterstitialAdSdk : SharedBase
- (instancetype)init __attribute__((swift_name("init()"))) __attribute__((objc_designated_initializer));
+ (instancetype)new __attribute__((availability(swift, unavailable, message="use object initializers instead")));
- (void)createAlias:(NSString *)alias adUnitIdsCsv:(NSString *)adUnitIdsCsv preloadBufferSize:(int32_t)preloadBufferSize autoReload:(BOOL)autoReload __attribute__((swift_name("create(alias:adUnitIdsCsv:preloadBufferSize:autoReload:)")));
- (void)destroyAlias:(NSString *)alias __attribute__((swift_name("destroy(alias:)")));
- (BOOL)isReadyAlias:(NSString *)alias __attribute__((swift_name("isReady(alias:)")));
- (void)loadRootViewController:(UIViewController *)rootViewController alias:(NSString *)alias __attribute__((swift_name("load(rootViewController:alias:)")));
- (void)loadRootViewController:(UIViewController *)rootViewController alias:(NSString *)alias bufferSize:(int32_t)bufferSize __attribute__((swift_name("load(rootViewController:alias:bufferSize:)")));
- (void)loadWithConfigRootViewController:(UIViewController *)rootViewController alias:(NSString *)alias adUnitIdsCsv:(NSString *)adUnitIdsCsv preloadBufferSize:(int32_t)preloadBufferSize autoReload:(BOOL)autoReload onStateChanged:(void (^)(SharedNativeAdState *))onStateChanged __attribute__((swift_name("loadWithConfig(rootViewController:alias:adUnitIdsCsv:preloadBufferSize:autoReload:onStateChanged:)")));
- (void)setCallbackAlias:(NSString *)alias callback:(id<SharedInterstitialAdCallback> _Nullable)callback __attribute__((swift_name("setCallback(alias:callback:)")));
- (BOOL)showRootViewController:(UIViewController *)rootViewController alias:(NSString *)alias __attribute__((swift_name("show(rootViewController:alias:)")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("IosPopupNativeAdSdk")))
@interface SharedIosPopupNativeAdSdk : SharedBase
- (instancetype)init __attribute__((swift_name("init()"))) __attribute__((objc_designated_initializer));
+ (instancetype)new __attribute__((availability(swift, unavailable, message="use object initializers instead")));
- (void)closeAlias:(NSString *)alias __attribute__((swift_name("close(alias:)")));
- (void)createAlias:(NSString *)alias adUnitIdsCsv:(NSString *)adUnitIdsCsv layoutName:(NSString *)layoutName timeShowSeconds:(int32_t)timeShowSeconds timeReloadSeconds:(int32_t)timeReloadSeconds xDp:(float)xDp yDp:(float)yDp adWidthDp:(float)adWidthDp adHeightDp:(float)adHeightDp autoClose:(BOOL)autoClose enableCtrOverlay:(BOOL)enableCtrOverlay __attribute__((swift_name("create(alias:adUnitIdsCsv:layoutName:timeShowSeconds:timeReloadSeconds:xDp:yDp:adWidthDp:adHeightDp:autoClose:enableCtrOverlay:)")));
- (void)destroyAlias:(NSString *)alias __attribute__((swift_name("destroy(alias:)")));
- (void)hideAlias:(NSString *)alias __attribute__((swift_name("hide(alias:)")));
- (BOOL)isDisplayableAlias:(NSString *)alias __attribute__((swift_name("isDisplayable(alias:)")));
- (BOOL)isReadyAlias:(NSString *)alias __attribute__((swift_name("isReady(alias:)")));
- (void)loadRootViewController:(UIViewController * _Nullable)rootViewController alias:(NSString *)alias __attribute__((swift_name("load(rootViewController:alias:)")));
- (void)loadWithConfigRootViewController:(UIViewController * _Nullable)rootViewController alias:(NSString *)alias adUnitIdsCsv:(NSString *)adUnitIdsCsv layoutName:(NSString *)layoutName timeShowSeconds:(int32_t)timeShowSeconds timeReloadSeconds:(int32_t)timeReloadSeconds xDp:(float)xDp yDp:(float)yDp adWidthDp:(float)adWidthDp adHeightDp:(float)adHeightDp autoClose:(BOOL)autoClose enableCtrOverlay:(BOOL)enableCtrOverlay onStateChanged:(void (^)(SharedNativeAdState *))onStateChanged __attribute__((swift_name("loadWithConfig(rootViewController:alias:adUnitIdsCsv:layoutName:timeShowSeconds:timeReloadSeconds:xDp:yDp:adWidthDp:adHeightDp:autoClose:enableCtrOverlay:onStateChanged:)")));
- (void)setCallbackAlias:(NSString *)alias callback:(void (^)(SharedNativeAdState *))callback __attribute__((swift_name("setCallback(alias:callback:)")));
- (void)showRootViewController:(UIViewController * _Nullable)rootViewController alias:(NSString *)alias __attribute__((swift_name("show(rootViewController:alias:)")));
- (NSString *)stateForAlias:(NSString *)alias __attribute__((swift_name("stateFor(alias:)")));
- (void)stopAlias:(NSString *)alias __attribute__((swift_name("stop(alias:)")));
- (void)updatePlacementAlias:(NSString *)alias xDp:(float)xDp yDp:(float)yDp adWidthDp:(float)adWidthDp adHeightDp:(float)adHeightDp __attribute__((swift_name("updatePlacement(alias:xDp:yDp:adWidthDp:adHeightDp:)")));
@end

__attribute__((swift_name("PopupNativeAdCallback")))
@protocol SharedPopupNativeAdCallback
@required
- (void)onClickedAd__:(SharedPopupNativeAdInfo *)ad __attribute__((swift_name("onClicked(ad__:)")));
- (void)onClosedAd__:(SharedPopupNativeAdInfo *)ad __attribute__((swift_name("onClosed(ad__:)")));
- (void)onDisplayableAlias:(NSString *)alias __attribute__((swift_name("onDisplayable(alias:)")));
- (void)onDisplayedAd__:(SharedPopupNativeAdInfo *)ad __attribute__((swift_name("onDisplayed(ad__:)")));
- (void)onFailedToLoadError__:(SharedPopupNativeAdError *)error __attribute__((swift_name("onFailedToLoad(error__:)")));
- (void)onLoadedAd__:(SharedPopupNativeAdInfo *)ad __attribute__((swift_name("onLoaded(ad__:)")));
- (void)onOpenedAd__:(SharedPopupNativeAdInfo *)ad __attribute__((swift_name("onOpened(ad__:)")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("PopupNativeAdConfig")))
@interface SharedPopupNativeAdConfig : SharedBase
- (instancetype)initWithIds:(NSArray<NSString *> *)ids layoutName:(NSString *)layoutName timeShowSeconds:(int32_t)timeShowSeconds timeReloadSeconds:(int32_t)timeReloadSeconds xDp:(float)xDp yDp:(float)yDp adWidthDp:(float)adWidthDp adHeightDp:(float)adHeightDp autoClose:(BOOL)autoClose enableCtrOverlay:(BOOL)enableCtrOverlay __attribute__((swift_name("init(ids:layoutName:timeShowSeconds:timeReloadSeconds:xDp:yDp:adWidthDp:adHeightDp:autoClose:enableCtrOverlay:)"))) __attribute__((objc_designated_initializer));
- (SharedPopupNativeAdConfig *)doCopyIds:(NSArray<NSString *> *)ids layoutName:(NSString *)layoutName timeShowSeconds:(int32_t)timeShowSeconds timeReloadSeconds:(int32_t)timeReloadSeconds xDp:(float)xDp yDp:(float)yDp adWidthDp:(float)adWidthDp adHeightDp:(float)adHeightDp autoClose:(BOOL)autoClose enableCtrOverlay:(BOOL)enableCtrOverlay __attribute__((swift_name("doCopy(ids:layoutName:timeShowSeconds:timeReloadSeconds:xDp:yDp:adWidthDp:adHeightDp:autoClose:enableCtrOverlay:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) float adHeightDp __attribute__((swift_name("adHeightDp")));
@property (readonly) float adWidthDp __attribute__((swift_name("adWidthDp")));
@property (readonly) BOOL autoClose __attribute__((swift_name("autoClose")));
@property (readonly) BOOL enableCtrOverlay __attribute__((swift_name("enableCtrOverlay")));
@property (readonly) NSArray<NSString *> *ids __attribute__((swift_name("ids")));
@property (readonly) NSString *layoutName __attribute__((swift_name("layoutName")));
@property (readonly) int32_t timeReloadSeconds __attribute__((swift_name("timeReloadSeconds")));
@property (readonly) int32_t timeShowSeconds __attribute__((swift_name("timeShowSeconds")));
@property (readonly) float xDp __attribute__((swift_name("xDp")));
@property (readonly) float yDp __attribute__((swift_name("yDp")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("PopupNativeAdError")))
@interface SharedPopupNativeAdError : SharedBase
- (instancetype)initWithAlias:(NSString *)alias adUnitId:(NSString *)adUnitId code:(int32_t)code message:(NSString *)message __attribute__((swift_name("init(alias:adUnitId:code:message:)"))) __attribute__((objc_designated_initializer));
- (SharedPopupNativeAdError *)doCopyAlias:(NSString *)alias adUnitId:(NSString *)adUnitId code:(int32_t)code message:(NSString *)message __attribute__((swift_name("doCopy(alias:adUnitId:code:message:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) NSString *adUnitId __attribute__((swift_name("adUnitId")));
@property (readonly) NSString *alias __attribute__((swift_name("alias")));
@property (readonly) int32_t code __attribute__((swift_name("code")));
@property (readonly) NSString *message __attribute__((swift_name("message")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("PopupNativeAdInfo")))
@interface SharedPopupNativeAdInfo : SharedBase
- (instancetype)initWithAlias:(NSString *)alias instanceId:(NSString *)instanceId adUnitId:(NSString *)adUnitId layoutName:(NSString *)layoutName __attribute__((swift_name("init(alias:instanceId:adUnitId:layoutName:)"))) __attribute__((objc_designated_initializer));
- (SharedPopupNativeAdInfo *)doCopyAlias:(NSString *)alias instanceId:(NSString *)instanceId adUnitId:(NSString *)adUnitId layoutName:(NSString *)layoutName __attribute__((swift_name("doCopy(alias:instanceId:adUnitId:layoutName:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) NSString *adUnitId __attribute__((swift_name("adUnitId")));
@property (readonly) NSString *alias __attribute__((swift_name("alias")));
@property (readonly) NSString *instanceId __attribute__((swift_name("instanceId")));
@property (readonly) NSString *layoutName __attribute__((swift_name("layoutName")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("PopupNativeAdLayoutCatalog")))
@interface SharedPopupNativeAdLayoutCatalog : SharedBase
+ (instancetype)alloc __attribute__((unavailable));
+ (instancetype)allocWithZone:(struct _NSZone *)zone __attribute__((unavailable));
+ (instancetype)popupNativeAdLayoutCatalog __attribute__((swift_name("init()")));
@property (class, readonly, getter=shared) SharedPopupNativeAdLayoutCatalog *shared __attribute__((swift_name("shared")));
- (float)minHeightDpLayoutName:(NSString * _Nullable)layoutName __attribute__((swift_name("minHeightDp(layoutName:)")));
- (NSString *)normalizeLayoutName:(NSString * _Nullable)layoutName __attribute__((swift_name("normalize(layoutName:)")));
- (BOOL)usesAdChoicesViewLayoutName:(NSString * _Nullable)layoutName __attribute__((swift_name("usesAdChoicesView(layoutName:)")));
- (BOOL)usesMediaViewLayoutName:(NSString * _Nullable)layoutName __attribute__((swift_name("usesMediaView(layoutName:)")));
@property (readonly) NSArray<NSString *> *All __attribute__((swift_name("All")));
@property (readonly) NSString *Default __attribute__((swift_name("Default")));
@property (readonly) float MinCompactHeightDp __attribute__((swift_name("MinCompactHeightDp")));
@property (readonly) float MinHeightDp __attribute__((swift_name("MinHeightDp")));
@property (readonly) float MinWidthDp __attribute__((swift_name("MinWidthDp")));
@property (readonly) NSString *PopupSingleManual01 __attribute__((swift_name("PopupSingleManual01")));
@property (readonly) NSString *PopupSingleManual02 __attribute__((swift_name("PopupSingleManual02")));
@property (readonly) NSString *PopupSingleManual03 __attribute__((swift_name("PopupSingleManual03")));
@property (readonly) NSString *PopupSingleManual04 __attribute__((swift_name("PopupSingleManual04")));
@property (readonly) NSString *PopupSingleManual05 __attribute__((swift_name("PopupSingleManual05")));
@property (readonly) NSString *PopupSingleManual06 __attribute__((swift_name("PopupSingleManual06")));
@property (readonly) NSString *PopupSingleManual07 __attribute__((swift_name("PopupSingleManual07")));
@property (readonly) NSString *PopupSingleManual08 __attribute__((swift_name("PopupSingleManual08")));
@property (readonly) NSString *PopupSingleManual09 __attribute__((swift_name("PopupSingleManual09")));
@property (readonly) NSString *PopupSingleManual10 __attribute__((swift_name("PopupSingleManual10")));
@property (readonly) NSString *PopupSingleManual11 __attribute__((swift_name("PopupSingleManual11")));
@property (readonly) NSString *PopupSingleManual12 __attribute__((swift_name("PopupSingleManual12")));
@property (readonly) NSString *PopupSingleManual13 __attribute__((swift_name("PopupSingleManual13")));
@property (readonly) NSString *PopupSingleManual14 __attribute__((swift_name("PopupSingleManual14")));
@property (readonly) NSString *PopupSingleManual15 __attribute__((swift_name("PopupSingleManual15")));
@end

__attribute__((swift_name("PopupNativeAdLoader")))
@protocol SharedPopupNativeAdLoader
@required
- (void)destroyInstanceId:(NSString *)instanceId __attribute__((swift_name("destroy(instanceId:)")));
- (void)loadRequest:(SharedPopupNativeAdRequest *)request onStateChanged__:(void (^)(SharedNativeAdState *))onStateChanged __attribute__((swift_name("load(request:onStateChanged__:)")));
- (void)loadWithResultRequest:(SharedPopupNativeAdRequest *)request onResult__:(void (^)(SharedNativeAdLoadResult *))onResult __attribute__((swift_name("loadWithResult(request:onResult__:)")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("PopupNativeAdRequest")))
@interface SharedPopupNativeAdRequest : SharedBase
- (instancetype)initWithInstanceId:(NSString *)instanceId adUnitIds:(NSArray<NSString *> *)adUnitIds __attribute__((swift_name("init(instanceId:adUnitIds:)"))) __attribute__((objc_designated_initializer));
- (SharedPopupNativeAdRequest *)doCopyInstanceId:(NSString *)instanceId adUnitIds:(NSArray<NSString *> *)adUnitIds __attribute__((swift_name("doCopy(instanceId:adUnitIds:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) NSArray<NSString *> *adUnitIds __attribute__((swift_name("adUnitIds")));
@property (readonly) NSString *instanceId __attribute__((swift_name("instanceId")));
@end


/**
 * @note annotations
 *   androidx.compose.runtime.Immutable
*/
__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("PopupNativeAdComposeCallbacks")))
@interface SharedPopupNativeAdComposeCallbacks : SharedBase
- (instancetype)initWithOnMediaClick:(void (^)(void))onMediaClick onIconClick:(void (^)(void))onIconClick onCallToActionClick:(void (^)(void))onCallToActionClick onCloseClick:(void (^)(void))onCloseClick __attribute__((swift_name("init(onMediaClick:onIconClick:onCallToActionClick:onCloseClick:)"))) __attribute__((objc_designated_initializer));
- (SharedPopupNativeAdComposeCallbacks *)doCopyOnMediaClick:(void (^)(void))onMediaClick onIconClick:(void (^)(void))onIconClick onCallToActionClick:(void (^)(void))onCallToActionClick onCloseClick:(void (^)(void))onCloseClick __attribute__((swift_name("doCopy(onMediaClick:onIconClick:onCallToActionClick:onCloseClick:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) void (^onCallToActionClick)(void) __attribute__((swift_name("onCallToActionClick")));
@property (readonly) void (^onCloseClick)(void) __attribute__((swift_name("onCloseClick")));
@property (readonly) void (^onIconClick)(void) __attribute__((swift_name("onIconClick")));
@property (readonly) void (^onMediaClick)(void) __attribute__((swift_name("onMediaClick")));
@end


/**
 * @note annotations
 *   androidx.compose.runtime.Immutable
*/
__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("PopupNativeAdComposeState")))
@interface SharedPopupNativeAdComposeState : SharedBase
- (instancetype)initWithHeadline:(NSString *)headline body:(NSString *)body advertiser:(NSString *)advertiser callToAction:(NSString *)callToAction __attribute__((swift_name("init(headline:body:advertiser:callToAction:)"))) __attribute__((objc_designated_initializer));
- (SharedPopupNativeAdComposeState *)doCopyHeadline:(NSString *)headline body:(NSString *)body advertiser:(NSString *)advertiser callToAction:(NSString *)callToAction __attribute__((swift_name("doCopy(headline:body:advertiser:callToAction:)")));
- (BOOL)isEqual:(id _Nullable)other __attribute__((swift_name("isEqual(_:)")));
- (NSUInteger)hash __attribute__((swift_name("hash()")));
- (NSString *)description __attribute__((swift_name("description()")));
@property (readonly) NSString *advertiser __attribute__((swift_name("advertiser")));
@property (readonly) NSString *body __attribute__((swift_name("body")));
@property (readonly) NSString *callToAction __attribute__((swift_name("callToAction")));
@property (readonly) NSString *headline __attribute__((swift_name("headline")));
@end

@interface SharedNativeAdState (Extensions)
@property (readonly) NSString *consoleLabel __attribute__((swift_name("consoleLabel")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("AdsConsoleUiStateKt")))
@interface SharedAdsConsoleUiStateKt : SharedBase
+ (NSArray<NSString *> *)layoutOptionsForFeature:(SharedAdsConsoleFeature *)feature __attribute__((swift_name("layoutOptionsFor(feature:)")));
+ (NSArray<NSString *> *)layoutOptionsForFeature:(SharedAdsConsoleFeature *)feature fullscreenMode:(SharedFullscreenNativeAdMode *)fullscreenMode __attribute__((swift_name("layoutOptionsFor(feature:fullscreenMode:)")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("BannerNativeAdContractsKt")))
@interface SharedBannerNativeAdContractsKt : SharedBase
@property (class, readonly) NSString *DefaultBannerNativeAdInstanceId __attribute__((swift_name("DefaultBannerNativeAdInstanceId")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("InterstitialAdContractsKt")))
@interface SharedInterstitialAdContractsKt : SharedBase
@property (class, readonly) NSString *DefaultInterstitialAdInstanceId __attribute__((swift_name("DefaultInterstitialAdInstanceId")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("MainViewControllerKt")))
@interface SharedMainViewControllerKt : SharedBase
+ (UIViewController *)MainViewControllerOnOrientationRequested:(void (^)(NSString *))onOrientationRequested __attribute__((swift_name("MainViewController(onOrientationRequested:)")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("NativeAdContractsKt")))
@interface SharedNativeAdContractsKt : SharedBase
@property (class, readonly) NSString *DefaultNativeAdInstanceId __attribute__((swift_name("DefaultNativeAdInstanceId")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("NativeAdView_iosKt")))
@interface SharedNativeAdView_iosKt : SharedBase
+ (NSString *)defaultNativeAdUnitId __attribute__((swift_name("defaultNativeAdUnitId()")));
+ (NSString *)defaultNativeVideoAdUnitId __attribute__((swift_name("defaultNativeVideoAdUnitId()")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("Platform_iosKt")))
@interface SharedPlatform_iosKt : SharedBase
+ (id<SharedPlatform>)getPlatform __attribute__((swift_name("getPlatform()")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("PlatformInterstitialAd_iosKt")))
@interface SharedPlatformInterstitialAd_iosKt : SharedBase
+ (NSString *)defaultInterstitialAdUnitId __attribute__((swift_name("defaultInterstitialAdUnitId()")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("PopupNativeAdContractsKt")))
@interface SharedPopupNativeAdContractsKt : SharedBase
@property (class, readonly) NSString *DefaultPopupNativeAdInstanceId __attribute__((swift_name("DefaultPopupNativeAdInstanceId")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("KotlinEnumCompanion")))
@interface SharedKotlinEnumCompanion : SharedBase
+ (instancetype)alloc __attribute__((unavailable));
+ (instancetype)allocWithZone:(struct _NSZone *)zone __attribute__((unavailable));
+ (instancetype)companion __attribute__((swift_name("init()")));
@property (class, readonly, getter=shared) SharedKotlinEnumCompanion *shared __attribute__((swift_name("shared")));
@end

__attribute__((objc_subclassing_restricted))
__attribute__((swift_name("KotlinArray")))
@interface SharedKotlinArray<T> : SharedBase
+ (instancetype)arrayWithSize:(int32_t)size init:(T _Nullable (^)(SharedInt *))init __attribute__((swift_name("init(size:init:)")));
+ (instancetype)alloc __attribute__((unavailable));
+ (instancetype)allocWithZone:(struct _NSZone *)zone __attribute__((unavailable));
- (T _Nullable)getIndex:(int32_t)index __attribute__((swift_name("get(index:)")));
- (id<SharedKotlinIterator>)iterator __attribute__((swift_name("iterator()")));
- (void)setIndex:(int32_t)index value:(T _Nullable)value __attribute__((swift_name("set(index:value:)")));
@property (readonly) int32_t size __attribute__((swift_name("size")));
@end

__attribute__((swift_name("KotlinIterator")))
@protocol SharedKotlinIterator
@required
- (BOOL)hasNext __attribute__((swift_name("hasNext()")));
- (id _Nullable)next __attribute__((swift_name("next()")));
@end

#pragma pop_macro("_Nullable_result")
#pragma clang diagnostic pop
NS_ASSUME_NONNULL_END
