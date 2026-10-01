using System;
using System.Collections.Generic;
using DG.Tweening;
using Eco.TweenAnimation.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Eco.TweenAnimation
{
    public class TweenAnimationObject : TweenAnimationBase, IArrayTransform
    {
        [HideLabel] public AnimationDebug AnimationDebug;
        private List<TweenAnimationBase> _tweenAnimations = new ();

        private void Awake()
        {
            AnimationDebug = new AnimationDebug(this);
            TweenAnimationBase[] animations = GetComponents<TweenAnimationBase>();
            for (var i = 0; i < animations.Length; i++)
            {
                TweenAnimationBase tweenAnimationBase = animations[i];
                if(tweenAnimationBase == this) continue;
                _tweenAnimations.Add(tweenAnimationBase);
            }
        }

        public override void Show(TweenCallback onComplete = null)
        {
            gameObject.SetActive(true);
            OnShowComplete = onComplete;
            
            TweenAnimation lastTweenAnimation = null;
            for (var i = 0; i < _tweenAnimations.Count; i++)
            {
                TweenAnimationBase tweenAnimationBase = _tweenAnimations[i];
                if (tweenAnimationBase is TweenAnimation tweenAnimation)
                {
                    float totalTime = tweenAnimation.BaseOptions.StartDelay + tweenAnimation.BaseOptions.Duration;
                    if (lastTweenAnimation == null || totalTime > lastTweenAnimation.BaseOptions.StartDelay + lastTweenAnimation.BaseOptions.Duration)
                        lastTweenAnimation = tweenAnimation;
                }
                tweenAnimationBase.Show();
            }
            lastTweenAnimation.OnShowComplete = CallBack_OnShowComplete;
        }

        public override void Hide(TweenCallback onComplete = null)
        {
            gameObject.SetActive(true);
            OnHideComplete = onComplete;
            
            TweenAnimation lastTweenAnimation = null;
            for (var i = 0; i < _tweenAnimations.Count; i++)
            {
                TweenAnimationBase tweenAnimationBase = _tweenAnimations[i];
                if (tweenAnimationBase is TweenAnimation tweenAnimation)
                {
                    float totalTime = tweenAnimation.BaseOptions.StartDelay + tweenAnimation.BaseOptions.Duration;
                    if (lastTweenAnimation == null || totalTime > lastTweenAnimation.BaseOptions.StartDelay + lastTweenAnimation.BaseOptions.Duration)
                        lastTweenAnimation = tweenAnimation;
                }
                tweenAnimationBase.Hide();
            }
            lastTweenAnimation.OnHideComplete = CallBack_OnHideComplete;
        }

        private void CallBack_OnShowComplete()
        {
            OnShowComplete?.Invoke();
        }
        
        private void CallBack_OnHideComplete()
        {
            OnHideComplete?.Invoke();
        }
        
        public override Transform GetTransform()
        {
            return transform;
        }
        
        public Transform[] GetTransforms()
        {
            List<Transform> transforms = new();
            foreach (var tweenAnimationBase in _tweenAnimations)
                transforms.Add(tweenAnimationBase.GetTransform());
            return transforms.ToArray();
        }

        public override void Kill()
        {
            foreach (var tweenAnimationBase in _tweenAnimations)
                tweenAnimationBase.Kill();
        }

        public override void Complete()
        {
            foreach (var tweenAnimationBase in _tweenAnimations)
                tweenAnimationBase.Complete();
        }
    }
}