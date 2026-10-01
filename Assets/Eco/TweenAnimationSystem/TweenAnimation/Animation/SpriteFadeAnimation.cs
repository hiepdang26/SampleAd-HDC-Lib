using DG.Tweening;
using EcoMine.Common;
using UnityEngine;

namespace Eco.TweenAnimation
{
    public class SpriteFadeAnimation : IAnimation
    {
        private AnimationFactory _factory;
        private SpriteGroup _spriteGroup;
        private BaseOptions _options;
        private FloatOptions _customOptions;
        
        public void Initialized(AnimationFactory animationFactory)
        {
            _factory = animationFactory;
            _spriteGroup = animationFactory.TweenAnimation.SpriteGroup;
            _options = animationFactory.TweenAnimation.BaseOptions;
            _customOptions = animationFactory.TweenAnimation.FloatOptions;
        }

        public void SetAnimationFrom()
        {
            _spriteGroup.SetAlpha(_customOptions.From);
        }

        public Tweener Show()
        {
            return DOVirtual.Float(_customOptions.From, _customOptions.To, _options.Duration, _spriteGroup.SetAlpha)
                .SetEase(_options.ShowEase)
                .SetUpdate(_options.IgnoreTimeScale)
                .SetDelay(_options.StartDelay);
        }

        public Tweener Hide()
        {
            return DOVirtual.Float(_customOptions.To, _customOptions.From, _options.Duration, _spriteGroup.SetAlpha)
                .SetEase(_options.ShowEase)
                .SetUpdate(_options.IgnoreTimeScale)
                .SetDelay(_options.StartDelay);
        }
    }
}