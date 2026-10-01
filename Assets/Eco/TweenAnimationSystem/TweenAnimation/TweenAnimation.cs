using System;
using DG.Tweening;
using EcoMine.Common;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Eco.TweenAnimation
{
    public enum EAnimation
    {
        Move = 0, MoveLocal = 1, MoveArchors = 2,
        Scale = 3, Rotation = 4, Fade = 5,
        SizeDelta = 6, FillAmount = 7, AnchorMin = 8, AnchorMax = 9,
        SpriteColor = 10, SpriteFade = 11, CameraSize = 12,
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

        [SerializeField, LabelText("Image"), TabGroup("Animation Setting"), ShowIf("IsImageAnimation")]
        private Image _image;
        
        [SerializeField, LabelText("Sprite Renderer"), TabGroup("Animation Setting"), ShowIf("IsSpriteAnimation")]
        private SpriteRenderer _spriteRenderer;
        
        [SerializeField, LabelText("Sprite Group"), TabGroup("Animation Setting"), ShowIf("IsSpriteFade")]
        private SpriteGroup _spriteGroup;
        
        [SerializeField, LabelText("Camera Group"), TabGroup("Animation Setting"), ShowIf("IsCameraAnimation")]
        private Camera _camera;

        [SerializeField, HideLabel, TabGroup("Animation Setting")]
        private BaseOptions _baseOptions;

        [SerializeField, HideLabel, TabGroup("Animation Setting"), ShowIf("IsVector3Option")]
        private Vector3Options _vector3Options;

        [SerializeField, HideLabel, TabGroup("Animation Setting"), ShowIf("IsFadeAnimation")]
        private CanvasGroupOptions _canvasGroupOptions;
        
        [SerializeField, HideLabel, TabGroup("Animation Setting"), ShowIf("IsColorOption")]
        private ColorOptions _colorOptions;

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
        public SpriteRenderer SpriteRenderer { get => _spriteRenderer; }
        public Camera Camera { get => _camera; }
        public SpriteGroup SpriteGroup { get => _spriteGroup; }
        public Image Image { get => _image; }
        public BaseOptions BaseOptions { get => _baseOptions; }
        public Vector3Options Vector3Options { get => _vector3Options; }
        public CanvasGroupOptions CanvasGroupOptions { get => _canvasGroupOptions; }
        public ColorOptions ColorOptions { get => _colorOptions; }
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
                    _sequence.OnComplete(OnShowComplete);
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
        
        /*[SerializeField, LabelText("Canvas Group"), TabGroup("Animation Setting"), ShowIf("IsFadeAnimation")]
        private CanvasGroup _canvasGroup;

        [SerializeField, LabelText("Image"), TabGroup("Animation Setting"), ShowIf("IsImageAnimation")]
        private Image _image;
        
        [SerializeField, LabelText("Sprite Renderer"), TabGroup("Animation Setting"), ShowIf("IsSpriteAnimation")]
        private SpriteRenderer _spriteRenderer;
        
        [SerializeField, LabelText("Sprite Group"), TabGroup("Animation Setting"), ShowIf("IsSpriteFade")]
        private SpriteGroup _spriteGroup;
        
        [SerializeField, LabelText("Camera Group"), TabGroup("Animation Setting"), ShowIf("IsCameraAnimation")]
        private Camera _camera;*/
        
        public override Transform GetTransform()
        {
            if(IsVector3Option())
                return _baseOptions.IsOverrideTransfrom ? _baseOptions.OverrideTransfrom : transform;
            if (IsFadeAnimation())
                return _canvasGroup.transform;
            if (IsImageAnimation())
                return _image.transform;
            if(IsSpriteAnimation())
                return _spriteRenderer.transform;
            if(IsSpriteFade())
                return _spriteGroup.transform;
            if(IsCameraAnimation())
                return _camera.transform;
            return transform;
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
            return _animation != EAnimation.Fade && !IsFloatOption() && !IsColorOption();
        }
        
        private bool IsColorOption()
        {
            return _animation == EAnimation.SpriteColor;
        }

        private bool IsFloatOption()
        {
            return _animation == EAnimation.FillAmount || _animation == EAnimation.SpriteFade || _animation == EAnimation.CameraSize;
        }

        private bool IsImageAnimation()
        {
            return _animation == EAnimation.FillAmount;
        }

        private bool IsSpriteAnimation()
        {
            return _animation == EAnimation.SpriteColor;
        }

        private bool IsSpriteFade()
        {
            return _animation == EAnimation.SpriteFade;
        }
        
        private bool IsCameraAnimation()
        {
            return _animation == EAnimation.CameraSize;
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
        
        [Button]
        private void GetFormPosition()
        {
            if (_baseOptions.IsOverrideTransfrom)
                _vector3Options.From = _baseOptions.OverrideTransfrom.localPosition;
            else
                _vector3Options.From = transform.localPosition;
        }
        
        [Button]
        private void GetEndToPosition()
        {
            if (_baseOptions.IsOverrideTransfrom)
                _vector3Options.EndTo = _baseOptions.OverrideTransfrom.localPosition;
            else
                _vector3Options.EndTo = transform.localPosition;
        }
        
                
        [Button]
        private void SetFormPosition()
        {
            if (_baseOptions.IsOverrideTransfrom)
                _baseOptions.OverrideTransfrom.localPosition = _vector3Options.From;
            else
                transform.localPosition = _vector3Options.From;
        }
        
        [Button]
        private void SetEndToPosition()
        {
            if (_baseOptions.IsOverrideTransfrom)
                _baseOptions.OverrideTransfrom.localPosition = _vector3Options.EndTo;
            else
                transform.localPosition = _vector3Options.EndTo;
        }
        
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

            if (IsSpriteAnimation() && _spriteRenderer == null)
            {
                if (!TryGetComponent(out _spriteRenderer))
                    _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
            
            if (IsSpriteFade() && _spriteGroup == null)
            {
                if (!TryGetComponent(out _spriteGroup))
                    _spriteGroup = gameObject.AddComponent<SpriteGroup>();
            }
        }
#endif
    }
}