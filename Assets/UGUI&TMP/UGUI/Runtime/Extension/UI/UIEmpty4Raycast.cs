using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
    //单纯为了点击事件,不要 Image
    [AddComponentMenu("UI/UIEmpty4Raycast", 52)]
    [RequireComponent(typeof(CanvasRenderer),typeof(RectTransform))]
    public class UIEmpty4Raycast : MaskableGraphic
    {
        protected UIEmpty4Raycast()
        {
            useLegacyMeshGeneration = false;
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            toFill.Clear();
        }
    }
}