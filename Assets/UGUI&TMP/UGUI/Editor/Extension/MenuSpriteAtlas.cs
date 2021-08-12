using System.IO;
using System.Linq;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace UnityEditor.UI
{
    public static class MenuSpriteAtlas
    {
        private static string SavePath = "Assets/Editor Default Resources/SpriteAtlas/";
        
        [MenuItem("Assets/UI/创建或者更新 SpriteAtlas", false, 1)]
        public static void CreateSpriteAtlas(MenuCommand menuCommand)
        {
            var select = Selection.activeObject;
            var dirPath = AssetDatabase.GetAssetPath(select);
            dirPath = dirPath.Replace("\\","/");
            if (string.IsNullOrEmpty(dirPath) || Path.HasExtension(dirPath))
            {
                Debug.LogError("不是文件夹,无法打包");
                return;
            }

            var spriteAtlas
#if !UNITY_2019_4
            = new SpriteAtlasAsset();
#else
             = new SpriteAtlas();
#endif
            SpriteAtlasPackingSettings packSetting = new SpriteAtlasPackingSettings()
            {
                blockOffset = 1,
                enableRotation = false,
                enableTightPacking = false,
                padding = 2,
            };
            spriteAtlas.SetPackingSettings(packSetting);

            SpriteAtlasTextureSettings textureSetting = new SpriteAtlasTextureSettings()
            {
                readable = false,
                generateMipMaps = false,
                sRGB = true,
                filterMode = FilterMode.Bilinear,
            };
            spriteAtlas.SetTextureSettings(textureSetting);

            TextureImporterPlatformSettings platformSetting = new TextureImporterPlatformSettings()
            {
                maxTextureSize = 2048,
                format = TextureImporterFormat.Automatic,
                crunchedCompression = true,
                textureCompression = TextureImporterCompression.Compressed,
                compressionQuality = 50,
            };
            spriteAtlas.SetPlatformSettings(platformSetting);
            
            DirectoryInfo dir = new DirectoryInfo(dirPath);
            foreach (FileInfo file in dir.GetFiles())
            {
                if (file.Name.Contains(".meta"))continue;
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{dirPath}/{file.Name}");
                if (sprite.texture.texelSize.x >= 1024)
                {
                    Debug.LogWarning(sprite.name +" 的宽度大于 1024,请确定此图是否需要打入图集中,或让美术重新制作");
                }
                if (sprite.texture.texelSize.y >= 1024)
                {
                    Debug.LogWarning(sprite.name +" 的高度大于 1024,请确定此图是否需要打入图集中,或让美术重新制作");
                }
                if (sprite.texture.texelSize.x % 2 != 0)
                {
                    Debug.LogWarning(sprite.name +" 的宽度不是 2 的倍数,请让美术重新制作");
                }
                if (sprite.texture.texelSize.y % 2 != 0)
                {
                    Debug.LogWarning(sprite.name +" 的高度不是 2 的倍数,请让美术重新制作");
                }
                spriteAtlas.Add(new[] {sprite});
            } 
            
            string suffix = ".spriteatlas";
#if !UNITY_2019_4
            if (EditorSettings.spritePackerMode == SpritePackerMode.SpriteAtlasV2) suffix = ".spriteatlasv2";
#endif
            if (Directory.Exists(SavePath))
            {
                SavePath += dirPath.Split('/').Last();
            }
            else
            {
                SavePath = dirPath;
            }
            AssetDatabase.CreateAsset(spriteAtlas, SavePath + suffix);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath(SavePath + suffix, typeof(Object));
            EditorUtility.FocusProjectWindow();
        }
    }
    

    
}