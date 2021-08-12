using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UnityEngine.UI
{
	/// <summary>
	/// 在运行时,动态打出一张 2048/4096 的一张图集.
	/// 主要是因为当前的界面 UI 有多个图集的其中一部分图片,将其中的图片动态生成一张图,并保存下来.
	/// 当前屏幕上的 UI 占用很少的 DC ,不需要使用动态图集
	/// 这个地方是给极端情况下使用的,以空间换时间,以高内存换取高性能,优化顺序 CPU>GPU>Memory.
	/// 注意:动态图集中,传入的Sprite或者Sprite所在的图集需要打开Read/Write,并且不能是 Tight Packing 排列方式
	/// </summary>
	public class DynamicSpriteAtlas
	{
		[Serializable]
		private class SpriteInfo
		{
			public int x;
			public int y;
			public int width;
			public int height;
			public string name;
		}

		private Dictionary<string, Sprite> dynamicSprites = new Dictionary<string, Sprite>();
		private float pixelsPerUnit = 100.0f;
		private int padding = 2;


		public Action<DynamicSpriteAtlas> OnCompleted;
		public Sprite this[string name] => dynamicSprites.TryGetValue(name, out Sprite sprite) ? sprite : null;

		public List<Sprite> GetSprites(string prefix = "")
		{
			List<Sprite> spriteNames = new List<Sprite>(10);
			foreach (var item in dynamicSprites)
			{
				if (item.Key.StartsWith(prefix))
				{
					spriteNames.Add(item.Value);
				}
			}

			return spriteNames;
		}

		//根据Texture2D[] 创建图集
		//如果使用Texture2D生成图集,请避免使用Sprite生成,2 者方法内图集,没有合并在一起
		public void CreateSpriteAtlasAsset(Texture2D[] texture2Ds)
		{
			if (texture2Ds == null || texture2Ds.Length <= 0) return;
			
			int maxTextureSize = 2048;
			
			List<Rect> rectangles = new List<Rect>(texture2Ds.Length);

			foreach (var item in texture2Ds)
			{
				if (item.width > 2048 || item.height > 2048)
				{
					maxTextureSize = 4096;
				}
				rectangles.Add(new Rect(0, 0, item.width, item.height));
			}
			Texture2D outTexture2D = new Texture2D(maxTextureSize, maxTextureSize, TextureFormat.RGBA32, true);
			List<SpriteInfo> spriteInfos = new List<SpriteInfo>(texture2Ds.Length);
			while (rectangles.Count>0)
			{
				RectanglePacker packer = new RectanglePacker(maxTextureSize, maxTextureSize, padding);
				for (int i = 0; i < rectangles.Count; i++)
				{
					packer.insertRectangle((int) rectangles[i].width, (int) rectangles[i].height, i);
				}
				packer.packRectangles();
				
				if (packer.rectangleCount <= 0) continue;
				
				RectanglePacker.IntegerRectangle rect = new RectanglePacker.IntegerRectangle();
				List<Rect> garbageRect = new List<Rect>();

				for (int i = 0; i < packer.rectangleCount; i++)
				{
					rect = packer.getRectangle(i, rect);
					int index = packer.getRectangleId(i);
					outTexture2D.SetPixels32(rect.x, rect.y, rect.width, rect.height, texture2Ds[index].GetPixels32());
					garbageRect.Add(rectangles[index]);
					SpriteInfo spriteInfo = new SpriteInfo
					{
						x = rect.x,
						y = rect.y,
						width = rect.width,
						height = rect.height,
						name = texture2Ds[index].name
					};
					spriteInfos.Add(spriteInfo);
				}
				foreach (Rect garbage in garbageRect)
					rectangles.Remove(garbage);
			}
			outTexture2D.Apply();
			//秒级时间戳
			File.WriteAllBytes(
				Application.persistentDataPath + "/DynamicSpriteAtlas2048_" + ((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000) + ".png",
				outTexture2D.EncodeToPNG());

			foreach (SpriteInfo item in spriteInfos)
			{
				dynamicSprites.Add(item.name,Sprite.Create(outTexture2D, new Rect(item.x, item.y, item.width, item.height), Vector2.zero, pixelsPerUnit, 0, SpriteMeshType.FullRect));
			}
			OnCompleted?.Invoke(this);
		}


		//根据 Sprite[] 创建动态图集
		//如果使用Sprite生成图集,请避免使用Texture2D生成,2 者方法内图集,没有合并在一起
		public void CreateSpriteAtlasAsset(Sprite[] sprites)
		{
			if (sprites == null || sprites.Length <= 0) return;
			
			int maxTextureSize = 2048;
			
			List<Rect> rectangles = new List<Rect>(sprites.Length);

			foreach (var item in sprites)
			{
				if (item.textureRect.width > 2048 || item.textureRect.height > 2048)
				{
					maxTextureSize = 4096;
				}
				rectangles.Add(new Rect(0, 0, item.textureRect.width, item.textureRect.height));
			}
			Texture2D outTexture2D = new Texture2D(maxTextureSize, maxTextureSize, TextureFormat.RGBA32, true);
			List<SpriteInfo> spriteInfos = new List<SpriteInfo>(sprites.Length);
			while (rectangles.Count>0)
			{
				RectanglePacker packer = new RectanglePacker(maxTextureSize, maxTextureSize, padding);
				for (int i = 0; i < rectangles.Count; i++)
				{
					packer.insertRectangle((int) rectangles[i].width, (int) rectangles[i].height, i);
				}
				packer.packRectangles();
				
				if (packer.rectangleCount <= 0) continue;
				
				RectanglePacker.IntegerRectangle rect = new RectanglePacker.IntegerRectangle();
				List<Rect> garbageRect = new List<Rect>();

				for (int i = 0; i < packer.rectangleCount; i++)
				{
					rect = packer.getRectangle(i, rect);
					int index = packer.getRectangleId(i);
						
					var pixels = sprites[index].texture.GetPixels(
						(int) sprites[index].textureRect.x,
						(int) sprites[index].textureRect.y,
						(int) sprites[index].textureRect.width,
						(int) sprites[index].textureRect.height);
					outTexture2D.SetPixels(rect.x, rect.y, rect.width, rect.height, pixels);
					garbageRect.Add(rectangles[index]);
					spriteInfos.Add(new SpriteInfo
					{
						x = rect.x,
						y = rect.y,
						width = rect.width,
						height = rect.height,
						name = sprites[index].name
					});
				}
				foreach (Rect garbage in garbageRect)
					rectangles.Remove(garbage);
			}
			outTexture2D.Apply();
			//秒级时间戳
			File.WriteAllBytes(
				Application.persistentDataPath + "/DynamicSpriteAtlas2048_" + ((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000) + ".png",
				outTexture2D.EncodeToPNG());

			foreach (SpriteInfo item in spriteInfos)
			{
				dynamicSprites.Add(item.name,Sprite.Create(outTexture2D, new Rect(item.x, item.y, item.width, item.height), Vector2.zero, pixelsPerUnit, 0, SpriteMeshType.FullRect));
			}
			OnCompleted?.Invoke(this);
		}
	}
}