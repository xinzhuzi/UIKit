using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    public class ScrollView : ScrollRect
    {
        [SerializeField]
        private Image m_BgScrollImage;
        public Image BgScrollImage
        {
            get => m_BgScrollImage;
            set
            {
                m_BgScrollImage = value;
                m_BgScrollImage.raycastTarget = true;
                SetBgScrollListener(m_BgScrollImage.gameObject);
            }
        }
        
        [SerializeField]
        private RawImage m_BgScrollRawImage;
        public RawImage BgScrollRawImage
        {
            get => m_BgScrollRawImage;
            set
            {
                m_BgScrollRawImage = value;
                m_BgScrollImage.raycastTarget = true;
                SetBgScrollListener(m_BgScrollRawImage.gameObject);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            if (m_BgScrollImage)
            {
                m_BgScrollImage.raycastTarget = true;
                SetBgScrollListener(m_BgScrollImage.gameObject);
            }
            else if (m_BgScrollRawImage)
            {
                m_BgScrollRawImage.raycastTarget = true;
                SetBgScrollListener(m_BgScrollRawImage.gameObject);
            }
        }

        public void SetBgScrollListener(GameObject go)
        {
            UIScrollListener.Get(go.gameObject).onScroll = OnScroll;
            UIDragListener.Get(go.gameObject).onInitializePotentialDrag = OnInitializePotentialDrag;
            UIDragListener.Get(go.gameObject).onBeginDrag = OnBeginDrag;
            UIDragListener.Get(go.gameObject).onDrag = OnDrag;
            UIDragListener.Get(go.gameObject).onEndDrag = OnEndDrag;
        }

        // public override void OnScroll(PointerEventData data)
        // {
        //     base.OnScroll(data);
        // }
        //
        // public override void OnInitializePotentialDrag(PointerEventData eventData)
        // {
        //     base.OnInitializePotentialDrag(eventData);
        // }
        //         
        // public override void OnBeginDrag(PointerEventData eventData)
        // {
        //     base.OnBeginDrag(eventData);
        // }
        //
        // public override void OnEndDrag(PointerEventData eventData)
        // {
        //     base.OnEndDrag(eventData);
        // }
        //
        // public override void OnDrag(PointerEventData eventData)
        // {
        //     base.OnDrag(eventData);
        // }
        
    }
}