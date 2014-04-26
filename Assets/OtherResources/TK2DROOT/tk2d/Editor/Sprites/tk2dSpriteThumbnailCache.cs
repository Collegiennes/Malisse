using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class tk2dSpriteThumbnailCache
{
	class SpriteThumbnailCache
	{
		public tk2dSpriteCollectionData cachedSpriteCollection;
		public int cachedSpriteId;
		public Texture2D cachedTexture;
		public bool needDestroy = false;
	}

	static List<SpriteThumbnailCache> thumbnailCache = new List<SpriteThumbnailCache>();
	
	public static void ReleaseSpriteThumbnailCache()
	{
		if (!EditorApplication.isPlaying)
		{
			foreach (var v in thumbnailCache)
			{
				if (v.needDestroy)
				{
					Texture2D.DestroyImmediate(v.cachedTexture);
				}
			}
			
			thumbnailCache.Clear();
			tk2dEditorUtility.UnloadUnusedAssets();
		}
	}
	
	public static Texture2D GetThumbnailTexture(tk2dSpriteCollectionData gen, int spriteId)
	{
		gen = gen.inst;
		
		// If we already have a cached texture which matches the requirements, use that
		foreach (var thumb in thumbnailCache)
		{
			if (thumb.cachedTexture	!= null && thumb.cachedSpriteCollection	== gen && thumb.cachedSpriteId == spriteId)
				return thumb.cachedTexture;
		}

		// Generate a texture
		var param = gen.spriteDefinitions[spriteId];
		if (param.sourceTextureGUID == null || param.sourceTextureGUID.Length != 0)
		{
			string assetPath = AssetDatabase.GUIDToAssetPath(param.sourceTextureGUID);
			if (assetPath.Length > 0)
			{
				Texture2D tex = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D)) as Texture2D;
				if (tex != null)
				{
					SpriteThumbnailCache thumbnail = new SpriteThumbnailCache();
					
					if (param.extractRegion)
					{
						Texture2D localTex = new Texture2D(param.regionW, param.regionH);
						for (int y = 0; y < param.regionH; ++y)
						{
							for (int x = 0; x < param.regionW; ++x)
							{
								localTex.SetPixel(x, y, tex.GetPixel(param.regionX + x, param.regionY + y));
							}
						}
						localTex.Apply();
						thumbnail.cachedTexture = localTex;
						thumbnail.needDestroy = true;
					}
					else
					{
						thumbnail.cachedTexture = tex;
					}

					// Prime cache for next time
					thumbnail.cachedSpriteCollection = gen;
					thumbnail.cachedSpriteId = spriteId;
					thumbnailCache.Add(thumbnail);
					
					return thumbnail.cachedTexture;
				}
			}
		}
		
		return null;
	}
}

