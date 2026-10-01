/*
using System;
using System.Collections.Generic;
using BG_Library.NET.Mediation.Android;
using UnityEditor;
using UnityEngine;

namespace BG_Library.NET.AdCore.MainAndroid
{
    [CustomPropertyDrawer(typeof(LayoutConfig))]
    public class LayoutConfigPropertyDrawer : PropertyDrawer
    {
        const string RootAssetConfigsPath = "data.assetsConfig.configs";

        static readonly GUIContent LayoutLabel = new GUIContent("Layout");
        static readonly GUIContent LayoutTimeLabel = new GUIContent("Layout Time");
        static readonly GUIContent DelayLabel = new GUIContent("Delay");
        static readonly GUIContent TimeUpCLabel = new GUIContent("Time Up C");
        static readonly GUIContent AssetsConfigLabel = new GUIContent("Assets Config");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var layoutProperty = property.FindPropertyRelative("layout");
            var title = !string.IsNullOrEmpty(layoutProperty?.stringValue)
                ? new GUIContent(layoutProperty.stringValue)
                : label;

            var line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(line, property.isExpanded, title, true);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                DrawProperty(ref line, property.FindPropertyRelative("layout"), LayoutLabel);
                DrawProperty(ref line, property.FindPropertyRelative("layoutTime"), LayoutTimeLabel);
                DrawProperty(ref line, property.FindPropertyRelative("delay"), DelayLabel);
                DrawProperty(ref line, property.FindPropertyRelative("timeUpC"), TimeUpCLabel);
                DrawAssetsConfigDropdown(ref line, property.FindPropertyRelative("assetConfigName"), property);
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var line = EditorGUIUtility.singleLineHeight;
            if (!property.isExpanded)
                return line;

            return (line + EditorGUIUtility.standardVerticalSpacing) * 6f - EditorGUIUtility.standardVerticalSpacing;
        }

        static void DrawProperty(ref Rect line, SerializedProperty property, GUIContent label)
        {
            MoveNextLine(ref line);
            if (property != null)
                EditorGUI.PropertyField(line, property, label);
        }

        static void DrawAssetsConfigDropdown(ref Rect line, SerializedProperty assetConfigNameProperty, SerializedProperty layoutProperty)
        {
            MoveNextLine(ref line);

            if (assetConfigNameProperty == null)
                return;

            var values = GetAssetConfigNames(layoutProperty);
            if (values.Length == 0)
            {
                EditorGUI.PropertyField(line, assetConfigNameProperty, AssetsConfigLabel);
                return;
            }

            var labels = new GUIContent[values.Length];
            for (var i = 0; i < values.Length; i++)
            {
                labels[i] = new GUIContent(string.IsNullOrEmpty(values[i]) ? "(None)" : values[i]);
            }

            var current = assetConfigNameProperty.stringValue ?? "";
            var selected = Array.IndexOf(values, current);
            if (selected < 0)
                selected = 0;

            var next = EditorGUI.Popup(line, AssetsConfigLabel, selected, labels);
            if (next >= 0 && next < values.Length && !string.Equals(current, values[next], StringComparison.Ordinal))
                assetConfigNameProperty.stringValue = values[next];
        }

        static string[] GetAssetConfigNames(SerializedProperty property)
        {
            var values = new List<string> { "" };
            var configsProperty = property.serializedObject.FindProperty(RootAssetConfigsPath);
            if (configsProperty == null || !configsProperty.isArray)
                return values.ToArray();

            for (var i = 0; i < configsProperty.arraySize; i++)
            {
                var configProperty = configsProperty.GetArrayElementAtIndex(i);
                var nameProperty = configProperty.FindPropertyRelative("configName");
                var name = nameProperty?.stringValue;
                if (!string.IsNullOrEmpty(name) && !values.Contains(name))
                    values.Add(name);
            }

            return values.ToArray();
        }

        static void MoveNextLine(ref Rect line)
        {
            line.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}
*/
