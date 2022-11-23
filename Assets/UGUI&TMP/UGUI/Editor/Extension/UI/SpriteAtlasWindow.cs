using System;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

namespace UnityEditor.UI
{
    public static class SpriteAtlasWindow
    {
        private static SpriteAtlas _spriteAtlas;
        private static Sprite _sprite;
        private static int controlID;
        private static int currentState;//当前状态, 1 表示展示出了图集窗口,2 表示展示出了图集中的精灵窗口,3 表示彻底关闭,每次进入从 1 开始

        public static Action<Sprite> OnSelectedSprite;

        public static Action<SpriteAtlas> OnSelectedSpriteAtlas;
        
        public static void ShowSpriteAtlasWindow(SpriteAtlas tempSA = null)
        {
            _spriteAtlas = tempSA;
            _sprite = null;
            controlID = GUIUtility.GetControlID(FocusType.Passive);

            if (_spriteAtlas == null)
            {
                currentState = 1;
                EditorGUIUtility.ShowObjectPicker<SpriteAtlas>(_spriteAtlas, false, "", controlID);
            }
            else
            {
                currentState = 2;
                EditorGUIUtility.ShowObjectPicker<Sprite>(_sprite, false, 
                    "l:" + SetAtlasLabelToSprites(_spriteAtlas,true), controlID);
            }
            EditorApplication.update += Update;
        }
        
        private static void Update()
        {
            int currentID = EditorGUIUtility.GetObjectPickerControlID();
            if (currentState == 1)
            {
                if (controlID == EditorGUIUtility.GetObjectPickerControlID())
                {
                    Object selected = EditorGUIUtility.GetObjectPickerObject();
                    _spriteAtlas = selected as SpriteAtlas;
                    OnSelectedSpriteAtlas?.Invoke(_spriteAtlas);
                }
                else if (currentID == 0 && currentID != controlID)//当前的图集窗口已经关闭了,需要展示出来图集精灵窗口了,状态1 结束,当前状态切换为 2
                {
                    if (_spriteAtlas == null) return;
                    currentState = 2;
                    //展示图集中的所有精灵(图片)
                    controlID = GUIUtility.GetControlID(FocusType.Passive);
                    EditorGUIUtility.ShowObjectPicker<Sprite>(_sprite, false, 
                        "l:" + SetAtlasLabelToSprites(_spriteAtlas,true), controlID);
                }
            }
            else if (currentState == 2)
            {
                if (controlID == EditorGUIUtility.GetObjectPickerControlID())
                {
                    Object selected = EditorGUIUtility.GetObjectPickerObject();
                    _sprite = selected as Sprite;
                    if (_sprite != null) _sprite = _spriteAtlas.GetSprite(_sprite.name);
                    OnSelectedSprite?.Invoke(_sprite);
                }
                else if (currentID == 0 && currentID != controlID) //状态 2 结束,进入状态 3,所有的事情全部清空
                {
                    currentState = 3;
                    EditorApplication.update -= Update;
                    if (_sprite != null) _sprite = _spriteAtlas.GetSprite(_sprite.name);
                    OnSelectedSprite?.Invoke(_sprite);
                }
            }
        }
        
        private static string SetAtlasLabelToSprites(SpriteAtlas atlas, bool add)
        {
            string[] assetLabels = {AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(atlas))};
            SerializedProperty spPackedSprites = new SerializedObject(atlas).FindProperty("m_PackedSprites");
            Sprite[] sprites = Enumerable.Range(0, spPackedSprites.arraySize)
                .Select(index => spPackedSprites.GetArrayElementAtIndex(index).objectReferenceValue)
                .OfType<Sprite>()
                .ToArray();

            foreach (var s in sprites)
            {
                string[] newLabels = add
                    ? AssetDatabase.GetLabels(s).Union(assetLabels).ToArray()
                    : AssetDatabase.GetLabels(s).Except(assetLabels).ToArray();
                AssetDatabase.SetLabels(s, newLabels);
            }

            return assetLabels[0];
        }
    }
}