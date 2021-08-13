using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UIKit
{
    
    //MVC 创建 Lua 脚本
    public static class UIModuleEditor
    {
        [MenuItem("GameObject/UI/模块/1. 创建一个Prefab模板", false, 1999)]
        [MenuItem("Assets/UI/模块/1. 创建一个Prefab模板", false, 1)]
        public static void CreateModule(MenuCommand menuCommand)
        {
            string path = "Assets/Resources/UI/PanelCanvas.prefab";
            var select = Selection.activeObject;
            var isPath = AssetDatabase.GetAssetPath(select);
            if (!string.IsNullOrEmpty(isPath) && !Path.HasExtension(isPath))
            {
                path = isPath + "/PanelCanvas.prefab";
            }
            var parent = new GameObject("Root", typeof(Canvas));

            var go = new GameObject("PanelCanvas", typeof(Canvas), typeof(GraphicRaycaster),typeof(UIKit.LuaModule))//
            {
                layer = LayerMask.NameToLayer("UI")
            };
            go.transform.SetParent(parent.transform);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.pivot = new Vector2(0.5f,0.5f);
            
            var canvas = go.GetComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingLayerName = "UI";

            var bg = DefaultControls.CreateRawImage(default);
            bg.transform.parent = go.transform;
            bg.name = "bg";
            bg.GetComponent<RawImage>().raycastTarget = false;
            var rectBg = bg.GetComponent<RectTransform>();
            rectBg.anchorMin = Vector2.zero;
            rectBg.anchorMax = Vector2.one;
            rectBg.sizeDelta = Vector2.zero;
            rectBg.pivot = new Vector2(0.5f,0.5f);
            
            var prefab = PrefabUtility.SaveAsPrefabAsset(go,path);
            Object.DestroyImmediate(go);
            Object.DestroyImmediate(parent);
            AssetDatabase.Refresh();
            AssetDatabase.OpenAsset(prefab);
            
            var windowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneView");
            var sceneView = EditorWindow.GetWindow(windowType) as UnityEditor.SceneView;
            if (sceneView == null)return;
            sceneView.drawGizmos = true;
            sceneView.in2DMode = true;
            sceneView.cameraMode = new SceneView.CameraMode()
            {
                drawMode=DrawCameraMode.Textured,
                name = "Shaded",
                section = "Shading Mode",
            };
        }
        
        [MenuItem("Assets/UI/模块/2. 模板-->MVC-->Lua",false,1)]
        public static void CreateOrUpdateLuaScripts()
        {
            var prefab = Selection.activeObject as GameObject;
            if (prefab == null || (!UnityEditor.EditorUtility.IsPersistent(prefab) && !UnityEditor.PrefabUtility.IsPartOfPrefabInstance(prefab)))
            {
                Debug.LogError("不是预制体");
                return;
            }
            //如果身上存在 Canvas 与 LuaModule 基本就是一个正常的UI模块
            var canvas = prefab.GetComponent<Canvas>();
            var luaModule = prefab.GetComponent<LuaModule>();
            if (!canvas || !luaModule)
            {
                Debug.LogError("不是UI模块");
                return;
            }
            //再判断此 prefab 的路径是否正常
            string prefabPath = AssetDatabase.GetAssetPath(prefab);
            prefabPath = prefabPath.Replace("\\", "/");
            var mPath = "Assets/Resources/UI/";
            if (!prefabPath.Contains(mPath))
            {
                Debug.LogError($"此 {prefabPath} 不在 {mPath} 文件夹下,请先放入UI 规定文件夹下再更新 Lua 脚本");
                return;
            }
            
            //以上完全确认选中的物体是一个 UI 下的 Prefab
            //需要根据此 prefab 创建或者更新 Lua 文件了
            //先搜索 lua 的文件夹下 是否已经存在了,如果不存在则直接创建

            var luaMainPath = Application.dataPath + "/Lua/UI/" + prefabPath.
                Replace(mPath,"").
                Replace(".prefab",".lua");//Lua 的完整路径
            
            var luaViewPath = Application.dataPath + "/Lua/UI/" + prefabPath.
                Replace(mPath,"").
                Replace(".prefab","_view.lua");//Lua view的完整路径
            
            var luaModelPath = Application.dataPath + "/Lua/UI/" + prefabPath.
                Replace(mPath,"").
                Replace(".prefab","_model.lua");//Lua model的完整路径
            
            var luaPreDirName = prefabPath.Replace(mPath, "").Replace("/" + prefab.name + ".prefab", "");//Lua 脚本的文件夹名字
            CreateDir(luaPreDirName);
            
            if (!File.Exists(luaMainPath))
            {
                var mainContent = CreateLuaMainContent(luaMainPath,prefab,luaPreDirName);
                File.WriteAllText(luaMainPath,mainContent,Encoding.UTF8);
            }

            if (!File.Exists(luaViewPath))
            {
                var mainContent = CreateLuaViewContent(luaViewPath,prefab,luaPreDirName);
                File.WriteAllText(luaViewPath,mainContent,Encoding.UTF8);
            }

            if (!File.Exists(luaModelPath))
            {
                var mainContent = CreateLuaModelContent(luaModelPath,prefab,luaPreDirName);
                File.WriteAllText(luaModelPath,mainContent,Encoding.UTF8);
            }

            AssetDatabase.Refresh();
        }


        private static string CreateLuaMainContent(string luaPath, GameObject prefab ,string luaPreDirName)
        {
            var luaSB = new StringBuilder();
            luaSB.Append("\n\n\n");
            luaSB.Append($"local {prefab.name} = class(\"{prefab.name}\",UIController)");
            luaSB.Append("\n");
            luaSB.Append($"local this = {prefab.name}");
            luaSB.Append("\n\n\n");
            luaSB.Append("-------------------------------------变量-----------------------------------\n");
            luaSB.Append("--声明 local xxx = nil\n");

            luaSB.Append("\n\n\n");
            luaSB.Append("-------------------------------------方法-----------------------------------\n");
            luaSB.Append("--声明 local xxx = nil 此方法是私有方法\n");
            
            luaSB.Append("\n\n\n");

            luaSB.Append("function this:Awake()\n");
            luaSB.Append($"    this.view  = require(\"UI/{luaPreDirName}/{prefab.name}_view\")\n");
            luaSB.Append($"    this.model = require(\"UI/{luaPreDirName}/{prefab.name}_model\")\n");
            luaSB.Append("end\n");
            luaSB.Append("\n\n\n");

            luaSB.Append("function this:OnEnable()\n");
            luaSB.Append("    self:refresh()\n");
            luaSB.Append("end\n");
            
            luaSB.Append("\n\n\n");

            
            luaSB.Append("function this:Start()\n");
            luaSB.Append("    self:initialize()\n");
            luaSB.Append("end\n");
            
            luaSB.Append("\n\n\n");

            luaSB.Append("function this:OnDisable()\n");
            luaSB.Append("    self:reset()\n");
            luaSB.Append("end\n");
            
            luaSB.Append("\n\n\n");
            

            luaSB.Append("function this:OnDestroy()\n");
            luaSB.Append("    self:destroy()\n");
            luaSB.Append("end\n");
            
            luaSB.Append("\n\n\n");
            luaSB.Append("-------------------------------------public公开方法-----------------------------------\n");
            luaSB.Append("--使用 function this.xxx()  end 方式编写,外部调用也以此方式\n");

            luaSB.Append("\n\n\n");
            luaSB.Append("-------------------------------------private方法-----------------------------------\n");
            luaSB.Append("--在头部先声明私有方法变量,然后在这个地方使用 function xxx()  end\n");

            luaSB.Append("\n\n\n");

            luaSB.Append("return this");
            return luaSB.ToString();
        }
         
        private static string CreateLuaViewContent(string luaPath, GameObject prefab ,string luaPreDirName)
        {
            var luaSB = new StringBuilder();
            //子节点,只包括最外层,里层不创建,并且最外层 bg 不创建
            var listView = new List<Transform>();
            for (int i = 0; i < prefab.transform.childCount; i++)
            {
                if ( i==0 && prefab.transform.GetChild(i).name == "bg") continue; //如果第一个对象是 bg 则不进行自动化处理
                listView.Add(prefab.transform.GetChild(i));
            }
            
            luaSB.Append("\n\n\n");
            luaSB.Append($"local {prefab.name}_view = class(\"{prefab.name}_view\",UIView)");
            luaSB.Append("\n");
            luaSB.Append($"local this = {prefab.name}_view");
            luaSB.Append("\n\n\n");
            luaSB.Append("-------------------------------------变量-----------------------------------\n");
            luaSB.Append("--声明 local xxx = nil\n");
            luaSB.Append("local _controller = nil\n");
            luaSB.Append("\n\n");

            luaSB.Append("--View 变量对象\n");
            foreach (var item in listView)
            {
                string varName = ReName(item.name);
                luaSB.Append($"local _{varName} = nil\n");
            }
            
            luaSB.Append("\n\n\n");
            luaSB.Append("-------------------------------------方法-----------------------------------\n");
            luaSB.Append("--声明 local xxx = nil 此方法是私有方法\n");
            
            foreach (var item in listView)
            {
                var rTableView = item.GetComponent<UITableView>();
                if (rTableView!=null)
                {
                    luaSB.Append($"local _updateCellData{listView.IndexOf(item)} = nil -- 在此方法里面刷新 cell 的数据\n");
                }
            }
            
            luaSB.Append("\n\n\n");
            
            luaSB.Append("function this:initView()\n");
            luaSB.Append("    --获取控制器\n");
            luaSB.Append($"    _controller = lua_data_center.Get(\"{prefab.name}\")\n");
            luaSB.Append("    --获取具体的 UI 节点\n");
            
            foreach (var item in listView)
            {
                string varName = ReName(item.name);

                var rText = item.GetComponent<TMPro.TextMeshProUGUI>();
                if (rText!=null)
                {
                    luaSB.Append($"    _{varName} = child(_controller.transform,\"{item.name}\"):GetComponent(typeof(TextMeshProUGUI))\n");
                    continue;
                }
                
                var rInput = item.GetComponent<TMPro.TMP_InputField>();
                if (rInput!=null)
                {
                    luaSB.Append($"    _{varName} = child(_controller.transform,\"{item.name}\"):GetComponent(typeof(TMP_InputField))\n");
                    continue;
                }
                
                var rDropdown = item.GetComponent<TMPro.TMP_Dropdown>();
                if (rDropdown!=null)
                {
                    luaSB.Append($"    _{varName} = child(_controller.transform,\"{item.name}\"):GetComponent(typeof(TMP_Dropdown))\n");
                    continue;
                }
                
                var rTableView = item.GetComponent<UITableView>();
                if (rTableView!=null)
                {
                    luaSB.Append($"    _{varName} = child(_controller.transform,\"{item.name}\"):GetComponent(typeof(UITableView))\n");
                    continue;
                }
                
                var rToggle = item.GetComponent<Toggle>();
                if (rToggle!=null)
                {
                    luaSB.Append($"    _{varName} = child(_controller.transform,\"{item.name}\"):GetComponent(typeof(Toggle))\n");
                    continue;
                }
                
                var image = item.GetComponent<Image>();
                if (image!=null)
                {
                    luaSB.Append($"    _{varName} = child(_controller.transform,\"{item.name}\"):GetComponent(typeof(Image))\n");
                    continue;
                }
                var rImage = item.GetComponent<RawImage>();
                if (rImage!=null)
                {
                    luaSB.Append($"    _{varName} = child(_controller.transform,\"{item.name}\"):GetComponent(typeof(RawImage))\n");
                    continue;
                }
                luaSB.Append($"    _{varName} = child(_controller.transform,\"{item.name}\")\n");
            }
            
            
            luaSB.Append("\n\n\n");
            luaSB.Append("    --添加事件 addClick(xxx,xxx)\n");

            
            luaSB.Append("\n\n\n");
            luaSB.Append("    --适配 adapter(xxx)\n");

            luaSB.Append("\n\n\n");
            luaSB.Append("    --初始逻辑 SetScale(xxx,false/true)\n");
            
            luaSB.Append("\n\n\n");
            luaSB.Append("    this:refreshView()\n");
            luaSB.Append("end\n");
            
            
            luaSB.Append("\n\n\n");
            luaSB.Append("--刷新数据\n");
            luaSB.Append("function this:refreshView()\n\n");
            luaSB.Append("\n\n\n");

            foreach (var item in listView)
            {
                string varName = ReName(item.name);
                var rTableView = item.GetComponent<UITableView>();
                if (rTableView == null) continue;
                luaSB.Append($"   _{varName}.QueryCellTemplateId = function() return \"CellTemplate\" end  -- 设置UITableView刷新的方法\n");
                luaSB.Append($"   _{varName}.UpdateCellData = _updateCellData{listView.IndexOf(item)}  -- 设置UITableView刷新的方法\n");
                luaSB.Append($"   _{varName}.CellTotalCount = 0  -- 设置UITableView的子 cell 总数\n");
                luaSB.Append($"   _{varName}:RefillCells() -- 充满整个UITableView\n");
                luaSB.Append($"   _{varName}:RefreshCells() -- 刷新之前必须要有方法\n");
            }
            luaSB.Append("end\n");
            
            
            luaSB.Append("\n\n\n");
            luaSB.Append("--重置View\n");
            luaSB.Append("function this:resetView()\n\n");
            luaSB.Append("end\n");
            
            luaSB.Append("\n\n\n");
            luaSB.Append("--界面销毁\n");
            luaSB.Append("function this:destroyView()\n\n");
            luaSB.Append("end\n");
            
            luaSB.Append("\n\n\n");
            luaSB.Append("-------------------------------------public公开方法-----------------------------------\n");
            luaSB.Append("--使用 function this.xxx()  end 方式编写,外部调用使用 xxx.view.xxx() 此方式\n");

            
            luaSB.Append("\n\n\n");
            luaSB.Append("-------------------------------------private方法-----------------------------------\n");
            luaSB.Append("--在头部先声明私有方法变量,然后在这个地方使用 function xxx()  end\n");

            
            luaSB.Append("\n\n\n");
            foreach (var item in listView)
            {
                var rTableView = item.GetComponent<UITableView>();
                if (rTableView == null) continue;
                luaSB.Append($"function _updateCellData{listView.IndexOf(item)}(index,cell)\n");
                luaSB.Append($"    local cIndex = index + 1\n");
                luaSB.Append($"    local data = nil\n");
                luaSB.Append("end\n");
            }
            luaSB.Append("\n\n\n");
            
            luaSB.Append("return this");
            return luaSB.ToString();
        }
        
        private static string CreateLuaModelContent(string luaPath, GameObject prefab ,string luaPreDirName)
        {
            var luaSB = new StringBuilder();
            luaSB.Append("\n\n\n");
            luaSB.Append($"local {prefab.name}_model = class(\"{prefab.name}_model\",UIModel)");
            luaSB.Append("\n");
            luaSB.Append($"local this = {prefab.name}_model");
            luaSB.Append("\n\n\n");
            luaSB.Append("-------------------------------------变量-----------------------------------\n");
            luaSB.Append("--声明 local xxx = nil\n");
            luaSB.Append("local _controller = nil\n");

            luaSB.Append("\n\n\n");
            luaSB.Append("-------------------------------------方法-----------------------------------\n");
            luaSB.Append("--声明 local xxx = nil 此方法是私有方法\n");
            
            luaSB.Append("\n\n\n");
            
            luaSB.Append("function this:initModel()\n");
            luaSB.Append("    --获取控制器\n");
            luaSB.Append($"    _controller = lua_data_center.Get(\"{prefab.name}\")\n");
            luaSB.Append("    this:refreshModel()\n");
            luaSB.Append("end\n");
            
            
            luaSB.Append("\n\n\n");
            luaSB.Append("--刷新网络数据\n");
            luaSB.Append("function this:refreshModel()\n\n");
            luaSB.Append("end\n");
            
            
            luaSB.Append("\n\n\n");
            luaSB.Append("--重置数据\n");
            luaSB.Append("function this:resetModel()\n\n");
            luaSB.Append("end\n");
            
            luaSB.Append("\n\n\n");
            luaSB.Append("--数据模块销毁\n");
            luaSB.Append("function this:destroyView()\n\n");
            luaSB.Append("    this.data = nil\n\n");
            luaSB.Append("    _controller = nil\n\n");
            luaSB.Append("end\n");
            
            luaSB.Append("\n\n\n");
            luaSB.Append("-------------------------------------public公开方法-----------------------------------\n");
            luaSB.Append("--使用 function this.xxx()  end 方式编写,外部调用使用 xxx.view.xxx() 此方式\n");

            
            luaSB.Append("\n\n\n");
            luaSB.Append("-------------------------------------private方法-----------------------------------\n");
            luaSB.Append("--在头部先声明私有方法变量,然后在这个地方使用 function xxx()  end\n");

            luaSB.Append("\n\n\n");
            
            luaSB.Append("return this");
            return luaSB.ToString();
        }

        //创建文件夹
        private static void CreateDir(string luaPreDirName)
        {
            string p = Application.dataPath + "/Lua/UI/" + luaPreDirName;
            if (!Directory.Exists(p))
            {
                Directory.CreateDirectory(p);
            }
            AssetDatabase.Refresh();
        }

        private static string ReName(string s)
        {
            var sb = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                bool upper = CompareChar(c);//是否为大写,如果是大写,则替换为下划线+小写
                if (upper)
                {
                    if (i!=0)
                    {
                        sb.Append('_' + char.ToLower(c).ToString());
                    }
                    else
                    {
                        sb.Append(char.ToLower(c).ToString());
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
        
        /// <summary>
        /// 判断字符是否为大写字母
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>       
        private static bool CompareChar(char c)
        {
            return c > 'A' && c < 'Z';
        }
        
    }
}