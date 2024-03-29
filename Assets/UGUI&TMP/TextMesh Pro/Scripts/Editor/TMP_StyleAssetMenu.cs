﻿using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;


namespace TMPro.EditorUtilities
{

    public static class TMP_StyleAssetMenu
    {

        [MenuItem("Assets/UI/TMP - Style Sheet", false, 6)]
        public static void CreateTextMeshProObjectPerform()
        {
            string filePath;
            if (Selection.assetGUIDs.Length == 0)
            {
                // No asset selected.
                filePath = "Assets";
            }
            else
            {
                // Get the path of the selected folder or asset.
                filePath = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);

                // Get the file extension of the selected asset as it might need to be removed.
                string fileExtension = Path.GetExtension(filePath);
                if (fileExtension != "")
                {
                    filePath = Path.GetDirectoryName(filePath);
                }
            }


            string filePathWithName = AssetDatabase.GenerateUniqueAssetPath(filePath + "/Text StyleSheet.asset");

            //// Create new Style Sheet Asset.
            TMP_StyleSheet styleSheet = ScriptableObject.CreateInstance<TMP_StyleSheet>();

            // Create Normal default style
            TMP_Style style = new TMP_Style("Normal", string.Empty, string.Empty);
            styleSheet.styles.Add(style);

            AssetDatabase.CreateAsset(styleSheet, filePathWithName);

            EditorUtility.SetDirty(styleSheet);

            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();
            EditorGUIUtility.PingObject(styleSheet);
        }
    }

}
