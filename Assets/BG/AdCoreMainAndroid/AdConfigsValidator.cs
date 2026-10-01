// =======================
// AdConfigsValidator.cs
// =======================
using System;
using System.Collections.Generic;
using System.Text;
using BG_Library.NET.AdSystem;

namespace BG_Library.NET.AdCore.MainAndroid
{
    /// <summary>
    /// Validate between:
    /// - AdSystemConfigs (system usage/enables/pos...)
    /// - AdCore.MainAndroid.Configs (actual ids/units/pos groups...)
    ///
    /// Output:
    /// - Errors / Warnings / Infos
    /// - All IDs used in AdCore configs (with format + mediation + name/group + notes)
    /// </summary>
    public static class AdConfigsValidator
    {
        public enum Severity
        {
            Info = 0,
            Warning = 1,
            Error = 2
        }

        [Serializable]
        public sealed class Issue
        {
            public Severity Severity;
            public string Area;     // e.g. "ForceAd", "AppLaunch", "FA", "Popup"
            public string Message;  // human readable

            public Issue(Severity severity, string area, string message)
            {
                Severity = severity;
                Area = area ?? "";
                Message = message ?? "";
            }
        }

        [Serializable]
        public sealed class IdEntry
        {
            public string Format;     // "FA", "RW", "AO", "BN", "MREC", "PU", "CL"
            public string Mediation;  // "Admob" / "Android"
            public string Name;       // groupName or fixed name
            public string Id;         // the actual id
            public string Note;       // preload/buffer/layout/time...

            public IdEntry(string format, string mediation, string name, string id, string note = "")
            {
                Format = format ?? "";
                Mediation = mediation ?? "";
                Name = name ?? "";
                Id = id ?? "";
                Note = note ?? "";
            }
        }

        [Serializable]
        public sealed class ValidationReport
        {
            public readonly List<Issue> Issues = new();
            public readonly List<IdEntry> Ids = new();

            public int ErrorCount
            {
                get
                {
                    int c = 0;
                    for (int i = 0; i < Issues.Count; i++)
                        if (Issues[i] != null && Issues[i].Severity == Severity.Error) c++;
                    return c;
                }
            }

            public int WarningCount
            {
                get
                {
                    int c = 0;
                    for (int i = 0; i < Issues.Count; i++)
                        if (Issues[i] != null && Issues[i].Severity == Severity.Warning) c++;
                    return c;
                }
            }

            public int InfoCount
            {
                get
                {
                    int c = 0;
                    for (int i = 0; i < Issues.Count; i++)
                        if (Issues[i] != null && Issues[i].Severity == Severity.Info) c++;
                    return c;
                }
            }

            public void Add(Severity sev, string area, string msg)
            {
                Issues.Add(new Issue(sev, area, msg));
            }

            public string ToDebugString()
            {
                var sb = new StringBuilder(1024);

                sb.Append("[AdConfigsValidator] Report").AppendLine();
                sb.Append("Errors: ").Append(ErrorCount)
                  .Append(" | Warnings: ").Append(WarningCount)
                  .Append(" | Infos: ").Append(InfoCount)
                  .AppendLine();

                // Issues
                if (Issues.Count > 0)
                {
                    sb.AppendLine("---- Issues ----");
                    for (int i = 0; i < Issues.Count; i++)
                    {
                        var it = Issues[i];
                        if (it == null) continue;

                        sb.Append('[').Append(it.Severity.ToString().ToUpperInvariant()).Append("] ");
                        sb.Append('[').Append(it.Area ?? "").Append("] ");
                        sb.Append(it.Message ?? "");
                        sb.AppendLine();
                    }
                }
                else
                {
                    sb.AppendLine("---- Issues ----");
                    sb.AppendLine("(none)");
                }

                // IDs
                sb.AppendLine("---- IDs ----");
                if (Ids.Count > 0)
                {
                    for (int i = 0; i < Ids.Count; i++)
                    {
                        var id = Ids[i];
                        if (id == null) continue;

                        sb.Append("[ID] [").Append(id.Format ?? "").Append("] [").Append(id.Mediation ?? "").Append("] ");
                        sb.Append("[Name=").Append(id.Name ?? "").Append("] ");
                        sb.Append("id=").Append(id.Id ?? "");

                        if (!string.IsNullOrEmpty(id.Note))
                        {
                            sb.Append(" | ").Append(id.Note);
                        }

                        sb.AppendLine();
                    }
                }
                else
                {
                    sb.AppendLine("(none)");
                }

                return sb.ToString();
            }
        }

        // =========================
        // Public API
        // =========================
        public static ValidationReport Validate(AdSystemConfigs system, Configs adcore)
        {
            var r = new ValidationReport();

            if (system == null)
            {
                r.Add(Severity.Error, "System", "AdSystemConfigs is NULL.");
                return r;
            }

            if (adcore == null)
            {
                r.Add(Severity.Error, "AdCore", "AdCore Configs is NULL.");
                return r;
            }

            // 1) Collect all IDs first (audit)
            CollectAllIds(adcore, r);

            // 2) Cross-check system usage vs adcore data
            ValidateAppLaunch(system, adcore, r);
            ValidateAppResume(system, adcore, r);

            ValidateForceAd(system, adcore, r);
            ValidateRewarded(system, adcore, r);

            ValidateBanner(system, adcore, r);
            ValidateMrec(system, adcore, r);

            ValidatePopup(system, adcore, r);
            ValidateCollap(system, adcore, r);

            // 3) Data hygiene checks (adcore internal conflicts)
            ValidateFAInternal(adcore, r);
            ValidatePUInternal(adcore, r);

            // 4) "Disabled but has ids" infos
            ValidateUnusedInfos(system, adcore, r);

            return r;
        }

        // =========================
        // Collect IDs
        // =========================
        private static void CollectAllIds(Configs adcore, ValidationReport r)
        {
            // FA (Admob / Android per-group, Max shared)
            var faArr = adcore.ForceAdGroups;
            if (faArr != null)
            {
                for (int i = 0; i < faArr.Length; i++)
                {
                    var g = faArr[i];
                    if (g == null) continue;

                    var gname = g.GroupName ?? "";
                    var maxShow = g.MaxShowCount;

                    // Admob
                    var adm = g.AdmobUnit;
                    if (!string.IsNullOrEmpty(adm.Id))
                    {
                        string note = $"preload={adm.PreloadAd} buffer={adm.AdBufferSize} maxShow={maxShow} priority={g.MediationPriority} backup={g.UseBackup}";
                        r.Ids.Add(new IdEntry("FA", "Admob", gname, adm.Id, note));
                    }

                    // Android
                    var and = g.AndroidUnit;
                    if (!string.IsNullOrEmpty(and.Id))
                    {
                        string note = $"layoutGroupName={and.LayoutGroupName} maxShow={maxShow} priority={g.MediationPriority} backup={g.UseBackup}";
                        r.Ids.Add(new IdEntry("FA", "Android", gname, and.Id, note));
                    }

                    if (!string.IsNullOrEmpty(SafeId(adcore.ForceAdMaxUnit?.MaxUnit)) &&
                        (g.MediationPriority == E_MediationPriority.Max || g.UseBackup))
                    {
                        string note = $"shared ForceAdMaxUnit maxShow={maxShow} priority={g.MediationPriority} backup={g.UseBackup}";
                        r.Ids.Add(new IdEntry("FA", "Max", gname, adcore.ForceAdMaxUnit.MaxUnit, note));
                    }
                }
            }

            // RW
            var rw = adcore.RewardedUnit;
            if (rw != null)
            {
                var adm = rw.AdmobUnit;
                if (!string.IsNullOrEmpty(adm.Id))
                {
                    string note = $"preload={adm.PreloadAd} buffer={adm.AdBufferSize} priority={rw.MediationPriority} backup={rw.UseBackup}";
                    r.Ids.Add(new IdEntry("RW", "Admob", "Rewarded", adm.Id, note));
                }

                var and = rw.AndroidUnit;
                if (!string.IsNullOrEmpty(and.Id))
                {
                    string note = $"layoutGroupName={and.LayoutGroupName} priority={rw.MediationPriority} backup={rw.UseBackup}";
                    r.Ids.Add(new IdEntry("RW", "Android", "Rewarded", and.Id, note));
                }

                if (!string.IsNullOrEmpty(SafeId(rw.MaxUnit)))
                {
                    r.Ids.Add(new IdEntry("RW", "Max", "Rewarded", rw.MaxUnit, $"priority={rw.MediationPriority} backup={rw.UseBackup}"));
                }
            }

            // AO
            var ao = adcore.AppOpenUnit;
            if (ao != null)
            {
                var adm = ao.AdmobUnit;
                if (!string.IsNullOrEmpty(adm.Id))
                {
                    string note = $"preload={adm.PreloadAd} buffer={adm.AdBufferSize} priority={ao.MediationPriority} backup={ao.UseBackup}";
                    r.Ids.Add(new IdEntry("AO", "Admob", "AppOpen", adm.Id, note));
                }

                if (!string.IsNullOrEmpty(SafeId(ao.MaxUnit)))
                {
                    r.Ids.Add(new IdEntry("AO", "Max", "AppOpen", ao.MaxUnit, $"priority={ao.MediationPriority} backup={ao.UseBackup}"));
                }
            }

            // BN (6 placements, Admob/Max)
            var bn = adcore.BannerUnit;
            if (bn != null)
            {
                CollectBannerSlotIds("FullBottom", bn.FullBottom, r);
                CollectBannerSlotIds("FullTop", bn.FullTop, r);
                CollectBannerSlotIds("TopLeft", bn.TopLeft, r);
                CollectBannerSlotIds("TopRight", bn.TopRight, r);
                CollectBannerSlotIds("BottomLeft", bn.BottomLeft, r);
                CollectBannerSlotIds("BottomRight", bn.BottomRight, r);
            }

            // MREC
            var mrec = adcore.MrecUnit;
            if (mrec != null)
            {
                var adm = mrec.AdmobUnit;
                if (!string.IsNullOrEmpty(adm.Id))
                {
                    r.Ids.Add(new IdEntry("MREC", "Admob", "Mrec", adm.Id, $"priority={mrec.MediationPriority} backup={mrec.UseBackup}"));
                }

                if (!string.IsNullOrEmpty(SafeId(mrec.MaxUnit)))
                {
                    r.Ids.Add(new IdEntry("MREC", "Max", "Mrec", mrec.MaxUnit, $"priority={mrec.MediationPriority} backup={mrec.UseBackup}"));
                }
            }

            // PU (Android only)
            var puArr = adcore.PopupGroups;
            if (puArr != null)
            {
                for (int i = 0; i < puArr.Length; i++)
                {
                    var g = puArr[i];
                    if (g == null) continue;

                    var gname = g.GroupName ?? "";
                    var and = g.AndroidUnit;
                    if (!string.IsNullOrEmpty(and.Id))
                    {
                        string note = $"layout={and.Layout} show={and.TimeShow} reload={and.ReloadTime}";
                        r.Ids.Add(new IdEntry("PU", "Android", gname, and.Id, note));
                    }
                }
            }

            // CL (Android only)
            var cl = adcore.CollapUnit;
            if (cl != null)
            {
                var and = cl.AndroidUnit;
                if (!string.IsNullOrEmpty(and.Id))
                {
                    string note = $"layout={and.Layout} close={and.TimeClose} reloadByClick={and.ReloadByClick} hiddenReload={and.EnableHiddenReload} hiddenTime={and.ReloadByHiddenTime}";
                    r.Ids.Add(new IdEntry("CL", "Android", "Collap", and.Id, note));
                }
            }
        }

        // =========================
        // System vs AdCore Checks
        // =========================
        private static void ValidateAppLaunch(AdSystemConfigs system, Configs adcore, ValidationReport r)
        {
            var launch = system.AppLaunchChannel;
            if (launch == null) return;

            if (!launch.IsEnabled) return;

            var cb = adcore.ComebackChannel;
            if (cb == null)
            {
                r.Add(Severity.Error, "AppLaunch", "AppLaunch is enabled but ComebackChannelInfo is NULL.");
                return;
            }

            if (cb.LaunchAdType == E_ComebackAdType.AO)
            {
                if (adcore.AppOpenUnit == null)
                {
                    r.Add(Severity.Error, "AppLaunch", "launchAdType=AO but AppOpenUnitConfig is null.");
                    return;
                }

                var aoId = adcore.AppOpenUnit.MediationPriority == E_AdmobMaxMediationPriority.Max
                    ? SafeId(adcore.AppOpenUnit.MaxUnit)
                    : SafeId(adcore.AppOpenUnit.AdmobUnit.Id);
                if (string.IsNullOrEmpty(aoId))
                    r.Add(Severity.Error, "AppLaunch", $"launchAdType=AO but AppOpen {adcore.AppOpenUnit.MediationPriority} id is empty.");

                if (adcore.AppOpenUnit.UseBackup)
                {
                    bool hasBackupId = adcore.AppOpenUnit.MediationPriority == E_AdmobMaxMediationPriority.Max
                        ? !string.IsNullOrEmpty(SafeId(adcore.AppOpenUnit.AdmobUnit.Id))
                        : !string.IsNullOrEmpty(SafeId(adcore.AppOpenUnit.MaxUnit));

                    if (!hasBackupId)
                        r.Add(Severity.Warning, "AppLaunch", "launchAdType=AO and AppOpen UseBackup=true but no backup AppOpen id is configured.");
                }
            }
            else // FA
            {
                var groupName = cb.LaunchForceAdGroupName ?? "";
                if (string.IsNullOrEmpty(groupName))
                {
                    r.Add(Severity.Error, "AppLaunch", "launchAdType=FA but launchForceAdGroupName is empty.");
                    return;
                }

                var group = FindFAGroupByName(adcore, groupName);
                if (group == null)
                {
                    r.Add(Severity.Error, "AppLaunch", $"launchForceAdGroupName=\"{groupName}\" not found in ForceAdGroupConfig[].groupName.");
                    return;
                }

                if (!HasValidFAIdByPriority(adcore, group))
                    r.Add(Severity.Error, "AppLaunch", $"launchForceAdGroupName=\"{groupName}\" exists but missing id for priority={group.MediationPriority}.");
            }
        }

        private static void ValidateAppResume(AdSystemConfigs system, Configs adcore, ValidationReport r)
        {
            var resume = system.AppResumeChannel;
            if (resume == null) return;

            if (!resume.IsEnabled) return;

            var cb = adcore.ComebackChannel;
            if (cb == null)
            {
                r.Add(Severity.Error, "AppResume", "AppResume is enabled but ComebackChannelInfo is NULL.");
                return;
            }

            if (cb.ResumeAdType == E_ComebackAdType.AO)
            {
                if (adcore.AppOpenUnit == null)
                {
                    r.Add(Severity.Error, "AppResume", "resumeAdType=AO but AppOpenUnitConfig is null.");
                    return;
                }

                var aoId = adcore.AppOpenUnit.MediationPriority == E_AdmobMaxMediationPriority.Max
                    ? SafeId(adcore.AppOpenUnit.MaxUnit)
                    : SafeId(adcore.AppOpenUnit.AdmobUnit.Id);
                if (string.IsNullOrEmpty(aoId))
                    r.Add(Severity.Error, "AppResume", $"resumeAdType=AO but AppOpen {adcore.AppOpenUnit.MediationPriority} id is empty.");

                if (adcore.AppOpenUnit.UseBackup)
                {
                    bool hasBackupId = adcore.AppOpenUnit.MediationPriority == E_AdmobMaxMediationPriority.Max
                        ? !string.IsNullOrEmpty(SafeId(adcore.AppOpenUnit.AdmobUnit.Id))
                        : !string.IsNullOrEmpty(SafeId(adcore.AppOpenUnit.MaxUnit));

                    if (!hasBackupId)
                        r.Add(Severity.Warning, "AppResume", "resumeAdType=AO and AppOpen UseBackup=true but no backup AppOpen id is configured.");
                }
            }
            else // FA
            {
                var groupName = cb.ResumeForceAdGroupName ?? "";
                if (string.IsNullOrEmpty(groupName))
                {
                    r.Add(Severity.Error, "AppResume", "resumeAdType=FA but resumeForceAdGroupName is empty.");
                    return;
                }

                var group = FindFAGroupByName(adcore, groupName);
                if (group == null)
                {
                    r.Add(Severity.Error, "AppResume", $"resumeForceAdGroupName=\"{groupName}\" not found in ForceAdGroupConfig[].groupName.");
                    return;
                }

                if (!HasValidFAIdByPriority(adcore, group))
                    r.Add(Severity.Error, "AppResume", $"resumeForceAdGroupName=\"{groupName}\" exists but missing id for priority={group.MediationPriority}.");
            }
        }

        private static void ValidateForceAd(AdSystemConfigs system, Configs adcore, ValidationReport r)
        {
            var force = system.ForceAdChannel;
            if (force == null) return;
            if (!force.IsEnabled) return;

            var posInfos = force.PositionConfigs;
            if (posInfos == null || posInfos.Length == 0)
            {
                r.Add(Severity.Warning, "ForceAd", "ForceAd is enabled but positionConfigs is null/empty.");
            }
            else
            {
                for (int i = 0; i < posInfos.Length; i++)
                {
                    var p = posInfos[i];
                    if (p == null) continue;

                    var pos = p.PositionName ?? "";
                    if (string.IsNullOrEmpty(pos))
                    {
                        r.Add(Severity.Warning, "ForceAd", $"positionConfigs[{i}] has empty positionName.");
                        continue;
                    }

                    var groupName = adcore.GetFAGroupByPos(pos);
                    if (string.IsNullOrEmpty(groupName))
                    {
                        r.Add(Severity.Error, "ForceAd", $"positionName \"{pos}\" not found in any ForceAdGroupConfig.positionNames (cannot map to group).");
                        continue;
                    }

                    var group = FindFAGroupByName(adcore, groupName);
                    if (group == null)
                    {
                        // Should not happen if GetFAGroupByPos is consistent, but keep safe.
                        r.Add(Severity.Error, "ForceAd", $"pos \"{pos}\" mapped to group \"{groupName}\" but group not found.");
                        continue;
                    }

                    if (!HasValidFAIdByPriority(adcore, group))
                        r.Add(Severity.Error, "ForceAd", $"pos \"{pos}\" uses group \"{groupName}\" but missing id for priority={group.MediationPriority}.");

                    if (group.UseBackup && !HasAnyFABackupIdBeyondPriority(adcore, group))
                        r.Add(Severity.Warning, "ForceAd", $"group \"{groupName}\" UseBackup=true but no backup ForceAd ids are configured beyond priority.");
                }
            }

            // BreakAds
            var breakAds = force.BreakAdConfig;
            if (breakAds != null && breakAds.IsEnabled)
            {
                var posName = breakAds.PositionName ?? "";
                if (string.IsNullOrEmpty(posName))
                {
                    r.Add(Severity.Error, "ForceAd.BreakAd", "BreakAd is enabled but positionName is empty.");
                }
                else
                {
                    var groupName = adcore.GetFAGroupByPos(posName);
                    if (string.IsNullOrEmpty(groupName))
                    {
                        r.Add(Severity.Error, "ForceAd.BreakAd", $"positionName \"{posName}\" not found in any ForceAdGroupConfig.positionNames.");
                    }
                    else
                    {
                        var group = FindFAGroupByName(adcore, groupName);
                        if (group == null)
                        {
                            r.Add(Severity.Error, "ForceAd.BreakAd", $"positionName \"{posName}\" mapped to group \"{groupName}\" but group not found.");
                        }
                        else if (!HasValidFAIdByPriority(adcore, group))
                        {
                            r.Add(Severity.Error, "ForceAd.BreakAd", $"positionName \"{posName}\" uses group \"{groupName}\" but missing id for priority={group.MediationPriority}.");
                        }
                        else if (group.UseBackup && !HasAnyFABackupIdBeyondPriority(adcore, group))
                        {
                            r.Add(Severity.Warning, "ForceAd.BreakAd", $"group \"{groupName}\" UseBackup=true but no backup ForceAd ids are configured beyond priority.");
                        }
                    }
                }

                if (breakAds.NotificationLeadTimeSeconds < 0)
                    r.Add(Severity.Warning, "ForceAd.BreakAd", $"notificationLeadTimeSeconds < 0 ({breakAds.NotificationLeadTimeSeconds}).");
            }
        }

        private static void ValidateRewarded(AdSystemConfigs system, Configs adcore, ValidationReport r)
        {
            var rwSys = system.RewardedChannel;
            if (rwSys == null) return;
            if (!rwSys.IsEnabled) return;

            var rw = adcore.RewardedUnit;
            if (rw == null)
            {
                r.Add(Severity.Error, "Rewarded", "Rewarded is enabled but RewardedUnitConfig is NULL.");
                return;
            }

            if (rw.MediationPriority == E_MediationPriority.Admob)
            {
                var id = SafeId(rw.AdmobUnit.Id);
                if (string.IsNullOrEmpty(id))
                    r.Add(Severity.Error, "Rewarded", "Rewarded priority=Admob but Admob id is empty.");
            }
            else if (rw.MediationPriority == E_MediationPriority.Android)
            {
                var id = SafeId(rw.AndroidUnit.Id);
                if (string.IsNullOrEmpty(id))
                    r.Add(Severity.Error, "Rewarded", "Rewarded priority=Android but Android id is empty.");
            }
            else
            {
                var id = SafeId(rw.MaxUnit);
                if (string.IsNullOrEmpty(id))
                    r.Add(Severity.Error, "Rewarded", "Rewarded priority=Max but Max id is empty.");
            }

            var adm = rw.AdmobUnit;
            if (adm.PreloadAd && adm.AdBufferSize <= 0)
                r.Add(Severity.Warning, "Rewarded", $"Admob preload=true but buffer={adm.AdBufferSize} (<=0).");

            if (rw.UseBackup)
            {
                bool hasBackupId = rw.MediationPriority switch
                {
                    E_MediationPriority.Admob =>
                        !string.IsNullOrEmpty(SafeId(rw.MaxUnit)) ||
                        !string.IsNullOrEmpty(SafeId(rw.AndroidUnit.Id)),
                    E_MediationPriority.Max =>
                        !string.IsNullOrEmpty(SafeId(rw.AdmobUnit.Id)) ||
                        !string.IsNullOrEmpty(SafeId(rw.AndroidUnit.Id)),
                    _ =>
                        !string.IsNullOrEmpty(SafeId(rw.AdmobUnit.Id)) ||
                        !string.IsNullOrEmpty(SafeId(rw.MaxUnit)),
                };

                if (!hasBackupId)
                    r.Add(Severity.Warning, "Rewarded", "Rewarded UseBackup=true but no backup rewarded ids are configured beyond priority.");
            }
        }

        private static void ValidateBanner(AdSystemConfigs system, Configs adcore, ValidationReport r)
        {
            var bnSys = system.BannerChannel;
            if (bnSys == null) return;
            if (!bnSys.IsEnabled) return;

            var bn = adcore.BannerUnit;
            if (bn == null)
            {
                r.Add(Severity.Error, "Banner", "Banner is enabled but BannerUnitConfig is NULL.");
                return;
            }

            ValidateBannerSlot("FullBottom", bn.FullBottom, r);
            ValidateBannerSlot("FullTop", bn.FullTop, r);
            ValidateBannerSlot("TopLeft", bn.TopLeft, r);
            ValidateBannerSlot("TopRight", bn.TopRight, r);
            ValidateBannerSlot("BottomLeft", bn.BottomLeft, r);
            ValidateBannerSlot("BottomRight", bn.BottomRight, r);
            ValidateBannerSlotDuplicateIds(bn, r);
        }

        private static void ValidateMrec(AdSystemConfigs system, Configs adcore, ValidationReport r)
        {
            var mSys = system.MrecChannel;
            if (mSys == null) return;
            if (!mSys.IsEnabled) return;

            var m = adcore.MrecUnit;
            if (m == null)
            {
                r.Add(Severity.Error, "Mrec", "Mrec is enabled but MrecUnitConfig is NULL.");
                return;
            }

            var id = m.MediationPriority == E_AdmobMaxMediationPriority.Max
                ? SafeId(m.MaxUnit)
                : SafeId(m.AdmobUnit.Id);
            if (string.IsNullOrEmpty(id))
                r.Add(Severity.Error, "Mrec", $"Mrec priority={m.MediationPriority} but corresponding id is empty.");

            if (m.UseBackup)
            {
                bool hasBackupId = m.MediationPriority == E_AdmobMaxMediationPriority.Max
                    ? !string.IsNullOrEmpty(SafeId(m.AdmobUnit.Id))
                    : !string.IsNullOrEmpty(SafeId(m.MaxUnit));

                if (!hasBackupId)
                    r.Add(Severity.Warning, "Mrec", "Mrec UseBackup=true but no backup mrec id is configured.");
            }
        }

        private static void ValidatePopup(AdSystemConfigs system, Configs adcore, ValidationReport r)
        {
            var puSys = system.PopupChannel;
            if (puSys == null) return;
            if (!puSys.IsEnabled) return;

            var posInfos = puSys.PositionConfigs;
            if (posInfos == null || posInfos.Length == 0)
            {
                r.Add(Severity.Warning, "Popup", "Popup is enabled but positionConfigs is null/empty.");
                return;
            }

            for (int i = 0; i < posInfos.Length; i++)
            {
                var p = posInfos[i];
                if (p == null) continue;

                if (!p.IsEnabled) continue;

                var pos = p.PositionName ?? "";
                if (string.IsNullOrEmpty(pos))
                {
                    r.Add(Severity.Warning, "Popup", $"positionConfigs[{i}] is enabled but positionName is empty.");
                    continue;
                }

                var groupName = adcore.GetPUGroupByPos(pos);
                if (string.IsNullOrEmpty(groupName))
                {
                    r.Add(Severity.Error, "Popup", $"positionName \"{pos}\" not found in any PopupGroupConfig.positionNames (cannot map to group).");
                    continue;
                }

                var group = FindPUGroupByName(adcore, groupName);
                if (group == null)
                {
                    r.Add(Severity.Error, "Popup", $"pos \"{pos}\" mapped to group \"{groupName}\" but group not found.");
                    continue;
                }

                var id = SafeId(group.AndroidUnit.Id);
                if (string.IsNullOrEmpty(id))
                    r.Add(Severity.Error, "Popup", $"pos \"{pos}\" uses PU group \"{groupName}\" but Android id is empty.");
            }
        }

        private static void ValidateCollap(AdSystemConfigs system, Configs adcore, ValidationReport r)
        {
            var clSys = system.CollapChannel;
            if (clSys == null) return;
            if (!clSys.IsEnabled) return;

            var cl = adcore.CollapUnit;
            if (cl == null)
            {
                r.Add(Severity.Error, "Collap", "Collap is enabled but CollapUnitConfig is NULL.");
                return;
            }

            var and = cl.AndroidUnit;
            var id = SafeId(and.Id);
            if (string.IsNullOrEmpty(id))
                r.Add(Severity.Error, "Collap", "Collap is enabled but Android id is empty.");

            if (and.TimeClose < 0) r.Add(Severity.Warning, "Collap", $"TimeClose < 0 ({and.TimeClose}).");
            if (and.ReloadByHiddenTime < 0) r.Add(Severity.Warning, "Collap", $"ReloadByHiddenTime < 0 ({and.ReloadByHiddenTime}).");
        }

        // =========================
        // AdCore Internal Checks
        // =========================
        private static void ValidateFAInternal(Configs adcore, ValidationReport r)
        {
            var fa = adcore.ForceAdGroups;
            if (fa == null || fa.Length == 0) return;

            var groupNameSeen = new HashSet<string>(StringComparer.Ordinal);
            var posOwner = new Dictionary<string, string>(StringComparer.Ordinal);

            for (int i = 0; i < fa.Length; i++)
            {
                var g = fa[i];
                if (g == null) continue;

                var gname = g.GroupName ?? "";
                if (string.IsNullOrEmpty(gname))
                    r.Add(Severity.Warning, "FA", $"ForceAdGroupConfig[{i}] GroupName is empty.");

                if (!string.IsNullOrEmpty(gname))
                {
                    if (!groupNameSeen.Add(gname))
                        r.Add(Severity.Warning, "FA", $"Duplicate GroupName \"{gname}\" found (priority will keep first in maps/lookups).");
                }

                var posArr = g.PositionNames;
                if (posArr == null || posArr.Length == 0)
                {
                    r.Add(Severity.Warning, "FA", $"Group \"{gname}\" has empty positionNames[] (GetFAGroupByPos may fail).");
                }
                else
                {
                    for (int p = 0; p < posArr.Length; p++)
                    {
                        var pos = posArr[p];
                        if (string.IsNullOrEmpty(pos)) continue;

                        if (posOwner.TryGetValue(pos, out var owner))
                        {
                            // duplicate pos across groups
                            r.Add(Severity.Warning, "FA", $"Duplicate pos \"{pos}\" found in groups: \"{owner}\" and \"{gname}\" (GetFAGroupByPos uses first).");
                        }
                        else
                        {
                            posOwner[pos] = gname;
                        }
                    }
                }

                if (g.MaxShowCount < 0)
                    r.Add(Severity.Warning, "FA", $"Group \"{gname}\" MaxShowCount < 0 ({g.MaxShowCount}).");

                // priority missing id warning
                if (g.MediationPriority == E_MediationPriority.Admob)
                {
                    if (string.IsNullOrEmpty(SafeId(g.AdmobUnit.Id)) && !string.IsNullOrEmpty(SafeId(g.AndroidUnit.Id)))
                        r.Add(Severity.Warning, "FA", $"Group \"{gname}\" priority=Admob but Admob id empty while Android id exists.");
                }
                else if (g.MediationPriority == E_MediationPriority.Android)
                {
                    if (string.IsNullOrEmpty(SafeId(g.AndroidUnit.Id)) && !string.IsNullOrEmpty(SafeId(g.AdmobUnit.Id)))
                        r.Add(Severity.Warning, "FA", $"Group \"{gname}\" priority=Android but Android id empty while Admob id exists.");
                }
                else if (g.MediationPriority == E_MediationPriority.Max)
                {
                    if (string.IsNullOrEmpty(SafeId(adcore.ForceAdMaxUnit?.MaxUnit)) &&
                        (!string.IsNullOrEmpty(SafeId(g.AdmobUnit.Id)) || !string.IsNullOrEmpty(SafeId(g.AndroidUnit.Id))))
                    {
                        r.Add(Severity.Warning, "FA", $"Group \"{gname}\" priority=Max but ForceAdMaxUnit empty while other mediation ids exist.");
                    }
                }

                if (g.UseBackup && !HasAnyFABackupIdBeyondPriority(adcore, g))
                    r.Add(Severity.Warning, "FA", $"Group \"{gname}\" UseBackup=true but no backup ForceAd ids are configured beyond priority.");
            }
        }

        private static void ValidatePUInternal(Configs adcore, ValidationReport r)
        {
            var pu = adcore.PopupGroups;
            if (pu == null || pu.Length == 0) return;

            var groupNameSeen = new HashSet<string>(StringComparer.Ordinal);
            var posOwner = new Dictionary<string, string>(StringComparer.Ordinal);

            for (int i = 0; i < pu.Length; i++)
            {
                var g = pu[i];
                if (g == null) continue;

                var gname = g.GroupName ?? "";
                if (string.IsNullOrEmpty(gname))
                    r.Add(Severity.Warning, "PU", $"PopupGroupConfig[{i}] GroupName is empty.");

                if (!string.IsNullOrEmpty(gname))
                {
                    if (!groupNameSeen.Add(gname))
                        r.Add(Severity.Warning, "PU", $"Duplicate GroupName \"{gname}\" found (lookups keep first).");
                }

                var posArr = g.PositionNames;
                if (posArr == null || posArr.Length == 0)
                {
                    r.Add(Severity.Warning, "PU", $"Group \"{gname}\" has empty positionNames[] (GetPUGroupByPos may fail).");
                }
                else
                {
                    for (int p = 0; p < posArr.Length; p++)
                    {
                        var pos = posArr[p];
                        if (string.IsNullOrEmpty(pos)) continue;

                        if (posOwner.TryGetValue(pos, out var owner))
                            r.Add(Severity.Warning, "PU", $"Duplicate pos \"{pos}\" found in groups: \"{owner}\" and \"{gname}\" (GetPUGroupByPos uses first).");
                        else
                            posOwner[pos] = gname;
                    }
                }

                var id = SafeId(g.AndroidUnit.Id);
                if (string.IsNullOrEmpty(id))
                    r.Add(Severity.Warning, "PU", $"Group \"{gname}\" Android id is empty.");
            }
        }

        private static void ValidateUnusedInfos(AdSystemConfigs system, Configs adcore, ValidationReport r)
        {
            // Banner disabled but ids exist
            var bnSys = system.BannerChannel;
            if (bnSys != null && !bnSys.IsEnabled)
            {
                var bn = adcore.BannerUnit;
                if (bn != null)
                {
                    bool hasAny =
                        HasAnyBannerId(bn.FullBottom) ||
                        HasAnyBannerId(bn.FullTop) ||
                        HasAnyBannerId(bn.TopLeft) ||
                        HasAnyBannerId(bn.TopRight) ||
                        HasAnyBannerId(bn.BottomLeft) ||
                        HasAnyBannerId(bn.BottomRight);
                    if (hasAny)
                        r.Add(Severity.Info, "Banner", "Banner is disabled in system but AdCore still has banner ids set.");
                }
            }

            // Mrec disabled but id exist
            var mSys = system.MrecChannel;
            if (mSys != null && !mSys.IsEnabled)
            {
                var m = adcore.MrecUnit;
                if (m != null && (!string.IsNullOrEmpty(SafeId(m.AdmobUnit.Id)) || !string.IsNullOrEmpty(SafeId(m.MaxUnit))))
                    r.Add(Severity.Info, "Mrec", "Mrec is disabled in system but AdCore still has mrec id set.");
            }

            // Rewarded disabled but ids exist
            var rwSys = system.RewardedChannel;
            if (rwSys != null && !rwSys.IsEnabled)
            {
                var rw = adcore.RewardedUnit;
                if (rw != null)
                {
                    bool hasAny =
                        !string.IsNullOrEmpty(SafeId(rw.AdmobUnit.Id)) ||
                        !string.IsNullOrEmpty(SafeId(rw.AndroidUnit.Id)) ||
                        !string.IsNullOrEmpty(SafeId(rw.MaxUnit));
                    if (hasAny)
                        r.Add(Severity.Info, "Rewarded", "Rewarded is disabled in system but AdCore still has rewarded ids set.");
                }
            }

            // Popup disabled but ids exist
            var popSys = system.PopupChannel;
            if (popSys != null && !popSys.IsEnabled)
            {
                var pu = adcore.PopupGroups;
                if (pu != null && pu.Length > 0)
                    r.Add(Severity.Info, "Popup", "Popup is disabled in system but AdCore still has PU groups configured.");
            }

            // Collap disabled but id exist
            var clSys = system.CollapChannel;
            if (clSys != null && !clSys.IsEnabled)
            {
                var cl = adcore.CollapUnit;
                if (cl != null && !string.IsNullOrEmpty(SafeId(cl.AndroidUnit.Id)))
                    r.Add(Severity.Info, "Collap", "Collap is disabled in system but AdCore still has collap id set.");
            }

            // AppLaunch/AppResume disabled but comeback exists (info)
            var la = system.AppLaunchChannel;
            if (la != null && !la.IsEnabled && adcore.ComebackChannel != null)
                r.Add(Severity.Info, "AppLaunch", "AppLaunch disabled but ComebackChannelInfo exists (ok).");

            var re = system.AppResumeChannel;
            if (re != null && !re.IsEnabled && adcore.ComebackChannel != null)
                r.Add(Severity.Info, "AppResume", "AppResume disabled but ComebackChannelInfo exists (ok).");
        }

        // =========================
        // Helpers
        // =========================
        private static string SafeId(string id)
        {
            return string.IsNullOrEmpty(id) ? "" : id;
        }

        private static ForceAdGroupConfig FindFAGroupByName(Configs adcore, string groupName)
        {
            if (adcore == null) return null;
            if (string.IsNullOrEmpty(groupName)) return null;

            var fa = adcore.ForceAdGroups;
            if (fa == null) return null;

            for (int i = 0; i < fa.Length; i++)
            {
                var g = fa[i];
                if (g == null) continue;

                if (string.Equals(g.GroupName, groupName, StringComparison.Ordinal))
                    return g;
            }

            return null;
        }

        private static PopupGroupConfig FindPUGroupByName(Configs adcore, string groupName)
        {
            if (adcore == null) return null;
            if (string.IsNullOrEmpty(groupName)) return null;

            var pu = adcore.PopupGroups;
            if (pu == null) return null;

            for (int i = 0; i < pu.Length; i++)
            {
                var g = pu[i];
                if (g == null) continue;

                if (string.Equals(g.GroupName, groupName, StringComparison.Ordinal))
                    return g;
            }

            return null;
        }

        private static bool HasValidFAIdByPriority(Configs adcore, ForceAdGroupConfig group)
        {
            if (group == null) return false;

            if (group.MediationPriority == E_MediationPriority.Admob)
                return !string.IsNullOrEmpty(SafeId(group.AdmobUnit.Id));

            if (group.MediationPriority == E_MediationPriority.Android)
                return !string.IsNullOrEmpty(SafeId(group.AndroidUnit.Id));

            return !string.IsNullOrEmpty(SafeId(adcore?.ForceAdMaxUnit?.MaxUnit));
        }

        private static bool HasAnyFABackupIdBeyondPriority(Configs adcore, ForceAdGroupConfig group)
        {
            if (group == null) return false;

            if (group.MediationPriority != E_MediationPriority.Admob &&
                !string.IsNullOrEmpty(SafeId(group.AdmobUnit.Id)))
                return true;

            if (group.MediationPriority != E_MediationPriority.Max &&
                !string.IsNullOrEmpty(SafeId(adcore?.ForceAdMaxUnit?.MaxUnit)))
                return true;

            if (group.MediationPriority != E_MediationPriority.Android &&
                !string.IsNullOrEmpty(SafeId(group.AndroidUnit.Id)))
                return true;

            return false;
        }

        private static void CollectBannerSlotIds(string slotName, BannerFullBottomSlotUnitConfig slot, ValidationReport r)
        {
            if (slot == null) return;

            var admobId = SafeId(slot.AdmobUnit.Id);
            if (!string.IsNullOrEmpty(admobId))
                r.Ids.Add(new IdEntry("BN", "Admob", slotName, admobId, $"priority={slot.MediationPriority} backup={slot.UseBackup}"));

            var maxId = SafeId(slot.MaxUnit);
            if (!string.IsNullOrEmpty(maxId))
                r.Ids.Add(new IdEntry("BN", "Max", slotName, maxId, $"priority={slot.MediationPriority} backup={slot.UseBackup}"));

            var androidId = SafeId(slot.AndroidUnit.Id);
            if (!string.IsNullOrEmpty(androidId))
                r.Ids.Add(new IdEntry("BN", "Android", slotName, androidId, $"layoutNames={FormatLayouts(slot.AndroidUnit.Layouts)} priority={slot.MediationPriority} backup={slot.UseBackup}"));
        }

        private static void CollectBannerSlotIds(string slotName, BannerSlotUnitConfig slot, ValidationReport r)
        {
            if (slot == null) return;

            var admobId = SafeId(slot.AdmobUnit.Id);
            if (!string.IsNullOrEmpty(admobId))
                r.Ids.Add(new IdEntry("BN", "Admob", slotName, admobId, $"priority={slot.MediationPriority} backup={slot.UseBackup}"));

            var maxId = SafeId(slot.MaxUnit);
            if (!string.IsNullOrEmpty(maxId))
                r.Ids.Add(new IdEntry("BN", "Max", slotName, maxId, $"priority={slot.MediationPriority} backup={slot.UseBackup}"));
        }

        private static void ValidateBannerSlot(string slotName, BannerFullBottomSlotUnitConfig slot, ValidationReport r)
        {
            if (slot == null)
            {
                r.Add(Severity.Error, "Banner", $"Banner slot {slotName} is NULL.");
                return;
            }

            var id = slot.MediationPriority switch
            {
                E_MediationPriority.Max => SafeId(slot.MaxUnit),
                E_MediationPriority.Android => SafeId(slot.AndroidUnit.Id),
                _ => SafeId(slot.AdmobUnit.Id),
            };

            if (string.IsNullOrEmpty(id))
                r.Add(Severity.Error, "Banner", $"Banner slot {slotName} priority={slot.MediationPriority} but corresponding id is empty.");

            if (slot.UseBackup &&
                !HasAnyBannerBackupIdBeyondPriority(slot))
                r.Add(Severity.Warning, "Banner", $"Banner slot {slotName} UseBackup=true but no backup banner id is configured beyond priority.");
        }

        private static string FormatLayouts(string[] layouts)
        {
            return layouts != null && layouts.Length > 0
                ? $"[{string.Join(",", layouts)}]"
                : "(empty)";
        }

        private static void ValidateBannerSlot(string slotName, BannerSlotUnitConfig slot, ValidationReport r)
        {
            if (slot == null)
            {
                r.Add(Severity.Error, "Banner", $"Banner slot {slotName} is NULL.");
                return;
            }

            var id = slot.MediationPriority == E_AdmobMaxMediationPriority.Max
                ? SafeId(slot.MaxUnit)
                : SafeId(slot.AdmobUnit.Id);

            if (string.IsNullOrEmpty(id))
                r.Add(Severity.Error, "Banner", $"Banner slot {slotName} priority={slot.MediationPriority} but corresponding id is empty.");

            if (slot.UseBackup && !HasAnyBannerBackupIdBeyondPriority(slot))
                r.Add(Severity.Warning, "Banner", $"Banner slot {slotName} UseBackup=true but no backup banner id is configured beyond priority.");
        }

        private static bool HasAnyBannerId(BannerFullBottomSlotUnitConfig slot)
        {
            if (slot == null) return false;
            return !string.IsNullOrEmpty(SafeId(slot.AdmobUnit.Id)) ||
                   !string.IsNullOrEmpty(SafeId(slot.MaxUnit)) ||
                   !string.IsNullOrEmpty(SafeId(slot.AndroidUnit.Id));
        }

        private static bool HasAnyBannerId(BannerSlotUnitConfig slot)
        {
            if (slot == null) return false;
            return !string.IsNullOrEmpty(SafeId(slot.AdmobUnit.Id)) || !string.IsNullOrEmpty(SafeId(slot.MaxUnit));
        }

        private static bool HasAnyBannerBackupIdBeyondPriority(BannerFullBottomSlotUnitConfig slot)
        {
            if (slot == null) return false;

            if (slot.MediationPriority != E_MediationPriority.Admob &&
                !string.IsNullOrEmpty(SafeId(slot.AdmobUnit.Id)))
                return true;

            if (slot.MediationPriority != E_MediationPriority.Max &&
                !string.IsNullOrEmpty(SafeId(slot.MaxUnit)))
                return true;

            if (slot.MediationPriority != E_MediationPriority.Android &&
                !string.IsNullOrEmpty(SafeId(slot.AndroidUnit.Id)))
                return true;

            return false;
        }

        private static bool HasAnyBannerBackupIdBeyondPriority(BannerSlotUnitConfig slot)
        {
            if (slot == null) return false;

            if (slot.MediationPriority != E_AdmobMaxMediationPriority.Admob &&
                !string.IsNullOrEmpty(SafeId(slot.AdmobUnit.Id)))
                return true;

            if (slot.MediationPriority != E_AdmobMaxMediationPriority.Max &&
                !string.IsNullOrEmpty(SafeId(slot.MaxUnit)))
                return true;

            return false;
        }

        private static void ValidateBannerSlotDuplicateIds(BannerUnitConfig unit, ValidationReport r)
        {
            if (unit == null) return;

            AddDuplicateBannerSlotWarnings(
                "Admob",
                new (string Name, string Id)[]
                {
                    ("FullBottom", SafeId(unit.FullBottom?.AdmobUnit.Id)),
                    ("FullTop", SafeId(unit.FullTop?.AdmobUnit.Id)),
                    ("TopLeft", SafeId(unit.TopLeft?.AdmobUnit.Id)),
                    ("TopRight", SafeId(unit.TopRight?.AdmobUnit.Id)),
                    ("BottomLeft", SafeId(unit.BottomLeft?.AdmobUnit.Id)),
                    ("BottomRight", SafeId(unit.BottomRight?.AdmobUnit.Id)),
                },
                "Banner slots are reusing the same AdMob unit id. Separate units per placement are recommended.",
                r
            );

            AddDuplicateBannerSlotWarnings(
                "Max",
                new (string Name, string Id)[]
                {
                    ("FullBottom", SafeId(unit.FullBottom?.MaxUnit)),
                    ("FullTop", SafeId(unit.FullTop?.MaxUnit)),
                    ("TopLeft", SafeId(unit.TopLeft?.MaxUnit)),
                    ("TopRight", SafeId(unit.TopRight?.MaxUnit)),
                    ("BottomLeft", SafeId(unit.BottomLeft?.MaxUnit)),
                    ("BottomRight", SafeId(unit.BottomRight?.MaxUnit)),
                },
                "Banner slots are reusing the same MAX unit id. Multi-banner flow should use a unique MAX unit per placement.",
                r
            );
        }

        private static void AddDuplicateBannerSlotWarnings(string mediation, (string Name, string Id)[] slots, string detail, ValidationReport r)
        {
            var map = new Dictionary<string, List<string>>(StringComparer.Ordinal);

            for (int i = 0; i < slots.Length; i++)
            {
                var id = slots[i].Id;
                if (string.IsNullOrEmpty(id))
                    continue;

                if (!map.TryGetValue(id, out var names))
                {
                    names = new List<string>();
                    map[id] = names;
                }

                names.Add(slots[i].Name);
            }

            foreach (var pair in map)
            {
                if (pair.Value.Count < 2)
                    continue;

                r.Add(Severity.Warning, "Banner",
                    $"{mediation} banner id is duplicated across placements [{string.Join(", ", pair.Value)}]. {detail}");
            }
        }
    }
}
