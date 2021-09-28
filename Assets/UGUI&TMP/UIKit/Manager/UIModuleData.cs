using System.Collections.Generic;
using UnityEngine;

namespace UIKit
{
    public class UIModuleData
    {
        public string Id = string.Empty;
        public GameObject Go;
        public int ChildIndex => this.Go.transform.GetSiblingIndex();//作为子物体所在的排序
        public bool OptCullUI = false;//是否需要优化被遮挡,剔除的UI,当此UI模块被加载之后,后面的UI需要全部被回收到池子里面

        private int _sortingOrder = -1;
        public int SortingOrder
        {
            get => _sortingOrder;
            set
            {
                _sortingOrder = value;
                if (Go == null) return;
                var canvas = Go.GetComponent<Canvas>();
                if (null == canvas) return;
                canvas.sortingOrder = _sortingOrder;
            }
        }
        
        
        /// <summary>
        /// 重置数据,将所有数据设置为默认的
        /// </summary>
        public void Reset()
        {
            this.Id = string.Empty;
            this.Go = null;
            this._sortingOrder = -1;
            OptCullUI = false;
        }

        public static UIModuleData Clone()
        {
            return  new UIModuleData();
        }
        
        // #region 数据池子,栈
        //
        // private static readonly Stack<UIModuleData> Pool = new Stack<UIModuleData>(40);
        //
        // public static UIModuleData Query()
        // {
        //     return Pool.Count > 0 ? Pool.Pop() : new UIModuleData();
        // }
        //
        // public static void Recycle(UIModuleData data)
        // {
        //     data.Reset();
        //     Pool.Push(data);
        // }
        //
        // #endregion

    }
}