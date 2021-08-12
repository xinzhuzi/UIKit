using System;
using UnityEngine;

namespace UIKit
{
    public sealed class UIManager : MonoBehaviour
    {
        public static UIManager Instance => m_Instance;
        public static GameObject UIRoot => m_UIRoot;
        public static Camera UICamera => m_UICamera;
        
        private static UIManager m_Instance;
        private static GameObject m_UIRoot;
        private static Camera m_UICamera;
        
        
        //仅当前显示的界面 UI,包括 子canvas 与 子控件,不包括UIPoolManager以及子节点
        private DoubleMap<string, GameObject> _children;
        
        private UIPoolManager _pool;
        private void Awake()
        {
            m_Instance = this;
            m_UICamera = GameObject.FindWithTag("UICamera").GetComponent<Camera>();
            _children = new DoubleMap<string, GameObject>(20);
            _pool = GetComponentInChildren<UIPoolManager>();
#if UNITY_EDITOR
            UIAnalyze.RunInEditor();
#endif
        }

        public GameObject AddLoad(string id)
        {
            //先检查当前子节点当中是否包含
            var go = _children.GetValueByKey(id);
            if (null != go) return go;
            //再检查缓存池子中当中是否包含
            go = _pool.Query(id);
            if (null != go) return go;

            //都不包含时,检查 Resources 文件夹下是否包含
            var template = Resources.Load<GameObject>(id);
            if (null == template) //如果在 Resources 文件夹下没有这个,才去加载 ab 包
            {
                template = UIHelper.Load(id) as GameObject;
            }
            //根据模板创建实例
            var module = GameObject.Instantiate(template, this.transform);
            _children.Add(id,module);
            module.GetComponent<RectTransform>().localScale = Vector3.one;
            return module;
        }
        
        #region UI 辅助操作,id 表示唯一标志性 id,也可以理解为 obj 的名字,当前 UI 控件

        //销毁某个 UI,不管是否在池子里面,返回 false 的含义:1.有可能没在 UI 节点下面,2.也许没有删掉,多删除一次等等.
        //一般情况下不使用判断写法,直接调用方法即可.
        public bool Destroy(string id)
        {
            if (_pool.Destroy(id))
            {
                return true;
            }
            //如果没有在池子里面
            var go = _children.GetValueByKey(id);
            if (null == go) return false;
            DestroyImmediate(go);
            return _children.RemoveByKey(id);
        }

        //清除 UIRoot 下面的所有 UI
        public void Destroy(bool isDestroyPoolChild = true)
        {
            if (isDestroyPoolChild)
            {
                _pool.DestroyAll();
            }
            
            //倒序删除
            for (var i = m_UIRoot.transform.childCount - 1; i >= 0; i--)
            {
                var t = m_UIRoot.transform.GetChild(i);
                if (ReferenceEquals(t,_pool.transform))
                {
                    continue;//UIPoolManager 不能删除
                }
                
                DestroyImmediate(t.gameObject);
            }
        }


        //查询某个子UI
        public GameObject Query(string id)
        {
            var go = _children.GetValueByKey(id);
            return null != go ? go : _pool.Query(id);
        }

        public bool IsOpen(string id)
        {
            return _children.ContainsKey(id);
        }
        
        public bool IsOpen(GameObject go)
        {
            return _children.ContainsValue(go);
        }
        
        public void Open(GameObject go)
        {
            go.layer = LayerMask.NameToLayer("UI");
            var id = go.name.Replace("(Clone)", "");
            if (_children.ContainsKey(id) || _children.ContainsValue(go))
            {
                throw new Exception("UIRoot 已经存在了传入的对象或者存在相同的名字:" + id);
            }
            go.transform.SetParent(this.transform,false);
            go.GetComponent<RectTransform>().localScale = Vector3.one;
            _children.Add(id,go);
            AutoIncrementLayer();
        }

        //查询某个子UI,将 UI 展示出来
        public GameObject Open(string id)
        {
            var go = _children.GetValueByKey(id);
            if (null != go)//如果当前已经打开了,则不需要再次打开
            {
                return go;
            }
            
            //查看 缓存池子里面是否有,有则拿出来
            go = _pool.Query(id);
            if (null != go)
            {
                //打开的时候,必须将缩放设置为 1,其他布局方式由使用者操作
                _children.Add(id,go);
            }
            else
            {
                go = AddLoad(id);//本身没有,缓存没有,则从本地加载 UI
            }

            AutoIncrementLayer();
            return go;
        }
        
        //将子UI模块移动到池子里面进行隐藏
        public void Close(string id)
        {
            var go = _children.GetValueByKey(id);
            if (null == go) return;
            //如果当前已经打开了,则不需要再次打开
            _pool.Add(id,go);
            _children.RemoveByValue(go);
        }
        
        public void Close(GameObject go)
        {
            if (_children.ContainsValue(go))
            {
                var goName = _children.GetKeyByValue(go);
                _pool.Add(goName,go);
                _children.RemoveByKey(goName);
            }
            else
            {
                Debug.LogError("不是 UIRoot 下的子节点,请不要使用此方式销毁");
            }
        }

        private void AutoIncrementLayer()
        {
            var i = 10;
            foreach (var item in _children.ForEachValues())
            {
                i++;
                var canvas = item.GetComponent<Canvas>();//没有 canvas 的不能进行自增 order
                if (canvas == null) continue;
                canvas.sortingOrder = item.CompareTag("GM") ? 30000 : i;
            }
        }
        
        #endregion
        
        
        //自动入口,目前不使用此方法,外部调用的第一个方法
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void RunGUIManager()
        {
            m_UIRoot = GameObject.Instantiate(Resources.Load<GameObject>("UIRoot"));
            GameObject.DontDestroyOnLoad(m_UIRoot);
        }
    }
}