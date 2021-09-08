using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Events/UIDragListener", 52)]
    public class UIDragListener : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler ,IInitializePotentialDragHandler
    {
        public Action<PointerEventData> onInitializePotentialDrag; //初始化拖拽
        public Action<PointerEventData> onBeginDrag; //开始拖拽
        public Action<PointerEventData> onDrag; //正在拖拽中
        public Action<PointerEventData> onEndDrag; //结束拖拽

        public static UIDragListener Get(Transform t)
        {
            return Get(t.gameObject);
        }
        
        public static UIDragListener Get(GameObject go)
        {
            UIDragListener listener = go.GetComponent<UIDragListener>();
            if (listener == null) listener = go.AddComponent<UIDragListener>();
            return listener;
        }

        public void OnInitializePotentialDrag(PointerEventData eventData) => onInitializePotentialDrag?.Invoke(eventData);

        public void OnBeginDrag(PointerEventData eventData) => onBeginDrag?.Invoke(eventData);

        public void OnDrag(PointerEventData eventData) => onDrag?.Invoke(eventData);

        public void OnEndDrag(PointerEventData eventData) => onEndDrag?.Invoke(eventData);
        

    }
}
