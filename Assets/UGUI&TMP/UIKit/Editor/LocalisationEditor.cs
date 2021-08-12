using UIKit;
using UnityEditor;

namespace UIKit
{
    public static class LocalisationTextEditor
    {
        [MenuItem("Tools/多主题/文本/中国")]
        private static void UpdateChinaTheme()
        {
            LocalisationManager.Instance().UpdateTheme(ThemeArea.China);
        }
        
        [MenuItem("Tools/多主题/文本/美国")]
        private static void UpdateAmericaTheme()
        {
            LocalisationManager.Instance().UpdateTheme(ThemeArea.America);
        }
        
        [MenuItem("Tools/多主题/文本/韩国")]
        private static void UpdateKoreaTheme()
        {
            LocalisationManager.Instance().UpdateTheme(ThemeArea.Korea);
        }
        
        [MenuItem("Tools/多主题/文本/香港")]
        private static void UpdateHongKongTheme()
        {
            LocalisationManager.Instance().UpdateTheme(ThemeArea.HongKong);
        }
        
        [MenuItem("Tools/多主题/文本/台湾")]
        private static void UpdateTaiwanTheme()
        {
            LocalisationManager.Instance().UpdateTheme(ThemeArea.Taiwan);
        }
        
        [MenuItem("Tools/多主题/文本/越南")]
        private static void UpdateVietnamTheme()
        {
            LocalisationManager.Instance().UpdateTheme(ThemeArea.Vietnam);
        }
    }
}