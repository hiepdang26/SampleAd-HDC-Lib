using DG.Tweening;

namespace Eco.TweenAnimation
{
    public interface IAnimation
    {
        public void Initialized(AnimationFactory animationFactory);
        public void SetAnimationFrom();
        public Tweener Show();
        public Tweener Hide();
    }
}