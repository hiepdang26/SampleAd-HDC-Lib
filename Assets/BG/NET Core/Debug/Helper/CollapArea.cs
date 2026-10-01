
using UnityEngine;
using UnityEngine.UI;

namespace BG_Library.DEBUG
{
    public class CollapArea : MonoBehaviour
    {
        [SerializeField] private Button TitleBtn;
        [SerializeField] private bool startExpanded = true;

        private bool isExpanded;
        private LayoutRebuildParent layoutRebuildParent;

        public bool IsExpanded => isExpanded;

        private void Awake()
        {
            TitleBtn.onClick.RemoveListener(Toggle);
            TitleBtn.onClick.AddListener(Toggle);

            ResolveLayoutRebuildParent();
            isExpanded = startExpanded;
            ApplyState();
        }

        public void Toggle()
        {
            if (TitleBtn == null)
            {
                isExpanded = false;
                ApplyState();
                return;
            }

            isExpanded = !isExpanded;
            ApplyState();
        }

        public void SetExpanded(bool expanded)
        {
            if (isExpanded == expanded)
                return;

            isExpanded = expanded;
            ApplyState();
        }

        private void ApplyState()
        {
            bool active = TitleBtn != null && isExpanded;

            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child == null)
                    continue;

                if (TitleBtn != null && child.gameObject == TitleBtn.gameObject)
                    continue;

                child.gameObject.SetActive(active);
            }

            layoutRebuildParent?.MarkDirty();
        }

        private void ResolveLayoutRebuildParent()
        {
            if (layoutRebuildParent != null)
                return;

            var current = transform.parent;
            while (current != null)
            {
                layoutRebuildParent = current.GetComponent<LayoutRebuildParent>();
                if (layoutRebuildParent != null)
                    return;

                current = current.parent;
            }
        }
    }
}
