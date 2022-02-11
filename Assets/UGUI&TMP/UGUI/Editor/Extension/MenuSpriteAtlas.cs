using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            SavePath = "Assets/Resources/";
             var suffix = ".spriteatlas";
#if !UNITY_2019_4
            if (EditorSettings.spritePackerMode == SpritePackerMode.SpriteAtlasV2) suffix = ".spriteatlasv2";
#endif
            var select = Selection.activeObject;
            var dirPath = AssetDatabase.GetAssetPath(select);
            dirPath = dirPath.Replace("\\", "/");
            if (string.IsNullOrEmpty(dirPath) || Path.HasExtension(dirPath))
            {
                Debug.LogError("不是文件夹,无法打包");
                return;
            }

            var isHave = false;
            Object[] haves = null;
#if !UNITY_2019_4
            var spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlasAsset>(SavePath + select.name + suffix);
            if (spriteAtlas == null)
            {
                spriteAtlas = new SpriteAtlasAsset();
            }
            else
            {
                isHave = true;
            }
#else
            var spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(SavePath + select.name + suffix);
            if (spriteAtlas == null)
            {
                spriteAtlas = new SpriteAtlas();
            }
            else
            {
                isHave = true;
            }
            haves = spriteAtlas.GetPackables();
#endif


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

            var platformSetting = new TextureImporterPlatformSettings()
            {
                maxTextureSize = 2048,
                format = TextureImporterFormat.Automatic,
                crunchedCompression = true,
                textureCompression = TextureImporterCompression.Compressed,
                compressionQuality = 50,
            };
            spriteAtlas.SetPlatformSettings(platformSetting);

            var dir = new DirectoryInfo(dirPath);
            var sprites = new List<Object>();
            foreach (var file in dir.GetFiles())
            {
                if (file.Name.Contains(".meta")) continue;
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{dirPath}/{file.Name}");
                if (sprite.texture.texelSize.x >= 1024)
                {
                    Debug.LogWarning(sprite.name + " 的宽度大于 1024,请确定此图是否需要打入图集中,或让美术重新制作");
                }

                if (sprite.texture.texelSize.y >= 1024)
                {
                    Debug.LogWarning(sprite.name + " 的高度大于 1024,请确定此图是否需要打入图集中,或让美术重新制作");
                }

                if (sprite.texture.texelSize.x % 2 != 0)
                {
                    Debug.LogWarning(sprite.name + " 的宽度不是 2 的倍数,请让美术重新制作");
                }

                if (sprite.texture.texelSize.y % 2 != 0)
                {
                    Debug.LogWarning(sprite.name + " 的高度不是 2 的倍数,请让美术重新制作");
                }

                if (haves != null && haves.Contains(sprite)) continue;
                sprites.Add(sprite);
            }
            spriteAtlas.Add(sprites.ToArray());

            var atlasPath = SavePath + select.name + suffix;
            var abName = AssetImporter.GetAtPath(atlasPath)?.assetBundleName;
            AssetDatabase.CreateAsset(spriteAtlas, atlasPath);
            AssetImporter.GetAtPath(atlasPath).assetBundleName = abName;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            SpriteAtlasUtility.PackAllAtlases(EditorUserBuildSettings.activeBuildTarget, false);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(SavePath + select.name + suffix);
            EditorUtility.FocusProjectWindow();
        }
    }
    

    
}