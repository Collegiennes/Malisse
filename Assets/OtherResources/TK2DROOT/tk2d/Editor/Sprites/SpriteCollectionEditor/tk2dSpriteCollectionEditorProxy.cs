using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace tk2dEditor.SpriteCollectionEditor
{
	// As nasty as this is, its a necessary evil for backwards compatibility
	public class SpriteCollectionProxy
	{
		public SpriteCollectionProxy()
		{
		}
		
		public SpriteCollectionProxy(tk2dSpriteCollection obj)
		{
			this.obj = obj;
			CopyFromSource();
		}
		
		public void CopyFromSource()
		{
			this.obj.Upgrade(); // make sure its up to date

			textureParams = new List<tk2dSpriteCollectionDefinition>(obj.textureParams.Length);
			foreach (var v in obj.textureParams)
			{
				if (v == null) 
					textureParams.Add(null);
				else 
				{
					var t = new tk2dSpriteCollectionDefinition();
					t.CopyFrom(v);
					textureParams.Add(t);
				}
			}
			
			spriteSheets = new List<tk2dSpriteSheetSource>();
			if (obj.spriteSheets != null)
			{
				foreach (var v in obj.spriteSheets)
				{
					if (v == null) 
						spriteSheets.Add(null);
					else
					{
						var t = new tk2dSpriteSheetSource();
						t.CopyFrom(v);
						spriteSheets.Add(t);
					}
				}
			}
			
			fonts = new List<tk2dSpriteCollectionFont>();
			if (obj.fonts != null)
			{
				foreach (var v in obj.fonts)
				{
					if (v == null)
						fonts.Add(null);
					else
					{
						var t = new tk2dSpriteCollectionFont();
						t.CopyFrom(v);
						fonts.Add(t);
					}
				}
			}

			UpgradeLegacySpriteSheets();
			
			var target = this;
			var source = obj;
			
			target.platforms = new List<tk2dSpriteCollectionPlatform>(source.platforms);
			if (target.platforms.Count == 0)
			{
				tk2dSpriteCollectionPlatform plat = new tk2dSpriteCollectionPlatform(); // add a null platform
				target.platforms.Add(plat);
			}

			target.assetName = source.assetName;

			target.maxTextureSize = source.maxTextureSize;
			target.forceTextureSize = source.forceTextureSize;
			target.forcedTextureWidth = source.forcedTextureWidth;
			target.forcedTextureHeight = source.forcedTextureHeight;
			
			target.textureCompression = source.textureCompression;
			target.atlasWidth = source.atlasWidth;
			target.atlasHeight = source.atlasHeight;
			target.forceSquareAtlas = source.forceSquareAtlas;
			target.atlasWastage = source.atlasWastage;
			target.allowMultipleAtlases = source.allowMultipleAtlases;
			
			target.spriteCollection = source.spriteCollection;
			target.premultipliedAlpha = source.premultipliedAlpha;
			
			CopyArray(ref target.altMaterials, source.altMaterials);
			CopyArray(ref target.atlasMaterials, source.atlasMaterials);
			CopyArray(ref target.atlasTextures, source.atlasTextures);
			
			target.useTk2dCamera = source.useTk2dCamera;
			target.targetHeight = source.targetHeight;
			target.targetOrthoSize = source.targetOrthoSize;
			target.globalScale = source.globalScale;
			target.physicsDepth = source.physicsDepth;
			target.disableTrimming = source.disableTrimming;
			target.normalGenerationMode = source.normalGenerationMode;
			target.padAmount = source.padAmount;
			target.autoUpdate = source.autoUpdate;
			target.editorDisplayScale = source.editorDisplayScale;

			// Texture settings
			target.filterMode = source.filterMode;
			target.wrapMode = source.wrapMode;
			target.userDefinedTextureSettings = source.userDefinedTextureSettings;
			target.mipmapEnabled = source.mipmapEnabled;
			target.anisoLevel = source.anisoLevel;
		}
		
		void CopyArray<T>(ref T[] dest, T[] source)
		{
			if (source == null)
			{
				dest = new T[0];
			}
			else
			{
				dest = new T[source.Length];
				for (int i = 0; i < source.Length; ++i)
					dest[i] = source[i];
			}
		}
		
		void UpgradeLegacySpriteSheets()
		{
			if (spriteSheets != null)
			{
				for (int i = 0; i < spriteSheets.Count; ++i)
				{
					var spriteSheet = spriteSheets[i];
					if (spriteSheet != null && spriteSheet.version == 0)
					{
						if (spriteSheet.texture == null)
						{
							spriteSheet.active = false;
						}
						else
						{
							spriteSheet.tileWidth = spriteSheet.texture.width / spriteSheet.tilesX;
							spriteSheet.tileHeight = spriteSheet.texture.height / spriteSheet.tilesY;
							spriteSheet.active = true;
							
							for (int j = 0; j < textureParams.Count; ++j)
							{
								var param = textureParams[j];
								if (param.fromSpriteSheet && param.texture == spriteSheet.texture)
								{
									param.fromSpriteSheet = false;
									param.hasSpriteSheetId = true;
									param.spriteSheetId = i;
									
									param.spriteSheetX = param.regionId % spriteSheet.tilesX;
									param.spriteSheetY = param.regionId / spriteSheet.tilesX;
								}
							}
						}
						
						spriteSheet.version = tk2dSpriteSheetSource.CURRENT_VERSION;
					}
				}				
			}
		}

		public void CopyToTarget()
		{
			CopyToTarget(obj);
		}
		
		public void CopyToTarget(tk2dSpriteCollection target)
		{
			target.textureParams = textureParams.ToArray();
			target.spriteSheets = spriteSheets.ToArray();
			target.fonts = fonts.ToArray();

			var source = this;
			target.platforms = new List<tk2dSpriteCollectionPlatform>(source.platforms);
			target.assetName = source.assetName;
			
			target.maxTextureSize = source.maxTextureSize;
			target.forceTextureSize = source.forceTextureSize;
			target.forcedTextureWidth = source.forcedTextureWidth;
			target.forcedTextureHeight = source.forcedTextureHeight;
			
			target.textureCompression = source.textureCompression;
			target.atlasWidth = source.atlasWidth;
			target.atlasHeight = source.atlasHeight;
			target.forceSquareAtlas = source.forceSquareAtlas;
			target.atlasWastage = source.atlasWastage;
			target.allowMultipleAtlases = source.allowMultipleAtlases;
			
			target.spriteCollection = source.spriteCollection;
			target.premultipliedAlpha = source.premultipliedAlpha;
			
			CopyArray(ref target.altMaterials, source.altMaterials);
			CopyArray(ref target.atlasMaterials, source.atlasMaterials);
			CopyArray(ref target.atlasTextures, source.atlasTextures);

			target.useTk2dCamera = source.useTk2dCamera;
			target.targetHeight = source.targetHeight;
			target.targetOrthoSize = source.targetOrthoSize;
			target.globalScale = source.globalScale;
			target.physicsDepth = source.physicsDepth;
			target.disableTrimming = source.disableTrimming;
			target.normalGenerationMode = source.normalGenerationMode;
			target.padAmount = source.padAmount; 
			target.autoUpdate = source.autoUpdate;
			target.editorDisplayScale = source.editorDisplayScale;

			// Texture settings
			target.filterMode = source.filterMode;
			target.wrapMode = source.wrapMode;
			target.userDefinedTextureSettings = source.userDefinedTextureSettings;
			target.mipmapEnabled = source.mipmapEnabled;
			target.anisoLevel = source.anisoLevel;
		}
		
		public bool AllowAltMaterials
		{
			get
			{
				return !allowMultipleAtlases;
			}
		}
		
		public int FindOrCreateEmptySpriteSlot()
		{
			for (int index = 0; index < textureParams.Count; ++index)
			{
				if (textureParams[index].texture == null)
					return index;
			}
			textureParams.Add(new tk2dSpriteCollectionDefinition());
			return textureParams.Count - 1;
		}
		
		public int FindOrCreateEmptyFontSlot()
		{
			for (int index = 0; index < fonts.Count; ++index)
			{
				if (!fonts[index].active)
				{
					fonts[index].active = true;
					return index;
				}
			}
			var font = new tk2dSpriteCollectionFont();
			font.active = true;
			fonts.Add(font);
			return fonts.Count - 1;
		}
		
		public int FindOrCreateEmptySpriteSheetSlot()
		{
			for (int index = 0; index < spriteSheets.Count; ++index)
			{
				if (!spriteSheets[index].active)
				{
					spriteSheets[index].active = true;
					spriteSheets[index].version = tk2dSpriteSheetSource.CURRENT_VERSION;
					return index;
				}
			}
			var spriteSheet = new tk2dSpriteSheetSource();
			spriteSheet.active = true;
			spriteSheet.version = tk2dSpriteSheetSource.CURRENT_VERSION;
			spriteSheets.Add(spriteSheet);
			return spriteSheets.Count - 1;
		}
		
		public string FindUniqueTextureName(string name)
		{
			List<string> textureNames = new List<string>();
			foreach (var entry in textureParams)
			{
				textureNames.Add(entry.name);
			}
			if (textureNames.IndexOf(name) == -1)
				return name;
			int count = 1;
			do 
			{
				string currName = name + " " + count.ToString();
				if (textureNames.IndexOf(currName) == -1)
					return currName;
				++count;
			} while(count < 1024); // arbitrary large number
			return name; // failed to find a name
		}
		
		public bool Empty { get { return textureParams.Count == 0 && fonts.Count == 0 && spriteSheets.Count == 0; } }
		
		// Call after deleting anything
		public void Trim()
		{
			int lastIndex = textureParams.Count - 1;
			while (lastIndex >= 0)
			{
				if (textureParams[lastIndex].texture != null)
					break;
				lastIndex--;
			}
			int count = textureParams.Count - 1 - lastIndex;
			if (count > 0)
			{
				textureParams.RemoveRange( lastIndex + 1, count );
			}
			
			lastIndex = fonts.Count - 1;
			while (lastIndex >= 0)
			{
				if (fonts[lastIndex].active)
					break;
				lastIndex--;
			}
			count = fonts.Count - 1 - lastIndex;
			if (count > 0) fonts.RemoveRange(lastIndex + 1, count);
			
			lastIndex = spriteSheets.Count - 1;
			while (lastIndex >= 0)
			{
				if (spriteSheets[lastIndex].active)
					break;
				lastIndex--;
			}
			count = spriteSheets.Count - 1 - lastIndex;
			if (count > 0) spriteSheets.RemoveRange(lastIndex + 1, count);
			
			lastIndex = atlasMaterials.Length - 1;
			while (lastIndex >= 0)
			{
				if (atlasMaterials[lastIndex] != null)
					break;
				lastIndex--;
			}
			count = atlasMaterials.Length - 1 - lastIndex;
			if (count > 0) 
				System.Array.Resize(ref atlasMaterials, lastIndex + 1);
		}
		
		public int GetSpriteSheetId(tk2dSpriteSheetSource spriteSheet)
		{
			for (int index = 0; index < spriteSheets.Count; ++index)
				if (spriteSheets[index] == spriteSheet) return index;
			return 0;
		}
		
		// Delete all sprites from a spritesheet
		public void DeleteSpriteSheet(tk2dSpriteSheetSource spriteSheet)
		{
			int index = GetSpriteSheetId(spriteSheet);
			
			for (int i = 0; i < textureParams.Count; ++i)
			{
				if (textureParams[i].hasSpriteSheetId && textureParams[i].spriteSheetId == index)
				{
					textureParams[i] = new tk2dSpriteCollectionDefinition();
				}
			}
			
			spriteSheets[index] = new tk2dSpriteSheetSource();
			Trim();
		}
		
		public string GetAssetPath()
		{
			return AssetDatabase.GetAssetPath(obj);
		}

		public string GetOrCreateDataPath()
		{
			return tk2dSpriteCollectionBuilder.GetOrCreateDataPath(obj);
		}

		public bool Ready { get { return obj != null; } }
		tk2dSpriteCollection obj;
		

		// Mirrored data objects
		public List<tk2dSpriteCollectionDefinition> textureParams = new List<tk2dSpriteCollectionDefinition>();
		public List<tk2dSpriteSheetSource> spriteSheets = new List<tk2dSpriteSheetSource>();
		public List<tk2dSpriteCollectionFont> fonts = new List<tk2dSpriteCollectionFont>();
		
		// Mirrored from sprite collection
		public string assetName;
		public int maxTextureSize;
		public tk2dSpriteCollection.TextureCompression textureCompression;
		public int atlasWidth, atlasHeight;
		public bool forceSquareAtlas;
		public float atlasWastage;
		public bool allowMultipleAtlases;
		public tk2dSpriteCollectionData spriteCollection;
	    public bool premultipliedAlpha;

	    public List<tk2dSpriteCollectionPlatform> platforms = new List<tk2dSpriteCollectionPlatform>();
		
		public Material[] altMaterials;
		public Material[] atlasMaterials;
		public Texture2D[] atlasTextures;
		
		public bool useTk2dCamera;
		public int targetHeight;
		public float targetOrthoSize;
		public float globalScale;

		// Texture settings
		public FilterMode filterMode;
		public TextureWrapMode wrapMode;
		public bool userDefinedTextureSettings;
		public bool mipmapEnabled = true;
		public int anisoLevel = 1;

		public float physicsDepth;
		public bool disableTrimming;
		
		public bool forceTextureSize = false;
		public int forcedTextureWidth = 1024;
		public int forcedTextureHeight = 1024;
		
		public tk2dSpriteCollection.NormalGenerationMode normalGenerationMode;
		public int padAmount;
		public bool autoUpdate;
		
		public float editorDisplayScale;
	}
}

