using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Eco.TweenAnimation;
using Eco.TweenAnimation.Core;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Eco.TweenAnimation
{
    public class TweenSequenceCustom : TweenAnimationBase
    {
        [SerializeField] private EShow _showOnAction;
        [SerializeField] private bool _ignoreTimeScale;
        [SerializeField] private bool _deActiveItNotShow = true;
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

        public void Show() => Show(null);
        public void Hide() => Hide(null);
        
        public override void Show(TweenCallback onComplete = null)
        {
            gameObject.SetActive(true);
            OnShowComplete += onComplete;
            
            AnimationCustom animationCustomLast = null;
            foreach (var animationCustom in _showAnimation)
            {
                if (animationCustomLast == null || animationCustom.DelayShow > animationCustomLast.DelayShow)
                    animationCustomLast = animationCustom;

                if (_deActiveItNotShow) CheckAndSetActive(animationCustom, false);
                DOVirtual.DelayedCall(animationCustom.DelayShow, () =>
                {
                    CheckAndSetActive(animationCustom, true);
                    TweenCallback complete = () =>
                    {
                        if (animationCustom.DeActivateOnComplete)
                            CheckAndSetActive(animationCustom, false);
                        if (animationCustom == animationCustomLast) CallBack_OnShowComplete();
                    };
                    
                    if (animationCustom.Option == AnimationCustom.AnimationOption.Show)
                        animationCustom.tweenAnimation.Show(complete);
                    else
                        animationCustom.tweenAnimation.Hide(complete);
                    
                }, _ignoreTimeScale);
            }
        }

        public override void Hide(TweenCallback onComplete = null)
        {
            OnHideComplete += onComplete;
            
            AnimationCustom animationCustomLast = null;
            foreach (var animationCustom in _hideAnimation)
            {
                if (animationCustomLast == null || animationCustom.DelayShow > animationCustomLast.DelayShow)
                    animationCustomLast = animationCustom;
                
                DOVirtual.DelayedCall(animationCustom.DelayShow, () =>
                {
                    CheckAndSetActive(animationCustom, true);
                    
                    TweenCallback complete = () =>
                    {
                        if (animationCustom.DeActivateOnComplete)
                            CheckAndSetActive(animationCustom, false);
                        if (animationCustom == animationCustomLast) CallBack_OnHideComplete();
                    };
                    
                    if (animationCustom.Option == AnimationCustom.AnimationOption.Show)
                        animationCustom.tweenAnimation.Show(complete);
                    else
                        animationCustom.tweenAnimation.Hide(complete);
                }, _ignoreTimeScale);
            }
        }
        
        public override Transform GetTransform()
        {
            return transform;
        }

        private void CheckAndSetActive(AnimationCustom animationCustom, bool isActive)
        {
            if (animationCustom.tweenAnimation is IArrayTransform arrayTransform)
                arrayTransform.GetTransforms().ForEach(trans => trans.gameObject.SetActive(isActive));
            else
                animationCustom.tweenAnimation.GetTransform().gameObject.SetActive(isActive);
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
            public enum AnimationOption
            {
                Show, Hide
            }
            
            public TweenAnimationBase tweenAnimation;
            public AnimationOption Option;
            public bool DeActivateOnComplete;
            public float DelayShow;
        }
    }
}