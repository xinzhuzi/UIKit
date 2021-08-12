using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(UIRawImageCircle), true)]
    [CanEditMultipleObjects]
    public class UIRawImageCircleEditor : RawImageEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI ();
            UIRawImageCircle circle = target as UIRawImageCircle;
            circle.segments = Mathf.Clamp(EditorGUILayout.IntField ("UICircle多边形", circle.segments),4,360);
        }
    }
}