using Sirenix.OdinInspector;
using UnityEngine;
using BG_Library.NET.AdSystem;
using BG_Library.NET;
using BG_Library.NET.Debug;

namespace BG_Library.Common
{
	public enum EOAAction
	{
		NONE, RATE, IAP, ATT, NOTIFICATION
	}

	[HideReferenceObjectPicker, HideLabel]
	public class BG_SETUP : MonoBehaviour
	{
		static BG_SETUP ins;
        private static bool srDebugInitialized;
		public static BG_SETUP Ins
		{
			get
			{
				if(ins == null)
                    ins = FindAnyObjectByType<BG_SETUP>();

				return ins;
			}
			private set
			{
				ins = value;
			}
		}

		public static EOAAction OpenAdAction = EOAAction.NONE;

		private void Awake()
		{
			DontDestroyOnLoad(gameObject);
			Ins = this;

			RefreshRuntimeDebugState();
        }

		public static void RefreshRuntimeDebugState()
		{
            var configs = NetConfigsSO.Ins;
            if (configs == null)
                return;

            bool debugEnabled = NetFlowDebugSystem.IsEnabledForPreset(configs.Debug_Preset);

#if !DISABLE_SRDEBUGGER
            if (debugEnabled && !srDebugInitialized)
            {
                SRDebug.Init();
                srDebugInitialized = true;
            }
#endif

            NetFlowDebugSystem.ReloadFromConfig();
            NetEventsBinder.RefreshTrackingRouting();

            Debug.unityLogger.logEnabled = debugEnabled;
        }
	}
}
