using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

namespace UnityEditor.UI
{
    public class UIDrawPopupTools
    {

        //SerializedProperty atlas
        //atlas?.objectReferenceValue as SpriteAtlas 即可转换为 SpriteAtlas
        public static void DrawAtlas(string label, string atlasLabel, SpriteAtlas atlas = null,
            Action<SpriteAtlas> onSelect = null)
        {

            Rect rect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.popup);
            rect = EditorGUI.PrefixLabel(rect, new GUIContent(label));
            rect.x -= 2;

            GUIContent atlasContent = new GUIContent(atlasLabel);
            var gm = new GenericMenu();
            if (GUI.Button(rect, atlas ? new GUIContent(atlas.name) : atlasContent, EditorStyles.popup))
            {
                gm.AddItem(atlasContent, !atlas, () => onSelect?.Invoke(null));

                foreach (string path in AssetDatabase.FindAssets("t:" + nameof(SpriteAtlas))
                    .Select(AssetDatabase.GUIDToAssetPath))
                {
                    string displayName = Path.GetFileNameWithoutExtension(path);
                    gm.AddItem(
                        new GUIContent(displayName),
                        atlas && (atlas.name == displayName),
                        x => onSelect?.Invoke(x == null
                            ? null
                            : AssetDatabase.LoadAssetAtPath((string) x, typeof(SpriteAtlas)) as SpriteAtlas),
                        path
                    );
                }

                gm.DropDown(rect);
            }
        }

        public static void DrawSprite(string label, SpriteAtlas atlas, string spriteName = "-", Action<string> onSelect = null)
        {
            spriteName = spriteName?.Replace("(Clone)", "");
            Rect rect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.popup);
            rect = EditorGUI.PrefixLabel(rect, new GUIContent(label));
            rect.x -= 2;
            
            GUIContent spriteContent = new GUIContent(spriteName);
            var gm = new GenericMenu();
            if (GUI.Button(rect, spriteName, EditorStyles.popup))
            {
                string[] assetLabels = {AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(atlas))};
                SerializedProperty spPackedSprites = new SerializedObject(atlas).FindProperty("m_PackedSprites");
                List<Sprite> sprites = Enumerable.Range(0, spPackedSprites.arraySize)
                    .Select(index => spPackedSprites.GetArrayElementAtIndex(index).objectReferenceValue)
                    .OfType<Sprite>()
                    .ToList()
                    .OrderBy(x=>x.name)
                    .ToList();
                
                foreach (Sprite sprite in sprites)
                {
                    gm.AddItem(
                        new GUIContent(sprite.name),
                        sprite && (sprite.name == spriteName),
                        (x) =>
                        {
                            spriteName = x.ToString().Replace("(Clone)","");
                            onSelect?.Invoke(spriteName);
                        },
                        sprite.name
                    );
                }

                gm.DropDown(rect);
            }
            
            // private static bool _openSelectorWindow = false;
            // var controlID = GUIUtility.GetControlID(FocusType.Passive);
            // if (_openSelectorWindow)
            // {
            //     var atlasLabel = SetAtlasLabelToSprites(atlas, true);
            //     EditorGUIUtility.ShowObjectPicker<Sprite>(atlas.GetSprite(spriteName), false, "l:" + atlasLabel,
            //         controlID);
            //     _openSelectorWindow = false;
            // }
            //
            // // Popup-styled button to select sprite in atlas.
            // using (new EditorGUI.DisabledGroupScope(!atlas))
            // using (new EditorGUILayout.HorizontalScope())
            // {
            //     EditorGUILayout.PrefixLabel(label);
            //     if (GUILayout.Button(string.IsNullOrEmpty(spriteName) ? "-" : spriteName.Replace("(Clone)",""), "minipopup") && atlas)
            //     {
            //         _openSelectorWindow = true;
            //     }
            // }
            //
            // if (controlID != EditorGUIUtility.GetObjectPickerControlID()) return;
            // var commandName = Event.current.commandName;
            // if (commandName == "ObjectSelectorUpdated")
            // {
            //     UnityEngine.Object picked = EditorGUIUtility.GetObjectPickerObject();
            //     onChange(picked ? picked.name.Replace("(Clone)", "") : "");
            // }
            // else if (commandName == "ObjectSelectorClosed")
            // {
            //     // On close selector window, reomove the atlas label from sprites.
            //     SetAtlasLabelToSprites(atlas, false);
            // }
        }

        // private static string SetAtlasLabelToSprites(SpriteAtlas atlas, bool add)
        // {
        //     string[] assetLabels = {AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(atlas))};
        //     SerializedProperty spPackedSprites = new SerializedObject(atlas).FindProperty("m_PackedSprites");
        //     Sprite[] sprites = Enumerable.Range(0, spPackedSprites.arraySize)
        //         .Select(index => spPackedSprites.GetArrayElementAtIndex(index).objectReferenceValue)
        //         .OfType<Sprite>()
        //         .ToArray();
        //
        //     foreach (var s in sprites)
        //     {
        //         string[] newLabels = add
        //             ? AssetDatabase.GetLabels(s).Union(assetLabels).ToArray()
        //             : AssetDatabase.GetLabels(s).Except(assetLabels).ToArray();
        //         AssetDatabase.SetLabels(s, newLabels);
        //     }
        //
        //     return assetLabels[0];
        // }
        
    }
}



        /**
         * 图集获取方式
         *
         *        private SpriteAtlas spriteAtlas
        {
            get
            {
                if (_spriteAtlas == null)
                {
                    string[] SAs = AssetDatabase.FindAssets("t:" + nameof(SpriteAtlas));
                    if (SAs != null && SAs.Length > 0)
                    {
                        _spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AssetDatabase.GUIDToAssetPath(SAs[0]));
                    }
                }
                return _spriteAtlas;
            }
            set
            {
                _spriteAtlas = value;
            }
        }
                    // if (spriteAtlas != null)
            // {
            //     UIDrawPopupTools.DrawAtlas(_spriteAtlasPlaceholder, spriteAtlas.name, onSelect: SetSpriteAtlas);
            // }
            
                        // spriteAtlas = EditorGUILayout.ObjectField(_spriteAtlasPlaceholder,spriteAtlas,typeof(SpriteAtlas),true) as SpriteAtlas;
            // if (spriteAtlas)
            // {
            //     Debug.Log(spriteAtlas.name);
            //     UpdateSpriteAtlas();
            //     GUID guid = AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(spriteAtlas));
            //     spriteAtlasGUID = guid.ToString();
            // }
            
                    /// <summary>
                    /// 设置第一章图片
                    /// </summary>
                    private void UpdateSpriteAtlas()
                    {
                        //设置图集的图片.
                        SerializedProperty spPackedSprites = new SerializedObject(spriteAtlas).FindProperty("m_PackedSprites");
                        string spriteName = Enumerable.Range(0, spPackedSprites.arraySize)
                            .Select(index => spPackedSprites.GetArrayElementAtIndex(index).objectReferenceValue)
                            .OfType<Sprite>()
                            .ToList()
                            .OrderBy(x => x.name)
                            .ToList()
                            .First()
                            .name;
                        SetSprite(spriteName);
                    }
                    
                    /// <summary>
                    /// 设置精灵图片
                    /// </summary>
                    private void SetSprite(string spriteName)
                    {
                        ((Image) target).sprite = spriteAtlas.GetSprite(spriteName);
                        ((Image) target).SetNativeSize();
                    }
         * 
         */