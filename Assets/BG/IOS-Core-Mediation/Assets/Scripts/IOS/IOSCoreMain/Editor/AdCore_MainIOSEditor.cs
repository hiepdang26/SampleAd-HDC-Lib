#if UNITY_EDITOR
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace BG_Library.NET.AdCore.MainIOS
{
    [CustomEditor(typeof(AdCore_MainIOS))]
    public class AdCore_MainIOSEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(10);
            SirenixEditorGUI.HorizontalLineSeparator();

            if (GUILayout.Button("OPEN EDITOR", GUILayout.Height(32)))
            {
                EditIOSConfigsWindow.Open();
            }
        }
    }
}
#endif
