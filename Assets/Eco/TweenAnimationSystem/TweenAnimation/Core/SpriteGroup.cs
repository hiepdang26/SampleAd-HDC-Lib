using UnityEngine;

namespace EcoMine.Common
{
    [DisallowMultipleComponent]
    public sealed class SpriteGroup : MonoBehaviour
    {
        private SpriteRenderer[] _spriteRenderers;
        private bool _needsRefresh = true;

        public void SetAlpha(float alpha)
        {
            if (_needsRefresh || _spriteRenderers == null)
                RefreshSpriteRenderers();

            for (int i = 0; i < _spriteRenderers.Length; i++)
            {
                SpriteRenderer spriteRenderer = _spriteRenderers[i];
                if (spriteRenderer == null)
                    continue;

                Color color = spriteRenderer.color;
                color.a = alpha;
                spriteRenderer.color = color;
            }
        }

        private void OnTransformChildrenChanged()
        {
            _needsRefresh = true;
        }

        private void RefreshSpriteRenderers()
        {
            _spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            _needsRefresh = false;
        }
    }
}
