using DG.Tweening;
using UnityEngine;

namespace Eco.TweenAnimation
{
    public abstract class TweenAnimationBase : MonoBehaviour
    {
        public TweenCallback OnShowComplete;
        public TweenCallback OnHideComplete;
        public abstract void Show(TweenCallback onComplete = null);
        public abstract void Hide(TweenCallback onComplete = null);
        public abstract void Kill();
        public abstract void Complete();
    }
}