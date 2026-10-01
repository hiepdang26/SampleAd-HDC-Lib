using System.IO;
using System.Linq;
using UnityEngine;

namespace BG_Library.Common
{
    public static class LoadSource
    {
        #region Methob
        public static T LoadObject<T>(string path) where T : Object
        {
            return Resources.Load<T>(path);
        }
        #endregion

        #region Class
        [System.Serializable]
        public class SpriteArr
        {
            public Sprite[] spriteArr;
        }
        #endregion

        #region EDITOR METHOB
        /// <summary>
        /// Path: Bat dau tu folder ben trong Assets
        /// </summary>
        public static int NumberItem(string path)
        {
#if UNITY_EDITOR
            var d = new DirectoryInfo(Application.dataPath + "/" + path);
            FileInfo[] fis = System.Array.FindAll(d.GetFiles(), f => f.Name.Contains(".meta"));

            return fis.Length;
#else
            return 0;
#endif
        }

        /// <summary>
        /// Path: Bat dau tu folder ben trong Assets
        /// </summary>
        public static int NumberItem(string path, string containtedSt)
        {
#if UNITY_EDITOR
            var d = new DirectoryInfo(Application.dataPath + "/" + path);
            FileInfo[] fis = System.Array.FindAll(d.GetFiles(), f => f.Name.Contains(".meta"));

            return System.Array.FindAll(fis, f => f.Name.Contains(containtedSt)).Length;
#else
            return 0;
#endif
        }

        /// <summary>
        /// Path: Bat dau tu folder ben trong Assets
        /// </summary>
        public static string[] NumberItemName(string path)
        {
#if UNITY_EDITOR
            var d = new DirectoryInfo(Application.dataPath + "/" + path);

            FileInfo[] fis = System.Array.FindAll(d.GetFiles(), f => f.Name.Contains(".meta"));

            var st = new string[fis.Length];
            for (int i = 0; i < st.Length; i++)
            {
                st[i] = fis[i].Name.Replace(".meta", "");
            }

            return st;
#else
            return null;
#endif
        }

        public static T[] LoadAllAssetAtPath<T>(string path) where T : Object
        {
#if UNITY_EDITOR
            var guids = UnityEditor.AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { path });

            T[] objs = new T[guids.Length];
            for (int i = 0; i < objs.Length; i++)
            {
                objs[i] = (T)UnityEditor.AssetDatabase.LoadAssetAtPath(
                    UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]), typeof(T));
            }
            return objs;
#else
            return null;
#endif
        }

        public static T LoadAssetAtPath<T>(string path) where T : Object
        {
#if UNITY_EDITOR
            return (T)UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(T));
#else
            return null;
#endif
        }

        public static Sprite[] LoadSpriteSheet(string path)
        {
#if UNITY_EDITOR
            var listObjs = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
            var sprites = new Sprite[listObjs.Length - 1];
            for (int i = 1; i < listObjs.Length; i++)
            {
                sprites[i - 1] = (Sprite)listObjs[i];
            }

            return sprites;
#else
            return null;
#endif
        }
        #endregion
    }
}