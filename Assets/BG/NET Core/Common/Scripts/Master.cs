using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BG_Library.Common
{
    public static class Master
    {
        #region Event strigger
        public static void AddEventTriggerListener(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> callback)
        {
            var _trigger = trigger.triggers.Find(e => e.eventID == eventType);
            if (_trigger == null)
            {
                EventTrigger.Entry entry = new EventTrigger.Entry
                {
                    eventID = eventType,
                    callback = new EventTrigger.TriggerEvent()
                };
                entry.callback.AddListener(callback);
                trigger.triggers.Add(entry);
            }
            else
            {
                _trigger.callback.AddListener(callback);
            }
        }

        public static void RemoveEventTriggerListener(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> callback)
        {
            var _trigger = trigger.triggers.Find(e => e.eventID == eventType);
            if (_trigger == null)
                return;
            _trigger.callback.RemoveListener(callback);
        }

        public static void RemoveAllEventTriggerListener(EventTrigger trigger, EventTriggerType eventType)
        {
            _ = trigger.triggers.RemoveAll(e => e.eventID == eventType);
        }
        #endregion

        #region Get hierarchy object
        public static GameObject GetChildByName(GameObject parent, string childName)
        {
            foreach (Transform item in parent.GetComponentsInChildren<Transform>(true))
            {
                string childNameItem = item.gameObject.name;
                if (childNameItem == childName)
                {
                    return item.gameObject;
                }
            }
            return null;
        }

        public static List<T> FindAllObjectsOfType<T>()
        {
            List<T> results = new List<T>();
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().ToList()
                .ForEach(g => results.AddRange(g.GetComponentsInChildren<T>(true)));
            return results;
        }
        #endregion

        #region convert time
        public static string ConvertSecond(int second)
        {
            System.TimeSpan time = System.TimeSpan.FromSeconds(second);

            if (second < 60)
            {
                return time.ToString(@"ss");
            }
            else if (second >= 60 && second < 3600)
            {
                return time.ToString(@"mm\:ss");
            }
            else if (second >= 3600 && second < 86400)
                return time.ToString(@"hh\:mm\:ss");
            else if (second >= 86400)
                return time.Days + "d " + time.ToString(@"hh\:mm\:ss");

            return "";
        }
        public static string ConvertSecond2(int second)
        {
            System.TimeSpan time = System.TimeSpan.FromSeconds(second);
            return time.ToString(@"mm\:ss");
        }

        public static string ConvertSecondWithPara(int second)
        {
            System.TimeSpan time = System.TimeSpan.FromSeconds(second);

            return second < 60
                ? second.ToString() + "s"
                : second >= 60 && second < 3600
                ? string.Format("{0:D2}m {1:D2}s", time.Minutes, time.Seconds)
                : second >= 3600 && second < 86400
                ? string.Format("{0:D2}h {1:D2}m {2:D2}s", time.Hours, time.Minutes, time.Seconds)
                : second >= 86400 ? "> 2 days" : "";
        }

        public static string ConvertMinute(int minute)
        {
            System.TimeSpan time = System.TimeSpan.FromMinutes(minute);

            if (minute < 60)
            {
                return minute.ToString() + "m";
            }
            else if (minute >= 60 && minute < 1440)
            {
                var st = time.Hours + "h";
                if (time.Minutes != 0) st += " " + time.Minutes + "m";

                return st;
            }
            else if (minute >= 1440)
            {
                var st = time.Days + "d";
                if (time.Hours != 0) st += " " + time.Hours + "h";
                if (time.Minutes != 0) st += " " + time.Minutes + "m";

                return st;
            }

            return "";
        }
        #endregion

        #region Application
        public static bool IsAndroid
        {
            get
            {
                if (Application.platform == RuntimePlatform.Android
                    || Application.platform == RuntimePlatform.WindowsEditor)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static bool IsPortrait
        {
			get
			{
                return Screen.width < Screen.height;
            }
        }
		#endregion

		#region MISSING
		public static List<int> DetectUnityEventError(UnityEvent e)
		{
			var listMissing = new List<int>();
			for (int i = 0; i < e.GetPersistentEventCount(); i++)
			{
				//Get the target class name
				UnityEngine.Object objectName = e.GetPersistentTarget(i);

				//Get the function name
				string methodName = e.GetPersistentMethodName(i); ;

				//////////////////////////////////////////////////////CHECK CLASS/SCRIPT EXISTANCE/////////////////////////////////////////

				//Check if the class that holds the function is null then exit if it is 
				if (objectName == null)
				{
					listMissing.Add(i);
					continue; //Don't run code below
				}

				//Get full target class name(including namespace)
				string objectFullNameWithNamespace = objectName.GetType().FullName;

				//Check if the class that holds the function exist then exit if it does not
				if (!ClassExist(objectFullNameWithNamespace))
				{
					listMissing.Add(i);
					continue; //Don't run code below
				}

				//////////////////////////////////////////////////////CHECK FUNCTION EXISTANCE/////////////////////////////////////////

				//Check if function Exist as public (the registered onClick function is ok if this returns true)
				if (FunctionExistAsPublicInTarget(objectName, methodName))
				{
					//No Need to Log if function exist
					//Debug.Log("<color=green>Function Exist</color>");
				}
				//Check if function Exist as private 
				else if (FunctionExistAsPrivateInTarget(objectName, methodName))
				{
					listMissing.Add(i);
				}
				//Function does not even exist at-all
				else
				{
					listMissing.Add(i);
				}
			}

			return listMissing;
		}

		//Checks if class exit or has been renamed
		static bool ClassExist(string className)
		{
			Type myType = Type.GetType(className);
			return myType != null;
		}

		//Checks if functions exist as public function
		static bool FunctionExistAsPublicInTarget(UnityEngine.Object target, string functionName)
		{
			try
			{
				Type type = target.GetType();
				MethodInfo targetinfo = type.GetMethod(functionName);
				return targetinfo != null;
			}
			catch (AmbiguousMatchException)
			{
				return true;
			}
		}

		//Checks if functions exist as private function
		static bool FunctionExistAsPrivateInTarget(UnityEngine.Object target, string functionName)
		{
			Type type = target.GetType();
			MethodInfo targetinfo = type.GetMethod(functionName, BindingFlags.Instance | BindingFlags.NonPublic);
			return targetinfo != null;
		}
		#endregion

		#region Others
		public static float GetScreenDensity()
		{
#if UNITY_ANDROID && !UNITY_EDITOR
            using (var metricsClass = new AndroidJavaClass("android.util.DisplayMetrics"))
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var metrics = new AndroidJavaObject("android.util.DisplayMetrics"))
            {
                var windowManager = activity.Call<AndroidJavaObject>("getWindowManager");
                var display = windowManager.Call<AndroidJavaObject>("getDefaultDisplay");
                display.Call("getMetrics", metrics);
                return metrics.Get<float>("density");
            }
#elif UNITY_IOS && !UNITY_EDITOR
            return UnityEngine.Screen.dpi / 160f;
#else
			return UnityEngine.Screen.dpi > 0 ? UnityEngine.Screen.dpi / 160f : 1f;
#endif
		}

		public static bool IsValidStringArray(string[] arr)
		{
			if (arr == null || arr.Length == 0)
				return false;

			for (int i = 0; i < arr.Length; i++)
			{
				if (string.IsNullOrEmpty(arr[i]))
					return false;
			}

			return true;
		}

		public static string ConvertString(string inputString)
		{
			if (string.IsNullOrEmpty(inputString))
			{
				return string.Empty; // Trả về chuỗi rỗng nếu chuỗi đầu vào rỗng
			}

			// Kiểm tra ký tự đầu tiên có phải là số không
			if (char.IsDigit(inputString[0]))
			{
				// Xóa ký tự đầu tiên nếu là số
				inputString = inputString.Substring(1);
			}

			// Thay thế khoảng trắng bằng dấu "_"
			inputString = inputString.Replace(" ", "_");

			// Loại bỏ tất cả các ký tự không phải chữ cái, số hoặc dấu gạch dưới
			string pattern = @"[^a-zA-Z0-9_]"; // Lưu ý dấu "_" trong biểu thức chính quy
			string result = Regex.Replace(inputString, pattern, "");

			// Chuyển đổi toàn bộ chuỗi thành chữ thường
			result = result.ToLower();

			// Cắt chuỗi nếu quá dài
			if (result.Length > 30)
			{
				result = result.Substring(0, 30);
			}

			return result;
		}

		public static void ChangeAlpha(Image ima, float a)
        {
            var color = ima.color;
            color.a = a;
            ima.color = color;
        }

        public static void ChangeAlpha(SpriteRenderer spriteRen, float a)
        {
            var color = spriteRen.color;
            color.a = a;
            spriteRen.color = color;
        }

        public static void ChangeAlpha(Text text, float a)
        {
            var color = text.color;
            color.a = a;
            text.color = color;
        }

        public static string ColorRichText(string st, string hexaColor)
        {
            return "<color=#" + hexaColor + ">" + st + "</color>";
        }

        public static Canvas GetTopmostCanvas(Component component)
        {
            Canvas[] parentCanvases = component.GetComponentsInParent<Canvas>();
            if (parentCanvases != null && parentCanvases.Length > 0)
            {
                return parentCanvases[parentCanvases.Length - 1];
            }
            return null;
        }

        public static Canvas GetNearestCanvas(Component component)
        {
            Canvas[] parentCanvases = component.GetComponentsInParent<Canvas>();
            if (parentCanvases != null && parentCanvases.Length > 0)
            {
                return parentCanvases[0];
            }
            return null;
        }

        public static bool HasTouchScreen()
        {
            return Input.GetMouseButton(0);
        }

        /// <summary>
        /// Set pivot without change posion in scene
        /// </summary>
        public static void SetPivot(RectTransform rectTransform, Vector2 pivot)
        {
            Vector3 deltaPosition = rectTransform.pivot - pivot;    // get change in pivot
            deltaPosition.Scale(rectTransform.rect.size);           // apply sizing
            deltaPosition.Scale(rectTransform.localScale);          // apply scaling
            deltaPosition = rectTransform.rotation * deltaPosition; // apply rotation

            rectTransform.pivot = pivot;                            // change the pivot
            rectTransform.localPosition -= deltaPosition;           // reverse the position change
        }
        public static string GetOrientationString()
        {
	        // 1) Ưu tiên check theo kích thước thực tế (ổn định nhất)
	        if (Screen.width > Screen.height) return "landscape";
	        if (Screen.height > Screen.width) return "portrait";

	        // 2) Trường hợp hiếm: vuông (w == h) -> fallback theo orientation
	        return Screen.orientation switch
	        {
		        ScreenOrientation.LandscapeLeft or ScreenOrientation.LandscapeRight => "landscape",
		        ScreenOrientation.Portrait or ScreenOrientation.PortraitUpsideDown => "portrait",
		        _ => "landscape" // default an toàn (m có thể đổi tùy game)
	        };
        }
#if UNITY_EDITOR
        public static T CreateSOInstance<T>(string assetPath) where T: ScriptableObject
		{
            T temp = ScriptableObject.CreateInstance<T>();

            UnityEditor.AssetDatabase.CreateAsset(temp, assetPath);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            UnityEditor.EditorUtility.FocusProjectWindow();

            return temp;
        }

        public static void CreateAndSelectAssetInResource<T>(string assetName) where T : ScriptableObject
		{
            var settings = LoadSource.LoadObject<T>(assetName);
            if (settings == null)
            {
                UnityEditor.Selection.activeObject = CreateSOInstance<T>($"Assets/Resources/{assetName}.asset");
            }
            else
            {
                UnityEditor.Selection.activeObject = settings;
            }
        }
#endif

		#endregion
	}

	public enum LogLevel
	{
		Info,
		Warning,
		Error
	}
}