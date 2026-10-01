using DG.Tweening;
using UnityEngine;

namespace Eco.TweenAnimation
{
    public class MoveLocalAnimation : IAnimation
    {
        private AnimationFactory _factory;
        private Transform _transform;
        private BaseOptions _options;
        private Vector3Options _customOptions;

        public void Initialized(AnimationFactory animationFactory)
        {
            _factory = animationFactory;
            _options = _factory.TweenAnimation.BaseOptions;
            _transform = _options.IsOverrideTransfrom ? _options.OverrideTransfrom : animationFactory.TweenAnimation.transform;
            _customOptions = _factory.TweenAnimation.Vector3Options;
            if (_customOptions.EndTo == Vector3.one * -1f)
                _customOptions.EndTo = _transform.localPosition;
        }

        public void SetAnimationFrom()
        {
            _transform.localPosition = _customOptions.From;
        }

        public Tweener Show()
        {
            SetAnimationFrom();
            return _transform
                .DOLocalMove(_customOptions.EndTo, _options.Duration)
                .SetEase(_options.ShowEase)
                .SetUpdate(_options.IgnoreTimeScale)
                .SetDelay(_options.StartDelay);
        }

        public Tweener Hide()
        {
            _transform.localPosition = _customOptions.EndTo;
            return _transform
                .DOLocalMove(_customOptions.From, _options.Duration)
                .SetEase(_options.HideEase)
                .SetUpdate(_options.IgnoreTimeScale)
                .SetDelay(_options.StartDelay);
        }
    }
}