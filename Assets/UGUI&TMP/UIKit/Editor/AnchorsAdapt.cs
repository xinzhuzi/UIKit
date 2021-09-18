using UnityEditor;
using UnityEngine;

namespace UIKit
{
    public class AnchorsAdapt
    {
        [MenuItem("UGUI/AnchorsAdapt")]
        private static void SelectionM()
        {
            var gos = Selection.gameObjects;
            for (var i = 0; i < gos.Length; i++)
            {
                if (gos[i].GetComponent<RectTransform>() == null)
                    continue;
                Adapt(gos[i]);
            }
        }

        private static void Adapt(GameObject go)
        {
            //位置信息
            Vector3 partentPos = go.transform.parent.position;
            Vector3 localPos = go.transform.position;
            //------获取rectTransform----
            RectTransform partentRect = go.transform.parent.GetComponent<RectTransform>();
            RectTransform localRect = go.GetComponent<RectTransform>();
            float partentWidth = partentRect.rect.width;
            float partentHeight = partentRect.rect.height;
            float localWidth = localRect.rect.width * 0.5f;
            float localHeight = localRect.rect.height * 0.5f;
            //---------位移差------
            float offX = localPos.x - partentPos.x;
            float offY = localPos.y - partentPos.y;

            float rateW = offX / partentWidth;
            float rateH = offY / partentHeight;
            localRect.anchorMax = localRect.anchorMin = new Vector2(0.5f + rateW, 0.5f + rateH);
            localRect.anchoredPosition = Vector2.zero;

            partentHeight *= 0.5f;
            partentWidth *= 0.5f;
            float rateX = (localWidth / partentWidth) * 0.5f;
            float rateY = (localHeight / partentHeight) * 0.5f;
            var anchorMax = localRect.anchorMax;
            anchorMax = new Vector2(anchorMax.x + rateX, anchorMax.y + rateY);
            localRect.anchorMax = anchorMax;
            var anchorMin = localRect.anchorMin;
            anchorMin = new Vector2(anchorMin.x - rateX, anchorMin.y - rateY);
            localRect.anchorMin = anchorMin;
            localRect.offsetMax = localRect.offsetMin = Vector2.zero;
        }
    }
}