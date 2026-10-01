using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using BG_Library.NET.AdSystem;
using BG_Library.NET.Debug;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BG_Library.DEBUG
{
    [DisallowMultipleComponent]
    public sealed class DiagnosticsSystemSection : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text hintText;
        [SerializeField] private Text summaryText;

        private void Awake()
        {
            ApplyStaticText();
        }

        private void OnEnable()
        {
            Refresh();
        }

        public void Refresh()
        {
            ApplyStaticText();
            UpdateSummary();
        }

        private void ApplyStaticText()
        {
            if (titleText != null)
                titleText.text = "Diagnostics system";

            if (hintText != null)
                hintText.text = "This section shows the active NetFlowDebugSystem preset. Change the preset in Configs SO or Build Tool before build.";
        }

        private void UpdateSummary()
        {
            if (summaryText == null)
                return;

            DebugPreset runtimePreset = NetFlowDebugSystem.CurrentPreset;

            if (NetConfigsSO.Ins == null)
            {
                summaryText.text =
                    $"Runtime Preset: {runtimePreset}\n" +
                    $"{DescribePreset(runtimePreset)}\n\n" +
                    "Configs SO not found.";
                return;
            }

            DebugPreset configPreset = NetConfigsSO.Ins.Debug_Preset;
            summaryText.text =
                $"Runtime Preset: {runtimePreset}\n" +
                $"{DescribePreset(runtimePreset)}\n\n" +
                $"Configs SO Preset: {configPreset}\n" +
                $"Runtime State: {(NetFlowDebugSystem.IsEnabledForPreset(runtimePreset) ? "Enabled" : "Disabled")}\n" +
                $"Unity Logger: {(NetFlowDebugSystem.IsEnabledForPreset(runtimePreset) ? "Enabled" : "Disabled")}\n" +
                $"Layers: {NetFlowDebugSystem.DescribeLayersVi(runtimePreset)}\n" +
                $"Tracking Detail: {NetFlowDebugSystem.DescribeTrackingVi(runtimePreset)}\n" +
                $"Tracking Firebase: {(NetConfigsSO.Ins.Tracking_SendToFirebase ? "ON (release AAB only)" : "OFF")}";
        }

        private static string DescribePreset(DebugPreset preset)
        {
            return NetFlowDebugSystem.DescribePresetVi(preset);
        }
    }
}
