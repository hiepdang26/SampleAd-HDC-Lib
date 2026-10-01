using Sirenix.OdinInspector;
using UnityEngine;

namespace Eco.TweenAnimation
{
    [System.Serializable]
    public class AnimationDebug
    {
        private TweenAnimationBase _tweenAnimation;

        public AnimationDebug(TweenAnimationBase tweenAnimation)
        {
            _tweenAnimation = tweenAnimation;
        }

        [Button("Show")]
        public void Show()
        {
            if (Application.isPlaying)
                _tweenAnimation.Show();
        }

        [Button("Hide")]
        public void Hide()
        {
            if (Application.isPlaying)
                _tweenAnimation.Hide();
        }
    }
}