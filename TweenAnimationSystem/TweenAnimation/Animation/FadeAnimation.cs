using DG.Tweening;
using UnityEngine;

namespace Eco.TweenAnimation
{
    public class FadeAnimation : IAnimation
    {
        private AnimationFactory _factory;
        private CanvasGroup _canvasGroup;
        private BaseOptions _options;
        private CanvasGroupOptions _customOptions;
        
        public void Initialized(AnimationFactory animationFactory)
        {
            _factory = animationFactory;
            _canvasGroup = animationFactory.TweenAnimation.CanvasGroup;
            _options = animationFactory.TweenAnimation.BaseOptions;
            _customOptions = animationFactory.TweenAnimation.CanvasGroupOptions;
            _customOptions.To = _canvasGroup.alpha;
        }

        public void SetAnimationFrom()
        {
            _canvasGroup.alpha = _customOptions.From;
        }

        public Tweener Show()
        {
            SetAnimationFrom();
            CheckAlpha();
            return _canvasGroup
                .DOFade(_customOptions.To, _options.Duration)
                .SetEase(_options.ShowEase)
                .SetUpdate(_options.IgnoreTimeScale)
                .SetDelay(_options.StartDelay)
                .OnComplete(CheckAlpha);
        }

        public Tweener Hide()
        {
            _canvasGroup.alpha = _customOptions.To;
            CheckAlpha();
            return _canvasGroup
                .DOFade(_customOptions.From, _options.Duration)
                .SetEase(_options.ShowEase)
                .SetUpdate(_options.IgnoreTimeScale)
                .SetDelay(_options.StartDelay)
                .OnComplete(CheckAlpha);
        }
        
        private void CheckAlpha()
        {
            _canvasGroup.blocksRaycasts = _canvasGroup.alpha > 0;
        }
    }
}