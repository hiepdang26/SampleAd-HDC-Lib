using UnityEditor;
using UnityEditor.Build;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolV2ProfileSettingsApplier
    {
        public static void Apply(BuildProfileV2 profile)
        {
            EditorUserBuildSettings.development = profile.DevelopmentBuild;

            string version = profile.ResolveVersion();
            PlayerSettings.productName = profile.ProductName;
            PlayerSettings.companyName = profile.CompanyName;
            PlayerSettings.bundleVersion = version;
            PlayerSettings.Android.bundleVersionCode = profile.VersionCode;
            PlayerSettings.iOS.buildNumber = BuildToolV2Utilities.ExtractDigits(version);
            NamedBuildTarget namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(profile.BuildTarget));
            PlayerSettings.SetApplicationIdentifier(namedBuildTarget, profile.PackageName);

            if (profile.BuildTarget != BuildTarget.Android)
                return;

            EditorUserBuildSettings.buildAppBundle = profile.BuildAppBundle;
            BuildToolV2AndroidPlayerSettings.SetSplitApplicationBinary(profile.BuildAppBundle && profile.SplitApplicationBinary);

            if (!profile.BuildAppBundle && !PlayerSettings.Android.useCustomKeystore)
                return;

            PlayerSettings.Android.useCustomKeystore = true;
            if (string.IsNullOrWhiteSpace(profile.KeystorePassword))
                return;

            PlayerSettings.Android.keystorePass = profile.KeystorePassword;
            PlayerSettings.Android.keyaliasPass = profile.KeystorePassword;
        }
    }
}
