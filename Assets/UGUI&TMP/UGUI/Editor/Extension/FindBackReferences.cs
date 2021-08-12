using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace UnityEditor.UI
{
    internal static class FindBackReferences
    {
        private static readonly string[] SearchObjects =
        {
            ".prefab",
            ".unity",
            ".mat",
            ".asset",
        };

        [MenuItem("Assets/反向查找资源引用")]
        private static void Find()
        {
            EditorSettings.serializationMode = SerializationMode.ForceText;
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path)) return; //找不到这个对象的路径
            StringBuilder sb = new StringBuilder();
            sb.Append("<color=cyan>开始反向查找引用到 " + path + " 的资源:\n");
            string guid = AssetDatabase.AssetPathToGUID(path);
            string[] files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories)
                .Where(s => SearchObjects.Contains(Path.GetExtension(s).ToLower())).ToArray();

            int length = files.Length;
            for (int i = 0; i < length; i++)
            {
                string filePath = files[i];
                bool isCancel = EditorUtility.DisplayCancelableProgressBar("匹配资源中", filePath, (float) i / length);
                if (Regex.IsMatch(File.ReadAllText(filePath), guid))
                {
                    sb.Append(filePath + "  引用到 " + path + "\n");
                }
            }

            EditorUtility.ClearProgressBar();
            sb.Append("反向查找引用结束</color>");
            Debug.Log(sb.ToString());
        }
    }
}