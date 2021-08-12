using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(UIImageCircle), true)]
    [CanEditMultipleObjects]
    public class UIImageCircleEditor : ImageEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI ();
            UIImageCircle circle = target as UIImageCircle;
            circle.segments = Mathf.Clamp(EditorGUILayout.IntField ("UICircle多边形", circle.segments),4,360);
        }
    }
}