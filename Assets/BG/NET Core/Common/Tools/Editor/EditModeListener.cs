using BG_Library.Common;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BG_Library.Common
{
	[InitializeOnLoadAttribute]
	public static class EditModeListener
	{
		static EditModeListener()
		{
			EditorApplication.hierarchyWindowItemOnGUI += OnHierarchy;
		}

		static bool isPress = false;
		static bool isCopied = false;

		static Vector3[] position;
		static Vector3[] scale;
		static Vector3[] eulerAngle;

		static void OnHierarchy(int instanceID, Rect selectionRect)
		{
			ClickKey(KeyCode.Alpha1, () =>
			{
				var length = Selection.objects.Length;
				position = new Vector3[length];
				scale = new Vector3[length];
				eulerAngle = new Vector3[length];

				for (int i = 0; i < length; i++)
				{
					position[i] = Selection.transforms[i].position;
					scale[i] = Selection.transforms[i].localScale;
					eulerAngle[i] = Selection.transforms[i].eulerAngles;
				}

				isCopied = true;
				if (length == 1)
				{
					Debug.Log($"COPY TRANSFORM {Selection.gameObjects[0].name}");
				}
				else if (length > 1)
				{
					Debug.Log($"COPY TRANSFORM {length} OBJECTS");
				}
			});
			ClickKey(KeyCode.Alpha2, () =>
			{
				if (!isCopied)
				{
					Debug.Log("NOT COPIED YET");
					return;
				}

				var length = Selection.objects.Length;
				if (length != position.Length)
				{
					Debug.Log("NOT FIT NUMBER OF SELECTED OBJECTS");
					return;
				}

				for (int i = 0; i < length; i++)
				{
					Selection.transforms[i].position = position[i];
					Selection.transforms[i].localScale = scale[i];
					Selection.transforms[i].eulerAngles = eulerAngle[i];

					EditorUtility.SetDirty(Selection.gameObjects[i]);
				}

				if (length == 1)
				{
					Debug.Log($"PASTE TRANSFORM => {Selection.gameObjects[0].name}");
				}
				else if (length > 1)
				{
					Debug.Log($"PASTE TRANSFORM => {length} OBJECTS");
				}
			});
			ClickKey(KeyCode.Alpha0, () =>
			{
				var b = !Selection.gameObjects[0].activeSelf;

				for (int i = 0; i < Selection.objects.Length; i++)
				{
					Selection.gameObjects[i].SetActive(b);
					EditorUtility.SetDirty(Selection.gameObjects[i]);
				}
			});
		}

		static void ClickKey(KeyCode keyCode, System.Action action)
		{
			Event e = Event.current;
			if (e == null || e.shift || e.control || e.command || e.alt)
			{
				return;
			}

			switch (e.type)
			{
				case EventType.KeyDown:
					{
						if (Event.current.keyCode == keyCode)
						{
							if (isPress)
							{
								break;
							}
							isPress = true;

							action?.Invoke();
						}
						break;
					}
				case EventType.KeyUp:
					{
						if (Event.current.keyCode == keyCode)
						{
							if (!isPress)
							{
								break;
							}
							isPress = false;
						}
						break;
					}
			}
		}
	}
}
