using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BG_Library.Common
{
    public static class JsonTool
    {
        const string INDENT_STRING = "    ";
        public static string FormatJson(string str)
        {
            var indent = 0;
            var quoted = false;
            var sb = new StringBuilder();
            for (var i = 0; i < str.Length; i++)
            {
                var ch = str[i];
                switch (ch)
                {
                    case '{':
                    case '[':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, ++indent).ForEach(item => sb.Append(INDENT_STRING));
                        }
                        break;
                    case '}':
                    case ']':
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, --indent).ForEach(item => sb.Append(INDENT_STRING));
                        }
                        sb.Append(ch);
                        break;
                    case '"':
                        sb.Append(ch);
                        bool escaped = false;
                        var index = i;
                        while (index > 0 && str[--index] == '\\')
                            escaped = !escaped;
                        if (!escaped)
                            quoted = !quoted;
                        break;
                    case ',':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, indent).ForEach(item => sb.Append(INDENT_STRING));
                        }
                        break;
                    case ':':
                        sb.Append(ch);
                        if (!quoted)
                            sb.Append(" ");
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Serialize to json string format
        /// </summary>
        public static string SerializeObject(object obj)
        {
            return FormatJson(JsonUtility.ToJson(obj));
        }

        public static T DeserializeObject<T>(string st)
        {
            return JsonUtility.FromJson<T>(st);
        }

        public static void CreateJsonFile(string path, string jsonString)
        {
            var temp = Application.dataPath + path + ".json";
            if (!File.Exists(temp))
            {
                File.WriteAllText(temp, jsonString);
            }
            else
            {
                SaveJson(path, jsonString);
            }

#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        public static void SaveJson(string path, string jsonString)
        {
            var temp = Application.dataPath + path + ".json";
            using (var stream = new FileStream(temp, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(jsonString);
                }
            }
        }
    }
}

static class Extensions
{
    public static void ForEach<T>(this IEnumerable<T> ie, System.Action<T> action)
    {
        foreach (var i in ie)
        {
            action(i);
        }
    }
}