using Sirenix.OdinInspector;
using UnityEngine;

namespace Eco.TweenAnimation
{
    [System.Serializable]
    public class ColorOptions
    {
        [FoldoutGroup("Custom Options")] public Color32 From = Color.white;
        [FoldoutGroup("Custom Options")] public Color32 To = Color.black;
    }
}