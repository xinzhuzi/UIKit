using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    /// <summary>
    /// 大部分情况下的点击事件都是由 click 来触发的,所以单独为此类编写一个触发器.
    /// 合理使用不同事件脚本,减少性能消耗
    /// </summary>
    [AddComponentMenu("UI/Events/UIPointClickListener", 56)]
    public class UIPointClickListener : MonoBehaviour, IPointerClickHandler
    {
        public Action<PointerEventData> onClick; //点击

        public static UIPointClickListener Get(RectTransform t)
        {
            return Get(t.gameObject);
        }
        
        public static UIPointClickListener Get(GameObject go)
        {
            UIPointClickListener listener = go.GetComponent<UIPointClickListener>();
            if (listener == null) listener = go.AddComponent<UIPointClickListener>();
            return listener;
        }

        public void OnPointerClick(PointerEventData eventData) => onClick?.Invoke(eventData);
    }
}