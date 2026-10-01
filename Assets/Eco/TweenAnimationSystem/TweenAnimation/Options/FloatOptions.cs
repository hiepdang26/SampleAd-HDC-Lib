using Sirenix.OdinInspector;
using UnityEngine;

namespace Eco.TweenAnimation
{
    [System.Serializable]
    public class FloatOptions
    {
        [FoldoutGroup("Custom Options")] public float From = 0;
        [FoldoutGroup("Custom Options")] public float To = 1;
    }
}