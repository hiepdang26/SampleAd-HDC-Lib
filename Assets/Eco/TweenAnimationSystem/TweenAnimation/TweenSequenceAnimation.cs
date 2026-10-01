using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Eco.TweenAnimation
{
    [Obsolete]
    public class TweenSequenceAnimation : TweenAnimationBase
    {
        [TabGroup("Settings Sequence"), SerializeField] 
        private GameObject Base;
        [TabGroup("Settings Sequence"), SerializeField] 
        private EShow _showOnAction;
        [TabGroup("Settings Sequence"), SerializeField] 
        private float _startDelay;
        [TabGroup("Settings Sequence"), SerializeField] 
        private bool _ignoreTimeScale;
        [TabGroup("Settings Sequence"), SerializeField] 
        private bool _deActivateOnShowComplete;
        [TabGroup("Settings Sequence"), SerializeField] 
        private SequenceOption _showOption;
        [TabGroup("Settings Sequence"), SerializeField] 
        private SequenceOption _hideOption;
        
        [TabGroup("Sequence Debug"), HideLabel, SerializeField]
        private AnimationDebug _sequenceDebugOption;
        
        private void Awake()
        {
            _sequenceDebugOption = new AnimationDebug(this);
            if (_showOnAction == EShow.Awake)
                Show();
        }
        
        private void OnEnable()
        {
            if (_showOnAction == EShow.Enable)
                Show();
        }

        private void ResetAnimation()
        {
            for (var i = 0; i < _showOption.Sequences.Length; i++)
                _showOption.Sequences[i].tweenAnimation.gameObject.SetActive(false);
            for (var i = 0; i < _hideOption.Sequences.Length; i++)
                _hideOption.Sequences[i].tweenAnimation.gameObject.SetActive(false);
        }

        public override Transform GetTransform()
        {
            return transform;
        }

        public override void Show(TweenCallback onComplete = null)
        {
            gameObject.SetActive(true);
            Base?.gameObject.SetActive(false);
            ResetAnimation();
            StartCoroutine(IEDelaySequence(() =>
                StartCoroutine(IERunSequence(_showOption, onComplete))));
        }
        
        public override void Hide(TweenCallback onComplete = null)
        {
            gameObject.SetActive(true);
            ResetAnimation();
            StartCoroutine(IEDelaySequence(() =>
                StartCoroutine(IERunSequence(_hideOption, onComplete))));
        }

        public override void Kill()
        {
            throw new NotImplementedException();
        }

        public override void Complete()
        {
            throw new NotImplementedException();
        }


        IEnumerator IEDelaySequence(Action onComplete)
        {
            if (_ignoreTimeScale)
                yield return new WaitForSecondsRealtime(_startDelay);
            else
                yield return new WaitForSeconds(_startDelay);
            onComplete.Invoke();
        }
        
        IEnumerator IERunSequence(SequenceOption sequenceOption, TweenCallback onComplete = null)
        {
            bool complete = false;
            for (int i = 0; i < sequenceOption.Sequences.Length; i++)
            {
                AnimationSequenceSetting animationSequenceSetting = sequenceOption.Sequences[i];
                animationSequenceSetting.tweenAnimation.Show(() => complete = true);
                yield return new WaitUntil(() => complete);
                if(_deActivateOnShowComplete) animationSequenceSetting.tweenAnimation.gameObject.SetActive(false);
                if (animationSequenceSetting.IsShowBase) ToggleBase(true);
                if(animationSequenceSetting.IsHideBase) ToggleBase(false);
                complete = false;
            }
            onComplete?.Invoke();
        }

        private void ToggleBase(bool isEnable)
        {
            TweenAnimation animation = Base.GetComponent<TweenAnimation>();
            if (animation != null && isEnable)
                animation.Show();
            else if (animation == null && isEnable)
                Base?.SetActive(true);
            if (animation != null && !isEnable)
                animation.Hide();
            else if (animation == null && !isEnable)
                Base?.SetActive(false);
        }
    }

    [System.Serializable, HideReferenceObjectPicker]
    public class SequenceOption
    {
        public AnimationSequenceSetting[] Sequences;
    }

    [System.Serializable]
    public class AnimationSequenceSetting
    {
        [FormerlySerializedAs("tweenAnimationBase")]
        [FormerlySerializedAs("TweenAnimation")]
        public TweenAnimationBase tweenAnimation;
        public bool IsShowBase;
        public bool IsHideBase;
    }
}