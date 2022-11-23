using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor.AnimatedValues;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    /// <summary>
    /// Editor class used to edit UI Sprites.
    /// </summary>

    [CustomEditor(typeof(UnityEngine.UI.UISprite), true)]
    [CanEditMultipleObjects]
    /// <summary>
    ///   Custom Editor for the Image Component.
    ///   Extend this class to write a custom editor for a component derived from Image.
    /// </summary>
    public class UISpriteEditor : ImageEditor
    {
        SerializedProperty m_mirrorType;
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            SpriteAtlasGUI();
            EditorGUILayout.PropertyField(m_mirrorType);
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
		
        protected override void OnEnable()
        {
            base.OnEnable();
            m_mirrorType = serializedObject.FindProperty("m_mirrorType");
        
        }

        private void SetSpriteAtlas(Object obj)
        {

            SpriteAtlas spriteAtlas = obj as SpriteAtlas;
            if (spriteAtlas == null) return;
            (target as UISprite).spriteAtlas = spriteAtlas;
            EditorUtility.SetDirty(target);
        }

        private void SetSprite(Sprite sprite)
        {
            if (sprite == null) return;
            (target as UISprite).spriteName = sprite?.name;
            (target as UISprite).sprite = sprite;
            EditorUtility.SetDirty(target);
        }


        /// <summary>
        /// Draw the atlas and Image selection fields.
        /// </summary>
        private void SpriteAtlasGUI()
        {
            UISprite uiSprite = target as UISprite;
            SpriteAtlas atlas = (target as UISprite).spriteAtlas;
            EditorGUILayout.BeginHorizontal();
            if (UGUIEditorTools.DrawPrefixButton("Atlas"))
            {
                ComponentSelector.Show<SpriteAtlas>(SetSpriteAtlas);
            }

            EditorGUILayout.ObjectField("", atlas, typeof(SpriteAtlas), false);
            EditorGUILayout.EndHorizontal();
            UGUIEditorTools.DrawAdvancedSpriteField(atlas, uiSprite.spriteName, SetSprite, false);
        }
    }
}