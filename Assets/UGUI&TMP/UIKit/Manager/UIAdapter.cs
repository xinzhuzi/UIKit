using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIKit
{
    
    /*
     * 使用 Manager 进行统一管理
     * 适配方案:
     * 1:计算屏幕安全区域与屏幕不安全区域,这个需要 iOS 与 Android 配合
     * 2:计算图形在屏幕 2d 不安全区域内的 Rect
     * 3:使用这个 Rect 对比安全区域,如果不在安全区域则进入 4
     * 4:将这个图形向内缩或者移动
     */
    public sealed class UIAdapter : MonoBehaviour
    {
        private Canvas _canvas;
        private CanvasScaler _canvasScaler;

        //屏幕的坐标系是从左下角 (0,0) 点开始,右上角(Screen.width, Screen.height) 点的坐标系.即全屏区域
        // private Rect AllRect = new Rect(0,0, Screen.width, Screen.height); 
        
        //安全区域,即手机厂商设定的没有刘海屏,挖孔位置的区域,但是根据自身需求,目前只做横屏左右的适配
        private Rect _safeArea;
        
        //与屏幕坐标系一样的一个RectTransform 用来计算,从左下角 (0,0) 点,右上角(Screen.width, Screen.height) 点的坐标系
        private RectTransform _screenRectTransform;
        
        //是否旋转了屏幕,或换成了其他模式,需要在 update 中进行检测通知等
        private static ScreenOrientation _cachedScreenOrientation;
        private static int _cachedWidth = Screen.width;
        private static int _cachedHeight = Screen.height;
        private List<RectTransform> _adapterGraphics;
        
        private void Start()
        {
            _canvas = UIManager.Instance.GetComponent<Canvas>();
            _canvasScaler = UIManager.Instance.GetComponent<CanvasScaler>();
            _screenRectTransform = this.GetComponent<RectTransform>();
            _adapterGraphics = new List<RectTransform>(10);
            Reset();
        }

        private void Reset()
        {
            _cachedScreenOrientation = Screen.orientation;
            _cachedWidth = Screen.width;
            _cachedHeight = Screen.height;
            
            _canvasScaler.referenceResolution = _canvas.GetComponent<RectTransform>().sizeDelta;
            _screenRectTransform.sizeDelta = _canvasScaler.referenceResolution;
            //第一步:计算屏幕安全区域与屏幕不安全区域,这个需要 iOS 与 Android 配合.目前直接使用 Unity 的 API 即可
            //计算安全区域的宽度,要与canvas的像素比例匹配
            var ratio = (_canvasScaler.referenceResolution.x / Screen.width);
            _safeArea = new Rect(Screen.safeArea.x * ratio,0, Screen.safeArea.width * ratio, Screen.height);
        }

        /// <summary>
        /// 对图形进行适配
        /// </summary>
        /// <param name="current"></param>
        private void Adapter(RectTransform current)
        {
            //第二步:计算图形在屏幕 2d 不安全区域内的 Rect
            //得到在 2d 平面坐标系下的位置
            // if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_screenRectTransform,
            //     UICamera.WorldToScreenPoint(graphic.transform.position),
            //     UICamera, out var screenRTLocalPos)) return;
            if (!UIHelper.WorldPositionToNewCoordinates(UIManager.UICamera, _screenRectTransform,
                current.position, out var screenRTLocalPos)) return;

            //计算当前的图形 Rect ,是在同一坐标系下的 Rect
            var currentRect = new Rect(
                screenRTLocalPos.x -  current.pivot.x * current.sizeDelta.x,/*这个地方是用来计算图形本身的具有的大小,要从左下角算起,如果轴心点偏右就需要减去*/
                screenRTLocalPos.y -  current.pivot.y * current.sizeDelta.y,/*这个地方是用来计算图形本身的具有的大小,要从左下角算起,如果轴心点偏右就需要减去*/
                current.sizeDelta.x,
                current.sizeDelta.y);
            
            //第三步:判断当前图形是否在安全区域内 safeArea  ,此时 2 个 rect 都在一个同一个坐标系下
            var rightMove = _safeArea.x - currentRect.x; //第四步:如果不在安全区域内,则进行修正,向内缩.这个情况,直接向右移动{rightMove}个像素
            if (rightMove > 0)
            {
                Debug.Log($"向右移动{rightMove}个像素");
                current.anchoredPosition = new Vector2(current.anchoredPosition.x + rightMove,current.anchoredPosition.y);
            }
            var leftMove = (currentRect.x + currentRect.width) - (_safeArea.x + _safeArea.width);
            if (leftMove>0) //第四步:如果不在安全区域内,则进行修正,向内缩.这个情况,直接向左移动 {leftMove}个像素
            {
                Debug.Log($"向左移动{leftMove}个像素");
                current.anchoredPosition = new Vector2(current.anchoredPosition.x - leftMove,current.anchoredPosition.y);
            }
        }

        //判断是否旋转了屏幕
        // private float durnTime = 1;
        // private float 
        private void LateUpdate()
        {
            if ((int)Time.realtimeSinceStartup%3 != 0)return;
            if (_cachedScreenOrientation == Screen.orientation && _cachedWidth == Screen.width &&
                _cachedHeight == Screen.height) return;
            Reset();
            //屏幕旋转了
            foreach (var item in _adapterGraphics)
            {
                Adapter(item);
            }
        }

        public void AddAdapter(RectTransform rt)
        {
            if (_adapterGraphics.Contains(rt)) return;
            _adapterGraphics.Add(rt);
            //第一次进行适配
            Adapter(rt);
        }
        
        public void AddAdapter(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            AddAdapter(rt);
        }
        
        public void RemoveAdapter(RectTransform rt)
        {
            _adapterGraphics.Remove(rt);
        }
        
        public void RemoveAdapter(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            RemoveAdapter(rt);
        }
    }
}