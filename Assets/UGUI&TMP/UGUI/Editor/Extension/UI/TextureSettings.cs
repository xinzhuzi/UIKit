
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;


public class TextureSettings : Editor
{
    [MenuItem("Assets/UI/SetTexture_Format", false)]
    static void PrefabRawImage()
    {
        string[] paths = Selection.assetGUIDs;
        string dirPath = AssetDatabase.GUIDToAssetPath(paths[0]);
        int index = dirPath.IndexOf("/");
        dirPath = dirPath.Substring(index, dirPath.Length - index);
        string []withoutExtensions = new string[]{".png",".jpg"};
        string[] files = Directory.GetFiles(Application.dataPath+dirPath, "*.*", SearchOption.AllDirectories)
            .Where(s => withoutExtensions.Contains(System.IO.Path.GetExtension(s).ToLower())).ToArray();
        int startIndex = 0;
        if (files!=null&&files.Length > 0)
        {
            EditorApplication.update = delegate()
            {
                string file = files[startIndex];
                bool isCancel = EditorUtility.DisplayCancelableProgressBar("匹配资源中", file, (float)startIndex / (float)files.Length);
                DoSetTexture(file);
                startIndex++;
                if (isCancel || startIndex >= files.Length)
                {
                    EditorUtility.ClearProgressBar();
                    EditorApplication.update = null;
                    startIndex = 0;
                    Debug.Log("匹配结束");
                    AssetDatabase.Refresh();
                }
            };
        }
    }
    

    static void DoSetTexture(string file)
    {

        bool isNeedSet = false;
        int index = file.IndexOf("Assets");
        file = file.Substring(index, file.Length - index);
        TextureImporter importer = TextureImporter.GetAtPath(file) as TextureImporter;
        importer.isReadable = false;
        
        TextureImporterPlatformSettings androidSetting = importer.GetPlatformTextureSettings("Android");
        TextureImporterPlatformSettings iPhoneSetting = importer.GetPlatformTextureSettings("iPhone");
        if (androidSetting.format != TextureImporterFormat.ASTC_4x4 &&
            androidSetting.format != TextureImporterFormat.ASTC_6x6 &&
            androidSetting.format != TextureImporterFormat.ASTC_8x8)
        {
            if (importer.DoesSourceTextureHaveAlpha())
            {
                androidSetting.format = TextureImporterFormat.ASTC_6x6;
            }
            else
            {
                androidSetting.format = TextureImporterFormat.ASTC_8x8;
            }
            
            androidSetting.overridden = true;
            androidSetting.compressionQuality = 50;
            androidSetting.textureCompression = TextureImporterCompression.Compressed;
            androidSetting.crunchedCompression = true;
            importer.SetPlatformTextureSettings(androidSetting);
            isNeedSet = true;
        }
        
        if (iPhoneSetting.format != TextureImporterFormat.ASTC_4x4 &&
            iPhoneSetting.format != TextureImporterFormat.ASTC_6x6 &&
            iPhoneSetting.format != TextureImporterFormat.ASTC_8x8)
        {
            if (importer.DoesSourceTextureHaveAlpha())
            {
                iPhoneSetting.format = TextureImporterFormat.ASTC_6x6;
            }
            else
            {
                iPhoneSetting.format = TextureImporterFormat.ASTC_8x8;
            }

            iPhoneSetting.overridden = true;
            iPhoneSetting.compressionQuality = 50;
            iPhoneSetting.textureCompression = TextureImporterCompression.Compressed;
            iPhoneSetting.crunchedCompression = true;
            importer.SetPlatformTextureSettings(iPhoneSetting);
            isNeedSet = true;
        }
        if (isNeedSet)
        {
            importer.SaveAndReimport();
        }
    }

    
    
    [MenuItem("Assets/UI/Delete_NoRefTexture", false)]
    static void DeleteNoRefTextur()
    {
        string[] paths = Selection.assetGUIDs;
        string dirPath = AssetDatabase.GUIDToAssetPath(paths[0]);
        int index = dirPath.IndexOf("/");
        dirPath = dirPath.Substring(index, dirPath.Length - index);
        string []withoutExtensions = new string[]{".png",".jpg",".jpeg"};
        string[] files = Directory.GetFiles(Application.dataPath+dirPath, "*.*", SearchOption.AllDirectories)
            .Where(s => withoutExtensions.Contains(System.IO.Path.GetExtension(s).ToLower())).ToArray();
        int startIndex = 0;
        if (files!=null&&files.Length > 0)
        {
            EditorApplication.update = delegate()
            {
                string file = files[startIndex];
                bool isCancel = EditorUtility.DisplayCancelableProgressBar("匹配资源中", file, (float)startIndex / (float)files.Length);
                int index = file.IndexOf("Assets");
                string path = file.Substring(index, file.Length - index);
                if (!GetRefCount(path))
                {
                    AssetDatabase.DeleteAsset(path);
                    Debug.Log("删除 --------->"+path);
                }
                startIndex++;
                if (isCancel || startIndex >= files.Length)
                {
                    EditorUtility.ClearProgressBar();
                    EditorApplication.update = null;
                    startIndex = 0;
                    Debug.Log("匹配结束");
                    AssetDatabase.Refresh();
                }
            };
        }
    }


    static bool GetRefCount(string path)
    {
        string guid = AssetDatabase.AssetPathToGUID(path);
        string[] withoutExtensions = new string[] {".prefab", ".unity", ".mat", ".asset"};
        string[] files = Directory
            .GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories)
            .Where(s => withoutExtensions.Contains(System.IO.Path.GetExtension(s).ToLower())).ToArray();
        for (int i = 0; i < files.Length; i++)
        {
            string file = files[i];
            if (Regex.IsMatch(File.ReadAllText(file), guid))
            {
                return true;
            }
        }
        return false;
    }

}
