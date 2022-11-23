using UnityEditor;
using UnityEngine.U2D;
using UnityEngine;

public class UGUISettings 
{

    static public bool GetBool (string name, bool defaultValue) { return EditorPrefs.GetBool(name, defaultValue); }

    static public void SetBool (string name, bool val) { EditorPrefs.SetBool(name, val); }
    
    static public void SetString (string name, string val) { EditorPrefs.SetString(name, val); }
    
    static public string GetString (string name, string defaultValue) { return EditorPrefs.GetString(name, defaultValue); }

    
    static public string selectedSprite
    {
        get { return GetString("UGUI Sprite", null); }
        set { SetString("UGUI Sprite", value); }
    }
    
    static public SpriteAtlas atlas
    {
        get { return Get<SpriteAtlas>("UGUI Atlas", null); }
        set { Set("UGUI Atlas", value); }
    }
    
    static public T Get<T> (string name, T defaultValue) where T : Object
    {
        string path = EditorPrefs.GetString(name);
        if (string.IsNullOrEmpty(path)) return null;
		
        T retVal = UGUIEditorTools.LoadAsset<T>(path);
		
        if (retVal == null)
        {
            int id;
            if (int.TryParse(path, out id))
                return EditorUtility.InstanceIDToObject(id) as T;
        }
        return retVal;
    }
    
    static public void Set (string name, Object obj)
    {
        if (obj == null)
        {
            EditorPrefs.DeleteKey(name);
        }
        else
        {
            if (obj != null)
            {
                string path = AssetDatabase.GetAssetPath(obj);

                if (!string.IsNullOrEmpty(path))
                {
                    EditorPrefs.SetString(name, path);
                }
                else
                {
                    EditorPrefs.SetString(name, obj.GetInstanceID().ToString());
                }
            }
            else EditorPrefs.DeleteKey(name);
        }
    }
    
    static public string partialSprite
    {
        get { return GetString("UGUI Partial", null); }
        set { SetString("UGUI Partial", value); }
    }
    
    static public bool minimalisticLook
    {
        get { return GetBool("UGUI Minimalistic", false); }
        set { SetBool("UGUI Minimalistic", value); }
    }

}
