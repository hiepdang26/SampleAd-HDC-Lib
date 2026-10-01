using UnityEditor;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolV2AndroidPlayerSettings
    {
        public static bool GetSplitApplicationBinary()
        {
            try
            {
                System.Type androidType = typeof(PlayerSettings.Android);
                bool hasValue = false;
                bool currentValue = false;

                System.Reflection.PropertyInfo splitProperty = androidType.GetProperty("splitApplicationBinary", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (splitProperty != null && splitProperty.PropertyType == typeof(bool) && splitProperty.CanRead)
                {
                    currentValue |= (bool)splitProperty.GetValue(null, null);
                    hasValue = true;
                }

                System.Reflection.PropertyInfo expansionProperty = androidType.GetProperty("useAPKExpansionFiles", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (expansionProperty != null && expansionProperty.PropertyType == typeof(bool) && expansionProperty.CanRead)
                {
                    currentValue |= (bool)expansionProperty.GetValue(null, null);
                    hasValue = true;
                }

                if (hasValue)
                    return currentValue;
            }
            catch
            {
                // Ignore editor-version-specific property mismatches.
            }

            return false;
        }

        public static void SetSplitApplicationBinary(bool value)
        {
            try
            {
                System.Type androidType = typeof(PlayerSettings.Android);
                System.Reflection.PropertyInfo splitProperty = androidType.GetProperty("splitApplicationBinary", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (splitProperty != null && splitProperty.PropertyType == typeof(bool) && splitProperty.CanWrite)
                    splitProperty.SetValue(null, value, null);

                System.Reflection.PropertyInfo expansionProperty = androidType.GetProperty("useAPKExpansionFiles", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (expansionProperty != null && expansionProperty.PropertyType == typeof(bool) && expansionProperty.CanWrite)
                    expansionProperty.SetValue(null, value, null);
            }
            catch
            {
                // Ignore editor-version-specific property mismatches.
            }
        }
    }
}
