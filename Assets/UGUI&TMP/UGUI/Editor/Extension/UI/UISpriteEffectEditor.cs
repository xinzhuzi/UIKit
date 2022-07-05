using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(UISpriteEffect), true)]
    [CanEditMultipleObjects]
    public class UISpriteEffectEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Set Native Size"))
            {
                (target as UISpriteEffect)?.SetNativeSize();
            }
        }
    }
}