#if UNITY_EDITOR
using BG_Library.Common;
using BG_Library.NET.AdCore;
using BG_Library.NET.AdCore.MainAndroid;
using UnityEditor;

namespace BG_Library.NET.AdCore.MainIOS
{
    public class EditIOSConfigsWindow : GenericJsonInspectorWindow<Configs, IOSStatsSettings>
    {
        [MenuItem(IOSStatsConst.MENU)]
        public static void Open()
        {
            var w = GetWindow<EditIOSConfigsWindow>(IOSStatsConst.TITLE);
            w.minSize = new UnityEngine.Vector2(800, 600);
            w.Show();
        }
    }

    public static class IOSStatsConst
    {
        public const string MENU = "BG/Edit stats/AdCore IOS configs #2";
        public const string TITLE = "Edit adcore_main_ios";
        public const string AD_CORE_NAME = "adcore_main_ios";
    }

    public class IOSStatsSettings : IGenericJsonInspectorSettings<Configs>
    {
        public string WindowTitle => IOSStatsConst.TITLE;
        public string LogTag => "AdCoreMainIOS_Configs";
        public string StorageDescription => NetConfigsSOEditorStorage.GetAdCoreStorageDescription(IOSStatsConst.AD_CORE_NAME, false);

        public float JsonHeight => 420f;
        public bool WordWrapJson => false;

        public Configs CreateDefault() => new Configs();

        public string Serialize(Configs data) => JsonTool.SerializeObject(data);
        public Configs Deserialize(string json) => JsonTool.DeserializeObject<Configs>(json);
        public string LoadStoredJson() => NetConfigsSOEditorStorage.LoadAdCoreConfigJson(IOSStatsConst.AD_CORE_NAME, false);
        public void SaveStoredJson(string json) => NetConfigsSOEditorStorage.SaveAdCoreConfigJson(IOSStatsConst.AD_CORE_NAME, false, json);
    }
}
#endif
