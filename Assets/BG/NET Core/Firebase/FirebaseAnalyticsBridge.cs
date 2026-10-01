using BG_Library.NET.Debug;
using BG_Library.NET.AdSystem;
using Firebase.Analytics;
using System;
using System.Collections.Generic;

namespace BG_Library.NET.Tracking
{
    public static class FirebaseAnalyticsBridge
    {
        private sealed class CachedEvent
        {
            public string EventName;
            public Parameter[] Parameters;
        }

        private static readonly Queue<CachedEvent> pendingEvents = new Queue<CachedEvent>();
        private static readonly object syncRoot = new object();
        private static bool isSubscribedToFirebaseInitialized;

        static FirebaseAnalyticsBridge()
        {
            EnsureSubscribed();
        }

        public static void LogEvent(string eventName)
        {
            if (string.IsNullOrEmpty(eventName)) return;

            if (!CanSendImmediately())
            {
                CacheEvent(eventName, Array.Empty<Parameter>());
                return;
            }

            FirebaseAnalytics.LogEvent(eventName);
            Debug(() => $"event={eventName}");
        }

        public static void LogEvent(string eventName, params Parameter[] parameters)
        {
            if (string.IsNullOrEmpty(eventName)) return;

            Parameter[] safeParameters = parameters ?? Array.Empty<Parameter>();

            if (!CanSendImmediately())
            {
                CacheEvent(eventName, safeParameters);
                return;
            }

            FirebaseAnalytics.LogEvent(eventName, safeParameters);
            Debug(() => $"event={eventName} params={safeParameters.Length}");
        }

        public static void SetUserProperty(string propertyName, string value)
        {
            if (string.IsNullOrEmpty(propertyName)) return;

            FirebaseAnalytics.SetUserProperty(propertyName, value);
            Debug(() => $"property={propertyName} value={value}");
        }

        private static void Debug(Func<string> messageFactory)
        {
            NetFlowDebugSystem.Log(Layer.sys, Module.adslogic, "FirebaseAnalytics", messageFactory);
        }

        private static bool CanSendImmediately()
        {
            EnsureSubscribed();

            return RemoteConfig.Ins != null && RemoteConfig.Ins.IsFirebaseInitialized;
        }

        private static void EnsureSubscribed()
        {
            if (isSubscribedToFirebaseInitialized)
                return;

            RemoteConfig.OnFirebaseInitialized += FlushPendingEvents;
            isSubscribedToFirebaseInitialized = true;
        }

        private static void CacheEvent(string eventName, Parameter[] parameters)
        {
            Parameter[] cachedParameters = parameters.Length == 0
                ? Array.Empty<Parameter>()
                : (Parameter[])parameters.Clone();

            int pendingCount;

            lock (syncRoot)
            {
                pendingEvents.Enqueue(new CachedEvent
                {
                    EventName = eventName,
                    Parameters = cachedParameters
                });

                pendingCount = pendingEvents.Count;
            }

            Debug(() => $"cached event={eventName} params={cachedParameters.Length} pending={pendingCount}");
        }

        private static void FlushPendingEvents()
        {
            CachedEvent[] cachedEvents;

            lock (syncRoot)
            {
                if (pendingEvents.Count == 0)
                    return;

                cachedEvents = pendingEvents.ToArray();
                pendingEvents.Clear();
            }

            for (int i = 0; i < cachedEvents.Length; i++)
            {
                CachedEvent cachedEvent = cachedEvents[i];
                FirebaseAnalytics.LogEvent(cachedEvent.EventName, cachedEvent.Parameters);
            }

            Debug(() => $"flush cached events count={cachedEvents.Length}");
        }
    }
}
