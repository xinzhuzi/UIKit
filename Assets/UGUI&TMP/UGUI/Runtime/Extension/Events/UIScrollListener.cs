using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    /// <summary>
    /// 大部分情况下的点击事件都是由 click 来触发的,所以单独为此类编写一个触发器.
    /// 合理使用不同事件脚本,减少性能消耗
    /// </summary>
    [AddComponentMenu("UI/Events/UIScrollListener", 57)]
    public class UIScrollListener : MonoBehaviour, IScrollHandler
    {
        public Action<PointerEventData> onScroll; //点击
        
        public static UIScrollListener Get(Transform t)
        {
            return Get(t.gameObject);
        }
        
        public static UIScrollListener Get(GameObject go)
        {
            UIScrollListener listener = go.GetComponent<UIScrollListener>();
            if (listener == null) listener = go.AddComponent<UIScrollListener>();
            return listener;
        }

        public void OnScroll(PointerEventData eventData) => onScroll?.Invoke(eventData);
    }
}