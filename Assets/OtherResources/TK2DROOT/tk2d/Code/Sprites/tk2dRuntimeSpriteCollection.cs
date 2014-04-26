using UnityEngine;
using System.Collections;

namespace tk2dRuntime
{
	public class SpriteCollectionSize
	{
		public static SpriteCollectionSize Explicit(float orthoSize, float targetHeight) { return new SpriteCollectionSize(orthoSize, targetHeight); }
		public static SpriteCollectionSize ForTk2dCamera() { return new SpriteCollectionSize(0.5f, 1.0f); }

		private SpriteCollectionSize(float orthoSize, float targetHeight)
		{
			this.orthoSize = orthoSize;
			this.targetHeight = targetHeight;
		}

		public float orthoSize;
		public float targetHeight;
	}

	static class SpriteCollectionGenerator 
	{
		public static tk2dSpriteCollectionData CreateFromTexture(Texture2D texture, SpriteCollectionSize size, Rect region, Vector2 anchor)
		{
			return CreateFromTexture(texture, size, new string[] { "Unnamed" }, new Rect[] { region },  new Vector2[] { anchor } );
		}
		
		public static tk2dSpriteCollectionData CreateFromTexture(Texture2D texture, SpriteCollectionSize size, string[] names, Rect[] regions, Vector2[] anchors)
		{
			GameObject go = new GameObject("SpriteCollection");
			tk2dSpriteCollectionData sc = go.AddComponent<tk2dSpriteCollectionData>();
			sc.Transient = true;
			sc.version = tk2dSpriteCollectionData.CURRENT_VERSION;
			
			sc.invOrthoSize = 1.0f / size.orthoSize;
			sc.halfTargetHeight = size.targetHeight * 0.5f;
			sc.premultipliedAlpha = false;
			
			sc.material = new Material(Shader.Find("tk2d/BlendVertexColor"));
			sc.material.mainTexture = texture;
			sc.materials = new Material[1] { sc.material };
			sc.textures = new Texture[1] { texture };
			
			float scale = 2.0f * size.orthoSize / size.targetHeight;
			
			// Generate geometry
			sc.spriteDefinitions = new tk2dSpriteDefinition[regions.Length];
			for (int i = 0; i < regions.Length; ++i)
			{
				sc.spriteDefinitions[i] = CreateDefinitionForRegionInTexture(texture, scale, regions[i], anchors[i]);
				sc.spriteDefinitions[i].name = names[i];
			}
			
			foreach (var def in sc.spriteDefinitions)
				def.material = sc.material;
			
			return sc;
		}

		static tk2dSpriteDefinition CreateDefinitionForRegionInTexture(Texture2D texture, float scale, Rect uvRegion, Vector2 anchor)
		{
			float h = uvRegion.height;
			float w = uvRegion.width;

	        Vector3 pos0 = new Vector3(-anchor.x * scale, -(h - anchor.y) * scale, 0.0f);
			Vector3 pos1 = pos0 + new Vector3(w * scale, h * scale, 0.0f);
			
			var def = new tk2dSpriteDefinition();
			def.flipped = false;
			def.extractRegion = false;
			def.name = texture.name;
			
			float fwidth = texture.width;
			float fheight = texture.height;
			
			Vector2 uvOffset = new Vector2(0.001f, 0.001f);
	        Vector2 v0 = new Vector2((uvRegion.x + uvOffset.x) / fwidth, 1.0f - (uvRegion.y + uvRegion.height + uvOffset.y) / fheight);
	        Vector2 v1 = new Vector2((uvRegion.x + uvRegion.width - uvOffset.x) / fwidth, 1.0f - (uvRegion.y - uvOffset.y) / fheight);
			
			Vector3 untrimmedPos0 = pos0;
			Vector3 untrimmedPos1 = pos1;
			
			def.positions = new Vector3[] {
				new Vector3(pos0.x, pos0.y, 0),
				new Vector3(pos1.x, pos0.y, 0),
				new Vector3(pos0.x, pos1.y, 0),
				new Vector3(pos1.x, pos1.y, 0)
			};
			
			def.uvs = new Vector2[] {
				new Vector2(v0.x,v0.y),
				new Vector2(v1.x,v0.y),
				new Vector2(v0.x,v1.y),
				new Vector2(v1.x,v1.y)
			};

			def.normals = new Vector3[0];
			def.tangents = new Vector4[0];
			
			def.indices = new int[] {
				0, 3, 1, 2, 3, 0	
			};
			
			Vector3 boundsMin = Vector3.Min(untrimmedPos0, untrimmedPos1);
			Vector3 boundsMax = Vector3.Max(untrimmedPos0, untrimmedPos1);
			def.boundsData = new Vector3[2] {
				(boundsMax + boundsMin) / 2.0f,
				(boundsMax - boundsMin)
			};
			def.untrimmedBoundsData = new Vector3[2] {
				(boundsMax + boundsMin) / 2.0f,
				(boundsMax - boundsMin)
			};
							
			return def;
		}
	}
}
