using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Events/UILongPressListener", 54)]
    public class UILongPressListener : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        //长按 1s
        private float duration = 1f;
        private float beginPress;
        private bool isPointerDown = false;
        private PointerEventData pEventData;//长按的事件内容
        
        public Action<PointerEventData> onLongPress;
        
        public static UILongPressListener Get(RectTransform t)
        {
            return Get(t.gameObject);
        }
        
        public static UILongPressListener Get(GameObject go)
        {
            UILongPressListener listener = go.GetComponent<UILongPressListener>();
            if (listener == null) listener = go.AddComponent<UILongPressListener>();
            return listener;
        }

        private void Update()
        {
            if (!isPointerDown) return;
            if (!(Time.unscaledTime - beginPress >= duration)) return;
            beginPress = Time.unscaledTime;//长按情况下,每隔 1s 进行一次触发
            onLongPress?.Invoke(pEventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            pEventData = eventData;
            beginPress = eventData.clickTime;
            isPointerDown = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            beginPress = 0;
            isPointerDown = false;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            beginPress = 0;
            isPointerDown = false;
        }
    }
}