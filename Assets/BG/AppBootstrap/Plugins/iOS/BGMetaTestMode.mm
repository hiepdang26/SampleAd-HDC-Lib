#import <Foundation/Foundation.h>
#import <objc/message.h>
#include <stdlib.h>
#include <string.h>

// Meta Audience Network test mode for AdMetaToggle.cs, called straight on FBAdSettings.
// The class is looked up at runtime, so this file builds even when FBAudienceNetwork is not linked.

static const NSInteger BGMetaAdLogLevelVerbose = 6; // FBAdLogLevelVerbose

static Class BGMetaAdSettings(void) {
    return NSClassFromString(@"FBAdSettings");
}

static NSString *BGMetaCurrentDeviceHash(Class settings) {
    SEL selector = @selector(testDeviceHash);
    if (settings == Nil || ![settings respondsToSelector:selector]) {
        return @"";
    }
    NSString *hash = ((NSString *(*)(id, SEL))objc_msgSend)(settings, selector);
    return hash ?: @"";
}

static BOOL BGMetaIsTestMode(Class settings) {
    SEL selector = @selector(isTestMode);
    if (settings == Nil || ![settings respondsToSelector:selector]) {
        return NO;
    }
    return ((BOOL (*)(id, SEL))objc_msgSend)(settings, selector);
}

static void BGMetaSetTestAdType(Class settings, NSInteger testAdType) {
    SEL selector = @selector(setTestAdType:);
    if ([settings respondsToSelector:selector]) {
        ((void (*)(id, SEL, NSInteger))objc_msgSend)(settings, selector, testAdType);
    }
}

extern "C" {
    // Registers this device (plus extraDeviceHashesCsv, separated by , ; or spaces) as a Meta test device.
    // Returns 1 when Meta reports test mode for this device.
    int BGMetaTest_Enable(const char *extraDeviceHashesCsv, int testAdType, int verboseLogs) {
        Class settings = BGMetaAdSettings();
        if (settings == Nil) {
            NSLog(@"[MetaTest] enable failed: FBAdSettings not found, FBAudienceNetwork is not linked");
            return 0;
        }

        NSMutableOrderedSet<NSString *> *hashes = [NSMutableOrderedSet orderedSet];
        NSString *currentHash = BGMetaCurrentDeviceHash(settings);
        if (currentHash.length > 0) {
            [hashes addObject:currentHash];
        }
        NSString *extra = extraDeviceHashesCsv != NULL ? [NSString stringWithUTF8String:extraDeviceHashesCsv] : @"";
        NSCharacterSet *separators = [NSCharacterSet characterSetWithCharactersInString:@",; \n"];
        for (NSString *part in [extra componentsSeparatedByCharactersInSet:separators]) {
            if (part.length > 0) {
                [hashes addObject:part];
            }
        }

        SEL addSelector = @selector(addTestDevice:);
        if ([settings respondsToSelector:addSelector]) {
            for (NSString *hash in hashes) {
                ((void (*)(id, SEL, NSString *))objc_msgSend)(settings, addSelector, hash);
            }
        }
        BGMetaSetTestAdType(settings, testAdType);

        SEL logSelector = @selector(setLogLevel:);
        if (verboseLogs != 0 && [settings respondsToSelector:logSelector]) {
            ((void (*)(id, SEL, NSInteger))objc_msgSend)(settings, logSelector, BGMetaAdLogLevelVerbose);
        }

        BOOL testMode = BGMetaIsTestMode(settings);
        NSLog(
            @"[MetaTest] enable testMode=%@ currentHash=%@ devices=%@ testAdType=%d",
            testMode ? @"true" : @"false",
            currentHash,
            [hashes.array componentsJoinedByString:@","],
            testAdType
        );
        return testMode ? 1 : 0;
    }

    void BGMetaTest_Disable(void) {
        Class settings = BGMetaAdSettings();
        if (settings == Nil) {
            return;
        }
        SEL clearSelector = @selector(clearTestDevices);
        if ([settings respondsToSelector:clearSelector]) {
            ((void (*)(id, SEL))objc_msgSend)(settings, clearSelector);
        }
        BGMetaSetTestAdType(settings, 0);
        NSLog(@"[MetaTest] disabled");
    }

    int BGMetaTest_IsEnabled(void) {
        return BGMetaIsTestMode(BGMetaAdSettings()) ? 1 : 0;
    }

    // Unity frees the returned string.
    const char *BGMetaTest_DeviceHash(void) {
        const char *source = BGMetaCurrentDeviceHash(BGMetaAdSettings()).UTF8String ?: "";
        char *copy = (char *)malloc(strlen(source) + 1);
        if (copy != NULL) {
            strcpy(copy, source);
        }
        return copy;
    }
}
