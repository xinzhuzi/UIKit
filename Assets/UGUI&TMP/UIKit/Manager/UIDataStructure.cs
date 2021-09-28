using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIKit
{
    /// <summary>
    /// 这个地方设计所有 UI 的数据结构
    /// id 表示唯一标志性 id,也可以理解为 obj 的名字,当前 UI 控件/模块
    /// </summary>
    public partial class UIManager 
    {
        //UIManager下面所有的子节点,不包含深度子节点
        private DoubleMap<string, GameObject> _children;
        private Dictionary<string, UIModuleData> _allData;//为了附加信息

        //记录用户打开关闭的 id 顺序,用户可能随机关闭,也可能随机打开,只在 open 和 close 代码里面进行记录.
        private Dictionary<string,List<string>> _moduleOptCull;
        
        
        private GameObject AddLoad(string id)
        {
            //都不包含时,检查 Resources 文件夹下是否包含
            var template = Resources.Load<GameObject>(id);
            if (null == template) //如果在 Resources 文件夹下没有这个,才去加载 ab 包
            {
                template = UIHelper.Load(id) as GameObject;
            }
            //根据模板创建实例
            var module = GameObject.Instantiate(template, this.transform);
            module.GetComponent<RectTransform>().localScale = Vector3.one;
            return module;
        }
        
        
        //销毁某个 UI,不管是否在池子里面,返回 false 的含义:1.有可能没在 UI 节点下面,2.也许没有删掉,多删除一次等等.
        //一般情况下不使用判断写法,直接调用方法即可.
        public bool Destroy(string id)
        {
            if (_pool.Destroy(id)) return true;
            //如果没有在池子里面
            var go = _children.GetValueByKey(id);
            if (null == go)return false;
            _children.RemoveByKey(id);
            DestroyImmediate(go);
            _allData[id].Go = null;
            return true;
        }
        
        
        //清除 UIRoot 下面的所有 UI
        public void Destroy(bool isDestroyPoolChild = true, bool unload = false)
        {
            if (isDestroyPoolChild)
            {
                _pool.DestroyAll(unload);
            }
            
            //倒序删除
            for (var i = m_UIRoot.transform.childCount - 1; i >= 0; i--)
            {
                var go = m_UIRoot.transform.GetChild(i).gameObject;
                if (ReferenceEquals(go,_pool.gameObject)) continue;//UIPoolManager 不能删除
                if (ReferenceEquals(go,UICamera.gameObject)) continue;//UICamera 不能删除
                var id = _children.GetKeyByValue(go);
                var data = _allData[id];
                if (data.SortingOrder > SortingOrderBoundary) continue;//固定的 order UI 无需删除
                _children.RemoveByKey(data.Id);
                DestroyImmediate(go);
                if (unload) UIHelper.Unload(data.Id);
                data.Go = null;
            }
            ClearOptCullUI();
        }
        
        
        //将子UI模块移动到池子里面进行隐藏
        public void Close(string id)
        {
            var go = _children.GetValueByKey(id);
            if (null == go)return ;//当前堆栈里面没有这个模块
            _pool.Add(id,go);
            _children.RemoveByKey(id);

            //处理数据UIModuleData,这个地方的数据无需清除,因为只是数据,没有排序功能
            if (!_allData.TryGetValue(id, out var data)) return;
            if (data.OptCullUI)
            {
                AutoOptCullUI(id,false);
            }
        }

        //查询某个子UI,将 UI 展示出来
        public GameObject Open(string id)
        {
            var module = _children.GetValueByKey(id);
            if (null != module)
            {
                module.transform.SetAsLastSibling();
                AutoIncrementLayer();
                return module;
            }
            
            //查看 缓存池子里面是否有,有则拿出来
            
            module = _pool.Query(id);
            if (null == module)
            {
                module = AddLoad(id);//本身没有,缓存没有,则从本地加载 UI
            }
            
            {//将数据填入,先从_cache 中拿到数据,如果没有,就从池子里面拿
                if (!_allData.TryGetValue(id,out var data))
                {
                    data = UIModuleData.Clone();
                }
                data.Id = id;
                data.Go = module;
                data.SortingOrder = data.SortingOrder;//重新赋值一次,防止某些值在之前是没有设置的
                _children.Add(id,module);
                _allData[id] = data;
                if (data.OptCullUI)
                {
                    AutoOptCullUI(id,true);
                }
            }
            AutoIncrementLayer();
            return module;
        }
        
        //查询某个子UI
        public GameObject Query(string id)
        {
            var module = _children.GetValueByKey(id);
            return module ? module : _pool.Query(id);
        }
    }
}