using UnityEditor;
using UnityEngine;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolWindowV2NetworkSection
    {
        internal static void Draw(BuildToolWindowV2 owner)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Network", EditorStyles.boldLabel);
                if (GUILayout.Button("Refresh Network", GUILayout.Width(140f)))
                    owner.RefreshSelectedDeviceNetwork();
            }

            if (owner.profile.BuildTarget != BuildTarget.Android)
            {
                EditorGUILayout.HelpBox("Network tools are only used for Android devices.", MessageType.None);
                return;
            }

            BuildAndroidDeviceInfo selectedDevice = owner.GetSelectedDevice();
            if (selectedDevice == null)
            {
                EditorGUILayout.HelpBox("No Android device is selected.", MessageType.Warning);
                return;
            }

            string shareToolPath = BuildToolV2Utilities.GetNetworkShareToolPath(owner.scrcpySettings);
            BuildToolWindowV2Ui.DrawSummaryRow("Shared Network", selectedDevice.IsUsingSharedComputerNetwork ? "Using computer network" : "Using device network");
            BuildToolWindowV2Ui.DrawSummaryRow("Reverse-Tether Tool", string.IsNullOrWhiteSpace(shareToolPath) ? "(missing)" : shareToolPath);

            if (!string.IsNullOrWhiteSpace(selectedDevice.NetworkSource))
                BuildToolWindowV2Ui.DrawSummaryRow("Network Source", selectedDevice.NetworkSource);
            if (!string.IsNullOrWhiteSpace(selectedDevice.NetworkInterfaceName))
                BuildToolWindowV2Ui.DrawSummaryRow("Interface", selectedDevice.NetworkInterfaceName);
            if (!string.IsNullOrWhiteSpace(selectedDevice.NetworkLocalIp))
                BuildToolWindowV2Ui.DrawSummaryRow("Local IP", selectedDevice.NetworkLocalIp);
            if (!string.IsNullOrWhiteSpace(selectedDevice.NetworkPublicIp))
                BuildToolWindowV2Ui.DrawSummaryRow("Public IP", selectedDevice.NetworkPublicIp);
            if (!string.IsNullOrWhiteSpace(selectedDevice.NetworkLocation))
                BuildToolWindowV2Ui.DrawSummaryRow("Network Location", selectedDevice.NetworkLocation);
            if (!string.IsNullOrWhiteSpace(selectedDevice.NetworkProvider))
                BuildToolWindowV2Ui.DrawSummaryRow("Provider", selectedDevice.NetworkProvider);
            if (!string.IsNullOrWhiteSpace(selectedDevice.NetworkVpn))
                BuildToolWindowV2Ui.DrawSummaryRow("VPN", selectedDevice.NetworkVpn);
            if (!string.IsNullOrWhiteSpace(selectedDevice.NetworkSharedBy))
                BuildToolWindowV2Ui.DrawSummaryRow("Shared By", selectedDevice.NetworkSharedBy);

            EditorGUILayout.Space(6f);

            bool isUsingSharedNetwork = selectedDevice.IsUsingSharedComputerNetwork;
            string networkButtonLabel = isUsingSharedNetwork ? "Stop Network" : "Share Network";
            string networkButtonTooltip = isUsingSharedNetwork
                ? "Stop using this computer's shared network on the selected phone. After stopping it, wait a moment and press Refresh Network."
                : "Share this computer's network to the selected phone through the reverse-tether tool. After starting it, wait a moment and press Refresh Network.";

            using (new EditorGUI.DisabledScope(string.Equals(selectedDevice.State, "device", System.StringComparison.OrdinalIgnoreCase) == false))
            {
                if (GUILayout.Button(new GUIContent(networkButtonLabel, networkButtonTooltip), GUILayout.Width(120f)))
                {
                    string message;
                    bool success = isUsingSharedNetwork
                        ? BuildToolV2Utilities.StopSharedDeviceNetwork(selectedDevice, owner.scrcpySettings, out message)
                        : BuildToolV2Utilities.ShareDeviceNetwork(selectedDevice, owner.scrcpySettings, out message);
                    string uiMessage = success
                        ? (isUsingSharedNetwork
                            ? "Stop Network command sent. Wait a moment, then press Refresh Network."
                            : "Share Network command started. Wait a moment, then press Refresh Network.")
                        : message;

                    owner.ShowNotification(new GUIContent(string.IsNullOrWhiteSpace(uiMessage) ? $"{networkButtonLabel} command finished." : uiMessage));
                    owner.SetScrcpyMessage(uiMessage, success ? MessageType.Info : MessageType.Warning);
                    if (!success)
                        Debug.LogWarning(message);
                }
            }

            if (selectedDevice.IsUsingSharedComputerNetwork)
                EditorGUILayout.HelpBox("This phone is currently using the computer's shared network.", MessageType.Info);
        }
    }
}
