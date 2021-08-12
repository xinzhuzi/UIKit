using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIKit
{
    /// <summary>
    /// UI 缓存池子
    /// 将目前不使用的 UI 缓存放入 UIPool 下面,内存仍然存在,但是界面上面不显示了
    /// 避免重新创建时耗费CPU 时间,以及 UI 重建,重建合批流程等等.
    /// </summary>
    public class UIPoolManager : MonoBehaviour
    {
        private static int _uiLayer;

        private DoubleMap<string, GameObject> _cache;
        private int _capacity = 10;
        private void Awake()
        {
            _uiLayer = LayerMask.NameToLayer("UI");
            this.gameObject.layer = LayerMask.NameToLayer("NoGraphics");
            _cache = new DoubleMap<string, GameObject>(_capacity);
        }

        /// <summary>
        /// 将 UI 移动到缓存池子下面
        /// </summary>
        /// <param name="obj"></param>
        public void Add(string id, GameObject module)
        {
            //检查Layer
            if (module.layer != _uiLayer)
            {
                throw new Exception("当前加入缓存池子的 obj 不是UI层级,不能加入到此缓存池子里面");
            }

            //达到上限了,需要从最底层的哪一个删除掉,缓存不易过多
            if (_cache.Count == _capacity) _cache.RemoveByValue(transform.GetChild(0).gameObject);
            
            //设置进池子里面
            module.transform.SetParent(this.transform);
            module.layer = this.gameObject.layer;
            // module.GetComponent<LuaModule>().enabled = false;
            _cache.Add(id, module);
        }

        public GameObject Query(string id)
        {
            var module = _cache.GetValueByKey(id);
            if (module == null) return default(GameObject);
            
            var parent = this.transform.parent;
            module.transform.SetParent(parent);
            module.layer = parent.gameObject.layer;
            module.GetComponent<RectTransform>().localScale = Vector3.one;
            // module.GetComponent<LuaModule>().enabled = true;
            _cache.RemoveByKey(id);
            return module;
        }
        
        //不需要此缓存了
        public bool Destroy(string id)
        {
            var module = _cache.GetValueByKey(id);
            if (module == null) return false;

            DestroyImmediate(module);
            return _cache.RemoveByKey(id);
        }

        //清除所有的 UI 缓存,切换场景时使用
        public void DestroyAll()
        {
            for (int i = this.transform.childCount - 1; i >= 0; i--)//倒序删除
            {
                DestroyImmediate(this.transform.GetChild(i).gameObject);
            }
        }
    }
}