using UnityEngine;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolV2SceneSetupMatching
    {
        public static GameObject ResolveRemovalTarget(GameObject candidate, string prefabName, string componentName)
        {
            return ResolveSetupChildRoot(candidate, prefabName, componentName);
        }

        public static GameObject ResolveSetupChildRoot(GameObject candidate, string prefabName, string componentName)
        {
            if (candidate == null)
                return null;

            if (string.Equals(candidate.name, prefabName, System.StringComparison.Ordinal))
            {
                if (string.IsNullOrWhiteSpace(componentName) || HasComponentOnSelfOrChildren(candidate, componentName))
                    return candidate;
            }

            if (string.IsNullOrWhiteSpace(componentName) || !HasComponent(candidate, componentName))
                return null;

            Transform current = candidate.transform;
            while (current != null)
            {
                if (string.Equals(current.name, prefabName, System.StringComparison.Ordinal))
                    return current.gameObject;

                current = current.parent;
            }

            return candidate;
        }

        public static bool HasComponentOnSelfOrChildren(GameObject target, string componentName)
        {
            if (target == null || string.IsNullOrWhiteSpace(componentName))
                return false;

            Component[] components = target.GetComponentsInChildren<Component>(true);
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component != null && component.GetType().Name == componentName)
                    return true;
            }

            return false;
        }

        public static bool HasComponent(GameObject target, string componentName)
        {
            if (target == null || string.IsNullOrWhiteSpace(componentName))
                return false;

            Component[] components = target.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component != null && component.GetType().Name == componentName)
                    return true;
            }

            return false;
        }
    }
}
