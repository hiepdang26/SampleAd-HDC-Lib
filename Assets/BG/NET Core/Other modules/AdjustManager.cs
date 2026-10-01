using System.Collections;
using System.Globalization;
using System.Text;
using AdjustSdk;
using BG_Library.Common;
using BG_Library.NET.AdSystem;
using BG_Library.NET.Tracking;
using UnityEngine;

namespace BG_Library.NET
{
    public class AdjustManager : MonoBehaviour
    {
        public const string user_network = "user_network";
        public const string user_campaign = "user_campaign";
        public const string user_creative = "user_creative";
        public const string user_cost = "user_cost";

        public static AdjustManager Ins { get; private set; }

        public static System.Action OnNetworkReady;

        public AdjustAttribution AdjustAttri;
        public bool AttributionFromAdjustReady;

        public string NetworkName;
        public int UserSourceOrder = -1; // -1 means Default

        public bool IsAdjustEnabledReady => isEnable;
        public bool DidAdjustWaitTimeout { get; private set; }
        public bool NetworkReadyRaised { get; private set; }
        public string DebugLifecycle { get; private set; } = "Awake";
        public string LastDebugMessage { get; private set; } = "";
        public float StartRealtime { get; private set; }
        public float AttributionReadyRealtime { get; private set; } = -1f;
        public float NetworkReadyRealtime { get; private set; } = -1f;

        private bool isEnable;

        private void Awake()
        {
            if (Ins == null)
                Ins = this;

            StartRealtime = Time.realtimeSinceStartup;
            DebugLifecycle = "Awake";
            LastDebugMessage = "AdjustManager created";
        }

        private IEnumerator Start()
        {
            DebugLifecycle = "Start";

            if (!Application.isEditor)
            {
                LastDebugMessage = "Waiting Adjust enabled";
                StartCoroutine(WaitAdjustEnable());
                yield return new WaitUntil(() => isEnable);
            }
            else
            {
                isEnable = true;
            }

            LastDebugMessage = "Requesting attribution";
            Adjust.GetAttribution(attribution =>
            {
                AdjustAttri = attribution;
                AttributionFromAdjustReady = true;
                AttributionReadyRealtime = Time.realtimeSinceStartup;
                DebugLifecycle = "AttributionReady";
                LastDebugMessage = attribution == null
                    ? "Adjust attribution callback returned null"
                    : "Adjust attribution received";
            });

            StartCoroutine(WaitDetectedNetwork());
            StartCoroutine(WaitAttribution());
        }

        private IEnumerator WaitAdjustEnable()
        {
            while (!isEnable)
            {
                Adjust.IsEnabled(enable =>
                {
                    if (enable)
                        isEnable = true;
                });
                yield return null;
            }

            DebugLifecycle = "AdjustEnabled";
            LastDebugMessage = "Adjust enabled confirmed";
        }

        private IEnumerator WaitDetectedNetwork()
        {
            DebugLifecycle = "WaitDetectedNetwork";
            float startTime = Time.time;

            while (!AttributionFromAdjustReady && Time.time - startTime < 7f)
                yield return null;

            DidAdjustWaitTimeout = !AttributionFromAdjustReady;

            yield return new WaitUntil(() => RemoteConfig.Ins.IsFirebaseInitialized);

            if (AttributionFromAdjustReady)
            {
                NetworkName = AdjustAttri != null ? AdjustAttri.Network : "";
                NetworkName = Master.ConvertString(NetworkName);
                DebugLifecycle = "AttributionApplied";
                LastDebugMessage = $"Adjust attribution applied | network={NetworkName}";

                if (AdjustAttri != null)
                {
                    SetPropertyAdjustAttribution(user_network, AdjustAttri.Network);
                    SetPropertyAdjustAttribution(user_campaign, AdjustAttri.Campaign);
                    SetPropertyAdjustAttribution(user_creative, AdjustAttri.Creative);
                    if (AdjustAttri.CostAmount.HasValue)
                    {
                        SetPropertyAdjustAttribution(
                            user_cost,
                            AdjustAttri.CostAmount.Value.ToString(CultureInfo.InvariantCulture));
                    }
                }
            }
            else
            {
                NetworkName = "time_out";
                DebugLifecycle = "AttributionTimeout";
                LastDebugMessage = "Adjust attribution timeout after 7 seconds";
            }
        }

        private IEnumerator WaitAttribution()
        {
            yield return new WaitUntil(() => AttributionFromAdjustReady);
            NetworkReadyRaised = true;
            NetworkReadyRealtime = Time.realtimeSinceStartup;
            DebugLifecycle = "OnNetworkReady";
            LastDebugMessage = "OnNetworkReady invoked";
            OnNetworkReady?.Invoke();
        }

        private static void SetPropertyAdjustAttribution(string propertyName, string value)
        {
            PlayerPrefs.SetString(propertyName, value);
            PlayerPrefs.Save();

            FirebaseAnalyticsBridge.SetUserProperty(propertyName, string.IsNullOrEmpty(value) ? "" : value);
        }

        public string GetDebugInfo()
        {
            var builder = new StringBuilder(1024);
            builder.AppendLine("Adjust manager");
            builder.AppendLine($"- lifecycle: {DebugLifecycle}");
            builder.AppendLine($"- last message: {LastDebugMessage}");
            builder.AppendLine($"- adjust enabled: {IsAdjustEnabledReady}");
            builder.AppendLine($"- attribution ready: {AttributionFromAdjustReady}");
            builder.AppendLine($"- wait timeout: {DidAdjustWaitTimeout}");
            builder.AppendLine($"- network ready raised: {NetworkReadyRaised}");
            builder.AppendLine($"- user source order: {UserSourceOrder}");
            builder.AppendLine($"- normalized network: {NetworkName}");
            builder.AppendLine($"- start realtime: {StartRealtime:0.00}");
            builder.AppendLine($"- attribution realtime: {(AttributionReadyRealtime < 0f ? "-" : AttributionReadyRealtime.ToString("0.00"))}");
            builder.AppendLine($"- network ready realtime: {(NetworkReadyRealtime < 0f ? "-" : NetworkReadyRealtime.ToString("0.00"))}");
            builder.AppendLine();
            builder.AppendLine("Stored attribution properties");
            builder.AppendLine($"- user_network: {PlayerPrefs.GetString(user_network, "")}");
            builder.AppendLine($"- user_campaign: {PlayerPrefs.GetString(user_campaign, "")}");
            builder.AppendLine($"- user_creative: {PlayerPrefs.GetString(user_creative, "")}");
            builder.AppendLine($"- user_cost: {PlayerPrefs.GetString(user_cost, "")}");
            builder.AppendLine();
            builder.AppendLine("Raw attribution");
            builder.AppendLine($"- network: {AdjustAttri?.Network ?? ""}");
            builder.AppendLine($"- campaign: {AdjustAttri?.Campaign ?? ""}");
            builder.AppendLine($"- creative: {AdjustAttri?.Creative ?? ""}");
            builder.AppendLine($"- cost amount: {(AdjustAttri != null && AdjustAttri.CostAmount.HasValue ? AdjustAttri.CostAmount.Value.ToString(CultureInfo.InvariantCulture) : "")}");
            return builder.ToString().TrimEnd();
        }
    }
}
