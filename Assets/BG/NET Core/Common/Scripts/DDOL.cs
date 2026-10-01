using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BG_Library.Common
{
	public class DDOL : MonoBehaviour
	{
		/// <summary>
		/// bool: pause
		/// </summary>
		public System.Action<bool> OnApplicationPauseE;

		/// <summary>
		/// Lan dau quay tro lai game, khong duoc goi neu nhu DDOL chua co
		/// </summary>
		public System.Action OnBackToTheGame;
		/// <summary>
		/// Khi app bi out ra ngoai => HOME
		/// </summary>
		public System.Action OnAppPause;
		/// <summary>
		/// Khi quay tro lai app sau khi Pause
		/// </summary>
		public System.Action OnAppResume;

		public System.Action OnApplicationQuitE;
		public System.Action OnUpdateE;

		static DDOL _instance;
		public static DDOL Instance
		{
			get
			{
				Setup();
				return _instance;
			}
		}

		static void Setup()
		{
			if (_instance != null)
			{
				return;
			}

			var obj = new GameObject("DDOL");
			_instance = obj.AddComponent<DDOL>();
			DontDestroyOnLoad(obj);
		}

		[SerializeField] AppState state = AppState.OpenApp;

		private void Awake()
		{
			if (_instance == null)
			{
				_instance = this;
				DontDestroyOnLoad(gameObject);
			}
		}

        private void Start()
        {
            
        }

        private void OnApplicationPause(bool pause)
		{
			//Log.Normal($"BACK TO THE GAME {pause}");

			if (pause) // Out ra ngoai
			{
				state = AppState.PauseApp;
				OnAppPause?.Invoke();
			}
			else // Vao lai game
			{
				if (state == AppState.PauseApp)
				{
					state = AppState.ResumeApp;
				}

				if (state == AppState.ResumeApp) // => Neu nhu quay tro lai game sau khi Pause app, chu khong phai Open app => Resume
				{
					OnAppResume?.Invoke();
				}
				else if(state == AppState.OpenApp) // Lan dau tien mo game len
                {
					OnBackToTheGame?.Invoke();
                }
			}

			OnApplicationPauseE?.Invoke(pause);
		}

		private void OnApplicationQuit()
		{
			OnApplicationQuitE?.Invoke();
		}

		private void Update()
		{
			OnUpdateE?.Invoke();
		}

		public void Preload()
		{

		}

/*#if UNITY_EDITOR
		[UnityEditor.MenuItem("BG/Create DDOL")]
		static void CreateDDOL()
		{
			var obj = new GameObject("DDOL");
			obj.AddComponent<DDOL>();
		}
#endif*/
	}

	public enum AppState
	{
		OpenApp = 0,
		PauseApp = 1,
		ResumeApp = 2
	}
}