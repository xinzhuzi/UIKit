#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UIKit;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;


namespace PSD2UGUI
{
    public class PrefabCreator
    {
        private TextureUtils.OutputObjectType _outputType;
        private bool _importIntoSelectedObject;
        private string _rootName;
        private float _pixelsPerUnit = 100.0f;
        private PsdFile _psdFile;
        private int _lastSortingOrder;
        private bool _createAtlas;
        private int _atlasMaxSize = 2048;
        private string _outputFolder;
        private bool _importOnlyVisibleLayers;
        private BlendingShaderType _blendingShader;
        private Dictionary<int, Tuple<Texture2D, Rect>> _layerTextures;
        private float _scale = 1;
        private List<SpriteAtlas> _spriteAtlasArray;
        private List<TMP_FontAsset> _fontArray;
        private GameObject _lastObject = null;

        public PrefabCreator(
            TextureUtils.OutputObjectType outputType,
            bool importIntoSelectedObject,
            string rootName,
            PsdFile psdFile,
            int lastSortingOrder,
            bool createAtlas,
            string outputFolder,
            bool importOnlyVisibleLayers,
            BlendingShaderType blendingShader,
            SpriteAtlas[] spriteAtlasArray,
            TMP_FontAsset[] fontArray,
            Dictionary<int, Tuple<Texture2D, Rect>> layerTextures)
        {
            _outputType = outputType;
            _importIntoSelectedObject = importIntoSelectedObject;
            _rootName = rootName;
            _psdFile = psdFile;
            _lastSortingOrder = lastSortingOrder;
            _createAtlas = createAtlas;
            _outputFolder = outputFolder;
            _importOnlyVisibleLayers = importOnlyVisibleLayers;
            _blendingShader = blendingShader;
            _layerTextures = layerTextures;
            _spriteAtlasArray = new List<SpriteAtlas>();
            _fontArray = new List<TMP_FontAsset>();
            for (var i = 0; i < spriteAtlasArray.Length; i++)
            {
                if (spriteAtlasArray[i] == null) continue;
                _spriteAtlasArray.Add(spriteAtlasArray[i]);
            }

            for (var i = 0; i < fontArray.Length; i++)
            {
                if (fontArray[i] == null) continue;
                _fontArray.Add(fontArray[i]);
            }
        }

        // public static Texture2D[] CreatePngFiles(
        //     string outputFolder,
        //     Dictionary<int, Tuple<Texture2D, Rect>> layerTextures, 
        //     PsdFile psdFile, 
        //     float scale,
        //     float pixelsPerUnit,
        //     bool createAtlas,
        //     int atlasMaxSize,
        //     bool importOnlyVisibleLayers
        //     )
        // {
        //     var textures = new List<Texture2D>(layerTextures.Select(obj => obj.Value.Item1));
        //     var namesAndTexturesList = new List<Tuple<string, Texture2D>>();
        //     foreach (var item in layerTextures.ToList())
        //     {
        //         if (item.Value.Item1 == null) continue;
        //         var layer = psdFile.GetLayer(item.Key);
        //         if (importOnlyVisibleLayers && !(layer.Visible && layer.VisibleInHierarchy)) continue;
        //         var layerName = layer.Name;
        //         var scaledRect = new Rect(item.Value.Item2.x * scale, item.Value.Item2.y * scale, item.Value.Item2.width * scale, item.Value.Item2.height * scale);
        //         var scaledTexture = TextureUtils.GetScaledTexture(item.Value.Item1, (int)scaledRect.width, (int)scaledRect.height);
        //         
        //         if (createAtlas)
        //         {
        //             namesAndTexturesList.Add(new Tuple<string, Texture2D>(item.Key.ToString("D4") + "_" + layerName, scaledTexture));
        //         }
        //         else
        //         {
        //             var fileName = Path.GetFileNameWithoutExtension(psdFile.Path) + "_" + item.Key.ToString("D4") + "_" + layerName;
        //             TextureUtils.SavePngAsset(scaledTexture, outputFolder + fileName + ".png", pixelsPerUnit);
        //         }
        //     }
        //     if (createAtlas)
        //     {
        //         var texture = TextureUtils.CreateAtlas(namesAndTexturesList.ToArray(), atlasMaxSize,
        //             outputFolder + Path.GetFileNameWithoutExtension(psdFile.Path) + ".png", pixelsPerUnit);
        //         textures.Add(texture);
        //     }
        //     return textures.ToArray();
        // }

        public void CreateGameObjets()
        {
            foreach (var item in _layerTextures.ToList())
            {
                if (item.Value.Item1 == null) continue;
                var scaledRect = new Rect(item.Value.Item2.x * _scale, item.Value.Item2.y * _scale,
                    item.Value.Item2.width * _scale, item.Value.Item2.height * _scale);
                var scaledTexture =
                    TextureUtils.GetScaledTexture(item.Value.Item1, (int) scaledRect.width, (int) scaledRect.height);
                _layerTextures[item.Key] = new Tuple<Texture2D, Rect>(scaledTexture, scaledRect);
            }

            var path = UIModuleEditor.CreateModule();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var root = UnityEngine.Object.Instantiate(prefab);
            _lastObject = root;
            var objectsAndTexturesList = new List<Tuple<string, GameObject, Texture2D, Rect>>();
            CreateHierarchy(
                root.transform,
                _psdFile.RootLayers.ToArray(),
                _lastSortingOrder, out _lastSortingOrder,
                ref objectsAndTexturesList);
            if (_createAtlas)
            {
                TextureUtils.CreateAtlas(_outputType, objectsAndTexturesList.ToArray(), _atlasMaxSize,
                    _outputFolder + Path.GetFileNameWithoutExtension(_psdFile.Path) + ".png", _pixelsPerUnit);
            }
            PrefabUtility.SaveAsPrefabAsset(root,path);
            UnityEngine.Object.DestroyImmediate(root);
            AssetDatabase.OpenAsset(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void CreateHierarchy(
            Transform parentTransform,
            Layer[] children,
            int initialSortingOrder,
            out int lastSortingOrder,
            ref List<Tuple<string, GameObject, Texture2D, Rect>> objectsAndTexturesList)
        {
            if (_outputType == TextureUtils.OutputObjectType.UI_IMAGE) Array.Reverse(children);

            lastSortingOrder = initialSortingOrder;
            foreach (var childLayer in children)
            {

                if (_importOnlyVisibleLayers && !(childLayer.Visible && childLayer.VisibleInHierarchy)) continue;

                GameObject childGameObject = null;

                string objName = childLayer.Name;

                if (childLayer is LayerGroup)
                {
                    childGameObject = new GameObject(objName);
                    childGameObject.transform.parent = parentTransform.transform;
                    _lastObject = childGameObject;
                    CreateHierarchy(
                        childGameObject.transform,
                        ((LayerGroup) childLayer).Children,
                        lastSortingOrder, out lastSortingOrder,
                        ref objectsAndTexturesList);
                    childGameObject.AddComponent<CanvasGroup>();
                    if (_outputType == TextureUtils.OutputObjectType.UI_IMAGE)
                    {
                        var rectTransform = childGameObject.AddComponent<RectTransform>();
                        rectTransform.localScale = Vector3.one;
                        rectTransform.localPosition = new Vector3((childLayer.Rect.x), (childLayer.Rect.y * -1) - childLayer.Rect.height, 0f);
                        rectTransform.anchorMax = new Vector2(0, 1);
                        rectTransform.anchorMin = new Vector2(0, 1);
                        rectTransform.pivot = new Vector2(0f, 0f);
                        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, childLayer.Rect.width);
                        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, childLayer.Rect.height);
                    }

                    continue;
                }

                Rect layerRect = _layerTextures[childLayer.Id].Item2;
                Texture2D texture = _layerTextures[childLayer.Id].Item1;
                if (texture == null) continue;

                childGameObject = new GameObject(objName);
                childGameObject.transform.parent = parentTransform.transform;

                lastSortingOrder--;

                SpriteRenderer renderer = null;
                UISprite image = null;
                if (_outputType == TextureUtils.OutputObjectType.SPRITE_RENDERER)
                {
                    childGameObject.transform.position = new Vector3(
                        (layerRect.width / 2 + layerRect.x) / _pixelsPerUnit,
                        -(layerRect.height / 2 + layerRect.y) / _pixelsPerUnit, 0);
                    renderer = childGameObject.AddComponent<SpriteRenderer>();
                    renderer.sortingOrder = lastSortingOrder;
                }
                else
                {
                    image = childGameObject.AddComponent<UISprite>();
                    image.rectTransform.localScale = Vector3.one;
                    image.rectTransform.localPosition =
                        new Vector3((layerRect.x), (layerRect.y * -1) - layerRect.height, 0f);
                    image.rectTransform.anchorMax = new Vector2(0, 1);
                    image.rectTransform.anchorMin = new Vector2(0, 1);
                    image.rectTransform.pivot = new Vector2(0f, 0f);
                    image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, layerRect.width);
                    image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, layerRect.height);
                }

                if (!_createAtlas)
                {
                    var fileName = Path.GetFileNameWithoutExtension(_psdFile.Path) + "_" +
                                   childLayer.Id.ToString("D4") + "_" + childLayer.Name;
                    Sprite childSprite =
                        TextureUtils.SavePngAsset(texture, _outputFolder + fileName + ".png", _pixelsPerUnit);
                    if (_outputType == TextureUtils.OutputObjectType.SPRITE_RENDERER)
                    {
                        renderer.sprite = childSprite;
                    }
                    else
                    {
                        image.sprite = childSprite;
                    }
                }

                var tuple = new Tuple<string, GameObject, Texture2D, Rect>(childLayer.Id.ToString("D4") + "_" + objName,
                    childGameObject, texture, layerRect);
                objectsAndTexturesList.Add(tuple);
            }
        }
    }
}
#endif