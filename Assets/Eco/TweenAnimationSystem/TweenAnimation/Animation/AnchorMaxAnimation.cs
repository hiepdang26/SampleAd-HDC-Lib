using DG.Tweening;
using UnityEngine;

namespace Eco.TweenAnimation
{
    public class AnchorMaxAnimation : IAnimation
    {
        private AnimationFactory _factory;
        private RectTransform _transform;
        private BaseOptions _options;
        private Vector3Options _customOptions;
        
        public void Initialized(AnimationFactory animationFactory)
        {
            _factory = animationFactory;
            _transform = animationFactory.TweenAnimation.transform as RectTransform;
            _options = _factory.TweenAnimation.BaseOptions;
            _customOptions = _factory.TweenAnimation.Vector3Options;
            if(_customOptions.EndTo == Vector3.one * -1f)
                _customOptions.EndTo = _transform.anchorMax;
        }

        public void SetAnimationFrom()
        {
            _transform.anchorMax = _customOptions.From;
        }

        public Tweener Show()
        {
            SetAnimationFrom();
            return _transform
                .DOAnchorMax(_customOptions.EndTo, _options.Duration)
                .SetEase(_options.ShowEase)
                .SetUpdate(_options.IgnoreTimeScale)
                .SetDelay(_options.StartDelay);
        }

        public Tweener Hide()
        {
            _transform.anchorMax = _customOptions.EndTo;
            return _transform
                .DOAnchorMax(_customOptions.From, _options.Duration)
                .SetEase(_options.HideEase)
                .SetUpdate(_options.IgnoreTimeScale)
                .SetDelay(_options.StartDelay);
        }
    }
}