using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    /// <summary>
    /// 合理使用不同事件脚本
    /// </summary>
    [AddComponentMenu("UI/Events/UIPointAllListener", 55)]
    public class UIPointAllListener : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,
        IPointerUpHandler, IPointerClickHandler
    {

        public Action<PointerEventData> onClick; //点击
        public Action<PointerEventData> onEnter; //进入
        public Action<PointerEventData> onExit; //退出
        public Action<PointerEventData> onDown; //按下
        public Action<PointerEventData> onUp; //抬起
        public bool IsPressd => m_IsPressed;

        private bool m_IsPressed = false;


        public static UIPointAllListener Get(Transform t)
        {
            return Get(t.gameObject);
        }
        
        public static UIPointAllListener Get(GameObject go)
        {
            UIPointAllListener listener = go.GetComponent<UIPointAllListener>();
            if (listener == null) listener = go.AddComponent<UIPointAllListener>();
            return listener;
        }

        public void OnPointerEnter(PointerEventData eventData) => onEnter?.Invoke(eventData);


        public void OnPointerExit(PointerEventData eventData) => onExit?.Invoke(eventData);


        public void OnPointerDown(PointerEventData eventData)
        {
            m_IsPressed = true;
            onDown?.Invoke(eventData);
        }


        public void OnPointerUp(PointerEventData eventData)
        {
            m_IsPressed = false;
            onUp?.Invoke(eventData);
        }


        public void OnPointerClick(PointerEventData eventData) => onClick?.Invoke(eventData);

    }
}