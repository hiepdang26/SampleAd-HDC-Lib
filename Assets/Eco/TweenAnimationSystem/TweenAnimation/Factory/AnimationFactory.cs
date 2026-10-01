namespace Eco.TweenAnimation
{
    public class AnimationFactory
    {
        private TweenAnimation _tweenAnimation;
        public TweenAnimation TweenAnimation { get => _tweenAnimation; }

        public AnimationFactory(TweenAnimation tweenAnimation)
        {
            _tweenAnimation = tweenAnimation;
        }

        public IAnimation CreateAnimation()
        {
            switch (_tweenAnimation.Animation)
            {
                case EAnimation.Move:
                    return CreateObjectAnimation<MoveAnimation>();
                case EAnimation.MoveLocal:
                    return CreateObjectAnimation<MoveLocalAnimation>();
                case EAnimation.MoveArchors:
                    return CreateObjectAnimation<MoveArchorsAnimation>();
                case EAnimation.Scale:
                    return CreateObjectAnimation<ScaleAnimation>();
                case EAnimation.Rotation:
                    return CreateObjectAnimation<RotationAnimation>();
                case EAnimation.Fade:
                    return CreateObjectAnimation<FadeAnimation>();
                case EAnimation.SizeDelta:
                    return CreateObjectAnimation<SizeDeltaAnimation>();
                case EAnimation.FillAmount:
                    return CreateObjectAnimation<FillAmountAnimation>();
                case EAnimation.AnchorMax:
                    return CreateObjectAnimation<AnchorMaxAnimation>();
                case EAnimation.AnchorMin:
                    return CreateObjectAnimation<AnchorMinAnimation>();
                case EAnimation.SpriteColor:
                    return CreateObjectAnimation<SpriteColorAnimation>();
                case EAnimation.SpriteFade:
                    return CreateObjectAnimation<SpriteFadeAnimation>();
                case EAnimation.CameraSize:
                    return CreateObjectAnimation<CameraSizeAnimation>();
            }
            return null;
        }

        private IAnimation CreateObjectAnimation<T>() where T : new()
        {
            IAnimation animation = (IAnimation)new T();
            animation.Initialized(this);
            return animation;
        }
    }
}