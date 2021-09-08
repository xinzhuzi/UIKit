using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    /**
     * 在这个地方进行添加事件,缺少什么就添加什么
     * 此类是集合事件性质的类,如果不想使用此类,可以使用单一事件类型的类
        button = transform.Find("Button").GetComponent<Button>();
		image = transform.Find("Image").GetComponent<Image>();
		UIEventListener.Get(button.gameObject).onClick =OnButtonClick;
		UIEventListener.Get(image.gameObject).onClick =OnButtonClick;
     *
     * 
     */
    [AddComponentMenu("UI/Events/UIEventListener", 53)]
    public class UIEventListener :
        MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerClickHandler,
        IInitializePotentialDragHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IDropHandler,
        IScrollHandler,
        IUpdateSelectedHandler,
        ISelectHandler,
        IDeselectHandler,
        IMoveHandler,
        ISubmitHandler,
        ICancelHandler
    {
        public Action<PointerEventData> onClick; //点击
        public Action<PointerEventData> onDown;
        public Action<PointerEventData> onEnter;
        public Action<PointerEventData> onExit;
        public Action<PointerEventData> onUp;
        public Action<BaseEventData> onSelect;
        public Action<BaseEventData> onUpdateSelect;
        public Action<PointerEventData> onBeginDrag; //开始拖拽
        public Action<PointerEventData> onDrag; //正在拖拽中
        public Action<PointerEventData> onEndDrag; //结束拖拽
        public Action<PointerEventData> onIPDrag;
        public Action<PointerEventData> onDrop;
        public Action<PointerEventData> onScroll;
        public Action<BaseEventData> onDeselect;
        public Action<AxisEventData> onMove;
        public Action<BaseEventData> onSubmit;
        public Action<BaseEventData> onCancel;

        
        public static UIEventListener Get(Transform t)
        {
            return Get(t.gameObject);
        }

        public static UIEventListener Get(GameObject go)
        {
            UIEventListener listener = go.GetComponent<UIEventListener>();
            if (listener == null) listener = go.AddComponent<UIEventListener>();
            return listener;
        }

        public void OnBeginDrag(PointerEventData eventData) => onBeginDrag?.Invoke(eventData);


        public void OnDrag(PointerEventData eventData) => onDrag?.Invoke(eventData);


        public void OnEndDrag(PointerEventData eventData) => onEndDrag?.Invoke(eventData);


        public void OnPointerClick(PointerEventData eventData) => onClick?.Invoke(eventData);


        public void OnPointerDown(PointerEventData eventData) => onDown?.Invoke(eventData);


        public void OnPointerEnter(PointerEventData eventData) => onEnter?.Invoke(eventData);


        public void OnPointerExit(PointerEventData eventData) => onExit?.Invoke(eventData);


        public void OnPointerUp(PointerEventData eventData) => onUp?.Invoke(eventData);


        public void OnSelect(BaseEventData eventData) => onSelect?.Invoke(eventData);


        public void OnUpdateSelected(BaseEventData eventData) => onUpdateSelect?.Invoke(eventData);


        public void OnInitializePotentialDrag(PointerEventData eventData) => onIPDrag?.Invoke(eventData);


        public void OnDrop(PointerEventData eventData) => onDrop?.Invoke(eventData);


        public void OnScroll(PointerEventData eventData) => onScroll?.Invoke(eventData);


        public void OnDeselect(BaseEventData eventData) => onDeselect?.Invoke(eventData);


        public void OnMove(AxisEventData eventData) => onMove?.Invoke(eventData);


        public void OnSubmit(BaseEventData eventData) => onSubmit?.Invoke(eventData);


        public void OnCancel(BaseEventData eventData) => onCancel?.Invoke(eventData);

    }
}
