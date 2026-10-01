using DG.Tweening;
using UnityEngine;

namespace Eco.TweenAnimation
{
    public class AnchorMinAnimation : IAnimation
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
                _customOptions.EndTo = _transform.anchorMin;
        }

        public void SetAnimationFrom()
        {
            _transform.anchorMin = _customOptions.From;
        }

        public Tweener Show()
        {
            SetAnimationFrom();
            return _transform
                .DOAnchorMin(_customOptions.EndTo, _options.Duration)
                .SetEase(_options.ShowEase)
                .SetUpdate(_options.IgnoreTimeScale)
                .SetDelay(_options.StartDelay);
        }

        public Tweener Hide()
        {
            _transform.anchorMin = _customOptions.EndTo;
            return _transform
                .DOAnchorMin(_customOptions.From, _options.Duration)
                .SetEase(_options.HideEase)
                .SetUpdate(_options.IgnoreTimeScale)
                .SetDelay(_options.StartDelay);
        }
    }
}