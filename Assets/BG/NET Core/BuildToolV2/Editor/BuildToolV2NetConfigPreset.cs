using System.Collections.Generic;
using BG_Library.NET.AdSystem;
using BG_Library.NET.Debug;
using UnityEditor;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolV2NetConfigPreset
    {
        public static bool ApplyNow(BuildProfileV2 profile, NetConfigsSO netConfigs)
        {
            var steps = new List<string>();
            var dirtyAssetPaths = new HashSet<string>();
            Apply(profile, netConfigs, steps, dirtyAssetPaths);
            return dirtyAssetPaths.Count > 0;
        }

        public static void Apply(
            BuildProfileV2 profile,
            NetConfigsSO netConfigs,
            List<string> steps,
            HashSet<string> dirtyAssetPaths)
        {
            if (profile == null)
            {
                steps.Add("Skipped NET config preset because profile is missing.");
                return;
            }

            if (netConfigs == null)
            {
                steps.Add("Skipped NET config preset because NetConfigsSO is missing.");
                return;
            }

            var serializedObject = new SerializedObject(netConfigs);
            DebugPreset targetPreset = profile.DebugPreset;
            SerializedProperty debugPresetProperty = serializedObject.FindProperty("debug_Preset");
            bool debugPresetChanged = debugPresetProperty != null && debugPresetProperty.enumValueIndex != (int)targetPreset;
            if (debugPresetChanged)
                debugPresetProperty.enumValueIndex = (int)targetPreset;

            bool buildHackChanged = BuildToolV2SceneSaveAndSummary.SetBool(serializedObject, "build_Hack", profile.BuildHack);
            bool admobTestDeviceChanged = BuildToolV2SceneSaveAndSummary.SetBool(serializedObject, "admob_testDevice", profile.UsesAdmobTestDevice);
            bool admobTestIdChanged = BuildToolV2SceneSaveAndSummary.SetBool(serializedObject, "admob_testId", profile.UsesAdmobTestIds);
            bool trackingSendToFirebaseChanged = BuildToolV2SceneSaveAndSummary.SetBool(serializedObject, "tracking_SendToFirebase", profile.BuildAppBundle);

            if (!debugPresetChanged && !buildHackChanged && !admobTestDeviceChanged && !admobTestIdChanged && !trackingSendToFirebaseChanged)
            {
                steps.Add($"NET config already matched preset {profile.Preset}.");
                return;
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(netConfigs);

            string assetPath = AssetDatabase.GetAssetPath(netConfigs);
            if (!string.IsNullOrWhiteSpace(assetPath))
                dirtyAssetPaths.Add(assetPath);

            steps.Add($"Applied NET config preset {profile.Preset} to {netConfigs.name}.");
        }
    }
}
