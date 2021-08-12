using System.Collections.Generic;
using TMPro;

namespace UIKit
{
    /// <summary>
    /// 多国适配
    /// </summary>
    public enum ThemeArea
    {
        China = 0,//中国
        America = 1,//英语
        Vietnam = 2,//越南语
        Korea = 3,//韩国
        Taiwan = 4,//台湾
        HongKong = 5,//香港
    }
    
    public sealed class LocalisationManager
    {

        #region 单例

        private static LocalisationManager _instance;
        public static LocalisationManager Instance()
        {
            return _instance ?? (_instance = new LocalisationManager());
        }
        private LocalisationManager(){}
        #endregion
        
        
        #region 管理器文本控制
        
        private List<LocalisationText> allLTexts = new List<LocalisationText>();
        private ThemeArea Theme = ThemeArea.China;

        public void AddText(LocalisationText lText)
        {
            if (allLTexts.Contains(lText)) return;
            allLTexts.Add(lText);
        }
        
        public void RemoveText(LocalisationText lText)
        {
            if (!allLTexts.Contains(lText)) return;
            allLTexts.Remove(lText);
        }
        
        public void UpdateTheme(ThemeArea theme = ThemeArea.China)
        {
            Theme = theme;
            //这个地方应该是根据某个地区,获取一系列的 id,然后进行赋值,目前暂不设计
            foreach (var item in allLTexts)
            {
                // item.Id = ""; //直接设置字号,或者将其主题设置一遍,会自动刷新的.
            }
        }
        
        // /// <summary>
        // /// 根据 key 值,主题,配置表,查找文本,并赋值
        // /// </summary>
        // /// <param name="key"></param>
        // /// <returns></returns>
        // public string Query(string key)
        // {
        //     return key + Theme + "  多国语言适配";
        // }
        
        #endregion
                
                
    }
}