using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.U2D;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class UGUIEditorTools 
{
	static Texture2D mBackdropTex;
	static string mEditedName = null;
	static string mLastSprite = null;
	static GameObject mPrevious;
	static Texture2D mContrastTex;
	/// <summary>
	/// Returns a blank usable 1x1 white texture.
	/// </summary>
	static public Texture2D blankTexture
	{
		get
		{
			return EditorGUIUtility.whiteTexture;
		}
	}
	
	static public Texture2D backdropTexture
	{
		get
		{
			if (mBackdropTex == null) mBackdropTex = CreateCheckerTex(
				new Color(0.1f, 0.1f, 0.1f, 0.5f),
				new Color(0.2f, 0.2f, 0.2f, 0.5f));
			return mBackdropTex;
		}
	}
	
	static Texture2D CreateCheckerTex (Color c0, Color c1)
	{
		Texture2D tex = new Texture2D(16, 16);
		tex.name = "[Generated] Checker Texture";
		tex.hideFlags = HideFlags.DontSave;

		for (int y = 0; y < 8; ++y) for (int x = 0; x < 8; ++x) tex.SetPixel(x, y, c1);
		for (int y = 8; y < 16; ++y) for (int x = 0; x < 8; ++x) tex.SetPixel(x, y, c0);
		for (int y = 0; y < 8; ++y) for (int x = 8; x < 16; ++x) tex.SetPixel(x, y, c0);
		for (int y = 8; y < 16; ++y) for (int x = 8; x < 16; ++x) tex.SetPixel(x, y, c1);

		tex.Apply();
		tex.filterMode = FilterMode.Point;
		return tex;
	}
	
	/// <summary>
	/// Returns a usable texture that looks like a high-contrast checker board.
	/// </summary>

	static public Texture2D contrastTexture
	{
		get
		{
			if (mContrastTex == null) mContrastTex = CreateCheckerTex(
				new Color(0f, 0.0f, 0f, 0.5f),
				new Color(1f, 1f, 1f, 0.5f));
			return mContrastTex;
		}
	}
	
	static public bool DrawPrefixButton (string text)
	{
		return GUILayout.Button(text, "DropDown", GUILayout.Width(76f));
	}
	
	static public T LoadAsset<T> (string path) where T: Object
	{
		Object obj = LoadAsset(path);
		if (obj == null) return null;

		T val = obj as T;
		if (val != null) return val;

		if (typeof(T).IsSubclassOf(typeof(Component)))
		{
			if (obj.GetType() == typeof(GameObject))
			{
				GameObject go = obj as GameObject;
				return go.GetComponent(typeof(T)) as T;
			}
		}
		return null;
	}
	
	static public Object LoadAsset (string path)
	{
		if (string.IsNullOrEmpty(path)) return null;
		return AssetDatabase.LoadMainAssetAtPath(path);
	}
	
    static public void DrawAdvancedSpriteField (SpriteAtlas atlas, string spriteName, SpriteSelector.Callback callback, bool editable,
		params GUILayoutOption[] options)
	{
		if (atlas == null) return;

		// Give the user a warning if there are no sprites in the atlas
		if (atlas.spriteCount == 0)
		{
			EditorGUILayout.HelpBox("No sprites found", MessageType.Warning);
			return;
		}
		
		// Sprite selection drop-down list
		GUILayout.BeginHorizontal();
		{
			if (UGUIEditorTools.DrawPrefixButton("Sprite"))
			{
				UGUISettings.atlas = atlas;
				UGUISettings.selectedSprite = spriteName;
				SpriteSelector.Show(callback);
			}

			if (editable)
			{
				if (!string.Equals(spriteName, mLastSprite))
				{
					mLastSprite = spriteName;
					mEditedName = null;
				}

				string newName = GUILayout.TextField(string.IsNullOrEmpty(mEditedName) ? spriteName : mEditedName);

				if (newName != spriteName)
				{
					mEditedName = newName;

					if (GUILayout.Button("Rename", GUILayout.Width(60f)))
					{
						Sprite sprite = atlas.GetSprite(spriteName);

						if (sprite != null)
						{
							UGUIEditorTools.RegisterUndo("Edit Sprite Name", atlas);
							sprite.name = newName;

							List<UISprite> sprites = FindAll<UISprite>();

							for (int i = 0; i < sprites.Count; ++i)
							{
								UISprite sp = sprites[i];

								if (sp.spriteAtlas == atlas && sp.spriteName == spriteName)
								{
									UGUIEditorTools.RegisterUndo("Edit Sprite Name", sp);
									sp.spriteName = newName;
								}
							}

							mLastSprite = newName;
							spriteName = newName;
							mEditedName = null;

							UGUISettings.atlas = atlas;
							UGUISettings.selectedSprite = spriteName;
						}
					}
				}
			}
			else
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(spriteName, "HelpBox", GUILayout.Height(18f));
				UGUIEditorTools.DrawPadding();
				GUILayout.EndHorizontal();
			}
		}
		GUILayout.EndHorizontal();
	}
    

    
    static public List<T> FindAll<T> () where T : Component
    {
	    T[] comps = Resources.FindObjectsOfTypeAll(typeof(T)) as T[];

	    List<T> list = new List<T>();

	    foreach (T comp in comps)
	    {
		    if (comp.gameObject.hideFlags == 0)
		    {
			    string path = AssetDatabase.GetAssetPath(comp.gameObject);
			    if (string.IsNullOrEmpty(path)) list.Add(comp);
		    }
	    }
	    return list;
    }
    
    
    static public void DrawSeparator ()
    {
	    GUILayout.Space(12f);

	    if (Event.current.type == EventType.Repaint)
	    {
		    Texture2D tex = blankTexture;
		    Rect rect = GUILayoutUtility.GetLastRect();
		    GUI.color = new Color(0f, 0f, 0f, 0.25f);
		    GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 4f), tex);
		    GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 1f), tex);
		    GUI.DrawTexture(new Rect(0f, rect.yMin + 9f, Screen.width, 1f), tex);
		    GUI.color = Color.white;
	    }
    }
    
    
    static public void RepaintSprites ()
    {
	    if (SpriteSelector.instance != null)
		    SpriteSelector.instance.Repaint();
    }
    
        
    static public void DrawTiledTexture (Rect rect, Texture tex)
    {
	    GUI.BeginGroup(rect);
	    {
		    int width  = Mathf.RoundToInt(rect.width);
		    int height = Mathf.RoundToInt(rect.height);

		    for (int y = 0; y < height; y += tex.height)
		    {
			    for (int x = 0; x < width; x += tex.width)
			    {
				    GUI.DrawTexture(new Rect(x, y, tex.width, tex.height), tex);
			    }
		    }
	    }
	    GUI.EndGroup();
    }
    
    
    /// <summary>
	/// Draw a single-pixel outline around the specified rectangle.
	/// </summary>

	static public void DrawOutline (Rect rect)
	{
		if (Event.current.type == EventType.Repaint)
		{
			Texture2D tex = contrastTexture;
			GUI.color = Color.white;
			DrawTiledTexture(new Rect(rect.xMin, rect.yMax, 1f, -rect.height), tex);
			DrawTiledTexture(new Rect(rect.xMax, rect.yMax, 1f, -rect.height), tex);
			DrawTiledTexture(new Rect(rect.xMin, rect.yMin, rect.width, 1f), tex);
			DrawTiledTexture(new Rect(rect.xMin, rect.yMax, rect.width, 1f), tex);
		}
	}

	/// <summary>
	/// Draw a single-pixel outline around the specified rectangle.
	/// </summary>

	static public void DrawOutline (Rect rect, Color color)
	{
		if (Event.current.type == EventType.Repaint)
		{
			Texture2D tex = blankTexture;
			GUI.color = color;
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, 1f, rect.height), tex);
			GUI.DrawTexture(new Rect(rect.xMax, rect.yMin, 1f, rect.height), tex);
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, 1f), tex);
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMax, rect.width, 1f), tex);
			GUI.color = Color.white;
		}
	}

	/// <summary>
	/// Draw a selection outline around the specified rectangle.
	/// </summary>

	static public void DrawOutline (Rect rect, Rect relative, Color color)
	{
		if (Event.current.type == EventType.Repaint)
		{
			// Calculate where the outer rectangle would be
			float x = rect.xMin + rect.width * relative.xMin;
			float y = rect.yMax - rect.height * relative.yMin;
			float width = rect.width * relative.width;
			float height = -rect.height * relative.height;
			relative = new Rect(x, y, width, height);

			// Draw the selection
			DrawOutline(relative, color);
		}
	}

	/// <summary>
	/// Draw a selection outline around the specified rectangle.
	/// </summary>

	static public void DrawOutline (Rect rect, Rect relative)
	{
		if (Event.current.type == EventType.Repaint)
		{
			// Calculate where the outer rectangle would be
			float x = rect.xMin + rect.width * relative.xMin;
			float y = rect.yMax - rect.height * relative.yMin;
			float width = rect.width * relative.width;
			float height = -rect.height * relative.height;
			relative = new Rect(x, y, width, height);

			// Draw the selection
			DrawOutline(relative);
		}
	}

	/// <summary>
	/// Draw a 9-sliced outline.
	/// </summary>

	static public void DrawOutline (Rect rect, Rect outer, Rect inner)
	{
		if (Event.current.type == EventType.Repaint)
		{
			Color green = new Color(0.4f, 1f, 0f, 1f);

			DrawOutline(rect, new Rect(outer.x, inner.y, outer.width, inner.height));
			DrawOutline(rect, new Rect(inner.x, outer.y, inner.width, outer.height));
			DrawOutline(rect, outer, green);
		}
	}
	
	static public Rect ConvertToTexCoords (Rect rect, int width, int height)
	{
		Rect final = rect;

		if (width != 0f && height != 0f)
		{
			//modify by tree
			//0.5 scene窗口看没问题，一些分辨率下载game窗口有缝(无关的sprite也会剪裁，将相应的剪裁调整到UIBasicSprite下)
			//final.xMin = rect.xMin / width;
			//final.xMax = (rect.xMax - 1f) / width;
			//final.yMin = 1f - (rect.yMax - 1f) / height;
			//final.yMax = 1f - rect.yMin / height;

			//ngui原本
			final.xMin = rect.xMin / width;
			final.xMax = rect.xMax / width;
			final.yMin = 1f - rect.yMax / height;
			final.yMax = 1f - rect.yMin / height;

			//begine:modify by ColinChen
			float halfPixelWidth = 0.5f / width;
			float halfPixelHeight = 0.5f / height;

			final.xMin += halfPixelWidth;
			final.xMax -= halfPixelWidth;
			final.yMin += halfPixelHeight;
			final.yMax -= halfPixelHeight;
			//end:modify by ColinChen
		}
		return final;
	}
	
	static public SerializedProperty DrawProperty (string label, SerializedObject serializedObject, string property, params GUILayoutOption[] options)
	{
		return DrawProperty(label, serializedObject, property, false, options);
	}
	
	static public SerializedProperty DrawProperty (string label, SerializedObject serializedObject, string property, bool padding, params GUILayoutOption[] options)
	{
		SerializedProperty sp = serializedObject.FindProperty(property);

		if (sp != null)
		{
			if (UGUISettings.minimalisticLook) padding = false;

			if (padding) EditorGUILayout.BeginHorizontal();

			if (sp.isArray && sp.type != "string") DrawArray(serializedObject, property, label ?? property);
			else if (label != null) EditorGUILayout.PropertyField(sp, new GUIContent(label), options);
			else EditorGUILayout.PropertyField(sp, options);

			if (padding)
			{
				UGUIEditorTools.DrawPadding();
				EditorGUILayout.EndHorizontal();
			}
		}
		else Debug.LogWarning("Unable to find property " + property);
		return sp;
	}
	
	static public void DrawArray (SerializedObject obj, string property, string title)
	{
		SerializedProperty sp = obj.FindProperty(property + ".Array.size");

		if (sp != null && UGUIEditorTools.DrawHeader(title))
		{
			UGUIEditorTools.BeginContents();
			int size = sp.intValue;
			int newSize = EditorGUILayout.IntField("Size", size);
			if (newSize != size) obj.FindProperty(property + ".Array.size").intValue = newSize;

			EditorGUI.indentLevel = 1;

			for (int i = 0; i < newSize; i++)
			{
				SerializedProperty p = obj.FindProperty(string.Format("{0}.Array.data[{1}]", property, i));
				if (p != null) EditorGUILayout.PropertyField(p);
			}
			EditorGUI.indentLevel = 0;
			UGUIEditorTools.EndContents();
		}
	}
	
	
	/// <summary>
	/// Draw a distinctly different looking header label
	/// </summary>

	static public bool DrawHeader (string text, string key, bool forceOn, bool minimalistic)
	{
		bool state = EditorPrefs.GetBool(key, true);

		if (!minimalistic) GUILayout.Space(3f);
		if (!forceOn && !state) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
		GUILayout.BeginHorizontal();
		GUI.changed = false;

		if (minimalistic)
		{
			if (state) text = "\u25BC" + (char)0x200a + text;
			else text = "\u25BA" + (char)0x200a + text;

			GUILayout.BeginHorizontal();
			GUI.contentColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.7f);
			if (!GUILayout.Toggle(true, text, "PreToolbar2", GUILayout.MinWidth(20f))) state = !state;
			GUI.contentColor = Color.white;
			GUILayout.EndHorizontal();
		}
		else
		{
			text = "<b><size=11>" + text + "</size></b>";
			if (state) text = "\u25BC " + text;
			else text = "\u25BA " + text;
			if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) state = !state;
		}

		if (GUI.changed) EditorPrefs.SetBool(key, state);

		if (!minimalistic) GUILayout.Space(2f);
		GUILayout.EndHorizontal();
		GUI.backgroundColor = Color.white;
		if (!forceOn && !state) GUILayout.Space(3f);
		return state;
	}

	/// <summary>
	/// Begin drawing the content area.
	/// </summary>

	static public void BeginContents () { BeginContents(true); }

	static bool mEndHorizontal = false;

	/// <summary>
	/// Begin drawing the content area.
	/// </summary>

	static public void BeginContents (bool minimalistic)
	{
		if (!minimalistic)
		{
			mEndHorizontal = true;
			GUILayout.BeginHorizontal();
			EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(10f));
		}
		else
		{
			mEndHorizontal = false;
			EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(10f));
			GUILayout.Space(10f);
		}
		GUILayout.BeginVertical();
		GUILayout.Space(2f);
	}

	/// <summary>
	/// End drawing the content area.
	/// </summary>

	static public void EndContents ()
	{
		GUILayout.Space(3f);
		GUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();

		if (mEndHorizontal)
		{
			GUILayout.Space(3f);
			GUILayout.EndHorizontal();
		}

		GUILayout.Space(3f);
	}



	/// <summary>
	/// Unity 4.3 changed the way LookLikeControls works.
	/// </summary>

	static public void SetLabelWidth (float width)
	{
		EditorGUIUtility.labelWidth = width;
	}

	/// <summary>
	/// Create an undo point for the specified objects.
	/// </summary>

	static public void RegisterUndo (string name, params Object[] objects)
	{
		if (objects != null && objects.Length > 0)
		{
			UnityEditor.Undo.RecordObjects(objects, name);

			foreach (Object obj in objects)
			{
				if (obj == null) continue;
				EditorUtility.SetDirty(obj);
			}
		}
	}

	static MethodInfo s_GetInstanceIDFromGUID;

	static public void DrawPadding ()
	{
		GUILayout.Space(18f);
	}
    
    static public bool DrawHeader (string text) { return DrawHeader(text, text, false, true); }
    
    
    public static List<String> GetListOfSprites (Sprite[] spriteArray,string match)
    {
	    List<String> list = new List<String>();
	    // First try to find an exact match
	    for (int i = 0, imax = spriteArray.Length; i < imax; ++i)
	    {
		    Sprite s = spriteArray[i];
		    if (s != null && !string.IsNullOrEmpty(s.name) && string.Equals(match, s.name, StringComparison.OrdinalIgnoreCase))
		    {
			    list.Add(s.name);
			    return list;
		    }
	    }
	    // No exact match found? Split up the search into space-separated components.
	    string[] keywords = match.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
	    for (int i = 0; i < keywords.Length; ++i) keywords[i] = keywords[i].ToLower();

	    // Try to find all sprites where all keywords are present
	    for (int i = 0, imax = spriteArray.Length; i < imax; ++i)
	    {
		    Sprite s = spriteArray[i];
			
		    if (s != null && !string.IsNullOrEmpty(s.name))
		    {
			    string tl = s.name.ToLower();
			    int matches = 0;

			    for (int b = 0; b < keywords.Length; ++b)
			    {
				    if (tl.Contains(keywords[b])) ++matches;
			    }
			    if (matches == keywords.Length) list.Add(s.name);
		    }
	    }
	    return list;
    }

}
