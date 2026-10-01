using UnityEngine;
using UnityEngine.UI;

namespace BG_Library.DEBUG
{
    [DisallowMultipleComponent]
    public class LayoutRebuildParent : MonoBehaviour
    {
        [SerializeField] private RectTransform targetRoot;

        private bool dirty;

        private void Awake()
        {
            if (targetRoot == null)
                targetRoot = transform as RectTransform;
        }

        private void OnEnable()
        {
            dirty = true;
        }

        private void LateUpdate()
        {
            if (!dirty || targetRoot == null)
                return;

            dirty = false;
            RebuildUpwards(targetRoot);
        }

        public void MarkDirty()
        {
            dirty = true;
        }

        private static void RebuildUpwards(RectTransform start)
        {
            var current = start;
            while (current != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(current);
                current = current.parent as RectTransform;
            }

            Canvas.ForceUpdateCanvases();
        }
    }
}
