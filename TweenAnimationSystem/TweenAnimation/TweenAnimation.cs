using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Eco.TweenAnimation
{
    public enum EAnimation
    {
        Move = 0, MoveLocal = 1, MoveArchors = 2,
        Scale = 3, Rotation = 4, Fade = 5,
        SizeDelta = 6, FillAmount = 7, AnchorMin = 8, AnchorMax = 9
    }

    public enum EShow { None, Awake, Enable }

    [HideMonoScript]
    public class TweenAnimation : TweenAnimationBase
    {
        /// <summary>
        /// Animation Setting Group
        /// </summary>
        [SerializeField, LabelText("Select Animation"), TabGroup("Animation Setting")]
        private EAnimation _animation;

        [SerializeField, LabelText("Show On Action"), TabGroup("Animation Setting")]
        private EShow _showOn = EShow.Awake;

        [SerializeField, LabelText("Register In Screen Toggle"), TabGroup("Animation Setting")]
        private bool _registerScreenToggle = true;

        [SerializeField, LabelText("Canvas Group"), TabGroup("Animation Setting"), ShowIf("IsFadeAnimation")]
        private CanvasGroup _canvasGroup;

        [SerializeField, LabelText("Canvas Group"), TabGroup("Animation Setting"), ShowIf("IsImageAnimation")]
        private Image _image;

        [SerializeField, HideLabel, TabGroup("Animation Setting")]
        private BaseOptions _baseOptions;

        [SerializeField, HideLabel, TabGroup("Animation Setting"), ShowIf("IsVector3Option")]
        private Vector3Options _vector3Options;

        [SerializeField, HideLabel, TabGroup("Animation Setting"), ShowIf("IsFadeAnimation")]
        private CanvasGroupOptions _canvasGroupOptions;

        [SerializeField, HideLabel, TabGroup("Animation Setting"), ShowIf("IsFloatOption")]
        private FloatOptions _floatOptions;

        /// <summary>
        /// Animation Setting Group
        /// </summary>
        [SerializeField, HideLabel, TabGroup("Animation Debug")]
        internal AnimationDebug _animationDebug;

        private AnimationFactory _factory;
        private IAnimation _ianimation;
        private Tweener _tweener;
        private Sequence _sequence;
        private bool _isShow;

        public EAnimation Animation { get => _animation; }
        public bool IsRegisterScreenToggle { get => _registerScreenToggle; }
        public bool IsShow { get => _isShow; }
        public CanvasGroup CanvasGroup { get => _canvasGroup; }
        public Image Image { get => _image; }
        public BaseOptions BaseOptions { get => _baseOptions; }
        public Vector3Options Vector3Options { get => _vector3Options; }
        public CanvasGroupOptions CanvasGroupOptions { get => _canvasGroupOptions; }
        public FloatOptions FloatOptions { get => _floatOptions; }

        [OnInspectorInit]
        private void InitializedDebug()
        {
            _animationDebug = new AnimationDebug(this);
        }

        private void Awake()
        {
            if (_showOn == EShow.Awake)
                Show();
        }

        private void OnEnable()
        {
            if (_showOn == EShow.Enable)
                Show();
        }

        public override void Show(TweenCallback onComplete = null)
        {
            Kill();
            _isShow = true;
            gameObject.SetActive(true);
            OnShowComplete = onComplete;
            if (_baseOptions.LoopTime > 0 || _baseOptions.LoopTime == -1)
            {
                DOVirtual.DelayedCall(_baseOptions.StartDelay, () =>
                {
                    _sequence = DOTween.Sequence();
                    _sequence.Append(_ianimation.Show().SetDelay(0));
                    _sequence.AppendInterval(_baseOptions.DelayPerOneTimeLoop);
                    _sequence.SetLoops(_baseOptions.LoopTime, _baseOptions.LoopType);
                    _sequence.SetUpdate(_baseOptions.IgnoreTimeScale);
                    _sequence.OnComplete(onComplete);
                    _sequence.Play();
                }, _baseOptions.IgnoreTimeScale);
            }
            else
            {
                _tweener = _ianimation.Show();
                _tweener.onComplete += CallBackShowComplete;
            }
        }

        public override void Hide(TweenCallback onComplete = null)
        {
            CheckAndInitialized();
            gameObject.SetActive(true);
            OnHideComplete = onComplete;
            _isShow = false;
            _tweener = _ianimation.Hide();
            _tweener.onComplete += CallBackHideComplete;
        }

        public override void Kill()
        {
            CheckAndInitialized();
            _sequence?.Kill();
            _tweener?.Kill();
            _ianimation?.SetAnimationFrom();
        }

        public override void Complete()
        {
            CheckAndInitialized();
            _sequence?.Kill(true);
            _tweener?.Complete();
            _ianimation?.SetAnimationFrom();
        }

        private void CallBackShowComplete()
        {
            OnShowComplete?.Invoke();
        }
        
        private void CallBackHideComplete()
        {
            OnHideComplete?.Invoke();
        }

        private void CheckAndInitialized()
        {
            _ianimation ??= GetFactory().CreateAnimation();
        }

        private AnimationFactory GetFactory()
        {
            _factory ??= new AnimationFactory(this);
            return _factory;
        }

        private bool IsFadeAnimation()
        {
            return _animation == EAnimation.Fade;
        }

        private bool IsVector3Option()
        {
            return _animation != EAnimation.Fade && !IsFloatOption();
        }

        private bool IsFloatOption()
        {
            return _animation == EAnimation.FillAmount;
        }

        private bool IsImageAnimation()
        {
            return _animation == EAnimation.FillAmount;
        }

        private void OnDestroy()
        {
            _sequence?.Kill();
            _tweener?.Kill();
        }

        private void OnDisable()
        {
            _sequence?.Kill();
            _tweener?.Kill();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (IsFadeAnimation() && _canvasGroup == null)
            {
                if (!TryGetComponent(out _canvasGroup))
                    _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            if (IsImageAnimation() && _image == null)
            {
                if (!TryGetComponent(out _image))
                    _image = gameObject.AddComponent<Image>();
            }
        }
#endif
    }
}