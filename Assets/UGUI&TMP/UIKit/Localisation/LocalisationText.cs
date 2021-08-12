using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UIKit
{
    [DisallowMultipleComponent]
    [ExecuteAlways]
    [RequireComponent(typeof(TMP_Text))]
    public class LocalisationText : MonoBehaviour
    {
        //指的是 font Id
        [SerializeField] 
        private int m_id;
        public int Id
        {
            get => m_id;
            set
            {
                m_id = value;
                UpdateTheme();
            }
        }

        // private dataconfig.FontMould config; // Excel 表中配置的文本


        private TMP_FontAsset _fontAsset;
        
        private TMP_Text _text;

        private void Awake()
        {
            _text = this.GetComponent<TMP_Text>();
        }
        
        [ContextMenu("Update Setting")]
        private void Start()
        {
            UpdateTheme();
        }

#if UNITY_EDITOR
        private int lastId; 
        private void Update()
        {
            if (lastId == m_id) return;
            lastId = m_id;
            UpdateTheme();
        }
#endif

        public void UpdateTheme()
        {
            if (this.Id <= 0) return;
            
            var sId = "F" + this.Id;
            
            // if (!config.Invalid() && config.id == sId) return; //已经刷过一次了
            // config = ResBinData.Instance.GetFontMouldByid(sId); // 得到 Excel 表中的数据
            //
            // //FontType 字体,设置字体,判断这个名字是否一致,如果不一致,则赋值
            // if (!string.IsNullOrWhiteSpace(config.FontType) && !config.FontType.Equals(_text.font.name))
            // {
            //     _text.font = GetFont(config.FontType);
            // }
            //
            // //设置字体大小
            // if (config.Size > 0) _text.fontSize = config.Size;
            //
            // //设置主颜色
            // _text.color = ColorUtility.TryParseHtmlString(config.MainColor, out var mainColor) ? mainColor : Color.white;
            //
            // //设置字体渐变色
            // if (!string.IsNullOrWhiteSpace(config.GradientTop) && !string.IsNullOrWhiteSpace(config.GradientBottom))
            // {
            //     _text.enableVertexGradient = true;      
            //     ColorUtility.TryParseHtmlString(config.GradientTop, out var gradientTop);
            //     ColorUtility.TryParseHtmlString(config.GradientBottom, out var gradientBottom);
            //     _text.colorGradient = new VertexGradient(gradientTop, gradientTop, gradientBottom, gradientBottom);
            // }
            // else
            // {
            //     _text.enableVertexGradient = false;            
            // }
            //
            // //TODO:设置 outline 目前 tmp 的属性设置方式与之前的差别较大,之后做处理
            // if (config.OutlineSize > 0)
            // {
            //     // _text.effectStyle = UILabel.Effect.Outline8;
            //     // _text.effectDistance = new Vector2(config.OutlineSize, config.OutlineSize);
            // }
            // else if (config.ShadowSize > 0)
            // {
            //     // _text.effectStyle = UILabel.Effect.Shadow;
            //     // _text.effectDistance = new Vector2(config.ShadowSize, config.ShadowSize);
            //     // ColorUtility.TryParseHtmlString(config.ShadowColor, out var shadowColor);
            //     // if (config.ShadowAlpha != string.Empty)
            //     // {
            //     //     shadowColor.a = float.Parse(config.ShadowAlpha);
            //     //     _text.effectColor = shadowColor;
            //     // }
            // }
            // else
            // {
            //     // _text.effectStyle = UILabel.Effect.None;
            // }
            //
            // if (!string.IsNullOrEmpty(config.OutlineColor))
            // {
            //     ColorUtility.TryParseHtmlString(config.OutlineColor, out var outlineColor);
            //
            //     float ca = 1;
            //     if (!string.IsNullOrEmpty(config.OutlineAlpha))
            //     {
            //         ca = float.Parse(config.OutlineAlpha);
            //     }         
            //     outlineColor.a = ca;
            //     _text.outlineColor = outlineColor;
            // }
            //
            // var fs = FontStyles.Normal;
            // switch (config.FontStyleValue)
            // {
            //     case 1:
            //         fs = FontStyles.Bold;
            //         break;
            //     case 2:
            //         fs = FontStyles.Italic;
            //         break;
            //     case 3:
            //         fs = FontStyles.Bold | FontStyles.Italic;
            //         break;
            // }
            //
            // _text.fontStyle = fs;
            //
            // //TODO:不知道那个属性,属性有点多.
            // // _text.spacingX = config.HorizontalSpace > 0 ? config.HorizontalSpace : 0;
            // // _text.spacingY = config.verticalSpace > 0 ? config.verticalSpace : 0;    
        }

        private TMP_FontAsset GetFont(string fontName)
        {
            // var tmpFontAsset = AB.Load<TMP_FontAsset>(fontName, AB.ELOADTIME.UI);
            // if (tmpFontAsset == null)
            // {
            //     Debug.LogError("没有这个字体:" + fontName +"      对象:" + this.name + "   对象的父Canvas:" + _text.canvas.name);
            // }
            //
            // return tmpFontAsset;
            return null;
        }

    }
}