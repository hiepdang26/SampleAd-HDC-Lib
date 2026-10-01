using DG.Tweening;
using UnityEngine;

namespace Eco.TweenAnimation
{
    public class CameraSizeAnimation : IAnimation
    {
        private AnimationFactory _factory;
        private Camera _camera;
        private BaseOptions _options;
        private FloatOptions _customOptions;
        
        public void Initialized(AnimationFactory animationFactory)
        {
            _factory = animationFactory;
            _camera = animationFactory.TweenAnimation.Camera;
            _options = animationFactory.TweenAnimation.BaseOptions;
            _customOptions = animationFactory.TweenAnimation.FloatOptions;
        }

        public void SetAnimationFrom()
        {
            _camera.orthographicSize = _customOptions.From;
        }

        public Tweener Show()
        {
            SetAnimationFrom();
            return _camera.DOOrthoSize(_customOptions.To, _options.Duration)
                .SetEase(_options.ShowEase)
                .SetUpdate(_options.IgnoreTimeScale)
                .SetDelay(_options.StartDelay);
        }

        public Tweener Hide()
        {
            _camera.orthographicSize = _customOptions.To;
            return  _camera.DOOrthoSize(_customOptions.From, _options.Duration)
                .SetEase(_options.ShowEase)
                .SetUpdate(_options.IgnoreTimeScale)
                .SetDelay(_options.StartDelay);
        }
    }
}
