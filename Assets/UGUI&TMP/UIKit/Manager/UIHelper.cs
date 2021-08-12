using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.U2D;

namespace UIKit
{
    public static class UIHelper
    {
        #region 加载
       
        public const string PrefabPath = "Prefabs/UI/";
        
        //Load 完毕之后还是一个解压的内存块,类似一个模板,不能直接使用,是被缓存的,需要实例化.
        public static UnityEngine.Object Load(string path)
        {
            return Resources.Load(path);
        }

        //创建是在 Load 之后进行实例化,可以直接使用,需要使用 UIManager.Instance:Open("xxx");才能正常显示
        public static UnityEngine.Object Create(string path)
        {
            return UnityEngine.Object.Instantiate(Resources.Load(path));
        }

        public static void LoadAsync(string path, Action<UnityEngine.Object> action)
        {
            
        }


        //创建是在 Load 之后进行实例化,可以直接使用,需要使用 UIManager.Instance:Open("xxx");才能正常显示
        public static void CreateAsync(string path, Action<UnityEngine.GameObject> action)
        {
           
        }

        public static Sprite GetSprite(string spriteAtlasName, string spriteName)
        {
            return Resources.Load<SpriteAtlas>(spriteAtlasName).GetSprite(spriteName);
        }
        
        public static void GetSpriteAsync(string spriteAtlasName, string spriteName, Action<Sprite> action)
        {
            
        }
        
        #endregion

        #region 给父节点添加一个子节点
        
        public static GameObject AddChild(Transform parent, GameObject child)
        {
            var go = UnityEngine.Object.Instantiate(child, parent.transform, false);
            var t = go.GetComponent<RectTransform>();
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
            return go;
        }

        public static GameObject AddChild(Transform parent, Transform child)
        {
            return AddChild(parent, child.gameObject);
        }

        public static GameObject AddChild(GameObject parent, Transform child)
        {
            return AddChild(parent.transform, child.gameObject);
        }

        public static GameObject AddChild(GameObject parent, GameObject child)
        {
            return AddChild(parent.transform, child);
        }


        #endregion


        #region 纯工具

        public static void SetScale(GameObject go , bool show = false)
        {
            go.GetComponent<RectTransform>().localScale = show ? Vector3.one : Vector3.zero;
        }

        public static void SetScale(RectTransform rt , bool show = false)
        {
            rt.localScale = show ? Vector3.one : Vector3.zero;
        }
        
        public static void SetLocalPositionZero(RectTransform rt)
        {
            rt.localPosition = Vector3.zero;
        }

        public static string Clipboard
        {
            get
            {
                var te = new TextEditor();
                te.Paste();
                return te.text;
            }
            set
            {
                var te = new TextEditor {text = value};
                te.OnFocus();
                te.Copy();
            }
        }

        public static Vector2 GetWidthHeight(RectTransform rt)
        {
            return rt.rect.size;
        }
        
        public static float GetWidth(RectTransform rt)
        {
            return rt.rect.width;
        }
        
        public static float GetHeight(RectTransform rt)
        {
            return rt.rect.height;
        }

        #endregion
        
        
        
        #region 坐标计算

        #region 世界坐标->屏幕坐标

        /// <summary>
        /// 世界坐标转屏幕坐标
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        public static Vector2 WorldToScreenPoint(Camera camera, Vector3 worldPosition)
        {
            return RectTransformUtility.WorldToScreenPoint(camera, worldPosition);
        }

        #endregion

        #region 屏幕坐标->世界坐标

        /// <summary>
        /// 屏幕坐标转世界坐标
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="screenPoint"></param>
        /// <param name="cam"></param>
        /// <param name="worldPoint"></param>
        /// <returns></returns>
        public static bool ScreenPointToWorldPointInRectangle(RectTransform rect, Vector2 screenPoint, Camera cam, out Vector3 worldPoint)
        {
            return RectTransformUtility.ScreenPointToWorldPointInRectangle(rect, screenPoint, cam, out worldPoint);
        }

        #endregion

        #region 屏幕坐标->UGUI坐标
        
        /// <summary>
        /// 屏幕坐标转某个RectTransform下的localPosition坐标
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="screenPoint"></param>
        /// <param name="cam"></param>
        /// <param name="localPoint"></param>
        /// <returns></returns>
        public static bool ScreenPointToLocalPointInRectangle(RectTransform rect, Vector2 screenPoint, Camera cam, out Vector2 localPoint)
        {
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPoint, cam, out localPoint);
        }

        #endregion


        #region 纯手动计算,大量计算适配时做缓存计算
        
        /// <summary>
        /// 将一个旧坐标系内的 Position 经过转换,转到另外一个坐标系的方法,专门为适配做的优化方法
        /// 世界坐标--->屏幕坐标-->UGUI 坐标
        /// </summary>
        /// <param name="camera">摄像机</param>
        /// <param name="rectTransform">新的坐标系</param>
        /// <param name="position">旧的坐标系内的点</param>
        /// <returns></returns>
        public static bool WorldPositionToNewCoordinates(Camera camera,RectTransform rectTransform ,Vector3 worldPosition, out Vector2 newPosition)
        {
            var screenPoint = camera.WorldToScreenPoint(worldPosition);
            newPosition = Vector2.zero;
            var ray = camera.ScreenPointToRay(screenPoint);
            var normal = Vector3.Normalize(rectTransform.rotation * Vector3.back);
            var distance = -Vector3.Dot(normal, rectTransform.position);
            var a = Vector3.Dot(ray.direction, normal);
            if (Mathf.Approximately(a, 0.0f)) return false;
            var num = -Vector3.Dot(ray.origin, normal) - distance;
            var enter = num / a;
            if (enter <= 0.0) return false;
            var worldPoint = ray.origin + enter * ray.direction;
            newPosition = (Vector2) rectTransform.InverseTransformPoint(worldPoint);
            return true;
        }
        /**
         * 上面的方法,仅仅是将相面的方法展开而已,在此做记录
         *
         *
         * if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_screenRectTransform,
                UICamera.WorldToScreenPoint(graphic.transform.position),
                UICamera, out var screenRTLocalPos)) return;
         */

        #endregion
        #endregion

//         //每一个 lua 脚本的完整路径
//         private static readonly Dictionary<string, string> UIPrefabPath = new Dictionary<string, string>(500);
//
//         public static void AddUIPrefabPath()
//         {
//             UIPrefabPath.Clear();
//             string path = 
// #if UNITY_EDITOR
//                 Application.dataPath + "/lua/UI";
// #else    
//                 LuaConst.luaResDir + "/UI";
// #endif
//             GetFile(path);
//         }
//         
//         /// <summary>  
//         /// 获取路径下所有文件以及子文件夹中文件  
//         /// </summary>  
//         /// <param name="path">全路径根目录</param>  
//         /// <param name="FileList">存放所有文件的全路径</param>  
//         /// <param name="RelativePath"></param>  
//         /// <returns></returns>  
//         public static void GetFile(string path)  
//         {
//             DirectoryInfo dir = new DirectoryInfo(path);
//             foreach (FileInfo f in dir.GetFiles())
//             {
//                 if (f.Name.EndsWith(".meta")) continue;
//                 var key = Path.GetFileNameWithoutExtension(f.Name);
//                 var value = f.FullName.Replace(Path.GetExtension(f.FullName), "");
//                 UIPrefabPath.Add(key, value);
//             }
//             //获取子文件夹内的文件列表，递归遍历  
//             foreach (DirectoryInfo d in dir.GetDirectories())  
//             {  
//                 GetFile(d.FullName);  
//             }  
//         }
    }
}