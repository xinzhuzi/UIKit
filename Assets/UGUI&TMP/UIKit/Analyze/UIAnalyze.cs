#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using Object = System.Object;

namespace UIKit
{
    [ExecuteInEditMode]
    internal class UIAnalyze : MonoBehaviour
    {
        public static UIAnalyze Instance;
        private const string UIDebugName = "[--Debug--UI--]";

        public bool ShowRaycastTargetUI = false;

        public bool ShowReBuildUI = false;
        private IList<ICanvasElement> m_LayoutRebuildQueue;
        private IList<ICanvasElement> m_GraphicRebuildQueue;

        private StringBuilder vertexSB;
        private List<Graphic> allGraphics;
        private StringBuilder cache;

        public bool ShowAllUI_Z_Position = false;

        private bool _isChangeSceneView;
        
        
        private void Awake()
        {
            if (Application.isPlaying)
            {
                GameObject.DontDestroyOnLoad(this.gameObject);
            }
            
            Type type = typeof(CanvasUpdateRegistry);
            FieldInfo field = type.GetField("m_LayoutRebuildQueue", BindingFlags.NonPublic | BindingFlags.Instance);
            m_LayoutRebuildQueue = (IList<ICanvasElement>) field.GetValue(CanvasUpdateRegistry.instance);
            field = type.GetField("m_GraphicRebuildQueue", BindingFlags.NonPublic | BindingFlags.Instance);
            m_GraphicRebuildQueue = (IList<ICanvasElement>) field.GetValue(CanvasUpdateRegistry.instance);
            vertexSB = new StringBuilder(500);
            allGraphics = new List<Graphic>(100);
            cache = new StringBuilder(500);
        }

        private void LateUpdate()
        {
            if (ShowReBuildUI)
            {
                for (int j = 0; j < m_LayoutRebuildQueue.Count; j++)
                {
                    var item = m_LayoutRebuildQueue[j];
                    Debug.LogFormat("<color=yellow>{0}{1} ??????????????????</color>", FindNodeToString(item.transform),
                        item.transform.name);
                }

                for (int j = 0; j < m_GraphicRebuildQueue.Count; j++)
                {
                    var element = m_GraphicRebuildQueue[j];
                    Debug.LogFormat("<color=yellow>{0}{1} ??????????????????</color>", FindNodeToString(element.transform),
                        element.transform.name);
                }
            }
            
            if (ShowAllUI_Z_Position)
            {
                foreach (Graphic g in GameObject.FindObjectsOfType<Graphic>())
                {
                    if (g.transform.position.z != 0)
                    {
                        g.transform.position = new Vector3(g.transform.position.x, g.transform.position.y, 0);
                        Debug.LogErrorFormat("<color=red>{0}{1} ??? Z ????????? 0,????????????????????? 0</color>",
                            FindNodeToString(g.transform), g.transform.name);
                    }
                }
            }
        }

        private readonly Vector3[] _fourCorners = new Vector3[4];
        private void OnDrawGizmos()
        {
            if (ShowRaycastTargetUI)
            {
                foreach (Graphic g in GameObject.FindObjectsOfType<Graphic>())
                {
                    if (!g.raycastTarget) continue;

                    Debug.LogFormat("<color=yellow>{0}{1} ?????? raycastTarget,????????????????????????,?????????????????????raycastTarget</color>",
                        FindNodeToString(g.transform), g.transform.name);
                    g.GetComponent<RectTransform>().GetWorldCorners(_fourCorners);
                    Gizmos.color = Color.blue;
                    for (int i = 0; i < 4; i++)
                        Gizmos.DrawLine(_fourCorners[i], _fourCorners[(i + 1) % 4]);
                }
            }
        }

        private void OnApplicationQuit()
        {
            DestroyImmediate(this.gameObject);
            if (_isChangeSceneView)
            {
                ResetSceneView();
            }
            UIAnalyze[] ds = GameObject.FindObjectsOfType<UIAnalyze>();
            for (int i = ds.Length - 1; i >= 1; i--)
            {
                DestroyImmediate(ds[i].gameObject);
            }
        }

        private void FindUILinkFunc<T>(Transform root) where T : Graphic
        {
            T target = root.GetComponent<T>();
            if (target != null)
            {
                allGraphics.Add(target);
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                if (!child.GetComponent<Canvas>())
                {
                    FindUILinkFunc<T>(child);
                }
            }
        }

        private string FindNodeToString(Transform tr)
        {
            cache.Clear();
            Transform t = tr.transform.parent;
            while (t != null)
            {
                cache.Insert(0, t.name + "/");
                t = t.parent;
            }

            return cache.ToString();
        }

        #region ????????????&????????????
        // foreach (var item in typeof(EditorWindow).Assembly.GetTypes())
        // {
        //     if (item.ToString().Contains("Scene") && item.ToString().Contains("Window"))
        //     {
        //         Debug.Log(item.ToString());
        //     }
        // }
        //????????????
        public void ShowMeshOverlaps()
        {
            if (!EditorApplication.ExecuteMenuItem("Window/General/Scene")) return;
            
            var windowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneView");
            var sceneView = EditorWindow.GetWindow(windowType) as UnityEditor.SceneView;
            if (sceneView == null)return;
            sceneView.in2DMode = true;
            sceneView.drawGizmos = false;
            sceneView.cameraMode = new SceneView.CameraMode()
            {
                drawMode=DrawCameraMode.Wireframe,
                name = "Wireframe",
                section = "Shading Mode",
            };
            _isChangeSceneView = true;
            UIBatchesAnalyze.AnalyzeOverlaps();
        }
        
        
        //????????????
        public void ShowUIOverdraw()
        {
            if (!EditorApplication.ExecuteMenuItem("Window/General/Scene")) return;
            
            var windowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneView");
            var sceneView = EditorWindow.GetWindow(windowType) as UnityEditor.SceneView;
            if (sceneView == null)return;
            sceneView.in2DMode = true;
            sceneView.drawGizmos = false;
            sceneView.cameraMode = new SceneView.CameraMode()
            {
                drawMode=DrawCameraMode.Overdraw,
                name = "Overdraw",
                section = "Miscellaneous",
            };
            _isChangeSceneView = true;
            UIBatchesAnalyze.AnalyzeOverlaps();
        }
        
        public void ResetSceneView()
        {
            var windowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneView");
            var sceneView = EditorWindow.GetWindow(windowType) as UnityEditor.SceneView;
            if (sceneView == null)return;
            sceneView.drawGizmos = true;
            sceneView.in2DMode = false;
            sceneView.cameraMode = new SceneView.CameraMode()
            {
                drawMode=DrawCameraMode.Textured,
                name = "Shaded",
                section = "Shading Mode",
            };
        }
        
        #endregion
        
        #region ????????????

        
        private static List<string> ignoreReadable = new List<string>()
        {
            ""
        };

        private static List<string> ignoreMipMaps = new List<string>()
        {
            ""
        };

        public void PrintAllSpriteAtlasSettings()
        {
            List<UnityEngine.Object> selects = new List<UnityEngine.Object>();
            foreach (var item in AssetDatabase.FindAssets("t:" + nameof(SpriteAtlas)))
            {
                SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AssetDatabase.GUIDToAssetPath(item));
                SpriteAtlasTextureSettings textureSettings = atlas.GetTextureSettings();

                if (!ignoreMipMaps.Contains(atlas.name) && textureSettings.generateMipMaps)
                {
                    Debug.LogError("[ " + atlas.name + " ]????????? MipMaps ??? UI ??????????????????,?????????????????????????????????,????????????????????????????????????");
                    if (!selects.Contains(atlas))
                    {
                        selects.Add(atlas);
                    }
                }

                if (!ignoreReadable.Contains(atlas.name) && textureSettings.readable)
                {
                    Debug.LogError("[ " + atlas.name + " ]????????? Read/Write ??? UI ??????????????????,?????????????????????????????????,????????????????????????????????????");
                    if (!selects.Contains(atlas))
                    {
                        selects.Add(atlas);
                    }
                }
            }

            if (selects.Count > 0)
            {
                Selection.objects = selects.ToArray();
            }
        }

        public void PrintAllSpriteSize()
        {
            List<UnityEngine.Object> selects = new List<UnityEngine.Object>();
            foreach (var item in GameObject.FindObjectsOfType<MaskableGraphic>())
            {
                float width = item.mainTexture.width;
                float height = item.mainTexture.height;

                if (width % 2 != 0)
                {
                    Debug.LogFormat("<color=yellow>{0}{1}/{2} ??????????????? 2 ????????????</color>", FindNodeToString(item.transform),
                        item.transform.name, item.mainTexture.name);
                    if (!selects.Contains(item))
                    {
                        selects.Add(item);
                    }
                }

                if (height % 2 != 0)
                {
                    Debug.LogFormat("<color=yellow>{0}{1}/{2} ??????????????? 2 ????????????</color>", FindNodeToString(item.transform),
                        item.transform.name, item.mainTexture.name);
                    if (!selects.Contains(item))
                    {
                        selects.Add(item);
                    }
                }
            }

            if (selects.Count > 0)
            {
                Selection.objects = selects.ToArray();
            }
        }
        
    
        #endregion

        #region ??????Frame Debugger???Profiler???????????????
        
        public void ShowFrameDebuggerAndProfiler()
        {
            // var windowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.AnimationWindow");
            // foreach (var item in typeof(EditorWindow).Assembly.GetTypes())
            // {
            //     if (item.ToString().Contains("Profiler"))
            //     {
            //         Debug.Log(item.ToString());
            //     }
            // }
            
            if (EditorApplication.ExecuteMenuItem("Window/Analysis/Profiler"))
            {
                var windowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ProfilerWindow");
                var editorWindow = EditorWindow.GetWindow(windowType);
                // UnityEditor.ProfilerWindow
#if UNITY_2019
                //?????????????????????
                FieldInfo allModuleFieldInfo = windowType.GetField("m_ProfilerModules",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                object allModule = allModuleFieldInfo?.GetValue(editorWindow);
                //?????? 13 ?????????
                int length = (int)allModule.GetType().GetProperty("Length", 
                    BindingFlags.Instance | BindingFlags.Public)?.GetValue(allModule);
                //???????????????????????????,?????????,???????????????,??????????????? UI ??????
                var actives = windowType.GetField("m_Charts",
                    BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(editorWindow);
                for (int i = length-1; i >=0; i--)
                {
                    object module = ((IList) allModule)[i];
                    if (module.ToString().Contains("UnityEditorInternal.Profiling.UIProfilerModule") || 
                        module.ToString().Contains("UnityEditorInternal.Profiling.UIDetailsProfilerModule"))
                    {
                        continue;
                    }
                    var chart = ((IList) actives)[i];
                    bool active = (bool)chart.GetType().GetProperty("active",
                        BindingFlags.Instance | BindingFlags.Public)?.GetValue(chart);
                    if (active)
                    {
                        var addAreaClickMethodInfo = windowType.GetMethod("AddAreaClick", BindingFlags.Instance | BindingFlags.NonPublic);
                        addAreaClickMethodInfo?.Invoke(editorWindow, new object[3] {null, null, i});
                    }
                }
#elif UNITY_2020_3_OR_NEWER
                
                object allModule = windowType.GetField("m_Modules",
                    BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(editorWindow);
                int count = (int)allModule.GetType().GetProperty("Count", 
                    BindingFlags.Instance | BindingFlags.Public)?.GetValue(allModule);

                MethodInfo deleteMethodInfo = windowType.GetMethod("DeleteProfilerModuleAtIndex", BindingFlags.Instance | BindingFlags.NonPublic);
                for (int i = count-1; i >=0; i--)
                {
                    object module = ((IList) allModule)[i];
                    if (module.ToString().Contains("UnityEditorInternal.Profiling.UIProfilerModule") || 
                        module.ToString().Contains("UnityEditorInternal.Profiling.UIDetailsProfilerModule"))
                    {
                        continue;
                    }
                    //??????????????????,????????? UI ??????
                    deleteMethodInfo?.Invoke(editorWindow, new object[] {i});
                }

#endif
            }
            
            if (EditorApplication.ExecuteMenuItem("Window/Analysis/Frame Debugger"))
            {
                var windowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.FrameDebuggerWindow");
                var editorWindow = EditorWindow.GetWindow(windowType);
                System.Func<Task> func = async () =>
                {
                    await Task.Delay(System.TimeSpan.FromSeconds(2));
                    var clickEnableFrameDebuggerMethodInfo = windowType.GetMethod("ClickEnableFrameDebugger", BindingFlags.Instance | BindingFlags.NonPublic);
                    clickEnableFrameDebuggerMethodInfo?.Invoke(editorWindow, null);
                    
                    // var changeFrameEventLimitMethodInfo = windowType.GetMethod("ChangeFrameEventLimit", BindingFlags.Instance | BindingFlags.NonPublic);
                    // changeFrameEventLimitMethodInfo?.Invoke(editorWindow, new object[]{4});
                    // await Task.Delay(System.TimeSpan.FromSeconds(1));
                    // FieldInfo pelFI = windowType.GetField("m_PrevEventsLimit",
                    //     BindingFlags.Instance | BindingFlags.NonPublic);
                    // Debug.Log(pelFI.GetValue(editorWindow));
                    //
                    // FieldInfo pecFI = windowType.GetField("m_PrevEventsCount",
                    //     BindingFlags.Instance | BindingFlags.NonPublic);
                    // Debug.Log(pecFI.GetValue(editorWindow));
                };
                func();
                
                
                // FrameDebuggerUtility.limit
                // "Canvas.RenderOverlays"
                // editorWindow
                // UnityEditorInternal.FrameEventType
                // UnityEditorInternal.FrameDebuggerEventData
                // UnityEditorInternal.FrameDebuggerEvent
                // UnityEditorInternal.FrameDebuggerTreeView
                // UnityEditor.FrameDebuggerWindow
            }
            EditorUtility.DisplayDialog("??????", "????????????????????????:\n1. Frame Debugger??????Canvas.RenderOverlays \n2. Profiler??????UI/UIDetails\n?????????: Alt???+???????????? ?????????????????????","??????");
        }

        #endregion

        #region ????????????

        public void ShowOptimizationSuggestions()
        {
            UIBatchesAnalyze.Analyze();
        }

        #endregion
        
        
        public static void RunInEditor()
        {
            UIAnalyze[] ds = GameObject.FindObjectsOfType<UIAnalyze>();
            if (ds.Length > 1)
            {
                for (int i = ds.Length - 1; i >= 1; i--)
                {
                    DestroyImmediate(ds[i].gameObject);
                }
            }
        
            if (UIAnalyze.Instance)
            {
                if (!UIAnalyze.Instance.name.Equals(UIDebugName))
                {
                    UIAnalyze.Instance.name = UIDebugName;
                }
        
                if (UIAnalyze.Instance.transform.parent != null)
                {
                    UIAnalyze.Instance.transform.parent = null;
                }
        
                return;
            }
        
            GameObject go = new GameObject(UIDebugName);
            Instance = go.AddComponent<UIAnalyze>();
            go.layer = LayerMask.NameToLayer("UI");
        }
        
    }

    
    
    [CustomEditor(typeof(UIAnalyze), true)]
    internal class UIDebugEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            UIAnalyze myScript = (UIAnalyze) target;
            
            if (GUILayout.Button("????????????SpriteAtlas?????????"))
            {
                myScript.PrintAllSpriteAtlasSettings();
            }
            
            if (GUILayout.Button("????????????Texture???Size??????"))
            {
                myScript.PrintAllSpriteSize();
            }
            
            if (GUILayout.Button("Wireframe ????????????????????????"))
            {
                //??????Graphic??????????????????,????????? canvas ???????????????????????????,?????????????????????
                myScript.ShowMeshOverlaps();
            }

            if (GUILayout.Button("Overdraw ????????????????????????"))
            {
                //??????Graphic??????????????????,????????? canvas ???????????????????????????,?????????????????????
                myScript.ShowUIOverdraw();
            }
            
            if (GUILayout.Button("Frame Debugger / Profiler ??????,??????"))
            {
                myScript.ShowFrameDebuggerAndProfiler();
            }
            
            if (GUILayout.Button("????????????"))
            {
                myScript.ShowOptimizationSuggestions();
            }
            
            //Scene??????,???????????????,??????RaycastTarget???UI??????,?????????????????????
            myScript.ShowRaycastTargetUI = GUILayout.Toggle(myScript.ShowRaycastTargetUI, "??????RaycastTarget???UI??????,Scene????????????????????????");
            //Console??????,?????????????????????,????????????UI???????????????Canvas??????,??????RectTransform??????????????????
            myScript.ShowReBuildUI = GUILayout.Toggle(myScript.ShowReBuildUI, "??????Canvas,????????????Graphic?????????");
            //????????????UI ????????? Z ???
            myScript.ShowAllUI_Z_Position = GUILayout.Toggle(myScript.ShowAllUI_Z_Position, "??????UI?????? Z ???,????????? 0");
        }
    }

}

#endif