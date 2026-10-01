using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Eco.TweenAnimation
{
    [System.Serializable]
    public class BaseOptions
    {
        [FoldoutGroup("Base Options")] public Ease ShowEase = Ease.OutQuad;
        [FoldoutGroup("Base Options")] public Ease HideEase = Ease.InQuad;
        [FoldoutGroup("Base Options")] public float Duration = 0.1925f;
        [FoldoutGroup("Base Options")] public float StartDelay = 0f;
        [FoldoutGroup("Base Options")] public bool IgnoreTimeScale = false;
        [FoldoutGroup("Base Options")] public bool IsOverrideTransfrom = false;
        [FoldoutGroup("Base Options"), ShowIf("IsOverrideTransfrom")] public Transform OverrideTransfrom;
        [FoldoutGroup("Loop Options")] public int LoopTime;
        [FoldoutGroup("Loop Options")] public LoopType LoopType;
        [FoldoutGroup("Loop Options")] public float DelayPerOneTimeLoop;
    }
}