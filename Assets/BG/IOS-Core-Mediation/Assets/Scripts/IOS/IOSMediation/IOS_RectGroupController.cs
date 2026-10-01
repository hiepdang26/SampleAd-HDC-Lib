using BG_Library.NET.Debug;
using BG_Library.NET.Mediation.Base;
using BG_Library.NET.Tracking;

namespace BG_Library.NET.Mediation.IOS
{
	public class IOS_RectGroupController<T> : Rect_GroupControllerBase<T>
		where T : IOS_RectBaseInfo
	{
		private IOS_RectLogic<T> iosLogic;

		public IOS_RectGroupController(T info, string adType, string groupName, string mediation)
			: base(info, adType, groupName, mediation)
		{
			DisablePostInitReload = info.DisablePostInitReload;
		}

		protected override void MediationSetup()
		{
			using (NetFlowDebugSystem.Flow(Layer.group, Module.rect_group, $"Group.MediationSetup {GroupName}",
				() => $"adtype={Adtype} id={Id} logicCreated={(iosLogic != null)}"))
			{
				EnsureLogic();

				NetFlowDebugSystem.Log(Layer.group, Module.rect_group, $"Group.MediationSetup {GroupName}",
					() => $"logicCreated={(iosLogic != null)} attach={iosLogic?.GetAttachStateShort()}");
			}
		}

		protected override void API_RequestAd()
		{
			using (NetFlowDebugSystem.Flow(Layer.group, Module.rect_group, $"Group.API_RequestAd {GroupName}",
				() => $"adtype={Adtype} id={Id}"))
			{
				EnsureLogic();
				iosLogic.CreateAndLoad();
			}
		}

		protected override void API_Show()
		{
			using (NetFlowDebugSystem.Flow(Layer.group, Module.rect_group, $"Group.API_Show {GroupName}",
				() => $"adtype={Adtype} id={Id}"))
			{
				EnsureLogic();
				iosLogic.ShowAd();
			}
		}

		protected override void API_Hide()
		{
			using (NetFlowDebugSystem.Flow(Layer.group, Module.rect_group, $"Group.API_Hide {GroupName}",
				() => $"adtype={Adtype} id={Id}"))
			{
				EnsureLogic();
				iosLogic.HideAd();
			}
		}

		protected override bool API_Expand(bool enableClick)
		{
			using (NetFlowDebugSystem.Flow(Layer.group, Module.rect_group, $"Group.API_Expand {GroupName}",
				() => $"adtype={Adtype} id={Id} enableClick={enableClick}"))
			{
				EnsureLogic();
				return iosLogic.ExpandAd(enableClick);
			}
		}

		protected override void API_DestroyAd()
		{
			using (NetFlowDebugSystem.Flow(Layer.group, Module.rect_group, $"Group.API_DestroyAd {GroupName}",
				() => $"adtype={Adtype} id={Id}"))
			{
				iosLogic?.DestroyAd();
			}
		}

		protected override bool TryValidateShowBeforeApi(string pos, bool isActivateFlow, out TrackingReason reason, out string detail)
		{
			if (iosLogic != null && !iosLogic.TryValidatePopupLayoutBeforeShow(out reason, out detail))
				return false;

			return base.TryValidateShowBeforeApi(pos, isActivateFlow, out reason, out detail);
		}

		public override void Pu_UpdatePos(float xDp, float yDp, float w, float h)
		{
			using (NetFlowDebugSystem.Flow(Layer.group, Module.rect_group, $"Group.PU_UpdatePos {GroupName}",
				() => $"adtype={Adtype} x={xDp:0.##} y={yDp:0.##} w={w:0.##} h={h:0.##}"))
			{
				EnsureLogic();
				iosLogic.UpdatePUPosition(xDp, yDp, w, h);
			}
		}

		public override string GetDebugInfo()
		{
			var s = base.GetDebugInfo();
			if (string.IsNullOrEmpty(s)) s = "";

			s += "\n=== IOS Rect Extra ===";
			s += $"\nlogicCreated: {(iosLogic != null)}";

			if (iosLogic != null)
			{
				s += $"\nattach: {iosLogic.GetAttachStateShort()}";
				s += $"\nidKey: {iosLogic.GetIdKeySafe()}";
				s += $"\nhasBridgeInstance: {iosLogic.HasBridgeInstance}";
				s += $"\nbridgeInstance: {iosLogic.GetBridgeInstanceStateShort()}";
			}

			return s;
		}

		private void EnsureLogic()
		{
			if (iosLogic == null)
				iosLogic = new IOS_RectLogic<T>(this);
		}
	}
}
