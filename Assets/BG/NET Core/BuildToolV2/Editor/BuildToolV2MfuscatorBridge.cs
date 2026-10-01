using System;
using System.Reflection;

namespace BG_Library.BuildToolV2
{
    /// <summary>
    /// Snapshot of the values read back from <c>Mfuscator.Settings.Object</c>.
    /// Mirrors <see cref="MfuscatorPresetV2"/> so the section can diff the two.
    /// </summary>
    internal sealed class MfuscatorSettingsSnapshot
    {
        public bool enable;
        public int callbackOrder;
        public bool logInfo;
        public bool removeStringLiterals;
        public bool preserveUnityCrashHandler;
        public bool checkFunctionCalls;
        public bool renameExports;
        public string renameExportsBlacklist = string.Empty;
        public bool removeMonoExports;
        public bool modifyInternalStructures;
        public bool detectProxyLibraries;
        public string detectProxyLibrariesWhitelist = string.Empty;
    }

    /// <summary>
    /// Reflection bridge to Mfuscator's runtime <c>Settings.Object</c> singleton. Read and
    /// write are best-effort — both return false (without throwing) if Mfuscator is not
    /// present in the project or its internal layout changed in a way our reflection no
    /// longer matches. Caller treats false as "open MFS Settings and configure manually".
    /// </summary>
    internal static class BuildToolV2MfuscatorBridge
    {
        private const BindingFlags MemberFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        // Cache of resolved reflection handles. Reset when Unity reloads domain.
        private static Type cachedSettingsType;
        private static MemberInfo cachedObjectMember;
        private static MemberInfo cachedInterMember;
        private static bool layoutResolved;
        private static bool layoutResolveFailed;

        public static bool TryReadCurrent(out MfuscatorSettingsSnapshot snapshot)
        {
            snapshot = null;
            if (!TryResolveLayout(out object settingsObject, out object interObject))
                return false;

            try
            {
                snapshot = new MfuscatorSettingsSnapshot
                {
                    enable = ReadBool(settingsObject, "enable"),
                    callbackOrder = ReadInt(settingsObject, "callbackOrder"),
                    logInfo = ReadBool(settingsObject, "logInfo"),
                    removeStringLiterals = ReadBool(interObject, "removeStringLiterals"),
                    preserveUnityCrashHandler = ReadBool(interObject, "preserveUnityCrashHandler"),
                    checkFunctionCalls = ReadBool(interObject, "checkFunctionCalls"),
                    renameExports = ReadBool(interObject, "renameExports"),
                    renameExportsBlacklist = ReadString(interObject, "renameExportsBlacklist"),
                    removeMonoExports = ReadBool(interObject, "removeMonoExports"),
                    modifyInternalStructures = ReadBool(interObject, "modifyInternalStructures"),
                    detectProxyLibraries = ReadBool(interObject, "detectProxyLibraries"),
                    detectProxyLibrariesWhitelist = ReadString(interObject, "detectProxyLibrariesWhitelist"),
                };
                return true;
            }
            catch
            {
                snapshot = null;
                return false;
            }
        }

        public static bool TryApply(MfuscatorPresetV2 preset)
        {
            if (preset == null)
                return false;

            if (!TryResolveLayout(out object settingsObject, out object interObject))
                return false;

            try
            {
                WriteBool(settingsObject, "enable", preset.enable);
                WriteInt(settingsObject, "callbackOrder", preset.callbackOrder);
                WriteBool(settingsObject, "logInfo", preset.logInfo);
                WriteBool(interObject, "removeStringLiterals", preset.removeStringLiterals);
                WriteBool(interObject, "preserveUnityCrashHandler", preset.preserveUnityCrashHandler);
                WriteBool(interObject, "checkFunctionCalls", preset.checkFunctionCalls);
                WriteBool(interObject, "renameExports", preset.renameExports);
                WriteString(interObject, "renameExportsBlacklist", preset.renameExportsBlacklist ?? string.Empty);
                WriteBool(interObject, "removeMonoExports", preset.removeMonoExports);
                WriteBool(interObject, "modifyInternalStructures", preset.modifyInternalStructures);
                WriteBool(interObject, "detectProxyLibraries", preset.detectProxyLibraries);
                WriteString(interObject, "detectProxyLibrariesWhitelist", preset.detectProxyLibrariesWhitelist ?? string.Empty);

                // The `inter` member is a struct — writing fields above mutates a boxed copy.
                // Re-assign it back to the parent Settings.Object so changes stick.
                WriteMember(settingsObject, cachedInterMember, interObject);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>True when Mfuscator's editor assembly is loaded and our reflection found the expected members.</summary>
        public static bool IsAvailable()
        {
            return TryResolveLayout(out _, out _);
        }

        private static bool TryResolveLayout(out object settingsObject, out object interObject)
        {
            settingsObject = null;
            interObject = null;

            if (layoutResolveFailed)
                return false;

            if (!layoutResolved)
                ResolveLayout();

            if (cachedSettingsType == null || cachedObjectMember == null || cachedInterMember == null)
            {
                layoutResolveFailed = true;
                return false;
            }

            try
            {
                settingsObject = ReadMember(null, cachedObjectMember);
                if (settingsObject == null)
                    return false;

                interObject = ReadMember(settingsObject, cachedInterMember);
                return interObject != null;
            }
            catch
            {
                return false;
            }
        }

        private static void ResolveLayout()
        {
            layoutResolved = true;
            cachedSettingsType = ResolveType("Mfuscator.Settings");
            if (cachedSettingsType == null)
                return;

            cachedObjectMember = ResolveMember(cachedSettingsType, "Object");
            if (cachedObjectMember == null)
                return;

            Type interParentType = GetMemberType(cachedObjectMember);
            if (interParentType == null)
                return;

            cachedInterMember = ResolveMember(interParentType, "inter");
        }

        private static Type ResolveType(string fullName)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                Type type = assemblies[i].GetType(fullName, throwOnError: false);
                if (type != null)
                    return type;
            }
            return null;
        }

        private static MemberInfo ResolveMember(Type type, string name)
        {
            if (type == null || string.IsNullOrEmpty(name))
                return null;

            FieldInfo field = type.GetField(name, MemberFlags);
            if (field != null)
                return field;

            PropertyInfo property = type.GetProperty(name, MemberFlags);
            return property;
        }

        private static Type GetMemberType(MemberInfo member)
        {
            switch (member)
            {
                case FieldInfo field: return field.FieldType;
                case PropertyInfo property: return property.PropertyType;
                default: return null;
            }
        }

        private static object ReadMember(object target, MemberInfo member)
        {
            switch (member)
            {
                case FieldInfo field: return field.GetValue(target);
                case PropertyInfo property: return property.GetValue(target);
                default: return null;
            }
        }

        private static void WriteMember(object target, MemberInfo member, object value)
        {
            switch (member)
            {
                case FieldInfo field:
                    field.SetValue(target, value);
                    break;
                case PropertyInfo property:
                    if (property.CanWrite)
                        property.SetValue(target, value);
                    break;
            }
        }

        private static bool ReadBool(object target, string name)
        {
            MemberInfo member = ResolveMember(target?.GetType(), name);
            object value = ReadMember(target, member);
            return value is bool b && b;
        }

        private static int ReadInt(object target, string name)
        {
            MemberInfo member = ResolveMember(target?.GetType(), name);
            object value = ReadMember(target, member);
            return value is int i ? i : 0;
        }

        private static string ReadString(object target, string name)
        {
            MemberInfo member = ResolveMember(target?.GetType(), name);
            object value = ReadMember(target, member);
            return value as string ?? string.Empty;
        }

        private static void WriteBool(object target, string name, bool value)
        {
            MemberInfo member = ResolveMember(target?.GetType(), name);
            if (member != null)
                WriteMember(target, member, value);
        }

        private static void WriteInt(object target, string name, int value)
        {
            MemberInfo member = ResolveMember(target?.GetType(), name);
            if (member != null)
                WriteMember(target, member, value);
        }

        private static void WriteString(object target, string name, string value)
        {
            MemberInfo member = ResolveMember(target?.GetType(), name);
            if (member != null)
                WriteMember(target, member, value);
        }
    }
}
