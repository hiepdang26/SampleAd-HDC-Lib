using Sirenix.OdinInspector;
using UnityEngine;

namespace Eco.TweenAnimation
{
    [System.Serializable]
    public class CanvasGroupOptions : FloatOptions
    {
        [FoldoutGroup("Custom Options")] public bool BlockRaycast = true;
    }
}