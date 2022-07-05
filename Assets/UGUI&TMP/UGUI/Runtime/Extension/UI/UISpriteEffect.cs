using UnityEngine;

namespace UnityEngine.UI
{
    
    /// <summary>
    /// 对UI图形进行镜像处理
    /// UISprite - Sample顶点顺序
    /// ------
    /// |1 /2|
    /// |0/ 3|
    /// ------
    /// </summary>
    [RequireComponent(typeof(UISprite))]
    public class UISpriteEffect : BaseMeshEffect
    {
        public enum ImageType
        {
            None, // 水平镜像
            Mirror_Horizontal, // 水平镜像
            Mirror_Vertical, // 垂直镜像
            Mirror_Quater, // 四方镜像（先水平，后垂直）
        }
        
        [SerializeField] private ImageType _ImageType = ImageType.None;
        
        private const int AxisX = 0;
        private const int AxisY = 1;
        
        //修改mesh的地方
        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive()) return;
            if (_ImageType == ImageType.None) return;

            var img = graphic as UISprite;
            if (null == img) return;

            if (img.type == Image.Type.Simple)
            {
                _SimpleMirror(vh);
            }
        }


        #region ========= Image.Type.Simple模式 =========

        private void _SimpleMirror(VertexHelper vh)
        {
            Rect rect = graphic.GetPixelAdjustedRect();
            ShrinkVert(vh, rect);

            Vector2 doubleCenter = rect.center * 2;
            switch (_ImageType)
            {
                case ImageType.Mirror_Horizontal:
                    _SimpleMirror_Horizontal(vh, doubleCenter.x);
                    break;

                case ImageType.Mirror_Vertical:
                    _SimpleMirror_Vertical(vh, doubleCenter.y);
                    break;

                case ImageType.Mirror_Quater:
                    _SimpleMirror_Quater(vh, doubleCenter);
                    break;
            }
        }

        /// <summary>
        /// Simple模式的水平镜像
        /// 顶点布局：
        /// -----------
        /// |1 /2| \ 5|
        /// |0/ 3|  \4|
        /// -----------
        /// </summary>
        private void _SimpleMirror_Horizontal(VertexHelper vh, float doubleX)
        {
            AddMirrorVert(vh, 0, AxisX, doubleX); // 顶点4
            AddMirrorVert(vh, 1, AxisX, doubleX); // 顶点5

            vh.AddTriangle(2, 4, 3);
            vh.AddTriangle(2, 5, 4);
        }

        /// <summary>
        /// Simple模式的垂直镜像
        /// 顶点布局：
        /// ------
        /// |4\ 5|
        /// |  \ |
        /// ------
        /// |1 /2|
        /// |0/ 3|
        /// ------
        /// </summary>
        private void _SimpleMirror_Vertical(VertexHelper vh, float doubleY)
        {
            AddMirrorVert(vh, 0, AxisY, doubleY); // 顶点4
            AddMirrorVert(vh, 3, AxisY, doubleY); // 顶点5

            vh.AddTriangle(2, 1, 4);
            vh.AddTriangle(2, 4, 5);
        }

        /// <summary>
        /// Simple模式的四方镜像
        /// 顶点布局：
        /// -----------
        /// |6 /7| \ 8|
        /// | /  |  \ |
        /// -----------
        /// |1 /2| \ 5|
        /// |0/ 3|  \4|
        /// -----------
        /// </summary>
        private void _SimpleMirror_Quater(VertexHelper vh, Vector2 doubleCenter)
        {
            // 水平
            AddMirrorVert(vh, 0, AxisX, doubleCenter.x); // 顶点4
            AddMirrorVert(vh, 1, AxisX, doubleCenter.x); // 顶点5
            vh.AddTriangle(2, 4, 3);
            vh.AddTriangle(2, 5, 4);

            // 垂直
            AddMirrorVert(vh, 0, AxisY, doubleCenter.y); // 顶点6
            AddMirrorVert(vh, 3, AxisY, doubleCenter.y); // 顶点7
            AddMirrorVert(vh, 4, AxisY, doubleCenter.y); // 顶点8
            vh.AddTriangle(7, 1, 6);
            vh.AddTriangle(7, 2, 1);
            vh.AddTriangle(7, 5, 2);
            vh.AddTriangle(7, 8, 5);
        }

        #endregion


        /// <summary>
        /// 添加单个镜像顶点
        /// </summary>
        /// <param name="vh"></param>
        /// <param name="srcVertIdx">镜像源顶点的索引值</param>
        /// <param name="axis">轴向：0-X轴；1-Y轴</param>
        /// <param name="doubleCenter">Rect.center轴向分量的两倍值</param>
        private static void AddMirrorVert(VertexHelper vh, int srcVertIdx, int axis, float doubleCenter)
        {
            UIVertex vert = UIVertex.simpleVert;
            vh.PopulateUIVertex(ref vert, srcVertIdx);
            Vector3 pos = vert.position;
            pos[axis] = doubleCenter - pos[axis];
            vert.position = pos;
            vh.AddVert(vert);
        }

        /// <summary>
        /// 收缩顶点坐标
        /// 根据镜像类型，将原始顶点坐标向“起始点(左/下)”收缩
        /// </summary>
        private void ShrinkVert(VertexHelper vh, Rect rect)
        {
            int count = vh.currentVertCount;

            UIVertex vert = UIVertex.simpleVert;
            for (int i = 0; i < count; ++i)
            {
                vh.PopulateUIVertex(ref vert, i);
                Vector3 pos = vert.position;
                if (ImageType.Mirror_Horizontal == _ImageType || ImageType.Mirror_Quater == _ImageType)
                {
                    pos.x = (rect.x + pos.x) * 0.5f;
                }

                if (ImageType.Mirror_Vertical == _ImageType || ImageType.Mirror_Quater == _ImageType)
                {
                    pos.y = (rect.y + pos.y) * 0.5f;
                }

                vert.position = pos;
                vh.SetUIVertex(vert, i);
            }
        }


        #region ======设置Image的原尺寸======

        private RectTransform _rectTrans;

        public RectTransform RectTrans
        {
            get
            {
                if (null == _rectTrans)
                {
                    _rectTrans = GetComponent<RectTransform>();
                }

                return _rectTrans;
            }
        }

        public void SetNativeSize()
        {
            var img = graphic as UISprite;
            if (null == img) return;
            if (img.type != Image.Type.Simple) return;
            var sprite = img.overrideSprite;
            if (null == sprite) return;
            
            float w = sprite.rect.width / img.pixelsPerUnit;
            float h = sprite.rect.height / img.pixelsPerUnit;
            RectTrans.anchorMax = RectTrans.anchorMin;
            switch (_ImageType)
            {
                case ImageType.Mirror_Horizontal:
                    RectTrans.sizeDelta = new Vector2(w * 2, h);
                    break;
                case ImageType.Mirror_Vertical:
                    RectTrans.sizeDelta = new Vector2(w, h * 2);
                    break;
                case ImageType.Mirror_Quater:
                    RectTrans.sizeDelta = new Vector2(w * 2, h * 2);
                    break;
            }

            img.SetVerticesDirty();
        }

        #endregion
    }

}