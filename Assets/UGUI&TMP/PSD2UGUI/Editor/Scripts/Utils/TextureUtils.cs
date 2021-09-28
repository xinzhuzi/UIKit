#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace PSD2UGUI
{
    public static class TextureUtils
    {
        public enum OutputObjectType { SPRITE_RENDERER, UI_IMAGE }

        private class PixelsLoader
        {
            private readonly int _start;
            private readonly int _end;

            private readonly Vector2Int _canvasSize;
            private readonly Vector2Int _previewSize;
            private readonly Vector2Int _thumbnailSize;

            private readonly Rect _layerRect;
            private readonly Rect _clippedRect;

            private Color32[] _thumbnailPixels;
            private Color32[] _previewPixels;
            private Color32[] _clippedPixels;

            private readonly byte[] _red;
            private readonly byte[] _green;
            private readonly byte[] _blue;
            private readonly byte[] _alpha;

            public event Action OnePercentLoaded;

            public static bool Cancel = false;

            public PixelsLoader(
                int start,
                int end,
                Vector2Int canvasSize,
                Vector2Int previewSize,
                Vector2Int thumbnailSize,
                Rect layerRect,
                Rect clippedRect,
                Color32[] thumbnailPixels,
                Color32[] previewPixels,
                Color32[] clippedPixels,
                byte[] red,
                byte[] green,
                byte[] blue,
                byte[] alpha)
            {
                _start = start;
                _end = end;
                _canvasSize = canvasSize;
                _previewSize = previewSize;
                _thumbnailSize = thumbnailSize;
                _layerRect = layerRect;
                _clippedRect = clippedRect;
                _thumbnailPixels = thumbnailPixels;
                _previewPixels = previewPixels;
                _clippedPixels = clippedPixels;
                _red = red;
                _green = green;
                _blue = blue;
                _alpha = alpha;
            }

           
            public void Load()
            {
                var canvasSize = _canvasSize.x * _canvasSize.y;
                var onePercent = (float)canvasSize / 100f;
                var percentCountDown = onePercent;

                for (int i = _start; i < _end; ++i)
                {
                    if (Cancel) return;
                    var x = i % _canvasSize.x;
                    var y = i / _canvasSize.x;

                    var previewPos = new Vector2Int((x * _previewSize.x) / _canvasSize.x, (y * _previewSize.y) / _canvasSize.y);
                    var previewIdx = (_previewSize.y - 1 - previewPos.y) * _previewSize.x + previewPos.x;

                    var thumbnailPos = new Vector2Int((x * _thumbnailSize.x) / _canvasSize.x, (y * _thumbnailSize.y) / _canvasSize.y);
                    var thumbnailIdx = (_thumbnailSize.y - 1 - thumbnailPos.y) * _thumbnailSize.x + thumbnailPos.x;

                    if (x < _layerRect.xMin || x >= _layerRect.xMax ||
                        y < _layerRect.yMin || y >= _layerRect.yMax)
                    {
                        _thumbnailPixels[thumbnailIdx] = _previewPixels[previewIdx] = new Color32(255, 255, 255, 0);
                    }
                    else
                    {
                        var sourcePos = new Vector2Int(x - (int)_layerRect.x, y - (int)_layerRect.y);
                        var sourceIdx = sourcePos.y * (int)_layerRect.width + sourcePos.x;

                        byte r = _red[sourceIdx];
                        byte g = _green[sourceIdx];
                        byte b = _blue[sourceIdx];
                        byte a = (byte)(_alpha == null ? 255 : _alpha[sourceIdx]);
                        _thumbnailPixels[thumbnailIdx] = _previewPixels[previewIdx] = new Color32(r, g, b, a);

                        if (x >= _clippedRect.x && x < _clippedRect.xMax && y >= _clippedRect.y && y < _clippedRect.yMax)
                        {
                            var clippedX = x - (int)_clippedRect.x;
                            var clippedY = y - (int)_clippedRect.y;
                            var clippedIdx = ((int)_clippedRect.height - 1 - clippedY) * (int)_clippedRect.width + clippedX;
                            _clippedPixels[clippedIdx] = _previewPixels[previewIdx];
                        }
                    }
                    if (thumbnailPos.x < 1 || thumbnailPos.x >= (_thumbnailSize.x - 1) ||
                        thumbnailPos.y < 1 || thumbnailPos.y >= (_thumbnailSize.y - 1))
                    {
                        _thumbnailPixels[thumbnailIdx] = Color.black;
                    }

                    if (--percentCountDown <= 0)
                    {
                        OnePercentLoaded();
                        percentCountDown = onePercent;
                    }
                }
            }
        }

        public class LayerTextureLoader
        {
            public readonly Layer TargetLayer = null;
            public readonly int CanvasW = 0;
            public readonly int CanvasH = 0;
            public readonly int HierarchyCanvasW = 0;
            public readonly int HierarchyCanvasH = 0;
            public readonly int PreviewCanvasW = 0;
            public readonly int PreviewCanvasH = 0;

            private float _progress = 0f;
            private bool _cancel = false;

            public LayerTextureLoader(Layer targetLayer, int canvasW, int canvasH, int hierarchyCanvasW, int hierarchyCanvasH, int previewCanvasW, int previewCanvasH)
            {
                TargetLayer = targetLayer;
                CanvasW = canvasW;
                CanvasH = canvasH;
                HierarchyCanvasW = hierarchyCanvasW;
                HierarchyCanvasH = hierarchyCanvasH;
                PreviewCanvasW = previewCanvasW;
                PreviewCanvasH = previewCanvasH;
            }

            public event Action<float> ProgressChanged;

            public event Action<Layer, Color32[], Color32[], Color32[], Rect> OnLoadingComplete;

            public event Action OnError;

            public void Cancel()
            {
                PixelsLoader.Cancel = _cancel = true;
            }

            public void LoadLayerPixels()
            {
                PixelsLoader.Cancel = false;
                var layerWidth = (int)TargetLayer.Rect.width;
                var layerHeight = (int)TargetLayer.Rect.height;

                if (layerWidth == 0 || layerHeight == 0)
                {
                    if (_cancel) return;
                    OnLoadingComplete(TargetLayer, null, null, null, Rect.zero);
                    return;
                }

                Rect clippedRect = new Rect(
                    Mathf.Max(0f, TargetLayer.Rect.xMin),
                    Mathf.Max(0f, TargetLayer.Rect.yMin),
                    (CanvasW + layerWidth) - (Mathf.Max(TargetLayer.Rect.xMax, CanvasW) - Mathf.Min(TargetLayer.Rect.xMin, 0f)),
                    (CanvasH + layerHeight) - (Mathf.Max(TargetLayer.Rect.yMax, CanvasH) - Mathf.Min(TargetLayer.Rect.yMin, 0f)));

                if (clippedRect.width < 1f || clippedRect.height < 1f)
                {
                    if (_cancel) return;
                    OnLoadingComplete(TargetLayer, null, null, null, Rect.zero);
                    return;
                }

                Channel red = (from l in TargetLayer.Channels where l.ID == 0 select l).First();
                Channel green = (from l in TargetLayer.Channels where l.ID == 1 select l).First();
                Channel blue = (from l in TargetLayer.Channels where l.ID == 2 select l).First();
                Channel alpha = TargetLayer.AlphaChannel;

                if (red == null || green == null || blue == null
                || red.ImageData == null || green.ImageData == null || blue.ImageData == null || (alpha != null && alpha.ImageData == null))
                {
                    OnError();
                    return;
                }
                var layerX = Mathf.FloorToInt(TargetLayer.Rect.x);
                var layerY = Mathf.FloorToInt(TargetLayer.Rect.y);

                var previewPixels = new Color32[PreviewCanvasW * PreviewCanvasH];
                var thumbnailPixels = new Color32[HierarchyCanvasW * HierarchyCanvasH];
                var clippedPixels = new Color32[(int)clippedRect.width * (int)clippedRect.height];

                var canvasSize = CanvasW * CanvasH;
                _progress = 0f;

                var delta = 1000000;
                var taskCount = canvasSize / delta + Mathf.Min(canvasSize % delta, 1);
                var taskArray = new Task[taskCount];
                List<PixelsLoader> pixelsLoaders = new List<PixelsLoader>();
                for (int i = 0; i < taskCount; ++i)
                {
                    if (_cancel) break;
                    var currentIdx = i * delta;
                    var nextIdx = Mathf.Min(currentIdx + delta, canvasSize);
                    var pixelLoader = new PixelsLoader(
                        currentIdx,
                        nextIdx,
                        new Vector2Int(CanvasW, CanvasH),
                        new Vector2Int(PreviewCanvasW, PreviewCanvasH),
                        new Vector2Int(HierarchyCanvasW, HierarchyCanvasH),
                        TargetLayer.Rect,
                        clippedRect,
                        thumbnailPixels,
                        previewPixels,
                        clippedPixels,
                        red.ImageData,
                        green.ImageData,
                        blue.ImageData,
                        alpha == null ? null : alpha.ImageData);
                    pixelLoader.OnePercentLoaded += OnOnePercentLoaded;
                    pixelsLoaders.Add(pixelLoader);
                    taskArray[i] = Task.Run(pixelLoader.Load);
                }
                Task.WaitAll(taskArray);
                foreach(var loader in pixelsLoaders)
                {
                    loader.OnePercentLoaded -= OnOnePercentLoaded;
                }
                pixelsLoaders.Clear();
                if (_cancel) return;
                OnLoadingComplete(TargetLayer, previewPixels, thumbnailPixels, clippedPixels, clippedRect);
            }
            private void OnOnePercentLoaded()
            {
                if (_cancel) return;
                _progress += 0.01f;
                _progress = Mathf.Max(_progress, 1f);
                ProgressChanged(_progress);
            }
        }

        public static Sprite SavePngAsset(Texture2D texture, string pngPath, float pixelsPerUnit)
        {
            byte[] buffer = texture.EncodeToPNG();
            File.WriteAllBytes(pngPath, buffer);

            AssetDatabase.Refresh();
            AssetDatabase.LoadAssetAtPath<Texture2D>(pngPath);
            TextureImporter textureImporter = AssetImporter.GetAtPath(pngPath) as TextureImporter;
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spriteImportMode = SpriteImportMode.Single;
            textureImporter.spritePivot = new Vector2(0.5f, 0.5f);
            textureImporter.spritePixelsPerUnit = pixelsPerUnit;
            AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceUpdate);
            return AssetDatabase.LoadAssetAtPath<Sprite>(pngPath);
        }

        public static Texture2D CreateAtlas(Tuple<string, Texture2D>[] namesAndTextures, int atlasSize, string pngPath, float pixelsPerUnit)
        {
            if (namesAndTextures.Length == 0) return null;
            Rect[] rects;
            Texture2D atlas = new Texture2D(atlasSize, atlasSize);
            Texture2D[] textureArray = new Texture2D[namesAndTextures.Length];
            for (int i = 0; i < namesAndTextures.Length; ++i) textureArray[i] = namesAndTextures[i].Item2;
            rects = atlas.PackTextures(textureArray, 2, atlasSize);
            List<SpriteMetaData> Sprites = new List<SpriteMetaData>();

            for (int i = 0; i < rects.Length; i++)
            {
                SpriteMetaData smd = new SpriteMetaData();
                smd.name = namesAndTextures[i].Item1;
                smd.rect = new Rect(rects[i].xMin * atlas.width, rects[i].yMin * atlas.height, rects[i].width * atlas.width, rects[i].height * atlas.height);
                smd.pivot = new Vector2(0.5f, 0.5f);
                smd.alignment = (int)SpriteAlignment.Center;
                Sprites.Add(smd);
            }

            byte[] buf = atlas.EncodeToPNG();

            File.WriteAllBytes(pngPath, buf);
            AssetDatabase.Refresh();

            TextureImporter textureImporter = AssetImporter.GetAtPath(pngPath) as TextureImporter;

            textureImporter.maxTextureSize = atlasSize;
            textureImporter.spritesheet = Sprites.ToArray();
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spriteImportMode = SpriteImportMode.Multiple;
            textureImporter.spritePivot = new Vector2(0.5f, 0.5f);
            textureImporter.spritePixelsPerUnit = pixelsPerUnit;
            AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceUpdate);
            return atlas;
        }

        public static void CreateAtlas(OutputObjectType outputType, Tuple<string, GameObject, Texture2D, Rect>[] renderersAndTextures, int atlasSize, string pngPath, float pixelsPerUnit)
        {
            if (renderersAndTextures.Length == 0) return;
            Rect[] rects;
            Texture2D atlas = new Texture2D(atlasSize, atlasSize);
            Texture2D[] textureArray = new Texture2D[renderersAndTextures.Length];
            for (int i = 0; i < renderersAndTextures.Length; ++i) textureArray[i] = renderersAndTextures[i].Item3;
            rects = atlas.PackTextures(textureArray, 2, atlasSize);
            List<SpriteMetaData> Sprites = new List<SpriteMetaData>();

            for (int i = 0; i < rects.Length; i++)
            {
                SpriteMetaData smd = new SpriteMetaData();
                smd.name = renderersAndTextures[i].Item1;
                smd.rect = new Rect(rects[i].xMin * atlas.width, rects[i].yMin * atlas.height, rects[i].width * atlas.width, rects[i].height * atlas.height);
                smd.pivot = new Vector2(0.5f, 0.5f);
                smd.alignment = (int)SpriteAlignment.Center;
                Sprites.Add(smd);
            }

            byte[] buf = atlas.EncodeToPNG();
            File.WriteAllBytes(pngPath, buf);
            AssetDatabase.Refresh();

            atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(pngPath);
            TextureImporter textureImporter = AssetImporter.GetAtPath(pngPath) as TextureImporter;

            textureImporter.maxTextureSize = atlasSize;
            textureImporter.spritesheet = Sprites.ToArray();
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spriteImportMode = SpriteImportMode.Multiple;
            textureImporter.spritePivot = new Vector2(0.5f, 0.5f);
            textureImporter.spritePixelsPerUnit = pixelsPerUnit;
            AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceUpdate);

            for (int j = 0; j < textureImporter.spritesheet.Length; j++)
            {
                var sprites = AssetDatabase.LoadAllAssetsAtPath(pngPath);
                var spr = (Sprite)sprites.Single(s => s.name == Sprites[j].name);
                var rect = renderersAndTextures[j].Item4;
                var scale = new Vector2(rect.width / spr.bounds.size.x, rect.height / spr.bounds.size.y) / pixelsPerUnit;
                if (outputType == OutputObjectType.SPRITE_RENDERER)
                {
                    renderersAndTextures[j].Item2.GetComponent<SpriteRenderer>().sprite = spr;
                    renderersAndTextures[j].Item2.transform.localScale = new Vector3(scale.x, scale.y, 1f);
                }
                else
                {
                    renderersAndTextures[j].Item2.GetComponent<Image>().sprite = spr;
                }
            }
        }

        public static Texture2D SetTextureBorder(Texture2D source, int targetWidth, int targetHeight)
        {
            var textureWidth = source.width;
            var textureHeight = source.height;
            var texturePixels = source.GetPixels();

            var resultTexture = new Texture2D(textureWidth, textureHeight);
            resultTexture.SetPixels(source.GetPixels());

            var longestSide = textureHeight;
            var thumbnailLongestSide = targetHeight;
            if (textureWidth > textureHeight)
            {
                thumbnailLongestSide = targetWidth;
                longestSide = textureWidth;
            }
            var borderSize = (int)((float)longestSide / (float)thumbnailLongestSide);

            for (int x = 0; x < textureWidth; ++x)
            {
                for (int y = 0; y < borderSize; ++y)
                {
                    resultTexture.SetPixel(x, y, Color.black);
                    resultTexture.SetPixel(x, textureHeight - 1 - y, Color.black);
                }
            }

            for (int y = borderSize; y < textureHeight - borderSize; ++y)
            {
                for (int x = 0; x < borderSize; ++x)
                {
                    resultTexture.SetPixel(x, y, Color.black);
                    resultTexture.SetPixel(textureWidth - 1 - x, y, Color.black);
                }
            }

            resultTexture.Apply();
            return resultTexture;
        }

        public static Texture2D GetScaledTexture(Texture2D source, int width, int height)
        {
            source.filterMode = FilterMode.Point;
            RenderTexture renderTexture = RenderTexture.GetTemporary(width, height);
            renderTexture.filterMode = FilterMode.Point;
            RenderTexture.active = renderTexture;
            Graphics.Blit(source, renderTexture);
            Texture2D result = new Texture2D(width, height);
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply();
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(renderTexture);
            return result;
        }
    }
}
#endif