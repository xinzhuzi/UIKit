using System;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(LayoutElement),typeof(RectTransform))]
    public class UITableCell : MonoBehaviour
    {
        //属于哪一个模板
        public string Id;
        
        
        public RectTransform rectTransform;
        
        //当前 GameObject 上的控制自身大小的布局类
        public LayoutElement element;
        
        //当前在屏幕上所刷新的下标,是属于CellTotalCount中的下标
        public int Index;
        
        private void Awake()
        {
            rectTransform = this.GetComponent<RectTransform>();
            element = this.GetComponent<LayoutElement>();
            if (string.IsNullOrWhiteSpace(this.Id))
            {
                this.Id = this.name;
            }
        }

        /// <summary>
        /// 在屏幕上,此 cell 刷新到某个 index 下了,可以在这个方法里面刷新数据
        /// 也可以使用 UITableView 的 UpdateCell 进行 cell 的刷新
        /// </summary>
        /// <param name="index"></param>
        public virtual void UpdateCell(int index)
        {
            Index = index;
        }
        
        
#if UNITY_EDITOR
        private void Reset()
        {
            rectTransform = this.GetComponent<RectTransform>();
            element = this.GetComponent<LayoutElement>();
            this.Id = this.name;
        }
#endif
        
    }
}