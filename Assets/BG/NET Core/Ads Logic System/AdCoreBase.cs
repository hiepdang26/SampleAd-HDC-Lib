using BG_Library.NET.Debug;
using BG_Library.NET.Tracking;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace BG_Library.NET
{
	public enum BannerPlacement
	{
		FullBottom = 0,
		FullTop = 1,
		TopLeft = 2,
		TopRight = 3,
		BottomLeft = 4,
		BottomRight = 5
	}

	public abstract class AdCoreBase : ScriptableObject
	{
		[ShowInInspector, ReadOnly] public abstract string AdCoreName { get; }

		public abstract void InitPluginAtAwake();
		public abstract void InitStats(string configSt);
		public abstract void InitializeMediation();
		public abstract string DebugRecheckLogic();

		#region ===== CORE (Lifecycle helpers) =====
		// NOTE: InitPluginAtAwake / InitStats / InitializeMediation / DebugRecheckLogic
		// are abstract => implementation will log inside each concrete AdCore.
		// We keep a shared helper to keep output consistent if you choose to call it.
		protected void Debug_CoreLifecycle(string title, Func<string> detail = null)
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.adcore, title, () => $"core={AdCoreName}"))
			{
				if (detail != null)
					NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "Detail", detail);
			}
		}

		private static string BannerTrackingHint(BannerPlacement placement)
			=> NetTrackingSystem.PosTargetHint(NetTrackingSystem.BannerPlacementToken(placement));

		private static string DebugAdTypeOf(IGroup g)
		{
			if (g == null) return "NULL";
			var f = g.Adtype;
			if (!string.IsNullOrEmpty(f)) return f;

			// Fallback only (shouldn't be used if Format is implemented correctly)
			return g.GetType().Name;
		}

		private static void ApplyTrackingChannel(IGroup group, Channel channel)
		{
			group?.SetTrackingChannel(channel);
		}

		private static bool TryResolveTrackingIdentity(IGroup group, Channel channel, out GroupAdType adType, out string identitySource)
		{
			adType = default;
			identitySource = "";

			if (group == null)
				return false;

			ApplyTrackingChannel(group, channel);
			adType = group.TrackingAdType;
			identitySource = group.TrackingIdentitySource ?? "";
			return true;
		}
		#endregion

		#region FA

		protected abstract IFSGroup FA_GetGroup(string groupName);
		public abstract string FA_GroupByPos(string pos);
		public abstract string[] FA_GetListGroup();
		public abstract string[] FA_GetAutoInitGroupNames(BG_Library.NET.AdSystem.AdSystemConfigs.ForceAdChannelConfig channelConfig);

		public void FA_Initialize(string groupName, GroupAdType? knownAdType = null, string knownIdentitySource = "")
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_fa, "Core.Init", () => $"core={AdCoreName} group={groupName}"))
			{
                string groupHint = NetTrackingSystem.GroupTargetHint(groupName);
                var fa = FA_GetGroup(groupName);

				if (fa == null)
				{
					NetFlowDebugSystem.Warn(Layer.adcore, Module.format_fa, "ResolveGroup", () => "result=NULL");
                    NetTrackingSystem.RequestAdCoreResolveFail(Channel.ForceAd, TrackingReason.GroupMissing, groupHint);

                    return;
				}

				ApplyTrackingChannel(fa, Channel.ForceAd);
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_fa, "ResolveGroup", () => $"result=OK adtype={DebugAdTypeOf(fa)}");
				fa.Initialize();
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_fa, "Call", () => "IFSGroup.Initialize()");
			}
		}

		public void FA_ForceInitialize(string groupName, GroupAdType? knownAdType = null, string knownIdentitySource = "")
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_fa, "Core.ForceInit", () => $"core={AdCoreName} group={groupName}"))
			{
				string groupHint = NetTrackingSystem.GroupTargetHint(groupName);
				var fa = FA_GetGroup(groupName);

				if (fa == null)
				{
					NetFlowDebugSystem.Warn(Layer.adcore, Module.format_fa, "ResolveGroup", () => "result=NULL");
					NetTrackingSystem.RequestAdCoreResolveFail(Channel.ForceAd, TrackingReason.GroupMissing, groupHint);
					return;
				}

				ApplyTrackingChannel(fa, Channel.ForceAd);
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_fa, "ResolveGroup", () => $"result=OK adtype={DebugAdTypeOf(fa)}");
				var ok = fa.ForceInitialize();
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_fa, "Result", () => ok ? "force_init=true" : "force_init=false");
			}
		}

		public bool FA_GetReady(string groupName)
		{
            var fa = FA_GetGroup(groupName);

            if (fa == null)
                return false;

            var ready = fa.GetReady();
            return ready;
        }

		public bool FA_ShowAd(string groupName, string pos, Action onBeforeAdShow = null, Action onAdComplete = null, GroupAdType? knownAdType = null, string knownIdentitySource = "")
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_fa, "Core.Show", () => $"core={AdCoreName} group={groupName} pos={pos}"))
			{
				string posHint = NetTrackingSystem.PosTargetHint(pos);
				var fa = FA_GetGroup(groupName);

				if (fa == null)
				{
					NetFlowDebugSystem.Warn(Layer.adcore, Module.format_fa, "ResolveGroup", () => "result=NULL => show=false");
					NetTrackingSystem.ShowAdCoreResolveFail(Channel.ForceAd, TrackingReason.GroupMissing, posHint);
					return false;
				}

				ApplyTrackingChannel(fa, Channel.ForceAd);
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_fa, "ResolveGroup", () => $"result=OK adtype={DebugAdTypeOf(fa)}");

				var ok = fa.Show(pos, onBeforeAdShow, onAdComplete);
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_fa, "Result", () => ok ? "show=true" : "show=false");

				return ok;
			}
		}

		public string FA_GetDebugInfo(string groupName)
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_fa, "Core.GetDebugInfo", () => $"core={AdCoreName} group={groupName}"))
			{
				var g = FA_GetGroup(groupName);
				if (g == null)
				{
					NetFlowDebugSystem.Warn(Layer.adcore, Module.format_fa, "ResolveGroup", () => "result=NULL");
					return $"[FA] Group null | groupName={groupName}";
				}

				NetFlowDebugSystem.Log(Layer.adcore, Module.format_fa, "ResolveGroup", () => $"result=OK adtype={DebugAdTypeOf(g)}");
				return g.GetDebugInfo() ?? "";
			}
		}

		#endregion

		#region RW

		protected abstract IFSGroup RW_GetGroup();

		public void RW_Initialize()
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_rw, "Core.Init", () => $"core={AdCoreName}"))
			{
				var rw = RW_GetGroup();

				if (rw == null)
				{
					NetFlowDebugSystem.Warn(Layer.adcore, Module.format_rw, "ResolveGroup", () => "result=NULL");
					NetTrackingSystem.RequestAdCoreResolveFail(Channel.Rewarded, TrackingReason.GroupMissing);

					return;
				}

				ApplyTrackingChannel(rw, Channel.Rewarded);
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_rw, "ResolveGroup", () => $"result=OK adtype={DebugAdTypeOf(rw)}");
				rw.Initialize();
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_rw, "Call", () => "IFSGroup.Initialize()");
			}
		}

		public bool RW_GetReady()
		{
            var rw = RW_GetGroup();

            if (rw == null)
                return false;

            var ready = rw.GetReady();
            return ready;
        }

		public bool RW_ShowAd(string pos, Action onBeforeAdShow = null, Action onAdComplete = null)
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_rw, "Core.Show", () => $"core={AdCoreName} pos={pos}"))
			{
				string posHint = NetTrackingSystem.PosTargetHint(pos);
				var rw = RW_GetGroup();

				if (rw == null)
				{
					NetFlowDebugSystem.Warn(Layer.adcore, Module.format_rw, "ResolveGroup", () => "result=NULL => show=false");
					NetTrackingSystem.ShowAdCoreResolveFail(Channel.Rewarded, TrackingReason.GroupMissing, posHint);
					return false;
				}

				ApplyTrackingChannel(rw, Channel.Rewarded);
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_rw, "ResolveGroup", () => $"result=OK adtype={DebugAdTypeOf(rw)}");

				var ok = rw.Show(pos, onBeforeAdShow, onAdComplete);
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_rw, "Result", () => ok ? "show=true" : "show=false");

				return ok;
			}
		}

		public string RW_GetDebugInfo()
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_rw, "Core.GetDebugInfo", () => $"core={AdCoreName}"))
			{
				var g = RW_GetGroup();
				if (g == null)
				{
					NetFlowDebugSystem.Warn(Layer.adcore, Module.format_rw, "ResolveGroup", () => "result=NULL");
					return "[RW] Group null";
				}

				NetFlowDebugSystem.Log(Layer.adcore, Module.format_rw, "ResolveGroup", () => $"result=OK adtype={DebugAdTypeOf(g)}");
				return g.GetDebugInfo() ?? "";
			}
		}

		#endregion

		#region AL

		protected abstract IFSGroup AL_GetGroup();

		public void AL_Initialize()
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_al, "Core.Init", () => $"core={AdCoreName}"))
			{
                var al = AL_GetGroup();

				if (al == null)
				{
					NetFlowDebugSystem.Warn(Layer.adcore, Module.format_al, "ResolveGroup", () => "result=NULL");
                    NetTrackingSystem.RequestAdCoreResolveFail(Channel.AppLaunch, TrackingReason.GroupMissing);

                    return;
				}

				ApplyTrackingChannel(al, Channel.AppLaunch);
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_al, "ResolveGroup", () => $"result=OK adtype={DebugAdTypeOf(al)}");
                al.Initialize();
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_al, "Call", () => "IFSGroup.Initialize()");
			}
		}

		public bool AL_GetReady()
		{
            var al = AL_GetGroup();

            if (al == null)
                return false;

            var ready = al.GetReady();
            return ready;
        }

		public bool AL_ShowAd(string pos, Action onBeforeAdShow = null, Action onAdComplete = null)
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_al, "Core.Show", () => $"core={AdCoreName} pos={pos}"))
			{
				string posHint = NetTrackingSystem.PosTargetHint(pos);
				var al = AL_GetGroup();

				if (al == null)
				{
					NetFlowDebugSystem.Warn(Layer.adcore, Module.format_al, "ResolveGroup", () => "result=NULL => show=false");
					NetTrackingSystem.ShowAdCoreResolveFail(Channel.AppLaunch, TrackingReason.GroupMissing, posHint);
					return false;
				}

				ApplyTrackingChannel(al, Channel.AppLaunch);
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_al, "ResolveGroup", () => $"result=OK adtype={DebugAdTypeOf(al)}");

				var ok = al.Show(pos, onBeforeAdShow, onAdComplete);
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_al, "Result", () => ok ? "show=true" : "show=false");

				return ok;
			}
		}

		public string AL_GetDebugInfo()
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_al, "Core.GetDebugInfo", () => $"core={AdCoreName}"))
			{
				var g = AL_GetGroup();
				if (g == null)
				{
					NetFlowDebugSystem.Warn(Layer.adcore, Module.format_al, "ResolveGroup", () => "result=NULL");
					return "[AL] Group null";
				}

				NetFlowDebugSystem.Log(Layer.adcore, Module.format_al, "ResolveGroup", () => $"result=OK adtype={DebugAdTypeOf(g)}");
				return g.GetDebugInfo() ?? "";
			}
		}

		#endregion

		#region AR

		protected abstract IFSGroup AR_GetGroup();

		public void AR_Initialize()
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_ar, "Core.Init", () => $"core={AdCoreName}"))
			{
                var ar = AR_GetGroup();

				if (ar == null)
				{
					NetFlowDebugSystem.Warn(Layer.adcore, Module.format_ar, "ResolveGroup", () => "result=NULL");
                    NetTrackingSystem.RequestAdCoreResolveFail(Channel.AppResume, TrackingReason.GroupMissing);

                    return;
				}

				ApplyTrackingChannel(ar, Channel.AppResume);
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_ar, "ResolveGroup", () => $"result=OK adtype={DebugAdTypeOf(ar)}");
                ar.Initialize();
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_ar, "Call", () => "IFSGroup.Initialize()");
			}
		}

		public bool AR_GetReady()
		{
            var ar = AR_GetGroup();

            if (ar == null)
                return false;

            var ready = ar.GetReady();
            return ready;
        }

		public bool AR_ShowAd(string pos, Action onBeforeAdShow = null, Action onAdComplete = null)
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_ar, "Core.Show", () => $"core={AdCoreName} pos={pos}"))
			{
				string posHint = NetTrackingSystem.PosTargetHint(pos);
				var ar = AR_GetGroup();

				if (ar == null)
				{
					NetFlowDebugSystem.Warn(Layer.adcore, Module.format_ar, "ResolveGroup", () => "result=NULL => show=false");
					NetTrackingSystem.ShowAdCoreResolveFail(Channel.AppResume, TrackingReason.GroupMissing, posHint);
					return false;
				}

				ApplyTrackingChannel(ar, Channel.AppResume);
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_ar, "ResolveGroup", () => $"result=OK adtype={DebugAdTypeOf(ar)}");

				var ok = ar.Show(pos, onBeforeAdShow, onAdComplete);
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_ar, "Result", () => ok ? "show=true" : "show=false");

				return ok;
			}
		}

		public string AR_GetDebugInfo()
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_ar, "Core.GetDebugInfo", () => $"core={AdCoreName}"))
			{
				var g = AR_GetGroup();
				if (g == null)
				{
					NetFlowDebugSystem.Warn(Layer.adcore, Module.format_ar, "ResolveGroup", () => "result=NULL");
					return "[AR] Group null";
				}

				NetFlowDebugSystem.Log(Layer.adcore, Module.format_ar, "ResolveGroup", () => $"result=OK adtype={DebugAdTypeOf(g)}");
				return g.GetDebugInfo() ?? "";
			}
		}

		#endregion

		#region BN

		protected abstract IRectGroup BN_GetGroup(BannerPlacement placement);

		public void BN_Initialize(BannerPlacement placement)
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_bn, "Core.Init", () => $"core={AdCoreName} placement={placement}"))
			{
				string targetHint = BannerTrackingHint(placement);
				var bn = BN_GetGroup(placement);

				if (bn == null)
				{
					NetFlowDebugSystem.Warn(Layer.adcore, Module.format_bn, "ResolveGroup", () => "result=NULL");
					NetTrackingSystem.RequestAdCoreResolveFail(Channel.Banner, TrackingReason.GroupMissing, targetHint);

					return;
				}

				ApplyTrackingChannel(bn, Channel.Banner);
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_bn, "ResolveGroup", () => $"result=OK adtype={DebugAdTypeOf(bn)}");
				bn.Initialize();
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_bn, "Call", () => "IRectGroup.Initialize()");
			}
		}

		public void BN_ShowAd(BannerPlacement placement)
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_bn, "Core.Show(alias)", () => $"core={AdCoreName} placement={placement} -> ActivateView"))
			{
				BN_ActivateView(placement);
			}
		}

        public bool BN_ActivateView(BannerPlacement placement)
        {
            using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_bn, "Core.ActivateView", () => $"core={AdCoreName} placement={placement}"))
            {
                string posHint = BannerTrackingHint(placement);
                string trackingPos = NetTrackingSystem.BannerPlacementToken(placement);
                var bn = BN_GetGroup(placement);

                if (bn == null)
                {
                    NetFlowDebugSystem.Warn(Layer.adcore, Module.format_bn, "ResolveGroup", () => "result=NULL (Activate skipped)");
                    NetTrackingSystem.ActivateAdCoreResolveFail(Channel.Banner, TrackingReason.GroupMissing, posHint);
                    return false;
                }

                ApplyTrackingChannel(bn, Channel.Banner);
                NetFlowDebugSystem.Log(Layer.adcore, Module.format_bn, "ResolveGroup", () => $"result=OK adtype={DebugAdTypeOf(bn)}");
                var ok = bn.ActivateView(trackingPos);
                NetFlowDebugSystem.Log(Layer.adcore, Module.format_bn, "Result", () => ok ? "activate=true" : "activate=false");
                return ok;
            }
		}

		public void BN_HideAd(BannerPlacement placement)
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_bn, "Core.Hide", () => $"core={AdCoreName} placement={placement}"))
			{
				string posHint = BannerTrackingHint(placement);
				var bn = BN_GetGroup(placement);

				if (bn == null)
				{
					NetFlowDebugSystem.Warn(Layer.adcore, Module.format_bn, "ResolveGroup", () => "result=NULL (Hide skipped)");
					NetTrackingSystem.HideAdCoreResolveFail(Channel.Banner, TrackingReason.GroupMissing, posHint);
					return;
				}

				ApplyTrackingChannel(bn, Channel.Banner);
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_bn, "ResolveGroup", () => $"result=OK adtype={DebugAdTypeOf(bn)}");
				bn.Hide();
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_bn, "Call", () => "IRectGroup.Hide()");
			}
		}

		public bool BN_IsLoadedAt(BannerPlacement placement)
		{
			var bn = BN_GetGroup(placement);
			if (bn == null)
				return false;

			var loaded = bn.IsLoaded;
			return loaded;
		}

		public bool BN_Expand(BannerPlacement placement, bool enableClick = true)
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_bn, "Core.Expand", () => $"core={AdCoreName} placement={placement} enableClick={enableClick}"))
			{
				var bn = BN_GetGroup(placement);

				if (bn == null)
				{
					NetFlowDebugSystem.Warn(Layer.adcore, Module.format_bn, "ResolveGroup", () => "result=NULL (Expand skipped)");
					return false;
				}

				ApplyTrackingChannel(bn, Channel.Banner);
				var ok = bn.Expand(enableClick);
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_bn, "Result", () => ok ? "expand=true" : "expand=false");
				return ok;
			}
		}

		public string BN_GetDebugInfo(BannerPlacement placement)
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_bn, "Core.GetDebugInfo", () => $"core={AdCoreName} placement={placement}"))
			{
				var g = BN_GetGroup(placement);
				if (g == null)
				{
					NetFlowDebugSystem.Warn(Layer.adcore, Module.format_bn, "ResolveGroup", () => "result=NULL");
					return "[BN] Group null";
				}

				NetFlowDebugSystem.Log(Layer.adcore, Module.format_bn, "ResolveGroup", () => $"result=OK adtype={DebugAdTypeOf(g)}");
				return g.GetDebugInfo() ?? "";
			}
		}

		#endregion

		#region Mrec

		protected abstract IRectGroup Mrec_GetGroup();

		public void Mrec_Initialize()
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_mrec, "Core.Init", () => $"core={AdCoreName}"))
			{
				var mrec = Mrec_GetGroup();

				if (mrec == null)
				{
					NetFlowDebugSystem.Warn(Layer.adcore, Module.format_mrec, "ResolveGroup", () => "result=NULL");
					NetTrackingSystem.RequestAdCoreResolveFail(Channel.Mrec, TrackingReason.GroupMissing);

					return;
				}

				ApplyTrackingChannel(mrec, Channel.Mrec);
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_mrec, "ResolveGroup", () => $"result=OK adtype={DebugAdTypeOf(mrec)}");
				mrec.Initialize();
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_mrec, "Call", () => "IRectGroup.Initialize()");
			}
		}

		public void Mrec_ShowAd()
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_mrec, "Core.Show(alias)", () => $"core={AdCoreName} -> ActivateView"))
			{
				Mrec_ActivateView();
			}
		}

		public void Mrec_HideAd()
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_mrec, "Core.Hide", () => $"core={AdCoreName}"))
			{
				string posHint = NetTrackingSystem.PosTargetHint(NetTrackingSystem.MrecDefaultPos);
				var mrec = Mrec_GetGroup();

				if (mrec == null)
				{
					NetFlowDebugSystem.Warn(Layer.adcore, Module.format_mrec, "ResolveGroup", () => "result=NULL (Hide skipped)");
					NetTrackingSystem.HideAdCoreResolveFail(Channel.Mrec, TrackingReason.GroupMissing, posHint);
					return;
				}

				ApplyTrackingChannel(mrec, Channel.Mrec);
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_mrec, "ResolveGroup", () => $"result=OK adtype={DebugAdTypeOf(mrec)}");
				mrec.Hide();
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_mrec, "Call", () => "IRectGroup.Hide()");
			}
		}

		public bool Mrec_IsLoaded
		{
			get
			{
                var mrec = Mrec_GetGroup();
                if (mrec == null)
                    return false;

                var loaded = mrec.IsLoaded;
                return loaded;
            }
		}

        public bool Mrec_ActivateView()
        {
            using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_mrec, "Core.ActivateView", () => $"core={AdCoreName}"))
            {
                string posHint = NetTrackingSystem.PosTargetHint(NetTrackingSystem.MrecDefaultPos);
                var mrec = Mrec_GetGroup();

                if (mrec == null)
                {
                    NetFlowDebugSystem.Warn(Layer.adcore, Module.format_mrec, "ResolveGroup", () => "result=NULL (Activate skipped)");
                    NetTrackingSystem.ActivateAdCoreResolveFail(Channel.Mrec, TrackingReason.GroupMissing, posHint);
                    return false;
                }

                ApplyTrackingChannel(mrec, Channel.Mrec);
                NetFlowDebugSystem.Log(Layer.adcore, Module.format_mrec, "ResolveGroup", () => $"result=OK adtype={DebugAdTypeOf(mrec)}");
                var ok = mrec.ActivateView();
                NetFlowDebugSystem.Log(Layer.adcore, Module.format_mrec, "Result", () => ok ? "activate=true" : "activate=false");
                return ok;
            }
        }

		public Vector2 Mrec_GetSize()
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_mrec, "Core.GetSize", () => $"core={AdCoreName}"))
			{
				var mrec = Mrec_GetGroup();
				if (mrec == null)
					return Vector2.zero;

				NetFlowDebugSystem.Log(Layer.adcore, Module.format_mrec, "ResolveGroup", () => $"result=OK adtype={DebugAdTypeOf(mrec)}");
				return mrec.Mrec_GetSize();
			}
		}

		public void Mrec_UpdatePos(int pos)
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_mrec, "Core.UpdatePos", () => $"core={AdCoreName} mode=int pos={pos}"))
			{
				var mrec = Mrec_GetGroup();
				if (mrec == null)
				{
					NetFlowDebugSystem.Warn(Layer.adcore, Module.format_mrec, "ResolveGroup", () => "result=NULL (UpdatePos skipped)");
					return;
				}

				NetFlowDebugSystem.Log(Layer.adcore, Module.format_mrec, "ResolveGroup", () => $"result=OK adtype={DebugAdTypeOf(mrec)}");
				mrec.Mrec_UpdatePos(pos);
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_mrec, "Call", () => "IRectGroup.Mrec_UpdatePos(int)");
			}
		}

		public void Mrec_UpdatePos(GameObject targetObj, Camera camera = null)
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_mrec, "Core.UpdatePos", () => $"core={AdCoreName} mode=anchor target={(targetObj != null)} camera={(camera != null)}"))
			{
				var mrec = Mrec_GetGroup();
				if (mrec == null)
				{
					NetFlowDebugSystem.Warn(Layer.adcore, Module.format_mrec, "ResolveGroup", () => "result=NULL (UpdatePos skipped)");
					return;
				}

				NetFlowDebugSystem.Log(Layer.adcore, Module.format_mrec, "ResolveGroup", () => $"result=OK adtype={DebugAdTypeOf(mrec)}");
				mrec.Mrec_UpdatePos(targetObj, camera);
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_mrec, "Call", () => "IRectGroup.Mrec_UpdatePos(GameObject,Camera)");
			}
		}

		public string Mrec_GetDebugInfo()
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_mrec, "Core.GetDebugInfo", () => $"core={AdCoreName}"))
			{
				var g = Mrec_GetGroup();
				if (g == null)
				{
					NetFlowDebugSystem.Warn(Layer.adcore, Module.format_mrec, "ResolveGroup", () => "result=NULL");
					return "[MREC] Group null";
				}

				NetFlowDebugSystem.Log(Layer.adcore, Module.format_mrec, "ResolveGroup", () => $"result=OK adtype={DebugAdTypeOf(g)}");
				return g.GetDebugInfo() ?? "";
			}
		}

		#endregion

		#region PU

		protected abstract IRectGroup PU_GetGroup(string groupName);
		public abstract string PU_GroupByPos(string pos);
		public abstract string[] PU_GetListGroup();
		public abstract string[] PU_GetAutoInitGroupNames(BG_Library.NET.AdSystem.AdSystemConfigs.PopupChannelConfig channelConfig);

		public void PU_Initialize(string groupName, GroupAdType? knownAdType = null, string knownIdentitySource = "")
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_pu, "Core.Init", () => $"core={AdCoreName} group={groupName}"))
			{
				string groupHint = NetTrackingSystem.GroupTargetHint(groupName);
				var pu = PU_GetGroup(groupName);

				if (pu == null)
				{
					NetFlowDebugSystem.Warn(Layer.adcore, Module.format_pu, "ResolveGroup", () => "result=NULL");
                    NetTrackingSystem.RequestAdCoreResolveFail(Channel.Popup, TrackingReason.GroupMissing, groupHint);

                    return;
				}

				ApplyTrackingChannel(pu, Channel.Popup);
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_pu, "ResolveGroup", () => $"result=OK adtype={DebugAdTypeOf(pu)}");
				pu.Initialize();
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_pu, "Call", () => "IRectGroup.Initialize()");
			}
		}

		public void PU_ForceInitialize(string groupName, GroupAdType? knownAdType = null, string knownIdentitySource = "")
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_pu, "Core.ForceInit", () => $"core={AdCoreName} group={groupName}"))
			{
				string groupHint = NetTrackingSystem.GroupTargetHint(groupName);
				var pu = PU_GetGroup(groupName);

				if (pu == null)
				{
					NetFlowDebugSystem.Warn(Layer.adcore, Module.format_pu, "ResolveGroup", () => "result=NULL");
					NetTrackingSystem.RequestAdCoreResolveFail(Channel.Popup, TrackingReason.GroupMissing, groupHint);
					return;
				}

				ApplyTrackingChannel(pu, Channel.Popup);
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_pu, "ResolveGroup", () => $"result=OK adtype={DebugAdTypeOf(pu)}");
				var ok = pu.Rebuild();
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_pu, "Result", () => ok ? "force_init=true" : "force_init=false");
			}
		}

		public void PU_ShowAd(string groupName, string pos, GroupAdType? knownAdType = null, string knownIdentitySource = "")
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_pu, "Core.Show", () => $"core={AdCoreName} group={groupName} pos={pos}"))
			{
				string posHint = NetTrackingSystem.PosTargetHint(pos);
				var pu = PU_GetGroup(groupName);

				if (pu == null)
				{
					NetFlowDebugSystem.Warn(Layer.adcore, Module.format_pu, "ResolveGroup", () => "result=NULL (Show skipped)");
					NetTrackingSystem.ShowAdCoreResolveFail(Channel.Popup, TrackingReason.GroupMissing, posHint);
					return;
				}

				ApplyTrackingChannel(pu, Channel.Popup);
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_pu, "ResolveGroup", () => $"result=OK adtype={DebugAdTypeOf(pu)}");
				pu.Show(pos);
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_pu, "Call", () => "IRectGroup.Show(pos)");
			}
		}

		public void PU_HideAd(string groupName, string pos = "")
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_pu, "Core.Hide", () => $"core={AdCoreName} group={groupName}"))
			{
				string posHint = NetTrackingSystem.PosTargetHint(pos);
				var pu = PU_GetGroup(groupName);

				if (pu == null)
				{
					NetFlowDebugSystem.Warn(Layer.adcore, Module.format_pu, "ResolveGroup", () => "result=NULL (Hide skipped)");
					NetTrackingSystem.HideAdCoreResolveFail(Channel.Popup, TrackingReason.GroupMissing, posHint);
					return;
				}

				ApplyTrackingChannel(pu, Channel.Popup);
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_pu, "ResolveGroup", () => $"result=OK adtype={DebugAdTypeOf(pu)}");
				pu.Hide();
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_pu, "Call", () => "IRectGroup.Hide()");
			}
		}

		public bool PU_GetReady(string groupName)
		{
            var pu = PU_GetGroup(groupName);

            if (pu == null)
                return false;

            var loaded = pu.IsLoaded;
            return loaded;
        }

		public void PU_UpdatePos(string groupName, float xDp, float yDp, float w, float h)
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_pu, "Core.UpdatePos", () => $"core={AdCoreName} group={groupName} xDp={xDp:0.###} yDp={yDp:0.###} wDp={w:0.###} hDp={h:0.###}"))
			{
				var pu = PU_GetGroup(groupName);

				if (pu == null)
				{
					NetFlowDebugSystem.Warn(Layer.adcore, Module.format_pu, "ResolveGroup", () => "result=NULL (UpdatePos skipped)");
					return;
				}

				NetFlowDebugSystem.Log(Layer.adcore, Module.format_pu, "ResolveGroup", () => $"result=OK adtype={DebugAdTypeOf(pu)}");
				pu.Pu_UpdatePos(xDp, yDp, w, h);
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_pu, "Call", () => "IRectGroup.Pu_UpdatePos(xDp,yDp,w,h)");
			}
		}

		public string PU_GetDebugInfo(string groupName)
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_pu, "Core.GetDebugInfo", () => $"core={AdCoreName} group={groupName}"))
			{
				var g = PU_GetGroup(groupName);
				if (g == null)
				{
					NetFlowDebugSystem.Warn(Layer.adcore, Module.format_pu, "ResolveGroup", () => "result=NULL");
					return $"[PU] Group null | groupName={groupName}";
				}

				NetFlowDebugSystem.Log(Layer.adcore, Module.format_pu, "ResolveGroup", () => $"result=OK adtype={DebugAdTypeOf(g)}");
				return g.GetDebugInfo() ?? "";
			}
		}

		#endregion

		#region Collap

		protected abstract IRectGroup CL_GetGroup();

		public void CL_Initialize()
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_cl, "Core.Init", () => $"core={AdCoreName}"))
			{
				var cl = CL_GetGroup();

				if (cl == null)
				{
					NetFlowDebugSystem.Warn(Layer.adcore, Module.format_cl, "ResolveGroup", () => "result=NULL");
					NetTrackingSystem.RequestAdCoreResolveFail(Channel.Collap, TrackingReason.GroupMissing);

					return;
				}

				ApplyTrackingChannel(cl, Channel.Collap);
				ApplyTrackingChannel(cl, Channel.Collap);
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_cl, "ResolveGroup", () => $"result=OK adtype={DebugAdTypeOf(cl)}");
				cl.Initialize();
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_cl, "Call", () => "IRectGroup.Initialize()");
			}
		}

		public void CL_ShowAd()
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_cl, "Core.Show", () => $"core={AdCoreName}"))
			{
				string posHint = NetTrackingSystem.PosTargetHint(NetTrackingSystem.CollapDefaultPos);
				var cl = CL_GetGroup();

				if (cl == null)
				{
					NetFlowDebugSystem.Warn(Layer.adcore, Module.format_cl, "ResolveGroup", () => "result=NULL (Show skipped)");
					NetTrackingSystem.ShowAdCoreResolveFail(Channel.Collap, TrackingReason.GroupMissing, posHint);
					return;
				}

				ApplyTrackingChannel(cl, Channel.Collap);
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_cl, "ResolveGroup", () => $"result=OK adtype={DebugAdTypeOf(cl)}");
				cl.Show();
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_cl, "Call", () => "IRectGroup.Show()");
			}
		}

		public void CL_HideAd()
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_cl, "Core.Hide", () => $"core={AdCoreName}"))
			{
				string posHint = NetTrackingSystem.PosTargetHint(NetTrackingSystem.CollapDefaultPos);
				var cl = CL_GetGroup();

				if (cl == null)
				{
					NetFlowDebugSystem.Warn(Layer.adcore, Module.format_cl, "ResolveGroup", () => "result=NULL (Hide skipped)");
					NetTrackingSystem.HideAdCoreResolveFail(Channel.Collap, TrackingReason.GroupMissing, posHint);
					return;
				}

				ApplyTrackingChannel(cl, Channel.Collap);
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_cl, "ResolveGroup", () => $"result=OK adtype={DebugAdTypeOf(cl)}");
				cl.Hide();
				NetFlowDebugSystem.Log(Layer.adcore, Module.format_cl, "Call", () => "IRectGroup.Hide()");
			}
		}

		public bool CL_IsLoaded
		{
			get
			{
                var cl = CL_GetGroup();
                if (cl == null)
                    return false;

                var loaded = cl.IsLoaded;
                return loaded;
            }
		}

		public string CL_GetDebugInfo()
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.format_cl, "Core.GetDebugInfo", () => $"core={AdCoreName}"))
			{
				var g = CL_GetGroup();
				if (g == null)
				{
					NetFlowDebugSystem.Warn(Layer.adcore, Module.format_cl, "ResolveGroup", () => "result=NULL");
					return "[CL] Group null";
				}

				NetFlowDebugSystem.Log(Layer.adcore, Module.format_cl, "ResolveGroup", () => $"result=OK adtype={DebugAdTypeOf(g)}");
				return g.GetDebugInfo() ?? "";
			}
		}

		public bool AL_TryGetTrackingIdentity(out GroupAdType adType, out string identitySource)
		=> TryResolveTrackingIdentity(AL_GetGroup(), Channel.AppLaunch, out adType, out identitySource);

		public bool AR_TryGetTrackingIdentity(out GroupAdType adType, out string identitySource)
		=> TryResolveTrackingIdentity(AR_GetGroup(), Channel.AppResume, out adType, out identitySource);

		public bool RW_TryGetTrackingIdentity(out GroupAdType adType, out string identitySource)
		=> TryResolveTrackingIdentity(RW_GetGroup(), Channel.Rewarded, out adType, out identitySource);

		public bool BN_TryGetTrackingIdentity(BannerPlacement placement, out GroupAdType adType, out string identitySource)
		=> TryResolveTrackingIdentity(BN_GetGroup(placement), Channel.Banner, out adType, out identitySource);

		public bool Mrec_TryGetTrackingIdentity(out GroupAdType adType, out string identitySource)
		=> TryResolveTrackingIdentity(Mrec_GetGroup(), Channel.Mrec, out adType, out identitySource);

		public bool CL_TryGetTrackingIdentity(out GroupAdType adType, out string identitySource)
		=> TryResolveTrackingIdentity(CL_GetGroup(), Channel.Collap, out adType, out identitySource);

		public bool FA_TryGetTrackingIdentity(string groupName, out GroupAdType adType, out string identitySource)
		=> TryResolveTrackingIdentity(FA_GetGroup(groupName), Channel.ForceAd, out adType, out identitySource);

		public bool PU_TryGetTrackingIdentity(string groupName, out GroupAdType adType, out string identitySource)
		=> TryResolveTrackingIdentity(PU_GetGroup(groupName), Channel.Popup, out adType, out identitySource);

		#endregion
	}

	public interface IGroup
	{
			string Id { get; }
			string Adtype { get; }
			GroupAdType TrackingAdType { get; }
			string TrackingIdentitySource { get; }
			void SetTrackingChannel(Channel channel);
			string GetDebugInfo();
		}

	public interface IFSGroup : IGroup
	{
		void Initialize();
		bool ForceInitialize();
		bool GetReady();
		bool Show(string pos, Action OnBeforeAdShow = null, Action OnAdShowComplete = null);
	}

	public interface IRectGroup : IGroup
	{
		void Initialize();
		bool Rebuild();
		bool IsLoaded { get; }
        bool ActivateView(string pos = "");
		bool Expand(bool enableClick = true);
		void Show(string pos = "");
		void Hide();

		Vector2 Mrec_GetSize();
		void Mrec_UpdatePos(int pos);
		void Mrec_UpdatePos(GameObject targetObj, Camera camera = null);
		void Pu_UpdatePos(float xDp, float yDp, float w, float h);
	}
}
