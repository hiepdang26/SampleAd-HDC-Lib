using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AppBootstrap.Splash
{
    public sealed class SplashLoadingView : MonoBehaviour
    {
        [SerializeField] private TMP_Text loadingText;
        [SerializeField] private TMP_Text processText;
        [SerializeField] private Image progressFill;
        [SerializeField] private float fillAnimationDuration = 0.2f;
        [SerializeField] private float completeAnimationDuration = 0.65f;
        [SerializeField] private string[] loadingTextLines =
        {
            "Preparing startup modules...",
            "Checking local profile data...",
            "Syncing gameplay parameters...",
            "Verifying downloadable content...",
            "Warming up rendering pipeline...",
            "Loading environment descriptors...",
            "Resolving remote dependencies...",
            "Building runtime caches...",
            "Finalizing session bootstrap...",
            "Optimizing launch sequence..."
        };

        private CancellationTokenSource _fakeLoadingCancellationSource;
        private CancellationTokenSource _fillAnimationCancellationSource;
        private float _currentProgress;
        
        public RectTransform HeaderTextRect => loadingText.rectTransform;

        public void SetView(string text, float progress01)
        {
            SetRealLoadingText(text);
            SetProgressInstant(progress01);
        }

        public void SetRealLoadingText(string text)
        {
            if (loadingText != null)
            {
                loadingText.text = text;
            }
        }

        public void StartFakeLoading(float startProgress, float endProgress, float duration)
        {
            CancelFakeLoadingTask();

            SetProgressInstant(startProgress);
            _fakeLoadingCancellationSource = new CancellationTokenSource();
            RunFakeLoadingAsync(Mathf.Clamp01(startProgress), Mathf.Clamp01(endProgress), Mathf.Max(0f, duration), _fakeLoadingCancellationSource.Token).Forget();
        }

        public void StopLoadingAndCompleteLoading(string completedText = null)
        {
            CancelFakeLoadingTask();

            if (!string.IsNullOrWhiteSpace(completedText))
            {
                SetRealLoadingText(completedText);
            }

            AnimateProgressTo(1f, completeAnimationDuration);
        }

        private async UniTaskVoid RunFakeLoadingAsync(float startProgress, float endProgress, float duration,
            CancellationToken cancellationToken)
        {
            string[] lines = GetLoadingLines();
            if (lines.Length == 0)
            {
                AnimateProgressTo(endProgress, fillAnimationDuration);
                return;
            }

            int totalLength = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                totalLength += Mathf.Max(1, lines[i]?.Length ?? 0);
            }

            int completedLength = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string line = string.IsNullOrWhiteSpace(lines[i]) ? "Loading..." : lines[i];
                SetRealLoadingText(line);

                int lineLength = Mathf.Max(1, line.Length);
                completedLength += lineLength;

                float progressT = totalLength <= 0 ? 1f : Mathf.Clamp01((float)completedLength / totalLength);
                float targetProgress = Mathf.Lerp(startProgress, endProgress, progressT);
                AnimateProgressTo(targetProgress, fillAnimationDuration);

                float delaySeconds = totalLength <= 0 ? 0f : duration * lineLength / totalLength;
                if (delaySeconds > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(delaySeconds), DelayType.DeltaTime, cancellationToken: cancellationToken);
                }
            }
        }

        private void AnimateProgressTo(float targetProgress, float duration)
        {
            CancelFillAnimationTask();
            _fillAnimationCancellationSource = new CancellationTokenSource();
            RunFillAnimationAsync(Mathf.Clamp01(targetProgress), Mathf.Max(0.01f, duration), _fillAnimationCancellationSource.Token).Forget();
        }

        private async UniTaskVoid RunFillAnimationAsync(float targetProgress, float duration, CancellationToken cancellationToken)
        {
            if (progressFill == null)
            {
                _currentProgress = targetProgress;
                return;
            }

            float startProgress = progressFill.fillAmount;
            float startTime = Time.unscaledTime;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                float elapsed = Time.unscaledTime - startTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                float progress = Mathf.Lerp(startProgress, targetProgress, eased);
                int progressRate = Mathf.RoundToInt(progress * 100f);

                progressFill.fillAmount = progress;
                if (processText != null)
                {
                    processText.text = $"{progressRate}%";
                }
                _currentProgress = progress;

                if (t >= 1f)
                {
                    break;
                }

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }
        }

        public void SetProgressInstant(float progress01)
        {
            CancelFillAnimationTask();
            _currentProgress = Mathf.Clamp01(progress01);
            if (progressFill != null)
            {
                int progress = Mathf.RoundToInt(_currentProgress * 100f);

                progressFill.fillAmount = _currentProgress;
                if (processText != null)
                {
                    processText.text = $"{progress}%";
                }
            }
        }

        private string[] GetLoadingLines()
        {
            return loadingTextLines == null || loadingTextLines.Length == 0
                ? Array.Empty<string>()
                : loadingTextLines;
        }

        private void CancelFakeLoadingTask()
        {
            if (_fakeLoadingCancellationSource == null)
            {
                return;
            }

            _fakeLoadingCancellationSource.Cancel();
            _fakeLoadingCancellationSource.Dispose();
            _fakeLoadingCancellationSource = null;
        }

        private void CancelFillAnimationTask()
        {
            if (_fillAnimationCancellationSource == null)
            {
                return;
            }

            _fillAnimationCancellationSource.Cancel();
            _fillAnimationCancellationSource.Dispose();
            _fillAnimationCancellationSource = null;
        }

        private void OnDestroy()
        {
            CancelFakeLoadingTask();
            CancelFillAnimationTask();
        }
    }
}
