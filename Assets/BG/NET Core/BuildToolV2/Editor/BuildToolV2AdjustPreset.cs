using System.Collections.Generic;
using AdjustSdk;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolV2AdjustPreset
    {
        public static void Apply(
            BuildProfileV2 profile,
            List<string> steps,
            HashSet<string> dirtyScenePaths)
        {
            Adjust adjust = Object.FindFirstObjectByType<Adjust>();
            if (adjust == null)
            {
                steps.Add("Skipped Adjust preparation because no Adjust object exists in the primary build scene.");
                return;
            }

            bool changed = false;
            if (adjust.startManually)
            {
                adjust.startManually = false;
                changed = true;
            }

            AdjustEnvironment targetEnvironment = profile.UsesSandboxAdjust ? AdjustEnvironment.Sandbox : AdjustEnvironment.Production;
            if (adjust.environment != targetEnvironment)
            {
                adjust.environment = targetEnvironment;
                changed = true;
            }

            if (!changed)
            {
                steps.Add($"Adjust already matched {targetEnvironment} environment.");
                return;
            }

            PrefabUtility.RecordPrefabInstancePropertyModifications(adjust);
            adjust.startManually = false;
            EditorUtility.SetDirty(adjust);

            Scene scene = adjust.gameObject.scene;
            if (scene.IsValid() && !string.IsNullOrWhiteSpace(scene.path))
                dirtyScenePaths.Add(scene.path);

            steps.Add($"Applied Adjust environment {targetEnvironment} in the primary build scene.");
        }
    }
}
