using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Sprites;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace UnityEditor.UI
{
    public static class MenuSpriteAtlas
    {
        private static string SavePath = "";

        [MenuItem("Assets/UI/创建或者更新 SpriteAtlas", false, 1)]
        public static void CreateSpriteAtlas(MenuCommand menuCommand)
        {
            SavePath = "Assets/RAW/f1_ui/atlas/";
            var suffix = ".spriteatlas";
// #if !UNITY_2019_4
//             if (EditorSettings.spritePackerMode == SpritePackerMode.SpriteAtlasV2) suffix = ".spriteatlasv2";
// #endif
            string[] paths = Selection.assetGUIDs;
            string dirPath = AssetDatabase.GUIDToAssetPath(paths[0]);
            dirPath = dirPath.Replace("\\", "/");
            if (string.IsNullOrEmpty(dirPath) || Path.HasExtension(dirPath))
            {
                Debug.LogError("不是文件夹,无法打包");
                return;
            }

            string findW = "art_image";
            string atalsName = "";
            int index = dirPath.IndexOf(findW);
            if (index > 0)
            {
                string partPath =
                    dirPath.Substring(index + findW.Length + 1, dirPath.Length - index - findW.Length - 1) + "_atlas";

                if (!Directory.Exists(SavePath + partPath))
                {
                    Directory.CreateDirectory(SavePath + partPath);
                }

                string name = Path.GetFileName(dirPath) + "_atlas";
                atalsName = partPath + "/" + name;
            }

            //var isHave = false;
            Object[] haves = null;
// #if !UNITY_2019_4
//             var spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlasAsset>(SavePath + atalsName + suffix);
//             if (spriteAtlas == null)
//             {
//                 spriteAtlas = new SpriteAtlasAsset();
//             }
//             else
//             {
//                 isHave = true;
//             }
// #else
            var spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(SavePath + atalsName + suffix);
            if (spriteAtlas == null)
            {
                spriteAtlas = new SpriteAtlas();
            }

            // else
            // {
            //     isHave = true;
            // }
            haves = spriteAtlas.GetPackables();
// #endif
            var atlasPath = SavePath + atalsName + suffix;
            RemoveAllSprite(spriteAtlas, haves, atlasPath);

            spriteAtlas.SetIncludeInBuild(true);
            var packSetting = new SpriteAtlasPackingSettings()
            {
                blockOffset = 1,
                enableRotation = false,
                enableTightPacking = false,
                padding = 2,
            };
            spriteAtlas.SetPackingSettings(packSetting);
            var textureSetting = new SpriteAtlasTextureSettings()
            {
                readable = false,
                generateMipMaps = false,
                sRGB = true,
                filterMode = FilterMode.Bilinear,
            };
            spriteAtlas.SetTextureSettings(textureSetting);
            //IOS设置
            var iPhoneSetting = new TextureImporterPlatformSettings()
            {
                name = "iPhone",
                maxTextureSize = 1024,
                format = TextureImporterFormat.ASTC_4x4,
                overridden = true,
                crunchedCompression = true,
                textureCompression = TextureImporterCompression.Compressed,
                compressionQuality = 50,
            };
            //安卓设置
            var androidSetting = new TextureImporterPlatformSettings()
            {
                name = "Android",
                maxTextureSize = 1024,
                format = TextureImporterFormat.ASTC_4x4,
                overridden = true,
                crunchedCompression = true,
                textureCompression = TextureImporterCompression.Compressed,
                compressionQuality = 50,
            };

            var dir = new DirectoryInfo(dirPath);
            var sprites = new List<Object>();
            foreach (var file in dir.GetFiles())
            {
                if (file.Name.Contains(".meta")) continue;
                Sprite[] allS = AssetDatabase.LoadAllAssetsAtPath($"{dirPath}/{file.Name}").OfType<Sprite>().ToArray();
                foreach (var sprite in allS)
                {
                    if (sprite.textureRect.width >= 1024)
                    {
                        // Debug.LogError(sprite.name + " 的宽度大于 1024,请确定此图是否需要打入图集中,或让美术重新制作, 宽度:" + sprite.textureRect.width);
                        androidSetting.maxTextureSize = 2048;
                        iPhoneSetting.maxTextureSize = 2048;
                    }

                    if (sprite.textureRect.height >= 1024)
                    {
                        // Debug.LogError(sprite.name + " 的高度大于 1024,请确定此图是否需要打入图集中,或让美术重新制作, 高度:" + sprite.textureRect.height);
                        androidSetting.maxTextureSize = 2048;
                        iPhoneSetting.maxTextureSize = 2048;
                    }

                    if (sprite.textureRect.width % 2 != 0)
                    {
                        // Debug.LogError(sprite.name + " 的宽度不是 2 的倍数,请让美术重新制作");
                    }

                    if (sprite.textureRect.height % 2 != 0)
                    {
                        // Debug.LogError(sprite.name + " 的高度不是 2 的倍数,请让美术重新制作");
                    }

                    sprites.Add(sprite);
                }
            }

            spriteAtlas.SetPlatformSettings(iPhoneSetting);
            spriteAtlas.SetPlatformSettings(androidSetting);
            spriteAtlas.Add(sprites.ToArray());

            var abName = AssetImporter.GetAtPath(atlasPath)?.assetBundleName;
            if (!File.Exists(atlasPath))
            {
                AssetDatabase.CreateAsset(spriteAtlas, atlasPath);
            }

            AssetImporter.GetAtPath(atlasPath).assetBundleName = abName;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            SpriteAtlasUtility.PackAllAtlases(EditorUserBuildSettings.activeBuildTarget, false);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(SavePath + atalsName + suffix);
            EditorUtility.FocusProjectWindow();
        }


        //删除所有的图片
        private static void RemoveAllSprite(SpriteAtlas spriteAtlas, Object[] sprites, string atlasPath)
        {
            if (File.Exists(atlasPath))
            {
                spriteAtlas.Remove(sprites);
                AssetDatabase.SaveAssets(); //保存文件
            }

            SpriteAtlasUtility.PackAllAtlases(EditorUserBuildSettings.activeBuildTarget, false); //让文件生成一次大图,才能在后续使用
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/UI/SpriteAtlas-No-Include In Build", false, 1)]
        public static void SpriteAtlasSet()
        {
            string[] paths = Selection.assetGUIDs;
            string dirPath = AssetDatabase.GUIDToAssetPath(paths[0]);
            int index = dirPath.IndexOf("/");
            dirPath = dirPath.Substring(index, dirPath.Length - index);
            string[] withoutExtensions = new string[] {".spriteatlas"};
            string[] files = Directory.GetFiles(Application.dataPath + dirPath, "*.*", SearchOption.AllDirectories)
                .Where(s => withoutExtensions.Contains(System.IO.Path.GetExtension(s).ToLower())).ToArray();
            int startIndex = 0;
            if (files != null && files.Length > 0)
            {
                EditorApplication.update = delegate()
                {
                    string file = files[startIndex];
                    bool isCancel = EditorUtility.DisplayCancelableProgressBar("匹配资源中", file,
                        (float) startIndex / (float) files.Length);
                    SetSpriteAtlasNoInCludeInBuild(file, false);
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


        [MenuItem("Assets/UI/SpriteAtlas-Include In Build", false, 1)]
        public static void SpriteAtlasSetInCludeInBuild()
        {
            string[] paths = Selection.assetGUIDs;
            string dirPath = AssetDatabase.GUIDToAssetPath(paths[0]);
            int index = dirPath.IndexOf("/");
            dirPath = dirPath.Substring(index, dirPath.Length - index);
            string[] withoutExtensions = new string[] {".spriteatlas"};
            string[] files = Directory.GetFiles(Application.dataPath + dirPath, "*.*", SearchOption.AllDirectories)
                .Where(s => withoutExtensions.Contains(System.IO.Path.GetExtension(s).ToLower())).ToArray();
            int startIndex = 0;
            if (files != null && files.Length > 0)
            {
                EditorApplication.update = delegate()
                {
                    string file = files[startIndex];
                    bool isCancel = EditorUtility.DisplayCancelableProgressBar("匹配资源中", file,
                        (float) startIndex / (float) files.Length);
                    SetSpriteAtlasNoInCludeInBuild(file, true);
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

        static void SetSpriteAtlasNoInCludeInBuild(string file, bool IsIncludeInBuild)
        {
            int index = file.IndexOf("Assets");
            file = file.Substring(index, file.Length - index);
            SpriteAtlas spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(file);
            spriteAtlas.SetIncludeInBuild(IsIncludeInBuild);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}