#if UNITY_EDITOR
using BG_Library.NET.AdCore.MainAndroid; // namespace có EditConfigsWindow
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

// Custom inspector cho AdCore_MainScripts
[CustomEditor(typeof(AdCore_MainAndroid))]
public class AdCore_MainScriptsEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		GUILayout.Space(10);

		SirenixEditorGUI.HorizontalLineSeparator();

		// Button mở window
		if (GUILayout.Button("OPEN EDITOR", GUILayout.Height(32)))
		{
			EditConfigsWindow.Open();
		}
	}
}
#endif