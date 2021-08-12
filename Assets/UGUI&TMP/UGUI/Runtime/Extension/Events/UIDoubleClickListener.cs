using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    /// <summary>
    /// 双击
    /// </summary>
    [AddComponentMenu("UI/Events/UIDoubleClickListener", 51)]
    public class UIDoubleClickListener : MonoBehaviour, IPointerClickHandler
    {
        private float lastClickTime;
        private float intervalTime = 0.5f;//双击中间间隔时间,可以自行修订
        //eventData.clickCount 默认使用的是 0.3f,这个地方可以由用户自定义时间
        
        public Action<PointerEventData> onDoubleClick; //双击
        
        public static UIDoubleClickListener Get(RectTransform t)
        {
            return Get(t.gameObject);
        }
        
        public static UIDoubleClickListener Get(GameObject go, float intervalTime = 0.5f)
        {
            UIDoubleClickListener listener = go.GetComponent<UIDoubleClickListener>();
            if (listener == null) listener = go.AddComponent<UIDoubleClickListener>();
            listener.intervalTime = intervalTime;
            return listener;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //点击了一次,超过 0.5s 没有点击了,或者从没点击过,或者双击过了,都将重新赋值lastClickTime
            if (eventData.clickTime - lastClickTime > intervalTime || lastClickTime <= 0)
            {
                lastClickTime = eventData.clickTime;
            }
            else if (eventData.clickTime - lastClickTime <= intervalTime)
            {
                lastClickTime = 0;
                onDoubleClick?.Invoke(eventData);
            }
        }
    }
}