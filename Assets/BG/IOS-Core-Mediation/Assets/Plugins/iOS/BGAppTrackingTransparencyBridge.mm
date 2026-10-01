#import <Foundation/Foundation.h>

#if __has_include(<AppTrackingTransparency/AppTrackingTransparency.h>)
#import <AppTrackingTransparency/AppTrackingTransparency.h>
#endif

typedef void (*BG_ATT_AuthorizationCallback)(int status);

extern "C" int BG_ATT_GetAuthorizationStatus() {
#if __has_include(<AppTrackingTransparency/AppTrackingTransparency.h>)
    if (@available(iOS 14.0, *)) {
        return (int)ATTrackingManager.trackingAuthorizationStatus;
    }
#endif

    return -1;
}

extern "C" void BG_ATT_RequestAuthorization(BG_ATT_AuthorizationCallback callback) {
#if __has_include(<AppTrackingTransparency/AppTrackingTransparency.h>)
    if (@available(iOS 14.0, *)) {
        [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(ATTrackingManagerAuthorizationStatus status) {
            if (callback == NULL) {
                return;
            }

            dispatch_async(dispatch_get_main_queue(), ^{
                callback((int)status);
            });
        }];
        return;
    }
#endif

    if (callback != NULL) {
        callback(-1);
    }
}
