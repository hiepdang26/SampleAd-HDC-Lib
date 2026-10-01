using System;
using System.Collections.Generic;
using System.Text;
using Firebase.Analytics;

namespace BG_Library.NET.Tracking
{
    internal enum NetTrackingEventKind
    {
        Unknown,
        Command,
        Callback,
        Revenue
    }

    internal readonly struct NetTrackingEventExplanation
    {
        public NetTrackingEventExplanation(
            string eventName,
            string explanation,
            NetTrackingEventKind kind,
            string kindLabel,
            Channel? channel,
            TrackingHistoryAction action,
            TrackingHistoryScope scope)
        {
            EventName = eventName ?? "";
            Explanation = explanation ?? "";
            Kind = kind;
            KindLabel = kindLabel ?? "Unknown";
            Channel = channel;
            Action = action;
            Scope = scope;
        }

        public string EventName { get; }
        public string Explanation { get; }
        public NetTrackingEventKind Kind { get; }
        public string KindLabel { get; }
        public Channel? Channel { get; }
        public TrackingHistoryAction Action { get; }
        public TrackingHistoryScope Scope { get; }
    }

    internal static class NetTrackingEventExplainer
    {
        public static NetTrackingEventExplanation Explain(string eventName)
        {
            if (string.IsNullOrWhiteSpace(eventName))
                return Build(eventName, "(event rỗng)", NetTrackingEventKind.Unknown, "Unknown", null, TrackingHistoryAction.Other, TrackingHistoryScope.System);

            if (string.Equals(eventName, FirebaseAnalytics.EventAdImpression, StringComparison.Ordinal))
            {
                return Build(
                    eventName,
                    "Revenue impression từ paid callback.",
                    NetTrackingEventKind.Revenue,
                    "Revenue",
                    null,
                    TrackingHistoryAction.Revenue,
                    TrackingHistoryScope.Revenue);
            }

            if (string.Equals(eventName, NetTrackingSystem.EventBgAdImpression, StringComparison.Ordinal))
            {
                return Build(
                    eventName,
                    "BG revenue impression from paid callback.",
                    NetTrackingEventKind.Revenue,
                    "Revenue",
                    null,
                    TrackingHistoryAction.Revenue,
                    TrackingHistoryScope.Revenue);
            }

            string[] tokens = eventName.Split('_');
            if (tokens.Length < 3 || !string.Equals(tokens[0], "ad", StringComparison.Ordinal))
                return Build(eventName, eventName, NetTrackingEventKind.Unknown, "Unknown", null, TrackingHistoryAction.Other, TrackingHistoryScope.System);

            if (TryExplainCallbackV2(eventName, tokens, out var callbackV2))
                return callbackV2;

            if (TryExplainCommandV2(eventName, tokens, out var commandV2))
                return commandV2;

            return Build(eventName, eventName, NetTrackingEventKind.Unknown, "Unknown", null, TrackingHistoryAction.Other, TrackingHistoryScope.System);
        }

        public static string GetLegendText()
        {
            return
                "Grammar mới\n" +
                "command: ad_{layer}_{channel?}_{action}_{id?}_{context?}_{target?}_n_{reason?}_api\n" +
                "callback: ad_evt_{evtName}_{action}_{id}_{context?}_{target?}_{reason?}_{net?}_{speed?}\n" +
                "\n" +
                "layer: atsy / sy / ac / gr\n" +
                "channel: al / ar / fa / rw / bn / mr / pu / cl\n" +
                "action: rq / sh / act / hid\n" +
                "evtName: dsp / clk / imp / cls / rwd / shf / ls / lf\n" +
                "context: ini / rtN / idl / hdr / sfr\n" +
                "target: pos hoặc groupName\n" +
                "n: marker báo flow bị ngắt\n" +
                "reason: cfg / dis / nready / upos / size0 / ec123 ...\n" +
                "api: flow đã chạm API\n" +
                "net: net0 / net1\n" +
                "speed: f / n / s / x / rl";
        }

        public static string NormalizeDisplayText(string value) => value ?? "";

        private static bool TryExplainCallbackV2(string eventName, string[] tokens, out NetTrackingEventExplanation explanation)
        {
            explanation = default;

            if (tokens.Length < 5 || tokens[1] != "evt")
                return false;

            string evtName = tokens[2];
            string actionToken = tokens[3];
            if (actionToken != "sh" && actionToken != "rq")
                return false;

            string id = tokens[4];
            int index = 5;
            string context = "";
            string target = "";
            string reason = "";
            string net = "";
            string speed = "";

            if (actionToken == "rq" && index < tokens.Length && IsContextToken(tokens[index]))
                context = tokens[index++];

            if (index < tokens.Length && !IsReasonToken(tokens[index]) && !IsNetToken(tokens[index]) && !IsSpeedToken(tokens[index]))
                target = tokens[index++];

            if (index < tokens.Length && IsReasonToken(tokens[index]))
                reason = tokens[index++];

            if (index < tokens.Length && IsNetToken(tokens[index]))
                net = tokens[index++];

            if (index < tokens.Length && IsSpeedToken(tokens[index]))
                speed = tokens[index];

            Channel? channel = TryParseChannelFromGroupId(id, out var parsedChannel) ? parsedChannel : null;
            TrackingHistoryAction action = actionToken == "rq" ? TrackingHistoryAction.Load : TrackingHistoryAction.Event;
            TrackingHistoryScope scope = TrackingHistoryScope.Group;

            var parts = new List<string>();
            parts.Add($"callback {ExpandCallback(evtName)}");
            parts.Add(actionToken == "rq" ? "sau request" : "sau show");
            parts.Add($"id {id}");
            if (!string.IsNullOrEmpty(context))
                parts.Add($"context {ExpandContext(context)}");
            if (!string.IsNullOrEmpty(target))
                parts.Add($"target {target}");
            if (!string.IsNullOrEmpty(reason))
                parts.Add($"reason {ExpandReason(reason)}");
            if (!string.IsNullOrEmpty(net))
                parts.Add(net == "net1" ? "mạng online" : "mạng offline");
            if (!string.IsNullOrEmpty(speed))
                parts.Add($"tốc độ {ExpandSpeed(speed)}");

            explanation = Build(eventName, JoinSentence(parts), NetTrackingEventKind.Callback, "Callback", channel, action, scope);
            return true;
        }

        private static bool TryExplainCommandV2(string eventName, string[] tokens, out NetTrackingEventExplanation explanation)
        {
            explanation = default;

            if (tokens.Length < 4 || tokens[1] == "evt" || !IsLayerToken(tokens[1]))
                return false;

            string layer = tokens[1];
            int index = 2;
            Channel? channel = null;
            if (index < tokens.Length && TryParseChannelToken(tokens[index], out var parsedChannel))
            {
                channel = parsedChannel;
                index++;
            }

            if (index >= tokens.Length)
                return false;

            string actionToken = tokens[index++];
            string id = "";
            string context = "";
            string target = "";
            string reason = "";
            bool reachedApi = false;

            if (index < tokens.Length && IsIdentityToken(tokens[index]))
                id = tokens[index++];

            if (index < tokens.Length && IsContextToken(tokens[index]))
                context = tokens[index++];

            bool hasContext = !string.IsNullOrEmpty(context);
            if (hasContext)
            {
                if (index < tokens.Length && tokens[index] == "n")
                {
                    index++;
                    if (index < tokens.Length)
                        reason = tokens[index++];
                }

                if (index < tokens.Length && tokens[index] != "api")
                    target = tokens[index++];
            }
            else
            {
                if (index < tokens.Length && tokens[index] != "n" && tokens[index] != "api")
                    target = tokens[index++];

                if (index < tokens.Length && tokens[index] == "n")
                {
                    index++;
                    if (index < tokens.Length)
                        reason = tokens[index++];
                }
            }

            if (index < tokens.Length && tokens[index] == "api")
                reachedApi = true;

            TrackingHistoryAction action = ParseAction(actionToken);
            TrackingHistoryScope scope = ParseScope(layer);
            if (!channel.HasValue && TryParseChannelFromGroupId(id, out parsedChannel))
                channel = parsedChannel;

            var parts = new List<string>();
            parts.Add($"{ExpandLayer(layer)} {DescribeAction(action)}");
            if (channel.HasValue)
                parts.Add($"channel {ExpandChannel(channel.Value)}");
            if (!string.IsNullOrEmpty(id))
                parts.Add($"id {id}");
            if (!string.IsNullOrEmpty(context))
                parts.Add($"context {ExpandContext(context)}");
            if (!string.IsNullOrEmpty(target))
                parts.Add($"target {target}");
            if (!string.IsNullOrEmpty(reason))
                parts.Add($"bị chặn bởi {ExpandReason(reason)}");
            if (reachedApi)
                parts.Add("đã chạm API");

            explanation = Build(eventName, JoinSentence(parts), NetTrackingEventKind.Command, "Lệnh", channel, action, scope);
            return true;
        }

        private static NetTrackingEventExplanation Build(string eventName, string explanation, NetTrackingEventKind kind, string kindLabel,
            Channel? channel, TrackingHistoryAction action, TrackingHistoryScope scope)
            => new NetTrackingEventExplanation(eventName, explanation, kind, kindLabel, channel, action, scope);

        private static string JoinSentence(List<string> parts)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < parts.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(parts[i]))
                    continue;
                if (builder.Length > 0)
                    builder.Append(" | ");
                builder.Append(parts[i]);
            }
            return builder.ToString();
        }

        private static TrackingHistoryAction ParseAction(string token)
        {
            return token switch
            {
                "rq" => TrackingHistoryAction.Request,
                "sh" => TrackingHistoryAction.Show,
                "act" => TrackingHistoryAction.Activate,
                "hid" => TrackingHistoryAction.Hide,
                _ => TrackingHistoryAction.Other
            };
        }

        private static TrackingHistoryScope ParseScope(string layer)
        {
            return layer switch
            {
                "sy" => TrackingHistoryScope.System,
                "atsy" => TrackingHistoryScope.System,
                "ac" => TrackingHistoryScope.AdCore,
                "gr" => TrackingHistoryScope.Group,
                _ => TrackingHistoryScope.System
            };
        }

        private static string DescribeAction(TrackingHistoryAction action)
        {
            return action switch
            {
                TrackingHistoryAction.Request => "request",
                TrackingHistoryAction.Show => "show",
                TrackingHistoryAction.Hide => "hide",
                TrackingHistoryAction.Activate => "activate",
                TrackingHistoryAction.Load => "load",
                TrackingHistoryAction.Event => "event",
                TrackingHistoryAction.Revenue => "revenue",
                _ => "khác"
            };
        }

        private static string ExpandLayer(string token) => token switch
        {
            "sy" => "layer system",
            "atsy" => "layer auto-show system",
            "ac" => "layer adcore",
            "gr" => "layer group",
            _ => $"layer {token}"
        };

        private static string ExpandChannel(Channel channel) => channel switch
        {
            Channel.AppLaunch => "AppLaunch",
            Channel.AppResume => "AppResume",
            Channel.ForceAd => "ForceAd",
            Channel.Rewarded => "Rewarded",
            Channel.Banner => "Banner",
            Channel.Mrec => "Mrec",
            Channel.Popup => "Popup",
            Channel.Collap => "Collap",
            _ => channel.ToString()
        };

        private static string ExpandContext(string token)
        {
            return token switch
            {
                "ini" => "init",
                "idl" => "idle",
                "hdr" => "hide reload",
                "sfr" => "show-fail reload",
                _ when TryParseRetryContext(token, out int retryAttempt) => $"retry {retryAttempt}",
                _ => token
            };
        }

        private static string ExpandReason(string token) => token switch
        {
            "cfg" => "config disabled",
            "dis" => "disabled",
            "iap" => "IAP removed",
            "group" => "missing group",
            "nready" => "not ready",
            "tmo" => "timeout",
            "ign" => "ignored",
            "host" => "host blocked",
            "gate" => "gate blocked",
            "aload" => "already loaded",
            "loading" => "loading in progress",
            "inited" => "already initialized",
            "noid" => "missing ad unit id",
            "budget" => "budget exhausted",
            "retry" => "retry waiting",
            "upos" => "missing update pos",
            "size0" => "invalid layout size",
            _ => token
        };

        private static string ExpandCallback(string token) => token switch
        {
            "dsp" => "displayed",
            "clk" => "clicked",
            "imp" => "impression",
            "cls" => "closed",
            "rwd" => "rewarded",
            "shf" => "show failed",
            "ls" => "load success",
            "lf" => "load fail",
            _ => token
        };

        private static string ExpandSpeed(string token) => token switch
        {
            "f" => "fast",
            "n" => "normal",
            "s" => "slow",
            "x" => "extra slow",
            "rl" => "refresh reload",
            _ => token
        };

        private static bool IsLayerToken(string token) => token == "sy" || token == "ac" || token == "atsy" || token == "gr";
        private static bool IsContextToken(string token)
            => token == "ini" || token == "idl" || token == "hdr" || token == "sfr" || TryParseRetryContext(token, out _);
        private static bool IsNetToken(string token) => token == "net0" || token == "net1";
        private static bool IsSpeedToken(string token) => token == "f" || token == "n" || token == "s" || token == "x" || token == "rl";
        private static bool IsReasonToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            if (token.StartsWith("ec", StringComparison.Ordinal))
                return true;

            return token == "cfg" ||
                   token == "dis" ||
                   token == "iap" ||
                   token == "group" ||
                   token == "nready" ||
                   token == "tmo" ||
                   token == "ign" ||
                   token == "host" ||
                   token == "gate" ||
                   token == "aload" ||
                   token == "loading" ||
                   token == "inited" ||
                   token == "noid" ||
                   token == "budget" ||
                   token == "retry" ||
                   token == "upos" ||
                   token == "size0";
        }

        private static bool IsIdentityToken(string token)
        {
            if (string.IsNullOrEmpty(token) || token.Length < 3)
                return false;

            return TryParseChannelFromGroupId(token, out _);
        }

        private static bool TryParseChannelFromGroupId(string id, out Channel channel)
        {
            channel = default;
            if (string.IsNullOrEmpty(id) || id.Length < 2)
                return false;
            return TryParseChannelToken(id.Substring(0, 2), out channel);
        }

        private static bool TryParseChannelToken(string token, out Channel channel)
        {
            switch (token)
            {
                case "al": channel = Channel.AppLaunch; return true;
                case "ar": channel = Channel.AppResume; return true;
                case "fa": channel = Channel.ForceAd; return true;
                case "rw": channel = Channel.Rewarded; return true;
                case "bn": channel = Channel.Banner; return true;
                case "mr": channel = Channel.Mrec; return true;
                case "pu": channel = Channel.Popup; return true;
                case "cl": channel = Channel.Collap; return true;
                case "ao": channel = Channel.AppLaunch; return true;
                default:
                    channel = default;
                    return false;
            }
        }

        private static bool TryParseRetryContext(string token, out int retryAttempt)
        {
            retryAttempt = 0;
            if (string.IsNullOrEmpty(token) || token.Length < 3)
                return false;

            if (!token.StartsWith("rt", StringComparison.Ordinal))
                return false;

            return int.TryParse(token.Substring(2), out retryAttempt) && retryAttempt > 0;
        }
    }
}
