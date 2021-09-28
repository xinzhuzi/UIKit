using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Assertions;
using System.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using TMPro;
using UnityEngine.U2D;
using Debug = UnityEngine.Debug;

namespace PSD2UGUI
{
    public class PSD2UGUIWindow : EditorWindow
    {
        #region 定义变量

        #region EDITOR

        private GUISkin _skin = null;
        private const int _layerRowHeight = 40;
        private const int _layerRowPadding = 4;
        private const int _hierarchyThumbnailH = 32;
        private int _hierarchyThumbnailW = 32;
        private Rect _previewRect = Rect.zero;
        private Vector2 _layerScrollPos = Vector2.zero;
        private Texture2D _resultTexture = null;
        private bool _updatePreview = false;
        private bool _createAtlas = true;
        private bool _importIntoSelectedObject = true;
        private string _rootName = null;
        private static string _outputFolder = "Assets/";
        private bool _importOnlyVisibleLayers = true;
        private BlendingShaderType _blendingShader = BlendingShaderType.DEFAULT;
        private PrefabCreator _layerCreator = null;
        private SpriteAtlas[] spriteAtlasArray;
        private TMP_FontAsset[] fontArray;
        #endregion

        #region HIERARCHY ITEMS

        private abstract class HierarchyItem
        {
            public readonly int Id = -1;
            protected HierarchyItem(int id) => (Id) = (id);
        }

        private class LayerItem : HierarchyItem
        {
            public readonly Texture2D TextureNoBorder = null;
            public readonly Texture2D TextureWithBorder = null;

            public LayerItem(int id, Texture2D textureNoBorder, Texture2D textureWithBorder) : base(id)
                => (TextureNoBorder, TextureWithBorder) = (textureNoBorder, textureWithBorder);
        }

        private class GroupItem : HierarchyItem
        {
            public bool IsOpen { get; set; }

            public GroupItem(int id, bool isOpen) : base(id) => (IsOpen) = (isOpen);
        }

        private Dictionary<int, HierarchyItem> _hierarchyItems = new Dictionary<int, HierarchyItem>();
        private int _openItemCount = 0;

        #endregion

        #region FILE

        private string _psdPath = "";
        private bool _pathChanged = false;
        private PsdFile _psdFile = null;
        private Thread _psdFileThread = null;
        private float _loadingProgress = 0f;
#if UNITY_2020_1_OR_NEWER
        private int _progressId = -1;
#endif

        #endregion

        #region PREVIEW LAYERS

        [DebuggerDisplay("Name = {Name}")]
        private class PreviewLayer
        {
            public readonly string Name = null;
            public readonly Texture2D Texture = null;
            public readonly PsdBlendModeType BlendMode = PsdBlendModeType.NORMAL;
            public bool Visible { get; set; }
            public readonly float Alpha = 1f;

            public PreviewLayer(string name, Texture2D texture, PsdBlendModeType blendMode, bool visible, float alpha)
                => (Name, Texture, BlendMode, Visible, Alpha) = (name, texture, blendMode, visible, alpha);
        }

        private Dictionary<int, PreviewLayer> _previewLayers = new Dictionary<int, PreviewLayer>();

        #endregion

        #region TEXTURES

        private Queue<Layer> _textureLoadingPendingLayers = new Queue<Layer>();
        private Thread _loadThumbnailThread = null;

        private int _textureCount = 0;

        private class PendingData
        {
            public bool pending = false;
            public Layer layer = null;
            public Color32[] thumbnailPixels = null;
            public Color32[] thumbnailPixelsWithBorder = null;
            public Color32[] layerPixels = null;
            public Rect layerRect = Rect.zero;

            public void Reset()
            {
                pending = false;
                layer = null;
                thumbnailPixels = null;
                thumbnailPixelsWithBorder = null;
                layerPixels = null;
                layerRect = Rect.zero;
            }
        }

        private PendingData _pixelsPending = new PendingData();
        private static bool _repaint = false;
        private TextureUtils.LayerTextureLoader _textureLoader = null;
        private Dictionary<int, Tuple<Texture2D, Rect>> _layerTextures = new Dictionary<int, Tuple<Texture2D, Rect>>();
        private bool _loadingError = false;
        private string _errorMsg = null;

        #endregion

        #endregion


        #region 傻瓜式操作内容

        [MenuItem("UGUI/PSD 2 UGUI", false, 200)]
        public static void ShowWindow()
        {
            var window = GetWindow<PSD2UGUIWindow>(true);
            window.titleContent = new GUIContent("PSD 2 UGUI");
        }


        private void OnEnable()
        {
            spriteAtlasArray = new SpriteAtlas[3];
            fontArray = new TMP_FontAsset[3];
            _skin = Resources.Load<GUISkin>("PsdImporterSkin");
            Assert.IsNotNull(_skin);
            maxSize = minSize = new Vector2(1920, 900);
        }

        private void OnDestroy()
        {
            if (_textureLoader != null || _psdFile != null)
            {
#if UNITY_2020_1_OR_NEWER
                if (_progressId > 0)
                {
                    _progressId = Progress.Remove(_progressId);
                }
#else
                EditorUtility.ClearProgressBar();
#endif
            }

            if (_textureLoader != null)
            {
                _textureLoader.ProgressChanged -= OnTextureLoading;
                _textureLoader.OnLoadingComplete -= OnTextureLoadingComplete;
                _textureLoader.Cancel();
                _textureLoader = null;
                _loadThumbnailThread.Abort();
                _pixelsPending.pending = false;
                _repaint = false;
            }

            DestroyAllTextures();

            if (_psdFile == null) return;
            _psdFile.OnProgressChanged -= OnFileLoading;
            _psdFile.OnDone -= OnFileLoaded;
            _psdFile.OnError -= OnFileLoadingError;
            _psdFile.Cancel();
            _psdFile = null;
            _psdFileThread.Abort();
            _repaint = false;
        }

        private bool BrowsePanel()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.Width(maxSize.x / 2-1)))
            {
                if (GUILayout.Button("浏览 PSD 文件,并选择一个导入...", _skin.button))
                {
                    var psdPath = EditorUtility.OpenFilePanel("选择一个 PSD 文件进行导入", _psdPath, "psd");
                    if (psdPath.Length != 0)
                    {
                        _psdPath = psdPath;
                        LoadFile(_psdPath);
                    }
                }

                if (_psdFile == null) return false;

                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(_psdPath, EditorStyles.helpBox,
                        GUILayout.Height(21), GUILayout.Width(maxSize.x / 2-1));
                }
            }

            if (!_pathChanged) return true;
            _pathChanged = false;
            _hierarchyItems.Clear();
            _textureLoadingPendingLayers.Clear();
            _layerTextures.Clear();
            CreateItemDictionary(_psdFile.RootLayers.ToArray());
            LoadPendingTextures();
            return true;
        }

        private bool ProgressBar()
        {
            if (_loadingProgress < 1f)
            {
#if UNITY_2020_1_OR_NEWER
                Progress.Report(_progressId, _loadingProgress);
#else
                EditorUtility.DisplayProgressBar("Loading", ((int)(_loadingProgress * 100)).ToString() + " %", _loadingProgress);
#endif
                return false;
            }

            return !_pixelsPending.pending && _textureLoader == null;
        }

        private void PreviewPanel()
        {
            if (_updatePreview)
            {
                _previewLayers.Clear();

                foreach (var item in _hierarchyItems.Values)
                {
                    if (!(item is LayerItem)) continue;
                    if (((LayerItem) item).TextureNoBorder == null) continue;
                    var scaledTexture = TextureUtils.GetScaledTexture(((LayerItem) item).TextureNoBorder,
                        (int) _previewRect.width, (int) _previewRect.height);
                    var layer = _psdFile.GetLayer(item.Id);
                    _previewLayers.Add(item.Id,
                        new PreviewLayer(layer.Name, scaledTexture, layer.BlendModeInHierarchy,
                            layer.Visible && layer.VisibleInHierarchy, layer.AlphaInHierarchy));
                }

                _resultTexture = GetPreviewTexture((int) _previewRect.width, (int) _previewRect.height);
                _resultTexture = TextureUtils.SetTextureBorder(_resultTexture, (int) _previewRect.width,
                    (int) _previewRect.height);
                _updatePreview = false;
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.Width(maxSize.x/2),
                GUILayout.Height(1080 / 2)))
            {
                GUILayout.FlexibleSpace();
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    var style = new GUIStyle(_skin.label) {alignment = TextAnchor.MiddleCenter};
                    GUILayout.Label(_resultTexture, style);
                    GUILayout.FlexibleSpace();
                }

                GUILayout.FlexibleSpace();
            }
        }

        private void HierarchyPanel()
        {
            var h = maxSize.y;
            using (new GUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.Width(maxSize.x / 2 - 15),
                GUILayout.Height(h)))
            {
                using (var scrollView = new EditorGUILayout.ScrollViewScope(_layerScrollPos, false, true, GUIStyle.none,
                    GUI.skin.verticalScrollbar, GUIStyle.none))
                {
                    _layerScrollPos = scrollView.scrollPosition;
                    using (new GUILayout.VerticalScope(_skin.GetStyle("bottomBorder"),
                        GUILayout.Height(_layerRowHeight * _openItemCount)))
                    {
                        _openItemCount = 0;
                        CreateLayerHierarchy(_psdFile.RootLayers.ToArray());
                    }
                }
            }
        }

        private void ImportVisiblePanel()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(4);
                    _importOnlyVisibleLayers =
                        EditorGUILayout.ToggleLeft("Import only visible Layers", _importOnlyVisibleLayers);
                }
            }
        }

        private void AtlasPanel()
        {
            using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Space(4);
                    for (var i = 0; i < spriteAtlasArray.Length; i++)
                    {
                        spriteAtlasArray[i] = (SpriteAtlas) EditorGUILayout.ObjectField($"图集 {i + 1}:",
                            spriteAtlasArray[i], typeof(SpriteAtlas), true);
                    }
                }
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Space(4);
                    for (var i = 0; i < fontArray.Length; i++)
                    {
                        fontArray[i] = (TMP_FontAsset)EditorGUILayout.ObjectField($"字体 {i+1}:", fontArray[i], typeof(TMP_FontAsset), true);
                    }
                }
            }
        }

        private void RootObjectPanel()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(4);
                    EditorGUIUtility.labelWidth = 110;
                    _rootName = EditorGUILayout.TextField("Root object name:", _rootName);
                    GUILayout.Space(10);
                    _importIntoSelectedObject =
                        EditorGUILayout.ToggleLeft("Import into selected object", _importIntoSelectedObject);
                }
            }
        }

        private void ButtonsPanel()
        {
            using (new GUILayout.HorizontalScope())
            {
                // if (GUILayout.Button("生成图集", _skin.button))
                // {
                //     var textures = PrefabCreator.CreatePngFiles(_outputFolder, _layerTextures, _psdFile, importScale,
                //         _pixelsPerUnit, _createAtlas, _importOnlyVisibleLayers);
                // }

                if (GUILayout.Button("生成 2D 对象", _skin.button))
                {
                    if (BlendModeWarning())
                    {
                        _layerCreator = new PrefabCreator(
                            TextureUtils.OutputObjectType.SPRITE_RENDERER,
                            _importIntoSelectedObject,
                            _rootName,
                            _psdFile,
                            _previewLayers.Count,
                            _createAtlas,
                            _outputFolder,
                            _importOnlyVisibleLayers,
                            _blendingShader,
                            spriteAtlasArray,
                            fontArray,
                            _layerTextures);
                        _layerCreator.CreateGameObjets();
                    }
                }

                if (!GUILayout.Button("生成 UGUI Prefab与脚本", _skin.button)) return;
                if (!BlendModeWarning()) return;
                if (!ScreenSpaceWarning()) return;
                _layerCreator = new PrefabCreator(
                    TextureUtils.OutputObjectType.UI_IMAGE,
                    _importIntoSelectedObject,
                    _rootName,
                    _psdFile,
                    _previewLayers.Count,
                    _createAtlas,
                    _outputFolder,
                    _importOnlyVisibleLayers,
                    _blendingShader,
                    spriteAtlasArray,
                    fontArray,
                    _layerTextures);
                _layerCreator.CreateGameObjets();
            }
        }

        private void OnGUI()
        {
            if (_loadingError && Event.current.type == EventType.Repaint)
            {
#if UNITY_2020_1_OR_NEWER
                _progressId = Progress.Remove(_progressId);
#else
                EditorUtility.ClearProgressBar();
#endif
                _psdFile = null;
                _loadingError = false;
                _loadingProgress = 0f;
                _hierarchyItems.Clear();
                _textureLoadingPendingLayers.Clear();
                _layerTextures.Clear();
                EditorUtility.DisplayDialog("File Error", "There was an error while opening the file.\n" +
                                                          "Try opening the file in Photoshop or another editor and save it under a different name, then try importing it again.\n\n" +
                                                          "Error Details: " + _errorMsg, "Ok");
                _errorMsg = null;
                return;
            }

            using (new GUILayout.HorizontalScope())
            {
                using (new GUILayout.VerticalScope())
                {
                    PreviewPanel();
                    if (!BrowsePanel()) return;
                    if (!ProgressBar()) return;
                    using (new GUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.Width(maxSize.x / 2-1)))
                    {
                        ImportVisiblePanel();
                        AtlasPanel();
                        RootObjectPanel();
                        GUILayout.Space(10);
                        ButtonsPanel();
                    }
                }
                HierarchyPanel();
            }
        }

        private bool ScreenSpaceWarning()
        {
            if (_blendingShader != BlendingShaderType.FAST) return true;
            if (!_importIntoSelectedObject) return true;
            var rootParent = Selection.activeTransform;
            if (rootParent == null) return true;
            var canvas = rootParent.GetComponent<Canvas>();
            if (canvas == null) return true;
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera) return true;
            return EditorUtility.DisplayDialog("Warning: 渲染模式将改变.",
                "The Fast shader only works with: Screen Space - Camera.\n\n" +
                "你想继续吗? 所选对象的画布渲染模式将被设置为 Screen Space - Camera.\n" +
                "或者你想回去选择一个不同的 shader?", "Continue", "Back to Import Window");
        }

        private bool BlendModeWarning()
        {
            if (_blendingShader == BlendingShaderType.GRAB_PASS) return true;

            foreach (var item in _hierarchyItems.Values)
            {
                var layer = _psdFile.GetLayer(item.Id);
                if (layer.VisibleInHierarchy && layer.Visible)
                {
                    switch (_blendingShader)
                    {
                        case BlendingShaderType.DEFAULT:
                            if (layer.BlendModeKey != PsdBlendModeType.NORMAL &&
                                layer.BlendModeKey != PsdBlendModeType.PASS_THROUGH)
                            {
                                return EditorUtility.DisplayDialog("Warning: 不支持的混合模式。",
                                    "一个或多个图层被设置为默认着色器不支持的混合模式。\n" +
                                    "默认着色器只支持普通模式。\n\n" +
                                    "你想继续吗?不支持的模式将被设置为正常模式。\n" +
                                    "或者你想回去选择一个不同的着色器吗?", "Continue",
                                    "Back to Import Window");
                            }

                            break;
                        case BlendingShaderType.FAST:
                            if (((PsdBlendModeType) layer.BlendModeKey).GrabPass)
                            {
                                return EditorUtility.DisplayDialog("Warning: 不支持的混合模式。",
                                    "一个或多个图层被设置为Fast Shader不支持的混合模式.\n" +
                                    "快速着色器不支持以下模式:  " +
                                    "深颜色，浅颜色，针光，硬混合，差异，色相，饱和度，颜色和亮度.\n\n" +
                                    "你想继续吗?不支持的模式将被设置为正常模式.\n" +
                                    "或者你想回去选择一个不同的着色器?", "Continue",
                                    "Back to Import Window");
                            }
                            break;
                    }
                }
            }

            return true;
        }



        #endregion

        #region 显示 PSD 的层级内容

        private void CreateLayerHierarchy(Layer[] layers)
        {
            foreach (var layer in layers)
            {
                GroupItem groupItem = null;
                using (new EditorGUILayout.HorizontalScope(_skin.GetStyle("tableRow"),
                    GUILayout.MinHeight(_layerRowHeight)))
                {
                    using (new EditorGUILayout.VerticalScope(_skin.GetStyle("tableCell"),
                        GUILayout.Width(_layerRowHeight)))
                    {
                        GUILayout.Space(8);
                        using (new EditorGUILayout.HorizontalScope(
                            GUILayout.Height(_hierarchyThumbnailH - _layerRowPadding * 2)))
                        {
                            GUILayout.Space(8);
                            var wasVisible = layer.Visible;
                            layer.Visible = EditorGUILayout.Toggle(layer.Visible,
                                _skin.GetStyle(layer.VisibleInHierarchy ? "visibilityToggle" : "invisibleToggle"),
                                GUILayout.MaxWidth(_hierarchyThumbnailH));
                            if (wasVisible != layer.Visible)
                            {
                                UpdateVisibility(layer, layer.Visible);
                                _updatePreview = true;
                            }
                        }
                    }

                    using (new EditorGUILayout.HorizontalScope(
                        GUILayout.Height(_hierarchyThumbnailH - _layerRowPadding * 2)))
                    {
                        var indentSize = 30 * layer.HierarchyDepth;
                        GUILayout.Space(indentSize);
                        using (new EditorGUILayout.VerticalScope(GUILayout.Width(_layerRowHeight),
                            GUILayout.Height(_layerRowHeight)))
                        {
                            if (layer is LayerGroup)
                            {
                                groupItem = (GroupItem) _hierarchyItems[layer.Id];
                                GUILayout.Space(14);
                                groupItem.IsOpen =
                                    EditorGUILayout.Toggle(groupItem.IsOpen, _skin.GetStyle("folderToggle"));
                            }
                            else
                            {
                                ++_openItemCount;
                                LayerItem layerItem = (LayerItem) _hierarchyItems[layer.Id];
                                GUILayout.Space(4);
                                GUILayout.Label(layerItem.TextureWithBorder, GUILayout.Width(_hierarchyThumbnailH),
                                    GUILayout.Height(_hierarchyThumbnailH));
                            }
                        }

                        GUILayout.Space(-8);
                        using (new EditorGUILayout.VerticalScope(_skin.GetStyle("tableCell"),
                            GUILayout.MinHeight(_layerRowHeight)))
                        {
                            GUILayout.Space(9);
                            GUILayout.Label(layer.Name, EditorStyles.miniLabel, GUILayout.MaxWidth(400 - indentSize));
                        }

                        using (new EditorGUILayout.VerticalScope(_skin.GetStyle("tableCell"),
                            GUILayout.MinHeight(_layerRowHeight)))
                        {
                            EditorGUILayout.LabelField("Blending: " + ((PsdBlendModeType) layer.BlendModeKey).Name,
                                EditorStyles.miniLabel, GUILayout.MaxHeight(15), GUILayout.Width(117));
                            EditorGUILayout.LabelField("Opacity: " + (int) ((float) layer.Opacity / 2.55f) + "%",
                                EditorStyles.miniLabel, GUILayout.MaxHeight(15), GUILayout.Width(117));
                        }
                    }
                }

                if (groupItem == null || !groupItem.IsOpen) continue;
                ++_openItemCount;
                var group = (LayerGroup) layer;
                CreateLayerHierarchy(@group.Children);
            }
        }

        private void UpdateVisibility(Layer layer, bool value, bool isChild = false)
        {
            if (isChild)
            {
                layer.VisibleInHierarchy = value;
            }

            if (layer is LayerGroup)
            {
                var children = ((LayerGroup) layer).Children;
                foreach (var child in children)
                {
                    UpdateVisibility(child, value, true);
                }
            }
            else if (_previewLayers.ContainsKey(layer.Id))
            {

                _previewLayers[layer.Id].Visible = value && layer.Visible;
            }
        }

        private static void DestroyAllTextures()
        {
            var textures = FindObjectsOfType<Texture2D>();
            DestroyTextures(textures);
        }

        private static void DestroyTextures(Texture2D[] textures)
        {
            foreach (var texture in textures)
            {
                DestroyImmediate(texture);
            }

            Resources.UnloadUnusedAssets();
        }

        private void DestroyUnusedTextures()
        {
            var textures = FindObjectsOfType<Texture2D>();
            var textureSet = new HashSet<Texture2D>(textures);
            textureSet.Remove(_resultTexture);
            foreach (var item in _hierarchyItems.Values)
            {
                if (!(item is LayerItem)) continue;
                var layerItem = item as LayerItem;
                textureSet.Remove(layerItem.TextureNoBorder);
                textureSet.Remove(layerItem.TextureWithBorder);
            }

            foreach (var item in _layerTextures)
            {
                textureSet.Remove(item.Value.Item1);
            }

            DestroyTextures(textureSet.ToArray());
            textureSet.Clear();
            textureSet = null;
            textures = null;
        }

        #endregion


        #region 加载 PSD 文件

        private void LoadFile(string path)
        {
            _pathChanged = false;
            _psdFile = new PsdFile(path);
            _psdFile.OnProgressChanged += OnFileLoading;
            _psdFile.OnDone += OnFileLoaded;
            _psdFile.OnError += OnFileLoadingError;
            var threadDelegate = new ThreadStart(_psdFile.Load);
            _psdFileThread = new Thread(threadDelegate);
            _psdFileThread.Start();
            DestroyAllTextures();
#if UNITY_2020_1_OR_NEWER
            _progressId = Progress.Start("Loading");
#endif
        }

        private void OnFileLoading(float progress)
        {
            _loadingProgress = progress * 0.3f;
            _repaint = true;
        }

        private void OnFileLoaded()
        {
            _psdFile.OnProgressChanged -= OnFileLoading;
            _psdFile.OnDone -= OnFileLoaded;
            _psdFile.OnError -= OnFileLoadingError;
            _pathChanged = true;
            _rootName = Path.GetFileNameWithoutExtension(_psdPath);
            var aspect = _psdFile.BaseLayer.Rect.width / _psdFile.BaseLayer.Rect.height;
            _hierarchyThumbnailW = (int) ((float) _hierarchyThumbnailH * aspect);
            _previewRect = GetPreviewRect((int) _psdFile.BaseLayer.Rect.width, (int) _psdFile.BaseLayer.Rect.height,
                1920 / 2, 1080 / 2);
            _repaint = true;
        }

        private void OnFileLoadingError(string message)
        {
            _errorMsg = message;
            _loadingError = true;
            _psdFile.OnProgressChanged -= OnFileLoading;
            _psdFile.OnDone -= OnFileLoaded;
            _psdFile.OnError -= OnFileLoadingError;
            _psdFile = null;
            _psdFileThread.Abort();
        }

        #endregion

        #region LOAD TEXTURES

        private void CreateItemDictionary(Layer[] layers)
        {
            foreach (var layer in layers)
            {
                if (layer is LayerGroup)
                {
                    _hierarchyItems.Add(layer.Id, new GroupItem(layer.Id, ((LayerGroup) layer).IsOpen));
                    CreateItemDictionary(((LayerGroup) layer).Children);
                    continue;
                }

                _textureLoadingPendingLayers.Enqueue(layer);
                ++_textureCount;
            }
        }

        private void LoadPendingTextures()
        {
            _pixelsPending = new PendingData();
            if (_textureLoadingPendingLayers.Count == 0)
            {
                LoadNextLayer();
                return;
            }

            var layer = _textureLoadingPendingLayers.Dequeue();

            _textureLoader = new TextureUtils.LayerTextureLoader(layer, (int) _psdFile.BaseLayer.Rect.width,
                (int) _psdFile.BaseLayer.Rect.height, _hierarchyThumbnailW, _hierarchyThumbnailH,
                (int) _previewRect.width, (int) _previewRect.height);
            _textureLoader.ProgressChanged += OnTextureLoading;
            _textureLoader.OnLoadingComplete += OnTextureLoadingComplete;
            _textureLoader.OnError += OnTextureLoadingError;
            _loadThumbnailThread = new Thread(_textureLoader.LoadLayerPixels);
            _loadThumbnailThread.Name = layer.Name;
            _loadThumbnailThread.Start();
        }

        private void OnTextureLoading(float progress)
        {
            _loadingProgress = 0.3f +
                               (progress + (float) _textureCount - (float) _textureLoadingPendingLayers.Count - 1f) /
                               (float) _textureCount * 0.7f;
            _repaint = true;
        }

        private void OnTextureLoadingComplete(Layer layer, Color32[] thumbnailPixels,
            Color32[] thumbnailPixelsWithBorder, Color32[] layerPixels, Rect layerRect)
        {
            _textureLoader.ProgressChanged -= OnTextureLoading;
            _textureLoader.OnLoadingComplete -= OnTextureLoadingComplete;

            _pixelsPending.pending = true;
            _pixelsPending.layer = layer;
            _pixelsPending.thumbnailPixels = thumbnailPixels;
            _pixelsPending.thumbnailPixelsWithBorder = thumbnailPixelsWithBorder;
            _pixelsPending.layerPixels = layerPixels;
            _pixelsPending.layerRect = layerRect;

            _textureLoader = null;
        }

        private void OnTextureLoadingError()
        {
            _textureLoader.ProgressChanged -= OnTextureLoading;
            _textureLoader.OnLoadingComplete -= OnTextureLoadingComplete;
            _textureLoader.OnError -= OnTextureLoadingError;
            _textureLoader = null;
            _loadingError = true;
            _loadThumbnailThread.Abort();
        }

        private void LoadNextLayer()
        {
            if (_textureLoadingPendingLayers.Count == 0)
            {
                _loadingProgress = 1f;
                _updatePreview = true;
#if UNITY_2020_1_OR_NEWER
                _progressId = Progress.Remove(_progressId);
#else
                EditorUtility.ClearProgressBar();
#endif
                _repaint = true;
                DestroyUnusedTextures();
            }
            else
            {
                LoadPendingTextures();
            }
        }

        private void Update()
        {
            if (_pixelsPending.pending && _textureLoader == null)
            {
                var layer = _pixelsPending.layer;
                if (_layerTextures.ContainsKey(layer.Id))
                {
                    LoadNextLayer();
                    _pixelsPending.pending = false;
                }

                if (_pixelsPending.thumbnailPixels == null)
                {
                    _hierarchyItems.Add(layer.Id, new LayerItem(layer.Id, null, null));
                    _layerTextures.Add(layer.Id, new Tuple<Texture2D, Rect>(null, Rect.zero));
                    LoadNextLayer();
                    _pixelsPending.pending = false;
                    return;
                }

                var thumbnailTexture = new Texture2D((int) _previewRect.width, (int) _previewRect.height,
                    TextureFormat.RGBA32, true);
                thumbnailTexture.SetPixels32(_pixelsPending.thumbnailPixels);
                thumbnailTexture.Apply();

                var hierarchyThumbnailTexture =
                    new Texture2D(_hierarchyThumbnailW, _hierarchyThumbnailH, TextureFormat.RGBA32, true);
                hierarchyThumbnailTexture.SetPixels32(_pixelsPending.thumbnailPixelsWithBorder);
                hierarchyThumbnailTexture.Apply();
                _hierarchyItems.Add(layer.Id, new LayerItem(layer.Id, thumbnailTexture, hierarchyThumbnailTexture));

                var layerTexture = new Texture2D((int) _pixelsPending.layerRect.width,
                    (int) _pixelsPending.layerRect.height, TextureFormat.RGBA32, true);
                layerTexture.SetPixels32(_pixelsPending.layerPixels);
                layerTexture.Apply();

                _layerTextures.Add(layer.Id, new Tuple<Texture2D, Rect>(layerTexture, _pixelsPending.layerRect));

                LoadNextLayer();

                _pixelsPending.pending = false;
                _pixelsPending.Reset();
            }

            if (_repaint)
            {
                Repaint();
                _repaint = false;
            }
        }

        #endregion

        #region PREVIEW IMAGE

        private Texture2D GetPreviewTexture(int width, int height)
        {
            var resultTexture = new Texture2D(width, height, TextureFormat.RGBA32, true);

            var resultTexturePixels = resultTexture.GetPixels();

            for (int i = 0; i < resultTexturePixels.Length; ++i)
            {
                var r = i / width;
                var c = i - r * width;
                resultTexturePixels[i] = (r % 16 < 8) == (c % 16 < 8) ? Color.white : new Color(0.8f, 0.8f, 0.8f);
            }

            for (int layerIdx = _previewLayers.Count - 1; layerIdx >= 0; --layerIdx)
            {
                var previewLayer = _previewLayers.ElementAt(layerIdx).Value;
                if (!previewLayer.Visible) continue;
                var sourcePixels = previewLayer.Texture.GetPixels();
                for (int i = 0; i < resultTexturePixels.Length; ++i)
                {
                    resultTexturePixels[i] = GetBlendedPixel(resultTexturePixels[i], sourcePixels[i],
                        previewLayer.Alpha, previewLayer.BlendMode);
                }
            }

            resultTexture.SetPixels(resultTexturePixels);
            resultTexture.Apply();
            return resultTexture;
        }

        private struct FloatColor
        {
            public float r, g, b;

            public FloatColor(float r, float g, float b)
            {
                this.r = r;
                this.g = g;
                this.b = b;
            }

            public FloatColor(Color color)
            {
                r = color.r;
                g = color.g;
                b = color.b;
            }

            public static implicit operator Color(FloatColor c) =>
                new Color(Mathf.Clamp01(c.r), Mathf.Clamp01(c.g), Mathf.Clamp01(c.b));

            public static implicit operator FloatColor(Color c) => new FloatColor(c.r, c.g, c.b);
        }

        private static float GetLuminosity(FloatColor color)
        {
            return 0.3f * color.r + 0.59f * color.g + 0.11f * color.b;
        }

        private static FloatColor SetLuminosity(FloatColor RGBColor, float L)
        {
            var delta = L - GetLuminosity(RGBColor);
            return ClipColor(new FloatColor(RGBColor.r + delta, RGBColor.g + delta, RGBColor.b + delta));
        }

        private static FloatColor ClipColor(FloatColor color)
        {
            var L = GetLuminosity(color);
            var min = Mathf.Min(color.r, color.g, color.b);
            var max = Mathf.Max(color.r, color.g, color.b);

            var result = color;
            if (min < 0f)
            {
                result.r = L + (((color.r - L) * L) / (L - min));
                result.g = L + (((color.g - L) * L) / (L - min));
                result.b = L + (((color.b - L) * L) / (L - min));
            }

            if (max > 1f)
            {
                result.r = L + (((color.r - L) * (1f - L)) / (max - L));
                result.g = L + (((color.g - L) * (1f - L)) / (max - L));
                result.b = L + (((color.b - L) * (1f - L)) / (max - L));
            }

            return result;
        }

        private static float GetSaturation(FloatColor RGBColor)
        {
            var min = Mathf.Min(RGBColor.r, RGBColor.g, RGBColor.b);
            var max = Mathf.Max(RGBColor.r, RGBColor.g, RGBColor.b);
            return max - min;
        }

        private class ColorComponent
        {
            public delegate float BlendDelegate(float backdrop, float source);
        }

        private static FloatColor SetSaturation(FloatColor RGBColor, float S)
        {
            const int MIN_COLOR = 0, MID_COLOR = 1, MAX_COLOR = 2;
            int R = MIN_COLOR, G = MIN_COLOR, B = MIN_COLOR;
            float minValue, midValue, maxValue;

            if (RGBColor.r <= RGBColor.g && RGBColor.r <= RGBColor.b)
            {
                minValue = RGBColor.r;
                if (RGBColor.g <= RGBColor.b)
                {
                    G = MID_COLOR;
                    B = MAX_COLOR;
                    midValue = RGBColor.g;
                    maxValue = RGBColor.b;
                }
                else
                {
                    B = MID_COLOR;
                    G = MAX_COLOR;
                    midValue = RGBColor.b;
                    maxValue = RGBColor.g;
                }
            }
            else if (RGBColor.g <= RGBColor.r && RGBColor.g <= RGBColor.b)
            {
                minValue = RGBColor.g;
                if (RGBColor.r <= RGBColor.b)
                {
                    R = MID_COLOR;
                    B = MAX_COLOR;
                    midValue = RGBColor.r;
                    maxValue = RGBColor.b;
                }
                else
                {
                    B = MID_COLOR;
                    R = MAX_COLOR;
                    midValue = RGBColor.b;
                    maxValue = RGBColor.r;
                }
            }
            else
            {
                minValue = RGBColor.b;
                if (RGBColor.r <= RGBColor.g)
                {
                    R = MID_COLOR;
                    G = MAX_COLOR;
                    midValue = RGBColor.r;
                    maxValue = RGBColor.g;
                }
                else
                {
                    G = MID_COLOR;
                    R = MAX_COLOR;
                    midValue = RGBColor.g;
                    maxValue = RGBColor.r;
                }
            }

            if (maxValue > minValue)
            {
                midValue = (((midValue - minValue) * S) / (maxValue - minValue));
                maxValue = S;
            }
            else
            {
                midValue = maxValue = 0.0f;
            }

            minValue = 0.0f;

            return new FloatColor(
                R == MIN_COLOR ? minValue : (R == MID_COLOR ? midValue : maxValue),
                G == MIN_COLOR ? minValue : (G == MID_COLOR ? midValue : maxValue),
                B == MIN_COLOR ? minValue : (B == MID_COLOR ? midValue : maxValue));
        }

        private static float AlphaBlend(float backdrop, float blend, float sourceAlpha)
        {
            return backdrop * (1f - sourceAlpha) + blend * sourceAlpha;
        }

        private static Color GetBlendResult(Color backdrop, Color source, ColorComponent.BlendDelegate blendDelegate)
        {
            var result = new Color(
                AlphaBlend(backdrop.r, blendDelegate(backdrop.r, source.r), source.a),
                AlphaBlend(backdrop.g, blendDelegate(backdrop.g, source.g), source.a),
                AlphaBlend(backdrop.b, blendDelegate(backdrop.b, source.b), source.a));
            return result;
        }

        private static Color GetBlendResult(Color backdrop, Color blended, float sourceAlpha)
        {
            var result = new Color(
                AlphaBlend(backdrop.r, blended.r, sourceAlpha),
                AlphaBlend(backdrop.g, blended.g, sourceAlpha),
                AlphaBlend(backdrop.b, blended.b, sourceAlpha));
            return result;
        }

        private static Color GetBlendedPixel(Color backdrop, Color source, float layerAlpha, PsdBlendModeType blendMode)
        {
            source.a *= layerAlpha;
            if (blendMode == PsdBlendModeType.NORMAL)
            {
                return GetBlendResult(backdrop, source, source.a);
            }
            else if (blendMode == PsdBlendModeType.DISSOLVE)
            {
                var randomAlpha = source.a;
                if (UnityEngine.Random.value > randomAlpha)
                {
                    randomAlpha = 0;
                }

                var result = backdrop * (1 - randomAlpha) + source * randomAlpha;
                result.a = backdrop.a + source.a;
                return result;
            }
            /////////////////////////////////////////////////////////
            else if (blendMode == PsdBlendModeType.DARKEN)
            {
                ColorComponent.BlendDelegate Darken = delegate(float bc, float sc) { return Mathf.Min(bc, sc); };
                return GetBlendResult(backdrop, source, Darken);
            }
            else if (blendMode == PsdBlendModeType.MULTIPLY)
            {
                ColorComponent.BlendDelegate Multiply = delegate(float bc, float sc) { return bc * sc; };
                return GetBlendResult(backdrop, source, Multiply);
            }
            else if (blendMode == PsdBlendModeType.COLOR_BURN)
            {
                ColorComponent.BlendDelegate ColorBurn = delegate(float bc, float sc)
                {
                    return sc == 0f ? 0f : 1f - Mathf.Min(1f, (1f - bc) / sc);
                };
                return GetBlendResult(backdrop, source, ColorBurn);
            }
            else if (blendMode == PsdBlendModeType.LINEAR_BURN)
            {
                ColorComponent.BlendDelegate LinearBurn = delegate(float bc, float sc)
                {
                    return Mathf.Clamp01(bc + sc - 1f);
                };
                return GetBlendResult(backdrop, source, LinearBurn);
            }
            else if (blendMode == PsdBlendModeType.DARKER_COLOR)
            {
                return GetLuminosity(backdrop) < GetLuminosity(source)
                    ? backdrop
                    : GetBlendResult(backdrop, source, source.a);
            }
            /////////////////////////////////////////////////////////
            else if (blendMode == PsdBlendModeType.LIGHTEN)
            {
                ColorComponent.BlendDelegate Lighten = delegate(float bc, float sc) { return Mathf.Max(bc, sc); };
                return GetBlendResult(backdrop, source, Lighten);
            }
            else if (blendMode == PsdBlendModeType.SCREEN)
            {
                ColorComponent.BlendDelegate Screen = delegate(float bc, float sc)
                {
                    return 1f - (1f - bc) * (1f - sc);
                };
                return GetBlendResult(backdrop, source, Screen);
            }
            else if (blendMode == PsdBlendModeType.COLOR_DODGE)
            {
                ColorComponent.BlendDelegate ColorDodge = delegate(float bc, float sc)
                {
                    return sc == 1f ? 1f : Mathf.Min(1f, bc / (1f - sc));
                };
                return GetBlendResult(backdrop, source, ColorDodge);
            }
            else if (blendMode == PsdBlendModeType.LINEAR_DODGE)
            {
                ColorComponent.BlendDelegate LinearDodge = delegate(float bc, float sc) { return bc + sc * source.a; };
                return GetBlendResult(backdrop, source, LinearDodge);
            }
            else if (blendMode == PsdBlendModeType.LIGHTER_COLOR)
            {
                return GetLuminosity(backdrop) > GetLuminosity(source)
                    ? backdrop
                    : GetBlendResult(backdrop, source, source.a);
            }
            /////////////////////////////////////////////////////////
            else if (blendMode == PsdBlendModeType.OVERLAY)
            {
                ColorComponent.BlendDelegate Overlay = delegate(float bc, float sc)
                {
                    return bc > 0.5f ? (1f - 2f * (1f - bc) * (1f - sc)) : 2f * bc * sc;
                };
                return GetBlendResult(backdrop, source, Overlay);
            }
            else if (blendMode == PsdBlendModeType.SOFT_LIGHT)
            {
                ColorComponent.BlendDelegate SoftLight = delegate(float bc, float sc)
                {
                    if (sc <= 0.5f)
                    {
                        return bc - (1f - 2f * sc) * bc * (1f - bc);
                    }
                    else
                    {
                        var d = bc <= 0.25f ? ((16f * bc - 12f) * bc + 4f) * bc : Mathf.Sqrt(bc);
                        return bc + (2f * sc - 1) * (d - bc);
                    }
                };
                return GetBlendResult(backdrop, source, SoftLight);
            }
            else if (blendMode == PsdBlendModeType.HARD_LIGHT)
            {
                ColorComponent.BlendDelegate HardLight = delegate(float bc, float sc)
                {
                    return bc <= 0.5f ? (1f - 2f * (1f - bc) * (1f - sc)) : 2f * bc * sc;
                };
                return GetBlendResult(backdrop, source, HardLight);
            }
            else if (blendMode == PsdBlendModeType.VIVID_LIGHT)
            {
                ColorComponent.BlendDelegate VividLight = delegate(float bc, float sc)
                {
                    return sc <= 0.5f ? sc == 0f ? 0f : Mathf.Clamp01(1f - (1f - bc) / (2f * sc)) :
                        sc == 1f ? 1f : bc / (2 * (1f - sc));
                };
                return GetBlendResult(backdrop, source, VividLight);
            }
            else if (blendMode == PsdBlendModeType.LINEAR_LIGHT)
            {
                ColorComponent.BlendDelegate LinearLight = delegate(float bc, float sc) { return bc + 2f * sc - 1f; };
                return GetBlendResult(backdrop, source, LinearLight);
            }
            else if (blendMode == PsdBlendModeType.PIN_LIGHT)
            {
                ColorComponent.BlendDelegate PinLight = delegate(float bc, float sc)
                {
                    return bc < 2f * sc - 1f ? 2f * sc - 1f : (bc < 2f * sc ? bc : 2f * sc);
                };
                return GetBlendResult(backdrop, source, PinLight);
            }
            else if (blendMode == PsdBlendModeType.HARD_MIX)
            {
                ColorComponent.BlendDelegate HardMix = delegate(float bc, float sc) { return sc < 1f - bc ? 0f : 1f; };
                return GetBlendResult(backdrop, source, HardMix);
            }
            /////////////////////////////////////////////////////////
            else if (blendMode == PsdBlendModeType.DIFFERENCE)
            {
                ColorComponent.BlendDelegate Difference = delegate(float bc, float sc) { return Mathf.Abs(sc - bc); };
                return GetBlendResult(backdrop, source, Difference);
            }
            else if (blendMode == PsdBlendModeType.EXCLUSION)
            {
                ColorComponent.BlendDelegate Exclusion = delegate(float bc, float sc)
                {
                    return sc + bc - 2f * sc * bc;
                };
                return GetBlendResult(backdrop, source, Exclusion);
            }
            else if (blendMode == PsdBlendModeType.SUBTRACT)
            {
                ColorComponent.BlendDelegate Substract = delegate(float bc, float sc) { return bc - sc; };
                return GetBlendResult(backdrop, source, Substract);
            }
            else if (blendMode == PsdBlendModeType.DIVIDE)
            {
                ColorComponent.BlendDelegate Divide = delegate(float bc, float sc) { return sc == 0f ? 1f : bc / sc; };
                return GetBlendResult(backdrop, source, Divide);
            }
            /////////////////////////////////////////////////////////
#if UNITY_WEBGL
#else
            else if (blendMode == PsdBlendModeType.HUE)
            {
                var result = (Color) SetLuminosity(SetSaturation(source, GetSaturation(backdrop)),
                    GetLuminosity(backdrop));
                return GetBlendResult(backdrop, result, source.a);
            }
            else if (blendMode == PsdBlendModeType.SATURATION)
            {
                var result = (Color) SetLuminosity(SetSaturation(backdrop, GetSaturation(source)),
                    GetLuminosity(backdrop));
                return GetBlendResult(backdrop, result, source.a);
            }
            else if (blendMode == PsdBlendModeType.COLOR)
            {
                var result = (Color) SetLuminosity(source, GetLuminosity(backdrop));
                return GetBlendResult(backdrop, result, source.a);
            }
            else if (blendMode == PsdBlendModeType.LUMINOSITY)
            {
                var result = (Color) SetLuminosity(backdrop, GetLuminosity(source));
                return GetBlendResult(backdrop, result, source.a);
            }
#endif
            /////////////////////////////////////////////////////////
            else if (blendMode == PsdBlendModeType.PASS_THROUGH)
            {
                return backdrop;
            }

            return Color.black;
        }

        #endregion

        #region UTILS

        private static Rect GetPreviewRect(int sourceWidth, int sourceHeight, int maxWidth, int maxHeight)
        {
            var resultWidth = maxWidth;
            var aspectRatio = (float) sourceWidth / (float) sourceHeight;
            var resultHeight = (int) ((float) resultWidth / aspectRatio);

            if (resultHeight > maxHeight)
            {
                resultHeight = maxHeight;
                resultWidth = (int) ((float) resultHeight * aspectRatio);
            }

            if (resultWidth < sourceWidth && resultHeight < sourceHeight)
            {
                return new Rect(0, 0, resultWidth, resultHeight);
            }

            return new Rect(0, 0, sourceWidth, sourceHeight);
        }

        #endregion
    }
}
