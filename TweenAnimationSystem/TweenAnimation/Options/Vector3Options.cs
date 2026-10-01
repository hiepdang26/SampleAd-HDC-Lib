using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Eco.TweenAnimation
{
    [System.Serializable]
    public class Vector3Options
    {
        [FoldoutGroup("Custom Options")] public Vector3 From = Vector3.zero;
        [FoldoutGroup("Custom Options")] public Vector3 EndTo = Vector3.one * -1;
    }
}