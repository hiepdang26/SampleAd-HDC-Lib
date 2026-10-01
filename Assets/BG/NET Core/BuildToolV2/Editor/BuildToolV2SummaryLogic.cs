using System;
using BG_Library.NET.AdSystem;
using UnityEditor.Build;

namespace BG_Library.BuildToolV2
{
    internal sealed class BuildToolSummaryDraftV2
    {
        public bool DevelopmentBuild;
        public string ProductName = string.Empty;
        public string CompanyName = string.Empty;
        public string Version = string.Empty;
        public int VersionCode;
        public string KeystorePassword = string.Empty;
        public string PackageName = string.Empty;
        public bool SplitApplicationBinary;
        public string ChangeLog = string.Empty;
    }

    internal sealed class BuildToolSummaryApplyRequestV2
    {
        public BuildProfileV2 Profile;
        public bool LocalStateChanged;
        public bool DevelopmentBuildChanged;
        public bool ProductNameChanged;
        public bool CompanyNameChanged;
        public bool VersionValueChanged;
        public bool VersionCodeChanged;
        public bool KeystorePasswordChanged;
        public bool PackageNameChanged;
        public bool SplitApplicationBinaryChanged;
        public bool DevelopmentBuild;
        public string ProductName = string.Empty;
        public string CompanyName = string.Empty;
        public string Version = string.Empty;
        public int VersionCode;
        public string KeystorePassword = string.Empty;
        public string PackageName = string.Empty;
        public bool SplitApplicationBinary;
    }

    internal sealed class BuildToolSummaryApplyResultV2
    {
        public bool AppliedAnyChanges;
        public bool RefreshedDisplaySnapshots;
        public bool ResetBuildResults;
    }

    internal static class BuildToolV2SummaryLogic
    {
        public static BuildToolSummaryDraftV2 RefreshDraft(BuildProfileV2 profile, BuildProfileV2 cachedProfile, BuildToolSummaryDraftV2 currentDraft, bool force)
        {
            if (profile == null)
                return new BuildToolSummaryDraftV2();

            if (!force && cachedProfile == profile && currentDraft != null)
                return currentDraft;

            return new BuildToolSummaryDraftV2
            {
                DevelopmentBuild = profile.DevelopmentBuild,
                ProductName = profile.ProductName,
                CompanyName = profile.CompanyName,
                Version = profile.Version,
                VersionCode = profile.VersionCode,
                KeystorePassword = profile.KeystorePassword,
                PackageName = profile.PackageName,
                SplitApplicationBinary = profile.BuildAppBundle && profile.SplitApplicationBinary,
                ChangeLog = profile.ChangeLog ?? string.Empty,
            };
        }

        public static BuildToolSummaryApplyResultV2 ApplyChanges(BuildToolSummaryApplyRequestV2 request)
        {
            var result = new BuildToolSummaryApplyResultV2();
            if (request == null || request.Profile == null)
                return result;

            bool shouldRepaintEditorViews = false;

            if (request.DevelopmentBuildChanged)
            {
                request.Profile.SetDevelopmentBuildValue(request.DevelopmentBuild);
                shouldRepaintEditorViews = true;
            }
            if (request.ProductNameChanged)
            {
                request.Profile.SetProductName(request.ProductName);
                shouldRepaintEditorViews = true;
            }
            if (request.CompanyNameChanged)
            {
                request.Profile.SetCompanyName(request.CompanyName);
                shouldRepaintEditorViews = true;
            }
            if (request.VersionValueChanged)
            {
                request.Profile.SetVersion(request.Version);
                shouldRepaintEditorViews = true;
            }
            if (request.VersionCodeChanged)
            {
                request.Profile.SetVersionCode(request.VersionCode);
                shouldRepaintEditorViews = true;
            }
            if (request.KeystorePasswordChanged)
            {
                request.Profile.SetKeystorePassword(request.KeystorePassword);
                shouldRepaintEditorViews = true;
            }
            if (request.PackageNameChanged)
            {
                request.Profile.SetPackageName(request.PackageName);
                shouldRepaintEditorViews = true;
            }
            if (request.Profile.BuildTarget == UnityEditor.BuildTarget.Android)
            {
                bool targetSplitApplicationBinary = request.Profile.BuildAppBundle && request.SplitApplicationBinary;
                if (request.SplitApplicationBinaryChanged || (!request.Profile.BuildAppBundle && request.Profile.SplitApplicationBinary))
                {
                    request.Profile.SetSplitApplicationBinary(targetSplitApplicationBinary);
                    shouldRepaintEditorViews = true;
                }
            }

            if (shouldRepaintEditorViews)
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();

            result.AppliedAnyChanges =
                request.DevelopmentBuildChanged
                || request.ProductNameChanged
                || request.CompanyNameChanged
                || request.VersionValueChanged
                || request.VersionCodeChanged
                || request.KeystorePasswordChanged
                || request.PackageNameChanged
                || request.SplitApplicationBinaryChanged
                || request.LocalStateChanged;

            result.RefreshedDisplaySnapshots = result.AppliedAnyChanges;
            result.ResetBuildResults = result.AppliedAnyChanges;
            return result;
        }
    }
}
