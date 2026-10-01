using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Eco.TweenAnimation
{
    public class FillAmountAnimation : IAnimation
    {
        private AnimationFactory _factory;
        private Image _image;
        private BaseOptions _options;
        private FloatOptions _customOptions;
        
        public void Initialized(AnimationFactory animationFactory)
        {
            _factory = animationFactory;
            _image = animationFactory.TweenAnimation.Image;
            _options = _factory.TweenAnimation.BaseOptions;
            _customOptions = _factory.TweenAnimation.FloatOptions;
            _customOptions.To = _image.fillAmount;
        }

        public void SetAnimationFrom()
        {
            _image.fillAmount = _customOptions.From;
        }

        public Tweener Show()
        {
            SetAnimationFrom();
            return _image
                .DOFillAmount(_customOptions.To, _options.Duration)
                .SetEase(_options.ShowEase)
                .SetUpdate(_options.IgnoreTimeScale)
                .SetDelay(_options.StartDelay);
        }

        public Tweener Hide()
        {
            _image.fillAmount = _customOptions.To;
            return _image
                .DOFillAmount(_customOptions.From, _options.Duration)
                .SetEase(_options.HideEase)
                .SetUpdate(_options.IgnoreTimeScale)
                .SetDelay(_options.StartDelay);
        }
    }
}