using DG.Tweening;
using UnityEngine;

namespace Eco.TweenAnimation
{
    public class SpriteColorAnimation : IAnimation
    {
        private AnimationFactory _factory;
        private SpriteRenderer _spriteRenderer;
        private BaseOptions _options;
        private ColorOptions _customOptions;
        
        public void Initialized(AnimationFactory animationFactory)
        {
            _factory = animationFactory;
            _spriteRenderer = animationFactory.TweenAnimation.SpriteRenderer;
            _options = animationFactory.TweenAnimation.BaseOptions;
            _customOptions = animationFactory.TweenAnimation.ColorOptions;
        }

        public void SetAnimationFrom()
        {
            _spriteRenderer.color = _customOptions.From;
        }

        public Tweener Show()
        {
            SetAnimationFrom();
            return _spriteRenderer
                .DOColor(_customOptions.To, _options.Duration)
                .SetEase(_options.ShowEase)
                .SetUpdate(_options.IgnoreTimeScale)
                .SetDelay(_options.StartDelay);
        }

        public Tweener Hide()
        {
            _spriteRenderer.color = _customOptions.To;
            return _spriteRenderer
                .DOColor(_customOptions.From, _options.Duration)
                .SetEase(_options.ShowEase)
                .SetUpdate(_options.IgnoreTimeScale)
                .SetDelay(_options.StartDelay);
        }
    }
}