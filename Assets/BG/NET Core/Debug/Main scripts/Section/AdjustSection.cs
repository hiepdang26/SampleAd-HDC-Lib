using BG_Library.NET;
using UnityEngine;
using UnityEngine.UI;

namespace BG_Library.DEBUG
{
    [DisallowMultipleComponent]
    public sealed class AdjustSection : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text hintText;
        [SerializeField] private Text summaryText;
        [SerializeField] private Button openViewerButton;
        [SerializeField] private DebugDetailViewer detailViewer;

        private void Awake()
        {
            ApplyStaticText();
            BindActions();
        }

        void Start()
        {
            Refresh();
        }

        public void Refresh()
        {
            summaryText.text = BuildSummaryText();
        }

        private void ApplyStaticText()
        {
            titleText.text = "Adjust debug";
            hintText.text = "Use this to inspect attribution state, saved user properties, and timeout state.";
        }

        private void BindActions()
        {
            openViewerButton.onClick.RemoveAllListeners();
            openViewerButton.onClick.AddListener(OpenViewer);
        }

        private void OpenViewer()
        {
            detailViewer.Open("Adjust Debug", BuildViewerText());
        }

        private static string BuildSummaryText()
        {
            var adjust = AdjustManager.Ins;
            if (adjust == null)
                return "AdjustManager instance missing.";

            return
                $"Lifecycle: {adjust.DebugLifecycle}\n" +
                $"Attribution ready: {adjust.AttributionFromAdjustReady} | Timeout: {adjust.DidAdjustWaitTimeout}\n" +
                $"Network: {adjust.NetworkName} | NetworkReady: {adjust.NetworkReadyRaised}\n" +
                "Open Full Viewer for the raw attribution and stored user properties.";
        }

        private static string BuildViewerText()
        {
            return AdjustManager.Ins == null
                ? "AdjustManager instance missing."
                : AdjustManager.Ins.GetDebugInfo();
        }
    }
}
