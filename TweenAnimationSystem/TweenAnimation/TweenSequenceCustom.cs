using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Eco.TweenAnimation;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Eco.TweenAnimation
{
    public class TweenSequenceCustom : TweenAnimationBase
    {
        [SerializeField] private EShow _showOnAction;
        [SerializeField] private bool _ignoreTimeScale;
        [SerializeField] private AnimationCustom[] _showAnimation;
        [SerializeField] private AnimationCustom[] _hideAnimation;
        [SerializeField, HideLabel] private AnimationDebug _debug;

        private void Awake()
        {
            _debug = new AnimationDebug(this);
            if (_showOnAction == EShow.Awake) 
                Show();
        }

        private void OnEnable()
        {
            if (_showOnAction == EShow.Enable) 
                Show();
        }

        public override void Show(TweenCallback onComplete = null)
        {
            gameObject.SetActive(true);
            OnShowComplete = onComplete;
            
            AnimationCustom animationCustomLast = null;
            foreach (var animationCustom in _showAnimation)
            {
                if (animationCustomLast == null || animationCustom.DelayShow > animationCustomLast.DelayShow)
                    animationCustomLast = animationCustom;
                
                animationCustom.tweenAnimation.gameObject.SetActive(false);
                DOVirtual.DelayedCall(animationCustom.DelayShow, () =>
                {
                    animationCustom.tweenAnimation.gameObject.SetActive(true);
                    animationCustom.tweenAnimation.Show(() =>
                    {
                        if (animationCustom.DeActivateOnComplete)
                            animationCustom.tweenAnimation.gameObject.SetActive(false);
                        if (animationCustom == animationCustomLast) CallBack_OnShowComplete();
                    });
                }, _ignoreTimeScale);
            }
        }

        public override void Hide(TweenCallback onComplete = null)
        {
            OnHideComplete = onComplete;
            
            AnimationCustom animationCustomLast = null;
            foreach (var animationCustom in _hideAnimation)
            {
                if (animationCustomLast == null || animationCustom.DelayShow > animationCustomLast.DelayShow)
                    animationCustomLast = animationCustom;
                
                DOVirtual.DelayedCall(animationCustom.DelayShow, () =>
                {
                    animationCustom.tweenAnimation.gameObject.SetActive(true);
                    animationCustom.tweenAnimation.Hide(() =>
                    {
                        if (animationCustom.DeActivateOnComplete)
                            animationCustom.tweenAnimation.gameObject.SetActive(false);
                        if (animationCustom == animationCustomLast) CallBack_OnHideComplete();
                    });
                }, _ignoreTimeScale);
            }
        }
        
        private void CallBack_OnShowComplete()
        {
            OnShowComplete?.Invoke();
        }
        
        private void CallBack_OnHideComplete()
        {
            OnHideComplete?.Invoke();
        }

        public override void Kill()
        {
            
        }

        public override void Complete()
        {
            
        }

        [System.Serializable]
        public class AnimationCustom
        {
            public TweenAnimationBase tweenAnimation;
            public bool DeActivateOnComplete;
            public float DelayShow;
        }
    }
}