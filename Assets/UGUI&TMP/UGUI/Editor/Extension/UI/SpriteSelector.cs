using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.U2D;

/// <summary>
/// Editor component used to display a list of sprites.
/// </summary>

public class SpriteSelector : ScriptableWizard
{
	static public SpriteSelector instance;

	void OnEnable () { instance = this; }
	void OnDisable () { instance = null; }

	public delegate void Callback (Sprite sprite);

	SerializedObject mObject;
	SerializedProperty mProperty;

	UISprite mSprite;
	Vector2 mPos = Vector2.zero;
	Callback mCallback;
	float mClickTime = 0f;
	
	private SpriteAtlas initAtlas;
	private Sprite[] spriteArray;

	/// <summary>
	/// Draw the custom wizard.
	/// </summary>

	void OnGUI ()
	{
		UGUIEditorTools.SetLabelWidth(80f);

		if (UGUISettings.atlas == null)
		{
			GUILayout.Label("No Atlas selected.", "LODLevelNotifyText");
		}
		else
		{
			SpriteAtlas atlas = UGUISettings.atlas;
			bool close = false;
			GUILayout.Label(atlas.name + " Sprites", "LODLevelNotifyText");
			UGUIEditorTools.DrawSeparator();

			GUILayout.BeginHorizontal();
			GUILayout.Space(84f);

			string before = UGUISettings.partialSprite;
			string after = EditorGUILayout.TextField("", before, "SearchTextField");
			if (before != after) UGUISettings.partialSprite = after;

			if (GUILayout.Button("", "SearchCancelButton", GUILayout.Width(18f)))
			{
				UGUISettings.partialSprite = "";
				GUIUtility.keyboardControl = 0;
			}
			GUILayout.Space(84f);
			GUILayout.EndHorizontal();
			
			 if (initAtlas != atlas || spriteArray == null||spriteArray.Length!=atlas.spriteCount)
			 {
				spriteArray = new Sprite[atlas.spriteCount];
				initAtlas = atlas;
				atlas.GetSprites(spriteArray);
			 }

			 List<String> filterList = UGUIEditorTools.GetListOfSprites(spriteArray, UGUISettings.partialSprite);
			 filterList.Sort((string a, string b) =>
			 {
				 return a.CompareTo(b);
			 });
			 float size = 80f;
			float padded = size + 10f;
#if UNITY_4_7
			int screenWidth = Screen.width;
#else
			int screenWidth = (int)EditorGUIUtility.currentViewWidth;
#endif
			int columns = Mathf.FloorToInt(screenWidth / padded);
			if (columns < 1) columns = 1;

			int offset = 0;
			Rect rect = new Rect(10f, 0, size, size);

			GUILayout.Space(10f);
			mPos = GUILayout.BeginScrollView(mPos);
			int rows = 1;

			while (offset < filterList.Count)
			{
				GUILayout.BeginHorizontal();
				{
					int col = 0;
					rect.x = 10f;

                    for (; offset < filterList.Count; ++offset)
                    {
	                    string findName = filterList[offset].Replace("(Clone)", "");
	                    Sprite sprite = atlas.GetSprite(findName);
	                    if (sprite == null) continue;
	                    Texture2D tex = sprite.texture;
                        // Button comes first
						if (GUI.Button(rect, ""))
						{
							if (Event.current.button == 0)
							{
								float delta = Time.realtimeSinceStartup - mClickTime;
								mClickTime = Time.realtimeSinceStartup;
								if (UGUISettings.selectedSprite != filterList[offset])
								{
									if (mSprite != null)
									{
										UGUIEditorTools.RegisterUndo("Atlas Selection", mSprite);
										mSprite.MakePixelPerfect();
										EditorUtility.SetDirty(mSprite.gameObject);
									}
									UGUISettings.selectedSprite = filterList[offset];
									UGUIEditorTools.RepaintSprites();
									if (mCallback != null) mCallback(sprite);
								}
								else if (delta < 0.5f) close = true;
							}
						}

						if (Event.current.type == EventType.Repaint)
						{
							// On top of the button we have a checkboard grid
							UGUIEditorTools.DrawTiledTexture(rect, UGUIEditorTools.backdropTexture);
							Rect uv = new Rect(sprite.rect.x, sprite.rect.y, sprite.rect.width, sprite.rect.height);
							uv = UGUIEditorTools.ConvertToTexCoords(uv, tex.width, tex.height);
	
							// Calculate the texture's scale that's needed to display the sprite in the clipped area
							float scaleX = rect.width / uv.width;
							float scaleY = rect.height / uv.height;
	
							// Stretch the sprite so that it will appear proper
							float aspect = (scaleY / scaleX) / ((float)tex.height / tex.width);
							Rect clipRect = rect;
	
							if (aspect != 1f)
							{
								if (aspect < 1f)
								{
									// The sprite is taller than it is wider
									float padding = size * (1f - aspect) * 0.5f;
									clipRect.xMin += padding;
									clipRect.xMax -= padding;
								}
								else
								{
									// The sprite is wider than it is taller
									float padding = size * (1f - 1f / aspect) * 0.5f;
									clipRect.yMin += padding;
									clipRect.yMax -= padding;
								}
							}
	
							GUI.DrawTextureWithTexCoords(clipRect, tex, uv);
	
							// Draw the selection
							if (UGUISettings.selectedSprite == filterList[offset])
							{
								UGUIEditorTools.DrawOutline(rect, new Color(0.4f, 1f, 0f, 1f));
							}
						}

						GUI.backgroundColor = new Color(1f, 1f, 1f, 0.5f);
						GUI.contentColor = new Color(1f, 1f, 1f, 0.7f);
						GUI.Label(new Rect(rect.x, rect.y + rect.height, rect.width, 32f), sprite.name, "ProgressBarBack");
						GUI.contentColor = Color.white;
						GUI.backgroundColor = Color.white;

						if (++col >= columns)
						{
							++offset;
							break;
						}
						rect.x += padded;
					}
				}
				GUILayout.EndHorizontal();
				GUILayout.Space(padded);
				rect.y += padded + 26;
				++rows;
			}
			GUILayout.Space(rows * 26);
			GUILayout.EndScrollView();

			if (close) Close();
		}
	}
	
	/// <summary>
	/// Show the selection wizard.
	/// </summary>

	static public void Show (Callback callback)
	{
		if (instance != null)
		{
			instance.Close();
			instance = null;
		}
		SpriteSelector comp = ScriptableWizard.DisplayWizard<SpriteSelector>("Select a Sprite");
		comp.mSprite = null;
		comp.mCallback = callback;
	}
}
