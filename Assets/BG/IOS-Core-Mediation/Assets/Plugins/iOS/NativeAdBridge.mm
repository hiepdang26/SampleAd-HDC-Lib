#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import <objc/message.h>
#import <objc/runtime.h>
#import <Shared/Shared.h>
#include <math.h>
#include <stdarg.h>
#include <stdlib.h>
#include <string.h>

extern void UnitySendMessage(const char *obj, const char *method, const char *msg);

extern "C" void UnityPause(int pause) __attribute__((weak_import));

static const char *AdsMultiplatformCallbackObject = "AdsMultiplatformCallbacks";
static const char *AdsMultiplatformCallbackMethod = "OnNativeAdEvent";
static NSString *const AdsMultiplatformDefaultPopupInstanceId = @"popup_native_default";
static NSString *const AdsMultiplatformBridgeLogTag = @"[ios-bridge]";

static void AdsMultiplatformBridgeLog(NSString *format, ...) NS_FORMAT_FUNCTION(1, 2);
static void AdsMultiplatformBridgeLog(NSString *format, ...) {
    va_list args;
    va_start(args, format);
    NSString *message = [[NSString alloc] initWithFormat:format arguments:args];
    va_end(args);
    NSLog(@"%@ %@", AdsMultiplatformBridgeLogTag, message ?: @"");
}

static BOOL AdsMultiplatformSharedClassAvailable(NSString *apiName, NSString *className, NSString *instanceId) {
    Class sharedClass = NSClassFromString(className);
    if (sharedClass == Nil) {
        AdsMultiplatformBridgeLog(
            @"api=%@ shared.xcframework unavailable class=%@ instance=%@",
            apiName ?: @"",
            className ?: @"",
            instanceId ?: @""
        );
        return NO;
    }

    AdsMultiplatformBridgeLog(
        @"api=%@ shared.xcframework reachable class=%@ instance=%@",
        apiName ?: @"",
        className ?: @"",
        instanceId ?: @""
    );
    return YES;
}

static void AdsMultiplatformBridgeException(NSString *apiName, NSString *instanceId, NSException *exception) {
    AdsMultiplatformBridgeLog(
        @"api=%@ exception instance=%@ name=%@ reason=%@",
        apiName ?: @"",
        instanceId ?: @"",
        exception.name ?: @"",
        exception.reason ?: @""
    );
}

static void AdsMultiplatformSendEvent(NSString *instanceId, NSString *stateName) {
    NSString *payload = [NSString stringWithFormat:@"%@|%@", instanceId ?: @"", stateName ?: @""];
    UnitySendMessage(AdsMultiplatformCallbackObject, AdsMultiplatformCallbackMethod, payload.UTF8String);
}

typedef void (*AdsMultiplatformAdEventCallback)(const char *payload);

// Set from C#. A direct call runs managed code immediately, even while Unity is paused for a
// fullscreen ad; UnitySendMessage would only be delivered on the next player loop (after close).
static AdsMultiplatformAdEventCallback AdsMultiplatformAdEventCallbackPtr = NULL;

extern "C" void AdsMultiplatform_SetAdEventCallback(AdsMultiplatformAdEventCallback callback) {
    AdsMultiplatformAdEventCallbackPtr = callback;
    AdsMultiplatformBridgeLog(@"api=AdEvents direct callback %@", callback != NULL ? @"registered" : @"cleared");
}

static void AdsMultiplatformDeliverAdEvent(NSString *payload) {
    AdsMultiplatformAdEventCallback callback = AdsMultiplatformAdEventCallbackPtr;
    if (callback == NULL) {
        UnitySendMessage(AdsMultiplatformCallbackObject, AdsMultiplatformCallbackMethod, payload.UTF8String);
        return;
    }
    if ([NSThread isMainThread]) {
        callback(payload.UTF8String);
        return;
    }
    dispatch_async(dispatch_get_main_queue(), ^{
        callback(payload.UTF8String);
    });
}

// Forwards impression / click / paid events from shared.xcframework as "instanceId|Event|json".
// Must run on the main thread; every load entry point calls it before loading.
static void AdsMultiplatformEnsureAdEventListener(void) {
    static BOOL subscribed = NO;
    if (subscribed) {
        return;
    }
    if (NSClassFromString(@"SharedIosAdEventCenter") == Nil) {
        AdsMultiplatformBridgeLog(@"api=AdEvents shared.xcframework unavailable class=SharedIosAdEventCenter");
        return;
    }
    subscribed = YES;
    [SharedIosAdEventCenter.shared subscribeListener:^(SharedIosAdEvent *event) {
        NSString *instanceId = event.instanceId ?: @"";
        NSString *eventName = event.eventName ?: @"";
        NSString *json = [event toUnityJson] ?: @"{}";
        AdsMultiplatformBridgeLog(
            @"api=AdEvents callback instance=%@ event=%@ format=%@ valueMicros=%lld currency=%@",
            instanceId,
            eventName,
            event.format ?: @"",
            event.valueMicros,
            event.currencyCode ?: @""
        );
        NSString *payload = [NSString stringWithFormat:@"%@|%@|%@", instanceId, eventName, json];
        AdsMultiplatformDeliverAdEvent(payload);
    }];
    AdsMultiplatformBridgeLog(@"api=AdEvents subscribed");
}

static NSString *AdsMultiplatformString(const char *value) {
    return value == nullptr ? @"" : [NSString stringWithUTF8String:value];
}

static NSString *AdsMultiplatformStateName(SharedNativeAdState *state) {
    return state == nil ? @"" : state.name;
}

static char *AdsMultiplatformCopyCString(NSString *value, const char *fallback) {
    const char *source = value.UTF8String;
    if (source == NULL) {
        source = fallback == NULL ? "" : fallback;
    }

    size_t length = strlen(source);
    char *copy = (char *)malloc(length + 1);
    if (copy == NULL) {
        return NULL;
    }

    memcpy(copy, source, length + 1);
    return copy;
}

static NSString *AdsMultiplatformPopupInstanceId(NSString *instanceId) {
    return instanceId.length == 0 ? AdsMultiplatformDefaultPopupInstanceId : instanceId;
}

static NSMutableSet<NSString *> *AdsMultiplatformShowingPopupInstances(void) {
    static NSMutableSet<NSString *> *instances;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        instances = [NSMutableSet set];
    });
    return instances;
}

static void AdsMultiplatformMarkPopupShowing(NSString *instanceId) {
    [AdsMultiplatformShowingPopupInstances() addObject:AdsMultiplatformPopupInstanceId(instanceId)];
}

static void AdsMultiplatformClearPopupShowing(NSString *instanceId) {
    [AdsMultiplatformShowingPopupInstances() removeObject:AdsMultiplatformPopupInstanceId(instanceId)];
}

static BOOL AdsMultiplatformPopupBoolSelector(SharedIosPopupNativeAdSdk *sdk, const char *selectorName, NSString *instanceId) {
    SEL selector = sel_registerName(selectorName);
    if (sdk == nil || ![sdk respondsToSelector:selector]) {
        return NO;
    }
    return ((BOOL (*)(id, SEL, NSString *))objc_msgSend)(sdk, selector, AdsMultiplatformPopupInstanceId(instanceId));
}

static NSString *AdsMultiplatformPopupStringSelector(SharedIosPopupNativeAdSdk *sdk, const char *selectorName, NSString *instanceId) {
    SEL selector = sel_registerName(selectorName);
    if (sdk == nil || ![sdk respondsToSelector:selector]) {
        return nil;
    }

    id value = ((id (*)(id, SEL, NSString *))objc_msgSend)(sdk, selector, AdsMultiplatformPopupInstanceId(instanceId));
    if ([value isKindOfClass:NSString.class]) {
        return (NSString *)value;
    }
    SEL nameSelector = sel_registerName("name");
    if (value != nil && [value respondsToSelector:nameSelector]) {
        id name = ((id (*)(id, SEL))objc_msgSend)(value, nameSelector);
        if ([name isKindOfClass:NSString.class]) {
            return (NSString *)name;
        }
    }
    return nil;
}

static NSString *AdsMultiplatformPopupState(SharedIosPopupNativeAdSdk *sdk, NSString *instanceId) {
    NSString *state = AdsMultiplatformPopupStringSelector(sdk, "stateForAlias:", instanceId);
    if (state.length > 0) {
        return state;
    }
    if (AdsMultiplatformPopupBoolSelector(sdk, "isDisplayableAlias:", instanceId)) {
        return @"Displayable";
    }
    if (AdsMultiplatformPopupBoolSelector(sdk, "isReadyAlias:", instanceId)) {
        return @"Loaded";
    }
    return @"NotLoaded";
}

static BOOL AdsMultiplatformIsValidPopupLayout(float xDp, float yDp, float adWidthDp, float adHeightDp) {
    return isfinite(xDp) &&
           isfinite(yDp) &&
           isfinite(adWidthDp) &&
           isfinite(adHeightDp) &&
           adWidthDp > 0.0f &&
           adHeightDp > 0.0f;
}

static NSString *AdsMultiplatformPopupShowBlockReason(
    SharedIosPopupNativeAdSdk *sdk,
    NSString *instanceId,
    UIViewController *presenter
) {
    NSString *safeInstanceId = AdsMultiplatformPopupInstanceId(instanceId);
    if (presenter == nil) {
        return @"missing_presenter";
    }
    if (UIApplication.sharedApplication.applicationState != UIApplicationStateActive) {
        return @"application_not_active";
    }
    if (presenter.view.window == nil) {
        return @"presenter_view_not_attached";
    }
    if (presenter.transitionCoordinator != nil || presenter.isBeingPresented || presenter.isBeingDismissed) {
        return @"presenter_transitioning";
    }

    NSMutableSet<NSString *> *showingInstances = AdsMultiplatformShowingPopupInstances();
    if (showingInstances.count > 0 && ![showingInstances containsObject:safeInstanceId]) {
        return @"another_popup_showing";
    }

    CGRect bounds = presenter.view.bounds;
    if (!isfinite(bounds.size.width) || !isfinite(bounds.size.height) || bounds.size.width <= 0.0 || bounds.size.height <= 0.0) {
        return @"invalid_presenter_bounds";
    }

    if (!AdsMultiplatformPopupBoolSelector(sdk, "isDisplayableAlias:", safeInstanceId)) {
        return [NSString stringWithFormat:@"not_displayable:%@", AdsMultiplatformPopupState(sdk, safeInstanceId)];
    }
    return nil;
}

static UIWindow *AdsMultiplatformActiveWindow(void) {
    UIWindow *candidate = nil;
    if (@available(iOS 13.0, *)) {
        for (UIScene *scene in UIApplication.sharedApplication.connectedScenes) {
            if (![scene isKindOfClass:[UIWindowScene class]]) {
                continue;
            }
            for (UIWindow *window in ((UIWindowScene *)scene).windows) {
                if (candidate == nil) {
                    candidate = window;
                }
                if (window.isKeyWindow) {
                    return window;
                }
            }
        }
    }

    if (UIApplication.sharedApplication.keyWindow != nil) {
        return UIApplication.sharedApplication.keyWindow;
    }
    for (UIWindow *window in UIApplication.sharedApplication.windows) {
        if (candidate == nil) {
            candidate = window;
        }
        if (window.isKeyWindow) {
            return window;
        }
    }
    return candidate;
}

static UIViewController *AdsMultiplatformPresenter(void) {
    UIWindow *window = AdsMultiplatformActiveWindow();
    UIViewController *presenter = window.rootViewController;
    while (presenter.presentedViewController != nil) {
        presenter = presenter.presentedViewController;
    }
    return presenter;
}

static UIViewController *AdsMultiplatformPreparedPresenter(void) {
    UIWindow *window = AdsMultiplatformActiveWindow();
    UIViewController *root = window.rootViewController;
    if (root != nil) {
        [root.view setNeedsLayout];
        [root.view layoutIfNeeded];
    }

    UIViewController *presenter = AdsMultiplatformPresenter();
    if (presenter != nil) {
        [presenter.view setNeedsLayout];
        [presenter.view layoutIfNeeded];
    }
    return presenter;
}

static void AdsMultiplatformRunOnMainSync(dispatch_block_t block) {
    if ([NSThread isMainThread]) {
        block();
        return;
    }
    dispatch_sync(dispatch_get_main_queue(), block);
}

static void AdsMultiplatformPrepareModal(UIViewController *controller) {
    controller.modalPresentationStyle = UIModalPresentationFullScreen;
    controller.modalPresentationCapturesStatusBarAppearance = YES;
    if (@available(iOS 13.0, *)) {
        controller.modalInPresentation = YES;
    }
}

static const NSInteger AdsMultiplatformMaxPresentationWaitAttempts = 600;
static const NSTimeInterval AdsMultiplatformPresentationRetryDelaySeconds = 0.1;

static BOOL AdsMultiplatformRootPresenterIsBusy(void) {
    UIWindow *window = AdsMultiplatformActiveWindow();
    UIViewController *root = window.rootViewController;
    if (root == nil) {
        return YES;
    }
    if (root.presentedViewController != nil) {
        return YES;
    }
    if (root.transitionCoordinator != nil || root.isBeingPresented || root.isBeingDismissed) {
        return YES;
    }
    return NO;
}

static void AdsMultiplatformRunWhenRootPresenterAvailable(
    NSString *instanceId,
    NSInteger attempt,
    dispatch_block_t block
) {
    if (!AdsMultiplatformRootPresenterIsBusy()) {
        block();
        return;
    }

    if (attempt == 0) {
        AdsMultiplatformBridgeLog(
            @"api=ShowFullscreen deferred reason=presenter_busy instance=%@",
            instanceId ?: @""
        );
    }

    if (attempt >= AdsMultiplatformMaxPresentationWaitAttempts) {
        AdsMultiplatformBridgeLog(
            @"api=ShowFullscreen failed reason=presenter_stayed_busy instance=%@",
            instanceId ?: @""
        );
        AdsMultiplatformSendEvent(instanceId, @"Failed");
        return;
    }

    dispatch_time_t retryTime = dispatch_time(
        DISPATCH_TIME_NOW,
        (int64_t)(AdsMultiplatformPresentationRetryDelaySeconds * (double)NSEC_PER_SEC)
    );
    dispatch_after(retryTime, dispatch_get_main_queue(), ^{
        AdsMultiplatformRunWhenRootPresenterAvailable(instanceId, attempt + 1, block);
    });
}

static NSMutableSet<NSString *> *AdsMultiplatformPausedFullscreenInstances(void) {
    static NSMutableSet<NSString *> *instances;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        instances = [NSMutableSet set];
    });
    return instances;
}

static void AdsMultiplatformSetUnityPaused(BOOL paused) {
    if (UnityPause != NULL) {
        UnityPause(paused ? 1 : 0);
        return;
    }

    id controller = UIApplication.sharedApplication.delegate;
    SEL setPausedSelector = sel_registerName("setPaused:");
    if ([controller respondsToSelector:setPausedSelector]) {
        ((void (*)(id, SEL, BOOL))objc_msgSend)(controller, setPausedSelector, paused);
        return;
    }

    SEL pauseSelector = sel_registerName("pause:");
    if ([controller respondsToSelector:pauseSelector]) {
        ((void (*)(id, SEL, BOOL))objc_msgSend)(controller, pauseSelector, paused);
    }
}

static void AdsMultiplatformPauseUnityForFullscreen(NSString *instanceId) {
    NSString *safeInstanceId = instanceId.length == 0 ? @"fullscreen_native_default" : instanceId;
    NSMutableSet<NSString *> *instances = AdsMultiplatformPausedFullscreenInstances();
    if ([instances containsObject:safeInstanceId]) {
        return;
    }

    [instances addObject:safeInstanceId];
    AdsMultiplatformSetUnityPaused(YES);
}

static void AdsMultiplatformResumeUnityForFullscreen(NSString *instanceId) {
    NSString *safeInstanceId = instanceId.length == 0 ? @"fullscreen_native_default" : instanceId;
    NSMutableSet<NSString *> *instances = AdsMultiplatformPausedFullscreenInstances();
    if (![instances containsObject:safeInstanceId]) {
        return;
    }

    [instances removeObject:safeInstanceId];
    if (instances.count == 0) {
        AdsMultiplatformSetUnityPaused(NO);
    }
}

extern "C" {
    void AdsMultiplatform_LoadNativeAd(const char *adUnitId) {
        NSString *unitId = adUnitId == nullptr ? @"" : [NSString stringWithUTF8String:adUnitId];
        NSString *apiName = @"LoadNativeAd";
        AdsMultiplatformBridgeLog(@"api=%@ request adUnitEmpty=%@", apiName, unitId.length == 0 ? @"true" : @"false");
        dispatch_async(dispatch_get_main_queue(), ^{
            if (!AdsMultiplatformSharedClassAvailable(apiName, @"SharedNativeAdIosBridge", @"single")) {
                return;
            }
            AdsMultiplatformEnsureAdEventListener();
            UIViewController *presenter = AdsMultiplatformPresenter();
            if (presenter == nil) {
                AdsMultiplatformBridgeLog(@"api=%@ failed reason=missing_presenter", apiName);
                return;
            }
            @try {
                SharedNativeAdIosBridge *bridge = [[SharedNativeAdIosBridge alloc] init];
                [bridge loadAdUnitId:unitId rootViewController:presenter];
                AdsMultiplatformBridgeLog(@"api=%@ dispatched shared call", apiName);
            } @catch (NSException *exception) {
                AdsMultiplatformBridgeException(apiName, @"single", exception);
            }
        });
    }

    void AdsMultiplatform_LoadFullscreenNativeAdWithReload(
        const char *instanceId,
        const char *adUnitIdsCsv,
        int enableReloadAfterShow
    ) {
        NSString *nativeInstanceId = instanceId == nullptr ? @"" : [NSString stringWithUTF8String:instanceId];
        NSString *unitIds = adUnitIdsCsv == nullptr ? @"" : [NSString stringWithUTF8String:adUnitIdsCsv];
        NSString *apiName = @"LoadFullscreen";
        AdsMultiplatformBridgeLog(
            @"api=%@ request instance=%@ idsEmpty=%@ reload=%@",
            apiName,
            nativeInstanceId,
            unitIds.length == 0 ? @"true" : @"false",
            enableReloadAfterShow != 0 ? @"true" : @"false"
        );
        dispatch_async(dispatch_get_main_queue(), ^{
            if (!AdsMultiplatformSharedClassAvailable(apiName, @"SharedNativeAdIosBridge", nativeInstanceId)) {
                AdsMultiplatformSendEvent(nativeInstanceId, @"Failed");
                return;
            }
            AdsMultiplatformEnsureAdEventListener();
            UIViewController *presenter = AdsMultiplatformPresenter();
            if (presenter == nil) {
                AdsMultiplatformBridgeLog(@"api=%@ failed reason=missing_presenter instance=%@", apiName, nativeInstanceId);
                AdsMultiplatformSendEvent(nativeInstanceId, @"Failed");
                return;
            }
            @try {
                SharedNativeAdIosBridge *bridge = [[SharedNativeAdIosBridge alloc] init];
                [bridge loadFullscreenInstanceId:nativeInstanceId
                                    adUnitIdsCsv:unitIds
                              rootViewController:presenter
                            enableReloadAfterShow:enableReloadAfterShow != 0
                                  onStateChanged:^(SharedNativeAdState *state) {
                    NSString *stateName = AdsMultiplatformStateName(state);
                    AdsMultiplatformBridgeLog(@"api=%@ callback instance=%@ state=%@", apiName, nativeInstanceId, stateName);
                    AdsMultiplatformSendEvent(nativeInstanceId, stateName);
                }];
                AdsMultiplatformBridgeLog(@"api=%@ dispatched shared call instance=%@", apiName, nativeInstanceId);
            } @catch (NSException *exception) {
                AdsMultiplatformBridgeException(apiName, nativeInstanceId, exception);
                AdsMultiplatformSendEvent(nativeInstanceId, @"Failed");
            }
        });
    }

    void AdsMultiplatform_LoadFullscreenNativeAd(const char *instanceId, const char *adUnitIdsCsv) {
        AdsMultiplatform_LoadFullscreenNativeAdWithReload(instanceId, adUnitIdsCsv, 0);
    }

    void AdsMultiplatform_LoadBannerNativeAdWithConfig(
        const char *instanceId,
        const char *adUnitIdsCsv,
        const char *layoutNamesCsv,
        int timeReloadSeconds,
        int timeCountdownSeconds,
        int timeCollapseSeconds
    ) {
        NSString *nativeInstanceId = AdsMultiplatformString(instanceId);
        NSString *unitIds = AdsMultiplatformString(adUnitIdsCsv);
        NSString *layoutNames = AdsMultiplatformString(layoutNamesCsv);
        NSString *apiName = @"LoadBanner";
        AdsMultiplatformBridgeLog(
            @"api=%@ request instance=%@ idsEmpty=%@ layouts=%@ reload=%d",
            apiName,
            nativeInstanceId,
            unitIds.length == 0 ? @"true" : @"false",
            layoutNames,
            timeReloadSeconds
        );
        dispatch_async(dispatch_get_main_queue(), ^{
            if (!AdsMultiplatformSharedClassAvailable(apiName, @"SharedIosBannerNativeAdSdk", nativeInstanceId)) {
                AdsMultiplatformSendEvent(nativeInstanceId, @"Failed");
                return;
            }
            AdsMultiplatformEnsureAdEventListener();
            UIViewController *presenter = AdsMultiplatformPresenter();
            if (presenter == nil) {
                AdsMultiplatformBridgeLog(@"api=%@ failed reason=missing_presenter instance=%@", apiName, nativeInstanceId);
                AdsMultiplatformSendEvent(nativeInstanceId, @"Failed");
                return;
            }
            @try {
                SharedIosBannerNativeAdSdk *sdk = [[SharedIosBannerNativeAdSdk alloc] init];
                [sdk loadWithConfigRootViewController:presenter
                                                alias:nativeInstanceId
                                         adUnitIdsCsv:unitIds
                                       layoutNamesCsv:layoutNames
                                    timeReloadSeconds:timeReloadSeconds
                                 timeCountdownSeconds:timeCountdownSeconds
                                  timeCollapseSeconds:timeCollapseSeconds
                                       onStateChanged:^(SharedNativeAdState *state) {
                    NSString *stateName = AdsMultiplatformStateName(state);
                    AdsMultiplatformBridgeLog(@"api=%@ callback instance=%@ state=%@", apiName, nativeInstanceId, stateName);
                    AdsMultiplatformSendEvent(nativeInstanceId, stateName);
                }];
                AdsMultiplatformBridgeLog(@"api=%@ dispatched shared call instance=%@", apiName, nativeInstanceId);
            } @catch (NSException *exception) {
                AdsMultiplatformBridgeException(apiName, nativeInstanceId, exception);
                AdsMultiplatformSendEvent(nativeInstanceId, @"Failed");
            }
        });
    }

    void AdsMultiplatform_ShowBannerNativeAd(const char *instanceId) {
        NSString *nativeInstanceId = AdsMultiplatformString(instanceId);
        NSString *apiName = @"ShowBanner";
        AdsMultiplatformBridgeLog(@"api=%@ request instance=%@", apiName, nativeInstanceId);
        dispatch_async(dispatch_get_main_queue(), ^{
            if (!AdsMultiplatformSharedClassAvailable(apiName, @"SharedIosBannerNativeAdSdk", nativeInstanceId)) {
                AdsMultiplatformSendEvent(nativeInstanceId, @"Failed");
                return;
            }
            @try {
                SharedIosBannerNativeAdSdk *sdk = [[SharedIosBannerNativeAdSdk alloc] init];
                [sdk showRootViewController:AdsMultiplatformPresenter() alias:nativeInstanceId];
                AdsMultiplatformBridgeLog(@"api=%@ dispatched shared call instance=%@", apiName, nativeInstanceId);
            } @catch (NSException *exception) {
                AdsMultiplatformBridgeException(apiName, nativeInstanceId, exception);
                AdsMultiplatformSendEvent(nativeInstanceId, @"Failed");
            }
        });
    }

    int AdsMultiplatform_ExpandBannerNativeAd(const char *instanceId, int enableClick) {
        NSString *nativeInstanceId = AdsMultiplatformString(instanceId);
        NSString *apiName = @"ExpandBanner";
        __block BOOL result = NO;
        if ([NSThread isMainThread]) {
            if (AdsMultiplatformSharedClassAvailable(apiName, @"SharedIosBannerNativeAdSdk", nativeInstanceId)) {
                @try {
                    SharedIosBannerNativeAdSdk *sdk = [[SharedIosBannerNativeAdSdk alloc] init];
                    result = [sdk expandAlias:nativeInstanceId enableClick:enableClick != 0];
                } @catch (NSException *exception) {
                    AdsMultiplatformBridgeException(apiName, nativeInstanceId, exception);
                }
            }
        } else {
            dispatch_sync(dispatch_get_main_queue(), ^{
                if (AdsMultiplatformSharedClassAvailable(apiName, @"SharedIosBannerNativeAdSdk", nativeInstanceId)) {
                    @try {
                        SharedIosBannerNativeAdSdk *sdk = [[SharedIosBannerNativeAdSdk alloc] init];
                        result = [sdk expandAlias:nativeInstanceId enableClick:enableClick != 0];
                    } @catch (NSException *exception) {
                        AdsMultiplatformBridgeException(apiName, nativeInstanceId, exception);
                    }
                }
            });
        }
        AdsMultiplatformBridgeLog(@"api=%@ result instance=%@ success=%@", apiName, nativeInstanceId, result ? @"true" : @"false");
        return result ? 1 : 0;
    }

    void AdsMultiplatform_CollapseBannerNativeAd(const char *instanceId) {
        NSString *nativeInstanceId = AdsMultiplatformString(instanceId);
        NSString *apiName = @"CollapseBanner";
        AdsMultiplatformBridgeLog(@"api=%@ request instance=%@", apiName, nativeInstanceId);
        dispatch_async(dispatch_get_main_queue(), ^{
            if (!AdsMultiplatformSharedClassAvailable(apiName, @"SharedIosBannerNativeAdSdk", nativeInstanceId)) {
                return;
            }
            @try {
                SharedIosBannerNativeAdSdk *sdk = [[SharedIosBannerNativeAdSdk alloc] init];
                [sdk collapseAlias:nativeInstanceId];
                AdsMultiplatformBridgeLog(@"api=%@ dispatched shared call instance=%@", apiName, nativeInstanceId);
            } @catch (NSException *exception) {
                AdsMultiplatformBridgeException(apiName, nativeInstanceId, exception);
            }
        });
    }

    void AdsMultiplatform_HideBannerNativeAd(const char *instanceId) {
        NSString *nativeInstanceId = AdsMultiplatformString(instanceId);
        NSString *apiName = @"HideBanner";
        AdsMultiplatformBridgeLog(@"api=%@ request instance=%@", apiName, nativeInstanceId);
        dispatch_async(dispatch_get_main_queue(), ^{
            if (!AdsMultiplatformSharedClassAvailable(apiName, @"SharedIosBannerNativeAdSdk", nativeInstanceId)) {
                return;
            }
            @try {
                SharedIosBannerNativeAdSdk *sdk = [[SharedIosBannerNativeAdSdk alloc] init];
                [sdk hideAlias:nativeInstanceId];
                AdsMultiplatformBridgeLog(@"api=%@ dispatched shared call instance=%@", apiName, nativeInstanceId);
            } @catch (NSException *exception) {
                AdsMultiplatformBridgeException(apiName, nativeInstanceId, exception);
            }
        });
    }

    void AdsMultiplatform_DestroyBannerNativeAd(const char *instanceId) {
        NSString *nativeInstanceId = AdsMultiplatformString(instanceId);
        NSString *apiName = @"DestroyBanner";
        AdsMultiplatformBridgeLog(@"api=%@ request instance=%@", apiName, nativeInstanceId);
        dispatch_async(dispatch_get_main_queue(), ^{
            if (!AdsMultiplatformSharedClassAvailable(apiName, @"SharedIosBannerNativeAdSdk", nativeInstanceId)) {
                return;
            }
            @try {
                SharedIosBannerNativeAdSdk *sdk = [[SharedIosBannerNativeAdSdk alloc] init];
                [sdk destroyAlias:nativeInstanceId];
                AdsMultiplatformBridgeLog(@"api=%@ dispatched shared call instance=%@", apiName, nativeInstanceId);
            } @catch (NSException *exception) {
                AdsMultiplatformBridgeException(apiName, nativeInstanceId, exception);
            }
        });
    }

    void AdsMultiplatform_LoadPopupNativeAdWithConfig(
        const char *instanceId,
        const char *adUnitIdsCsv,
        const char *layoutName,
        int timeShowSeconds,
        int timeReloadSeconds,
        float xDp,
        float yDp,
        float adWidthDp,
        float adHeightDp,
        int autoClose,
        int enableCtrOverlay
    ) {
        NSString *nativeInstanceId = AdsMultiplatformPopupInstanceId(AdsMultiplatformString(instanceId));
        NSString *unitIds = AdsMultiplatformString(adUnitIdsCsv);
        NSString *nativeLayoutName = AdsMultiplatformString(layoutName);
        NSString *apiName = @"LoadPopup";
        AdsMultiplatformBridgeLog(
            @"api=%@ request instance=%@ idsEmpty=%@ layout=%@ rect=(%.2f,%.2f,%.2f,%.2f) autoClose=%@ ctrOverlay=%@",
            apiName,
            nativeInstanceId,
            unitIds.length == 0 ? @"true" : @"false",
            nativeLayoutName,
            xDp,
            yDp,
            adWidthDp,
            adHeightDp,
            autoClose != 0 ? @"true" : @"false",
            enableCtrOverlay != 0 ? @"true" : @"false"
        );
        dispatch_async(dispatch_get_main_queue(), ^{
            if (!AdsMultiplatformSharedClassAvailable(apiName, @"SharedIosPopupNativeAdSdk", nativeInstanceId)) {
                AdsMultiplatformSendEvent(nativeInstanceId, @"Failed");
                return;
            }
            AdsMultiplatformEnsureAdEventListener();
            UIViewController *presenter = AdsMultiplatformPresenter();
            if (presenter == nil) {
                AdsMultiplatformBridgeLog(@"api=%@ skipped instance=%@ reason=missing_presenter", apiName, nativeInstanceId);
                AdsMultiplatformSendEvent(nativeInstanceId, @"Failed");
                return;
            }
            if (!AdsMultiplatformIsValidPopupLayout(xDp, yDp, adWidthDp, adHeightDp)) {
                AdsMultiplatformBridgeLog(@"api=%@ skipped instance=%@ reason=invalid_layout", apiName, nativeInstanceId);
                AdsMultiplatformSendEvent(nativeInstanceId, @"Failed");
                return;
            }
            SharedIosPopupNativeAdSdk *sdk = [[SharedIosPopupNativeAdSdk alloc] init];
            @try {
                [sdk loadWithConfigRootViewController:presenter
                                                alias:nativeInstanceId
                                         adUnitIdsCsv:unitIds
                                           layoutName:nativeLayoutName
                                      timeShowSeconds:timeShowSeconds
                                    timeReloadSeconds:timeReloadSeconds
                                                  xDp:xDp
                                                  yDp:yDp
                                            adWidthDp:adWidthDp
                                           adHeightDp:adHeightDp
                                            autoClose:autoClose != 0
                                     enableCtrOverlay:enableCtrOverlay != 0
                                       onStateChanged:^(SharedNativeAdState *state) {
                    NSString *stateName = AdsMultiplatformStateName(state);
                    AdsMultiplatformBridgeLog(@"api=%@ callback instance=%@ state=%@", apiName, nativeInstanceId, stateName);
                    if ([stateName isEqualToString:@"Failed"] || [stateName isEqualToString:@"onClosed"]) {
                        AdsMultiplatformClearPopupShowing(nativeInstanceId);
                    }
                    AdsMultiplatformSendEvent(nativeInstanceId, stateName);
                    if ([stateName isEqualToString:@"Loaded"] &&
                        AdsMultiplatformPopupBoolSelector(sdk, "isDisplayableAlias:", nativeInstanceId)) {
                        AdsMultiplatformBridgeLog(@"api=%@ callback instance=%@ state=Displayable", apiName, nativeInstanceId);
                        AdsMultiplatformSendEvent(nativeInstanceId, @"Displayable");
                    }
                }];
                AdsMultiplatformBridgeLog(@"api=%@ dispatched shared call instance=%@", apiName, nativeInstanceId);
            } @catch (NSException *exception) {
                AdsMultiplatformBridgeException(apiName, nativeInstanceId, exception);
                AdsMultiplatformClearPopupShowing(nativeInstanceId);
                AdsMultiplatformSendEvent(nativeInstanceId, @"Failed");
            }
        });
    }

    void AdsMultiplatform_UpdatePopupNativeAdPlacement(
        const char *instanceId,
        float xDp,
        float yDp,
        float adWidthDp,
        float adHeightDp
    ) {
        NSString *nativeInstanceId = AdsMultiplatformPopupInstanceId(AdsMultiplatformString(instanceId));
        NSString *apiName = @"UpdatePopupPlacement";
        AdsMultiplatformBridgeLog(
            @"api=%@ request instance=%@ rect=(%.2f,%.2f,%.2f,%.2f)",
            apiName,
            nativeInstanceId,
            xDp,
            yDp,
            adWidthDp,
            adHeightDp
        );
        dispatch_async(dispatch_get_main_queue(), ^{
            if (!AdsMultiplatformSharedClassAvailable(apiName, @"SharedIosPopupNativeAdSdk", nativeInstanceId)) {
                AdsMultiplatformSendEvent(nativeInstanceId, @"Failed");
                return;
            }
            if (!AdsMultiplatformIsValidPopupLayout(xDp, yDp, adWidthDp, adHeightDp)) {
                AdsMultiplatformBridgeLog(@"api=%@ skipped instance=%@ reason=invalid_layout", apiName, nativeInstanceId);
                AdsMultiplatformSendEvent(nativeInstanceId, @"Failed");
                return;
            }
            SharedIosPopupNativeAdSdk *sdk = [[SharedIosPopupNativeAdSdk alloc] init];
            @try {
                [sdk updatePlacementAlias:nativeInstanceId
	                                      xDp:xDp
	                                      yDp:yDp
	                                adWidthDp:adWidthDp
	                               adHeightDp:adHeightDp];
                AdsMultiplatformBridgeLog(@"api=%@ dispatched shared call instance=%@", apiName, nativeInstanceId);
            } @catch (NSException *exception) {
                AdsMultiplatformBridgeException(apiName, nativeInstanceId, exception);
                AdsMultiplatformSendEvent(nativeInstanceId, @"Failed");
            }
        });
    }

    void AdsMultiplatform_ShowPopupNativeAd(const char *instanceId) {
        NSString *nativeInstanceId = AdsMultiplatformPopupInstanceId(AdsMultiplatformString(instanceId));
        NSString *apiName = @"ShowPopup";
        AdsMultiplatformBridgeLog(@"api=%@ request instance=%@", apiName, nativeInstanceId);
        dispatch_async(dispatch_get_main_queue(), ^{
            if (!AdsMultiplatformSharedClassAvailable(apiName, @"SharedIosPopupNativeAdSdk", nativeInstanceId)) {
                AdsMultiplatformSendEvent(nativeInstanceId, @"Failed");
                AdsMultiplatformSendEvent(nativeInstanceId, @"onClosed");
                return;
            }
            SharedIosPopupNativeAdSdk *sdk = [[SharedIosPopupNativeAdSdk alloc] init];
            UIViewController *presenter = AdsMultiplatformPresenter();
            NSString *blockReason = AdsMultiplatformPopupShowBlockReason(sdk, nativeInstanceId, presenter);
            if (blockReason.length > 0) {
                AdsMultiplatformBridgeLog(@"api=%@ skipped instance=%@ reason=%@", apiName, nativeInstanceId, blockReason);
                AdsMultiplatformSendEvent(nativeInstanceId, @"onClosed");
                return;
            }
            @try {
                AdsMultiplatformMarkPopupShowing(nativeInstanceId);
                [sdk showRootViewController:presenter alias:nativeInstanceId];
                AdsMultiplatformBridgeLog(@"api=%@ dispatched shared call instance=%@", apiName, nativeInstanceId);
            } @catch (NSException *exception) {
                AdsMultiplatformBridgeException(apiName, nativeInstanceId, exception);
                AdsMultiplatformClearPopupShowing(nativeInstanceId);
                AdsMultiplatformSendEvent(nativeInstanceId, @"Failed");
                AdsMultiplatformSendEvent(nativeInstanceId, @"onClosed");
            }
        });
    }

    void AdsMultiplatform_ClosePopupNativeAd(const char *instanceId) {
        NSString *nativeInstanceId = AdsMultiplatformPopupInstanceId(AdsMultiplatformString(instanceId));
        NSString *apiName = @"ClosePopup";
        AdsMultiplatformBridgeLog(@"api=%@ request instance=%@", apiName, nativeInstanceId);
        dispatch_async(dispatch_get_main_queue(), ^{
            if (!AdsMultiplatformSharedClassAvailable(apiName, @"SharedIosPopupNativeAdSdk", nativeInstanceId)) {
                AdsMultiplatformSendEvent(nativeInstanceId, @"onClosed");
                return;
            }
            SharedIosPopupNativeAdSdk *sdk = [[SharedIosPopupNativeAdSdk alloc] init];
            @try {
                [sdk closeAlias:nativeInstanceId];
                AdsMultiplatformClearPopupShowing(nativeInstanceId);
                AdsMultiplatformBridgeLog(@"api=%@ dispatched shared call instance=%@", apiName, nativeInstanceId);
            } @catch (NSException *exception) {
                AdsMultiplatformBridgeException(apiName, nativeInstanceId, exception);
                AdsMultiplatformClearPopupShowing(nativeInstanceId);
                AdsMultiplatformSendEvent(nativeInstanceId, @"onClosed");
            }
        });
    }

    void AdsMultiplatform_HidePopupNativeAd(const char *instanceId) {
        NSString *nativeInstanceId = AdsMultiplatformPopupInstanceId(AdsMultiplatformString(instanceId));
        NSString *apiName = @"HidePopup";
        AdsMultiplatformBridgeLog(@"api=%@ request instance=%@", apiName, nativeInstanceId);
        dispatch_async(dispatch_get_main_queue(), ^{
            if (!AdsMultiplatformSharedClassAvailable(apiName, @"SharedIosPopupNativeAdSdk", nativeInstanceId)) {
                AdsMultiplatformSendEvent(nativeInstanceId, @"onClosed");
                return;
            }
            SharedIosPopupNativeAdSdk *sdk = [[SharedIosPopupNativeAdSdk alloc] init];
            @try {
                [sdk hideAlias:nativeInstanceId];
                AdsMultiplatformClearPopupShowing(nativeInstanceId);
                AdsMultiplatformBridgeLog(@"api=%@ dispatched shared call instance=%@", apiName, nativeInstanceId);
            } @catch (NSException *exception) {
                AdsMultiplatformBridgeException(apiName, nativeInstanceId, exception);
                AdsMultiplatformClearPopupShowing(nativeInstanceId);
                AdsMultiplatformSendEvent(nativeInstanceId, @"onClosed");
            }
        });
    }

    void AdsMultiplatform_StopPopupNativeAd(const char *instanceId) {
        NSString *nativeInstanceId = AdsMultiplatformPopupInstanceId(AdsMultiplatformString(instanceId));
        NSString *apiName = @"StopPopup";
        AdsMultiplatformBridgeLog(@"api=%@ request instance=%@", apiName, nativeInstanceId);
        dispatch_async(dispatch_get_main_queue(), ^{
            if (!AdsMultiplatformSharedClassAvailable(apiName, @"SharedIosPopupNativeAdSdk", nativeInstanceId)) {
                AdsMultiplatformSendEvent(nativeInstanceId, @"onClosed");
                return;
            }
            SharedIosPopupNativeAdSdk *sdk = [[SharedIosPopupNativeAdSdk alloc] init];
            @try {
                [sdk stopAlias:nativeInstanceId];
                AdsMultiplatformClearPopupShowing(nativeInstanceId);
                AdsMultiplatformBridgeLog(@"api=%@ dispatched shared call instance=%@", apiName, nativeInstanceId);
            } @catch (NSException *exception) {
                AdsMultiplatformBridgeException(apiName, nativeInstanceId, exception);
                AdsMultiplatformClearPopupShowing(nativeInstanceId);
                AdsMultiplatformSendEvent(nativeInstanceId, @"onClosed");
            }
        });
    }

    void AdsMultiplatform_DestroyPopupNativeAd(const char *instanceId) {
        NSString *nativeInstanceId = AdsMultiplatformPopupInstanceId(AdsMultiplatformString(instanceId));
        NSString *apiName = @"DestroyPopup";
        AdsMultiplatformBridgeLog(@"api=%@ request instance=%@", apiName, nativeInstanceId);
        dispatch_async(dispatch_get_main_queue(), ^{
            if (!AdsMultiplatformSharedClassAvailable(apiName, @"SharedIosPopupNativeAdSdk", nativeInstanceId)) {
                AdsMultiplatformSendEvent(nativeInstanceId, @"onClosed");
                return;
            }
            SharedIosPopupNativeAdSdk *sdk = [[SharedIosPopupNativeAdSdk alloc] init];
            @try {
                [sdk destroyAlias:nativeInstanceId];
                AdsMultiplatformClearPopupShowing(nativeInstanceId);
                AdsMultiplatformBridgeLog(@"api=%@ dispatched shared call instance=%@", apiName, nativeInstanceId);
            } @catch (NSException *exception) {
                AdsMultiplatformBridgeException(apiName, nativeInstanceId, exception);
                AdsMultiplatformClearPopupShowing(nativeInstanceId);
                AdsMultiplatformSendEvent(nativeInstanceId, @"onClosed");
            }
        });
    }

    int AdsMultiplatform_IsPopupNativeAdReady(const char *instanceId) {
        NSString *nativeInstanceId = AdsMultiplatformPopupInstanceId(AdsMultiplatformString(instanceId));
        NSString *apiName = @"IsPopupReady";
        __block BOOL result = NO;
        AdsMultiplatformRunOnMainSync(^{
            if (!AdsMultiplatformSharedClassAvailable(apiName, @"SharedIosPopupNativeAdSdk", nativeInstanceId)) {
                return;
            }
            SharedIosPopupNativeAdSdk *sdk = [[SharedIosPopupNativeAdSdk alloc] init];
            result = AdsMultiplatformPopupBoolSelector(sdk, "isReadyAlias:", nativeInstanceId);
        });
        AdsMultiplatformBridgeLog(@"api=%@ result instance=%@ success=%@", apiName, nativeInstanceId, result ? @"true" : @"false");
        return result ? 1 : 0;
    }

    int AdsMultiplatform_IsPopupNativeAdDisplayable(const char *instanceId) {
        NSString *nativeInstanceId = AdsMultiplatformPopupInstanceId(AdsMultiplatformString(instanceId));
        NSString *apiName = @"IsPopupDisplayable";
        __block BOOL result = NO;
        AdsMultiplatformRunOnMainSync(^{
            if (!AdsMultiplatformSharedClassAvailable(apiName, @"SharedIosPopupNativeAdSdk", nativeInstanceId)) {
                return;
            }
            SharedIosPopupNativeAdSdk *sdk = [[SharedIosPopupNativeAdSdk alloc] init];
            result = AdsMultiplatformPopupBoolSelector(sdk, "isDisplayableAlias:", nativeInstanceId);
        });
        AdsMultiplatformBridgeLog(@"api=%@ result instance=%@ success=%@", apiName, nativeInstanceId, result ? @"true" : @"false");
        return result ? 1 : 0;
    }

    void AdsMultiplatform_FreeCString(const char *value) {
        if (value != NULL) {
            free((void *)value);
        }
    }

    const char *AdsMultiplatform_PopupNativeAdStateFor(const char *instanceId) {
        NSString *nativeInstanceId = AdsMultiplatformPopupInstanceId(AdsMultiplatformString(instanceId));
        NSString *apiName = @"PopupStateFor";
        __block NSString *state = @"NotLoaded";
        AdsMultiplatformRunOnMainSync(^{
            if (!AdsMultiplatformSharedClassAvailable(apiName, @"SharedIosPopupNativeAdSdk", nativeInstanceId)) {
                return;
            }
            SharedIosPopupNativeAdSdk *sdk = [[SharedIosPopupNativeAdSdk alloc] init];
            state = AdsMultiplatformPopupState(sdk, nativeInstanceId);
        });
        AdsMultiplatformBridgeLog(@"api=%@ result instance=%@ state=%@", apiName, nativeInstanceId, state);

        return AdsMultiplatformCopyCString(state, "NotLoaded");
    }

    void AdsMultiplatform_ShowNativeAd(const char *adUnitId) {
        NSString *unitId = adUnitId == nullptr ? @"" : [NSString stringWithUTF8String:adUnitId];
        NSString *apiName = @"ShowNativeAd";
        AdsMultiplatformBridgeLog(@"api=%@ request adUnitEmpty=%@", apiName, unitId.length == 0 ? @"true" : @"false");
        dispatch_async(dispatch_get_main_queue(), ^{
            if (!AdsMultiplatformSharedClassAvailable(apiName, @"SharedNativeAdIosBridge", @"single")) {
                return;
            }
            @try {
                SharedNativeAdIosBridge *bridge = [[SharedNativeAdIosBridge alloc] init];
                UIViewController *controller = [bridge viewControllerAdUnitId:unitId];
                AdsMultiplatformPrepareModal(controller);
                [AdsMultiplatformPresenter() presentViewController:controller animated:YES completion:nil];
                AdsMultiplatformBridgeLog(@"api=%@ dispatched shared call", apiName);
            } @catch (NSException *exception) {
                AdsMultiplatformBridgeException(apiName, @"single", exception);
            }
        });
    }

    void AdsMultiplatform_ShowFullscreenNativeAd(const char *instanceId, const char *layoutName, double durationSeconds) {
        NSString *nativeInstanceId = instanceId == nullptr ? @"" : [NSString stringWithUTF8String:instanceId];
        NSString *nativeLayoutName = layoutName == nullptr ? @"" : [NSString stringWithUTF8String:layoutName];
        NSString *apiName = @"ShowFullscreen";
        AdsMultiplatformBridgeLog(
            @"api=%@ request instance=%@ layout=%@ duration=%.2f unityPause=shared",
            apiName,
            nativeInstanceId,
            nativeLayoutName,
            durationSeconds
        );
        dispatch_async(dispatch_get_main_queue(), ^{
            AdsMultiplatformRunWhenRootPresenterAvailable(nativeInstanceId, 0, ^{
                if (!AdsMultiplatformSharedClassAvailable(apiName, @"SharedFullscreenNativeAdRegistry", nativeInstanceId) ||
                    !AdsMultiplatformSharedClassAvailable(apiName, @"SharedNativeAdIosBridge", nativeInstanceId)) {
                    AdsMultiplatformSendEvent(nativeInstanceId, @"Failed");
                    return;
                }
                if (![SharedFullscreenNativeAdRegistry.shared isReadyAlias:nativeInstanceId]) {
                    AdsMultiplatformBridgeLog(@"api=%@ skipped instance=%@ reason=not_ready", apiName, nativeInstanceId);
                    AdsMultiplatformSendEvent(nativeInstanceId, @"Failed");
                    return;
                }
                @try {
                    SharedNativeAdIosBridge *bridge = [[SharedNativeAdIosBridge alloc] init];
                    UIViewController *controller = [bridge fullscreenViewControllerForUnityInstanceId:nativeInstanceId
                                                                                            layoutName:nativeLayoutName
                                                                                      durationSeconds:durationSeconds
                                                                                     fallbackAdUnitId:@""
                                                                                             onClosed:^{
                        AdsMultiplatformBridgeLog(@"api=%@ callback instance=%@ state=onClosed", apiName, nativeInstanceId);
                        AdsMultiplatformSendEvent(nativeInstanceId, @"onClosed");
                    }];
                    AdsMultiplatformPrepareModal(controller);
                    AdsMultiplatformBridgeLog(@"api=%@ unity pause delegated to shared.xcframework instance=%@", apiName, nativeInstanceId);
                    AdsMultiplatformSendEvent(nativeInstanceId, @"Shown");
                    [AdsMultiplatformPresenter() presentViewController:controller animated:YES completion:nil];
                    AdsMultiplatformBridgeLog(@"api=%@ dispatched shared call instance=%@", apiName, nativeInstanceId);
                } @catch (NSException *exception) {
                    AdsMultiplatformBridgeException(apiName, nativeInstanceId, exception);
                    AdsMultiplatformSendEvent(nativeInstanceId, @"Failed");
                }
            });
        });
    }

    void AdsMultiplatform_ShowFullscreenNativeAdWithOptions(
        const char *instanceId,
        const char *mode,
        const char *layoutNamesCsv,
        double durationSeconds,
        const char *durationsSecondsCsv,
        const char *orientation,
        int autoClose,
        int pauseGameplay,
        int enableAdComeback,
        int showTCD,
        double delaySeconds,
        int timeUpCSeconds,
        const char *fallbackAdUnitId,
        int cta,
        int headline,
        int body,
        int description,
        int icon,
        int advertiser,
        int media,
        int mediaImage,
        int mediaVideo
    ) {
        NSString *nativeInstanceId = instanceId == nullptr ? @"" : [NSString stringWithUTF8String:instanceId];
        NSString *nativeMode = mode == nullptr ? @"" : [NSString stringWithUTF8String:mode];
        NSString *nativeLayoutNamesCsv = layoutNamesCsv == nullptr ? @"" : [NSString stringWithUTF8String:layoutNamesCsv];
        NSString *nativeDurationsSecondsCsv = durationsSecondsCsv == nullptr ? @"" : [NSString stringWithUTF8String:durationsSecondsCsv];
        NSString *nativeOrientation = orientation == nullptr ? @"auto" : [NSString stringWithUTF8String:orientation];
        NSString *nativeFallbackAdUnitId = fallbackAdUnitId == nullptr ? @"" : [NSString stringWithUTF8String:fallbackAdUnitId];
        NSString *apiName = @"ShowFullscreenWithOptions";
        AdsMultiplatformBridgeLog(
            @"api=%@ request instance=%@ mode=%@ layouts=%@ duration=%.2f orientation=%@ pauseGameplay=%@ enableAdComeback=%@ unityPause=shared",
            apiName,
            nativeInstanceId,
            nativeMode,
            nativeLayoutNamesCsv,
            durationSeconds,
            nativeOrientation,
            pauseGameplay != 0 ? @"true" : @"false",
            enableAdComeback != 0 ? @"true" : @"false"
        );
        dispatch_async(dispatch_get_main_queue(), ^{
            AdsMultiplatformRunWhenRootPresenterAvailable(nativeInstanceId, 0, ^{
                if (!AdsMultiplatformSharedClassAvailable(apiName, @"SharedFullscreenNativeAdRegistry", nativeInstanceId) ||
                    !AdsMultiplatformSharedClassAvailable(apiName, @"SharedNativeAdIosBridge", nativeInstanceId)) {
                    AdsMultiplatformSendEvent(nativeInstanceId, @"Failed");
                    return;
                }
                if (![SharedFullscreenNativeAdRegistry.shared isReadyAlias:nativeInstanceId]) {
                    AdsMultiplatformBridgeLog(@"api=%@ skipped instance=%@ reason=not_ready", apiName, nativeInstanceId);
                    AdsMultiplatformSendEvent(nativeInstanceId, @"Failed");
                    return;
                }
                @try {
                    SharedNativeAdIosBridge *bridge = [[SharedNativeAdIosBridge alloc] init];
                    UIViewController *controller = [bridge fullscreenViewControllerForUnityWithOptionsInstanceId:nativeInstanceId
                                                                                                            mode:nativeMode
                                                                                                  layoutNamesCsv:nativeLayoutNamesCsv
                                                                                                 durationSeconds:durationSeconds
                                                                                              durationsSecondsCsv:nativeDurationsSecondsCsv
                                                                                                     orientation:nativeOrientation
                                                                                                       autoClose:autoClose != 0
                                                                                                   pauseGameplay:pauseGameplay != 0
                                                                                               enableAdComeback:enableAdComeback != 0
                                                                                                          showTCD:showTCD != 0
                                                                                                     delaySeconds:delaySeconds
                                                                                                  timeUpCSeconds:timeUpCSeconds
                                                                                                 fallbackAdUnitId:nativeFallbackAdUnitId
                                                                                                              cta:cta != 0
                                                                                                         headline:headline != 0
                                                                                                             body:body != 0
                                                                                                      description:description != 0
                                                                                                             icon:icon != 0
                                                                                                       advertiser:advertiser != 0
                                                                                                            media:media != 0
                                                                                                        mediaImage:mediaImage != 0
                                                                                                        mediaVideo:mediaVideo != 0
                                                                                                        onClosed:^{
                        AdsMultiplatformBridgeLog(@"api=%@ callback instance=%@ state=onClosed", apiName, nativeInstanceId);
                        AdsMultiplatformSendEvent(nativeInstanceId, @"onClosed");
                    }];
                    AdsMultiplatformPrepareModal(controller);
                    AdsMultiplatformBridgeLog(@"api=%@ unity pause delegated to shared.xcframework instance=%@", apiName, nativeInstanceId);
                    AdsMultiplatformSendEvent(nativeInstanceId, @"Shown");
                    [AdsMultiplatformPresenter() presentViewController:controller animated:YES completion:nil];
                    AdsMultiplatformBridgeLog(@"api=%@ dispatched shared call instance=%@", apiName, nativeInstanceId);
                } @catch (NSException *exception) {
                    AdsMultiplatformBridgeException(apiName, nativeInstanceId, exception);
                    AdsMultiplatformSendEvent(nativeInstanceId, @"Failed");
                }
            });
        });
    }

    void AdsMultiplatform_HideNativeAd(void) {
        NSString *apiName = @"HideNativeAd";
        AdsMultiplatformBridgeLog(@"api=%@ request", apiName);
        dispatch_async(dispatch_get_main_queue(), ^{
            [AdsMultiplatformPresenter() dismissViewControllerAnimated:YES completion:nil];
            AdsMultiplatformBridgeLog(@"api=%@ dispatched presenter dismiss", apiName);
        });
    }

    void AdsMultiplatform_HideFullscreenNativeAd(const char *instanceId) {
        NSString *nativeInstanceId = instanceId == nullptr ? @"" : [NSString stringWithUTF8String:instanceId];
        NSString *apiName = @"HideFullscreen";
        AdsMultiplatformBridgeLog(@"api=%@ request instance=%@", apiName, nativeInstanceId);
        dispatch_async(dispatch_get_main_queue(), ^{
            AdsMultiplatformResumeUnityForFullscreen(nativeInstanceId);
            AdsMultiplatformSendEvent(nativeInstanceId, @"onClosed");
            [AdsMultiplatformPresenter() dismissViewControllerAnimated:YES completion:nil];
            AdsMultiplatformBridgeLog(@"api=%@ dispatched presenter dismiss instance=%@", apiName, nativeInstanceId);
        });
    }

    void AdsMultiplatform_DestroyFullscreenNativeAd(const char *instanceId) {
        NSString *nativeInstanceId = instanceId == nullptr ? @"" : [NSString stringWithUTF8String:instanceId];
        NSString *apiName = @"DestroyFullscreen";
        AdsMultiplatformBridgeLog(@"api=%@ request instance=%@", apiName, nativeInstanceId);
        dispatch_async(dispatch_get_main_queue(), ^{
            if (!AdsMultiplatformSharedClassAvailable(apiName, @"SharedIosFullscreenNativeAdSdk", nativeInstanceId)) {
                return;
            }
            @try {
                SharedIosFullscreenNativeAdSdk *sdk = [[SharedIosFullscreenNativeAdSdk alloc] init];
                [sdk destroyAlias:nativeInstanceId];
                AdsMultiplatformBridgeLog(@"api=%@ dispatched shared call instance=%@", apiName, nativeInstanceId);
            } @catch (NSException *exception) {
                AdsMultiplatformBridgeException(apiName, nativeInstanceId, exception);
            }
        });
    }

    int AdsMultiplatform_CreateInterstitial(const char *instanceId, const char *configJson) {
        NSString *nativeInstanceId = instanceId == nullptr ? @"interstitial" : [NSString stringWithUTF8String:instanceId];
        NSString *nativeConfigJson = configJson == nullptr ? @"" : [NSString stringWithUTF8String:configJson];
        NSString *apiName = @"CreateInterstitial";
        __block BOOL result = NO;
        AdsMultiplatformRunOnMainSync(^{
            if (!AdsMultiplatformSharedClassAvailable(apiName, @"SharedNativeAdIosBridge", nativeInstanceId)) {
                return;
            }
            AdsMultiplatformEnsureAdEventListener();
            @try {
                SharedNativeAdIosBridge *bridge = [[SharedNativeAdIosBridge alloc] init];
                result = [bridge createInterstitialAlias:nativeInstanceId configJson:nativeConfigJson];
            } @catch (NSException *exception) {
                AdsMultiplatformBridgeException(apiName, nativeInstanceId, exception);
            }
        });
        AdsMultiplatformBridgeLog(@"api=%@ result instance=%@ success=%@", apiName, nativeInstanceId, result ? @"true" : @"false");
        return result ? 1 : 0;
    }

    int AdsMultiplatform_LoadInterstitialWithConfig(
        const char *instanceId,
        const char *adUnitIdsCsv,
        int preloadBufferSize,
        int autoReload
    ) {
        NSString *nativeInstanceId = instanceId == nullptr ? @"interstitial" : [NSString stringWithUTF8String:instanceId];
        NSString *unitIds = adUnitIdsCsv == nullptr ? @"" : [NSString stringWithUTF8String:adUnitIdsCsv];
        NSString *apiName = @"LoadInterstitial";
        __block BOOL result = NO;
        AdsMultiplatformRunOnMainSync(^{
            if (!AdsMultiplatformSharedClassAvailable(apiName, @"SharedIosInterstitialAdSdk", nativeInstanceId)) {
                AdsMultiplatformSendEvent(nativeInstanceId, @"Failed");
                return;
            }
            AdsMultiplatformEnsureAdEventListener();
            UIViewController *presenter = AdsMultiplatformPreparedPresenter();
            if (presenter == nil) {
                AdsMultiplatformBridgeLog(@"api=%@ failed reason=missing_presenter instance=%@", apiName, nativeInstanceId);
                AdsMultiplatformSendEvent(nativeInstanceId, @"Failed");
                return;
            }
            @try {
                SharedIosInterstitialAdSdk *sdk = [[SharedIosInterstitialAdSdk alloc] init];
                [sdk loadWithConfigRootViewController:presenter
                                               alias:nativeInstanceId
                                        adUnitIdsCsv:unitIds
                                   preloadBufferSize:preloadBufferSize < 1 ? 1 : preloadBufferSize
                                          autoReload:autoReload != 0
                                      onStateChanged:^(SharedNativeAdState *state) {
                    NSString *stateName = AdsMultiplatformStateName(state);
                    AdsMultiplatformBridgeLog(@"api=%@ callback instance=%@ state=%@", apiName, nativeInstanceId, stateName);
                    AdsMultiplatformSendEvent(nativeInstanceId, stateName);
                }];
                result = YES;
            } @catch (NSException *exception) {
                AdsMultiplatformBridgeException(apiName, nativeInstanceId, exception);
                AdsMultiplatformSendEvent(nativeInstanceId, @"Failed");
            }
        });
        AdsMultiplatformBridgeLog(@"api=%@ result instance=%@ success=%@", apiName, nativeInstanceId, result ? @"true" : @"false");
        return result ? 1 : 0;
    }

    int AdsMultiplatform_LoadInterstitial(const char *instanceId, int bufferSize) {
        NSString *nativeInstanceId = instanceId == nullptr ? @"interstitial" : [NSString stringWithUTF8String:instanceId];
        NSString *apiName = @"LoadInterstitialLegacy";
        __block BOOL result = NO;
        AdsMultiplatformRunOnMainSync(^{
            if (!AdsMultiplatformSharedClassAvailable(apiName, @"SharedNativeAdIosBridge", nativeInstanceId)) {
                AdsMultiplatformSendEvent(nativeInstanceId, @"Failed");
                return;
            }
            AdsMultiplatformEnsureAdEventListener();
            @try {
                SharedNativeAdIosBridge *bridge = [[SharedNativeAdIosBridge alloc] init];
                result = [bridge loadInterstitialRootViewController:AdsMultiplatformPreparedPresenter()
                                                              alias:nativeInstanceId
                                                         bufferSize:bufferSize < 1 ? 1 : bufferSize];
            } @catch (NSException *exception) {
                AdsMultiplatformBridgeException(apiName, nativeInstanceId, exception);
                AdsMultiplatformSendEvent(nativeInstanceId, @"Failed");
            }
        });
        AdsMultiplatformBridgeLog(@"api=%@ result instance=%@ success=%@", apiName, nativeInstanceId, result ? @"true" : @"false");
        return result ? 1 : 0;
    }

    int AdsMultiplatform_ShowInterstitial(const char *instanceId, const char *optionsJson) {
        NSString *nativeInstanceId = instanceId == nullptr ? @"interstitial" : [NSString stringWithUTF8String:instanceId];
        NSString *nativeOptionsJson = optionsJson == nullptr ? nil : [NSString stringWithUTF8String:optionsJson];
        NSString *apiName = @"ShowInterstitial";
        __block BOOL result = NO;
        AdsMultiplatformRunOnMainSync(^{
            if (!AdsMultiplatformSharedClassAvailable(apiName, @"SharedNativeAdIosBridge", nativeInstanceId)) {
                AdsMultiplatformSendEvent(nativeInstanceId, @"Failed");
                return;
            }
            @try {
                SharedNativeAdIosBridge *bridge = [[SharedNativeAdIosBridge alloc] init];
                AdsMultiplatformBridgeLog(@"api=%@ unity pause delegated to shared.xcframework instance=%@", apiName, nativeInstanceId);
                result = [bridge showInterstitialRootViewController:AdsMultiplatformPreparedPresenter()
                                                              alias:nativeInstanceId
                                                        optionsJson:nativeOptionsJson];
            } @catch (NSException *exception) {
                AdsMultiplatformBridgeException(apiName, nativeInstanceId, exception);
                AdsMultiplatformSendEvent(nativeInstanceId, @"Failed");
            }
        });
        AdsMultiplatformBridgeLog(@"api=%@ result instance=%@ success=%@", apiName, nativeInstanceId, result ? @"true" : @"false");
        return result ? 1 : 0;
    }

    int AdsMultiplatform_DestroyInterstitial(const char *instanceId) {
        NSString *nativeInstanceId = instanceId == nullptr ? @"interstitial" : [NSString stringWithUTF8String:instanceId];
        NSString *apiName = @"DestroyInterstitial";
        __block BOOL result = NO;
        AdsMultiplatformRunOnMainSync(^{
            if (!AdsMultiplatformSharedClassAvailable(apiName, @"SharedNativeAdIosBridge", nativeInstanceId)) {
                AdsMultiplatformSendEvent(nativeInstanceId, @"onClosed");
                return;
            }
            @try {
                SharedNativeAdIosBridge *bridge = [[SharedNativeAdIosBridge alloc] init];
                AdsMultiplatformBridgeLog(@"api=%@ unity resume delegated to shared.xcframework instance=%@", apiName, nativeInstanceId);
                result = [bridge destroyAlias:nativeInstanceId];
            } @catch (NSException *exception) {
                AdsMultiplatformBridgeException(apiName, nativeInstanceId, exception);
            }
        });
        AdsMultiplatformBridgeLog(@"api=%@ result instance=%@ success=%@", apiName, nativeInstanceId, result ? @"true" : @"false");
        return result ? 1 : 0;
    }

    int AdsMultiplatform_IsInterstitialReady(const char *instanceId) {
        NSString *nativeInstanceId = instanceId == nullptr ? @"interstitial" : [NSString stringWithUTF8String:instanceId];
        NSString *apiName = @"IsInterstitialReady";
        __block BOOL result = NO;
        AdsMultiplatformRunOnMainSync(^{
            if (!AdsMultiplatformSharedClassAvailable(apiName, @"SharedNativeAdIosBridge", nativeInstanceId)) {
                return;
            }
            @try {
                SharedNativeAdIosBridge *bridge = [[SharedNativeAdIosBridge alloc] init];
                result = [bridge isReadyAlias:nativeInstanceId];
            } @catch (NSException *exception) {
                AdsMultiplatformBridgeException(apiName, nativeInstanceId, exception);
            }
        });
        AdsMultiplatformBridgeLog(@"api=%@ result instance=%@ success=%@", apiName, nativeInstanceId, result ? @"true" : @"false");
        return result ? 1 : 0;
    }

    // Meta Audience Network test mode. Registers this device (plus optional extra hashes) as a
    // Meta test device; call before Meta ads are requested.
    int AdsMultiplatform_EnableMetaTestMode(const char *additionalDeviceHashesCsv, int testAdType) {
        NSString *hashes = AdsMultiplatformString(additionalDeviceHashesCsv);
        NSString *apiName = @"EnableMetaTestMode";
        __block BOOL result = NO;
        __block NSString *deviceHash = @"";
        AdsMultiplatformRunOnMainSync(^{
            if (!AdsMultiplatformSharedClassAvailable(apiName, @"SharedNativeAdIosBridge", @"meta")) {
                return;
            }
            @try {
                SharedNativeAdIosBridge *bridge = [[SharedNativeAdIosBridge alloc] init];
                result = [bridge enableMetaTestModeAdditionalDeviceHashesCsv:hashes testAdType:testAdType];
                deviceHash = [bridge getCurrentMetaTestDeviceHash] ?: @"";
            } @catch (NSException *exception) {
                AdsMultiplatformBridgeException(apiName, @"meta", exception);
            }
        });
        AdsMultiplatformBridgeLog(
            @"api=%@ result success=%@ currentDeviceHash=%@ testAdType=%d",
            apiName,
            result ? @"true" : @"false",
            deviceHash,
            testAdType
        );
        return result ? 1 : 0;
    }

    void AdsMultiplatform_DisableMetaTestMode(void) {
        NSString *apiName = @"DisableMetaTestMode";
        AdsMultiplatformRunOnMainSync(^{
            if (!AdsMultiplatformSharedClassAvailable(apiName, @"SharedNativeAdIosBridge", @"meta")) {
                return;
            }
            @try {
                SharedNativeAdIosBridge *bridge = [[SharedNativeAdIosBridge alloc] init];
                [bridge disableMetaTestMode];
            } @catch (NSException *exception) {
                AdsMultiplatformBridgeException(apiName, @"meta", exception);
            }
        });
        AdsMultiplatformBridgeLog(@"api=%@ dispatched shared call", apiName);
    }

    int AdsMultiplatform_IsMetaTestModeEnabled(void) {
        NSString *apiName = @"IsMetaTestModeEnabled";
        __block BOOL result = NO;
        AdsMultiplatformRunOnMainSync(^{
            if (!AdsMultiplatformSharedClassAvailable(apiName, @"SharedNativeAdIosBridge", @"meta")) {
                return;
            }
            @try {
                SharedNativeAdIosBridge *bridge = [[SharedNativeAdIosBridge alloc] init];
                result = [bridge isMetaTestModeEnabled];
            } @catch (NSException *exception) {
                AdsMultiplatformBridgeException(apiName, @"meta", exception);
            }
        });
        return result ? 1 : 0;
    }

    // Caller owns the returned string and must release it with AdsMultiplatform_FreeCString.
    const char *AdsMultiplatform_GetMetaTestDeviceHash(void) {
        NSString *apiName = @"GetMetaTestDeviceHash";
        __block NSString *deviceHash = @"";
        AdsMultiplatformRunOnMainSync(^{
            if (!AdsMultiplatformSharedClassAvailable(apiName, @"SharedNativeAdIosBridge", @"meta")) {
                return;
            }
            @try {
                SharedNativeAdIosBridge *bridge = [[SharedNativeAdIosBridge alloc] init];
                deviceHash = [bridge getCurrentMetaTestDeviceHash] ?: @"";
            } @catch (NSException *exception) {
                AdsMultiplatformBridgeException(apiName, @"meta", exception);
            }
        });
        AdsMultiplatformBridgeLog(@"api=%@ result currentDeviceHash=%@", apiName, deviceHash);
        return AdsMultiplatformCopyCString(deviceHash, "");
    }

    void loadNativeAdIOS(const char *adUnitId) {
        AdsMultiplatform_LoadNativeAd(adUnitId);
    }

    void showNativeAdIOS(const char *adUnitId) {
        AdsMultiplatform_ShowNativeAd(adUnitId);
    }
}
