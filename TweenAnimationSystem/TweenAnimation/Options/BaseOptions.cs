using DG.Tweening;
using Sirenix.OdinInspector;

namespace Eco.TweenAnimation
{
    [System.Serializable]
    public class BaseOptions
    {
        [FoldoutGroup("Base Options")] public Ease ShowEase = Ease.OutBack;
        [FoldoutGroup("Base Options")] public Ease HideEase = Ease.InBack;
        [FoldoutGroup("Base Options")] public float Duration = 0.1925f;
        [FoldoutGroup("Base Options")] public float StartDelay = 0f;
        [FoldoutGroup("Base Options")] public bool IgnoreTimeScale = false;
        [FoldoutGroup("Loop Options")] public int LoopTime;
        [FoldoutGroup("Loop Options")] public LoopType LoopType;
        [FoldoutGroup("Loop Options")] public float DelayPerOneTimeLoop;
    }
}