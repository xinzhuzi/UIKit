﻿using UnityEngine.U2D;

namespace UnityEngine.UI
{
	/// <summary>
	/// Atlas image.
	/// </summary>
	public class UISprite : Image
	{
		[SerializeField] 
		private string m_SpriteName;
		[SerializeField] 
		private SpriteAtlas m_SpriteAtlas;
		private string _lastSpriteName = "";


		/// <summary>SpriteAtlas. Get and set atlas assets created by AtlasMaker.</summary>
		public SpriteAtlas spriteAtlas
		{
			get => m_SpriteAtlas;
			set
			{
				if (m_SpriteAtlas == value) return;
				m_SpriteAtlas = value;
				SetAllDirty();
			}
		}

		/// <summary>
		/// Sprite Name. If there is no other sprite with the same name in the atlas, AtlasImage will display the default sprite.
		/// </summary>
		public string spriteName
		{
			get => m_SpriteName;
			set
			{
				if (m_SpriteName == value) return;
				m_SpriteName = value;
				SetAllDirty();
			}
		}


		/// <summary>
		/// Sets the material dirty.
		/// </summary>
		public override void SetMaterialDirty()
		{
			// Changing sprites from Animation.
			// If the "sprite" is changed by an animation or script, it will be reflected in the sprite name.
			if (_lastSpriteName == spriteName && sprite)
			{
				m_SpriteName = sprite.name.Replace("(Clone)", "");
			}

			if (_lastSpriteName != spriteName)
			{
				_lastSpriteName = spriteName;
				sprite = spriteAtlas ? spriteAtlas.GetSprite(spriteName) : null;
			}

			base.SetMaterialDirty();
		}

		/// <summary>
		/// Raises the populate mesh event.
		/// </summary>
		/// <param name="toFill">To fill.</param>
		protected override void OnPopulateMesh(VertexHelper toFill)
		{
			if (!overrideSprite)
			{
				toFill.Clear();
				return;
			}
			base.OnPopulateMesh(toFill);
		}
	}
}
