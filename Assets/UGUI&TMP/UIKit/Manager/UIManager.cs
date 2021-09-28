using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIKit
{
    public partial class UIManager : MonoBehaviour
    {
        public static UIManager Instance => m_Instance;
        public static GameObject UIRoot => m_UIRoot;
        public static Camera UICamera => m_UICamera;
        
        
        public const string UICameraName = "UICamera";
        public const string Adapter_Pool_Name = "Adapter_Pool";
        public const int SortingOrderBoundary = 20000;

        //TODO:主要是为了和 NGUI 区分,在 NGUI 干掉之后,这个参数也需要干掉
        public static bool IsRunUGUI;
        
        private static UIManager m_Instance;
        private static GameObject m_UIRoot;
        private static Camera m_UICamera;
        private UIPoolManager _pool;

        private void Awake()
        {
            m_Instance = this;
            _children = new DoubleMap<string, GameObject>(20);
            _allData = new Dictionary<string, UIModuleData>(20);
            _moduleOptCull = new Dictionary<string, List<string>>(20);
            //这 2 个是固定的.
            {//摄像机
                var data = UIModuleData.Clone();
                data.Id = UICameraName;
                data.Go = GameObject.FindWithTag(data.Id);
                data.SortingOrder = 0;
                m_UICamera = data.Go.GetComponent<Camera>();
                _children.Add(data.Id,data.Go);
                _allData.Add(data.Id,data);
            }

            {//池子与适配
                _pool = GetComponentInChildren<UIPoolManager>();
                var data = UIModuleData.Clone();
                data.Id = Adapter_Pool_Name;
                data.Go = _pool.gameObject;
                data.SortingOrder = 1;
                _children.Add(data.Id,data.Go);
                _allData.Add(data.Id,data);
            }
            
#if UNITY_EDITOR
            UIAnalyze.RunInEditor();
#endif
        }


        
        public bool IsOpen(string id)
        {
            return _children.ContainsKey(id);
        }

        // 更新当前 canvas 的 SortingOrder,数据从 lua 中传入,当成一种配置表使用
        public void UpdateFixedSortingOrder(string id,int sortingOrder)
        {
            if (!_allData.TryGetValue(id,out var data))//得到 UIModuleData
            {
                data = UIModuleData.Clone();
                data.Id = id;
                data.SortingOrder = sortingOrder;
            }
            data.SortingOrder = sortingOrder;
            _allData[id] = data;
            AutoIncrementLayer();
        }

        private void AutoIncrementLayer()
        {
            for (var i = 0; i < this.transform.childCount; i++)
            {
                var go = this.transform.GetChild(i).gameObject;
                var id = _children.GetKeyByValue(go);
                var data = _allData[id];
                if (data.SortingOrder > SortingOrderBoundary) continue;
                data.SortingOrder = i;
            }
        }
        
        /// <summary>
        /// 某个模块,打开时是否关闭后面的所有 UI 模块
        /// 此数据是设置某个UI 模块优化,当打开这个 UI 模块时,需要将其隐藏的 UI 模块全部关闭,关闭这个 UI 模块时,再将其隐藏的 UI 模块内全部打开.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="opt"></param>
        public void UpdateOptCullUI(string id,bool optCullUI)
        {
            if (!_allData.TryGetValue(id,out var data))//得到 UIModuleData
            {
                data = UIModuleData.Clone();
                data.Id = id;
            }
            data.OptCullUI = optCullUI;
            _allData[id] = data;
        }
        
        //清空优化的栈,使其再次打开时,不用打开其背后的UI
        public void ClearOptCullUI(string id)
        {
            if (!_moduleOptCull.TryGetValue(id,out var optCullUIOrder)) return;
            optCullUIOrder.Clear();
            _moduleOptCull[id] = optCullUIOrder;
        }
        
        //清空优化的栈
        private void ClearOptCullUI()
        {
            foreach (var id in new List<string>(_moduleOptCull.Keys))
            {
                if (!_moduleOptCull.TryGetValue(id,out var optCullUIOrder)) continue;
                optCullUIOrder.Clear();
                _moduleOptCull[id] = optCullUIOrder;
            }
        }
        
        private void AutoOptCullUI(string id, bool openOrClose)
        {
            if (openOrClose) //打开的情况
            {
                if (!_moduleOptCull.TryGetValue(id,out var optCullUIOrder))
                {
                    optCullUIOrder = new List<string>(10);
                }
                optCullUIOrder.Clear();
                for (var i = 0; i < this.transform.childCount; i++)
                {
                    var go = this.transform.GetChild(i).gameObject;
                    var data = _allData[_children.GetKeyByValue(go)];
                    if (data.Id == UICameraName || 
                        data.Id == Adapter_Pool_Name || 
                        data.SortingOrder > SortingOrderBoundary || 
                        data.OptCullUI) continue;
                    optCullUIOrder.Add(data.Id);
                }
                _moduleOptCull[id] = optCullUIOrder;
                foreach (var item in optCullUIOrder)
                {
                    Close(item);//全部被当前 id 模块隐藏的UI 模块,需要关闭
                }
            }
            else //关闭的情况
            {
                var optCullUIOrder = _moduleOptCull[id];
                foreach (var item in optCullUIOrder)
                {
                    Open(item);//全部被当前 id 模块隐藏的UI 模块,需要关闭
                }
                optCullUIOrder.Clear();
                _moduleOptCull[id] = optCullUIOrder;
            }
        }
        
        //自动入口,目前不使用此方法,外部调用的第一个方法
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RunGUIManager()
        {
            m_UIRoot = GameObject.Instantiate(Resources.Load<GameObject>("UIRoot"));
            GameObject.DontDestroyOnLoad(m_UIRoot);
        }
    }
}