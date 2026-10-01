using System.Text.RegularExpressions;
using UnityEditor;
using BG_Library.Common;
using BG_Library.NET.AdCore;

namespace BG_Library.NET.AdCore.MainAndroid
{
    public class EditConfigsWindow : GenericJsonInspectorWindow<Configs, StatsSettings>
    {
        [MenuItem(StatsConst.MENU)]
        public static void Open()
        {
            var w = GetWindow<EditConfigsWindow>(StatsConst.TITLE);
            w.minSize = new UnityEngine.Vector2(800, 600);
            w.Show();
        }
    }

    public static class StatsConst
    {
        public const string MENU = "BG/Edit stats/AdCore Android configs #1"; // Shift + M
        public const string TITLE = "Edit adcore_main_android";
    }

    public class StatsSettings : IGenericJsonInspectorSettings<Configs>
    {
        public string WindowTitle => StatsConst.TITLE;
        public string LogTag => "AdCoreMainAndroid_Configs";
        public string StorageDescription => NetConfigsSOEditorStorage.GetAdCoreStorageDescription("adcore_main_android", true);

        public float JsonHeight => 420f;
        public bool WordWrapJson => false;

        public Configs CreateDefault() => new Configs();

        public string Serialize(Configs data) => JsonTool.SerializeObject(data);
        public Configs Deserialize(string json) => JsonTool.DeserializeObject<Configs>(json);
        public string LoadStoredJson() => NetConfigsSOEditorStorage.LoadAdCoreConfigJson("adcore_main_android", true);
        public void SaveStoredJson(string json) => NetConfigsSOEditorStorage.SaveAdCoreConfigJson("adcore_main_android", true, json);

    }
}
