using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
/// <summary>
/// Sprite Definition.
/// </summary>
public class tk2dSpriteDefinition
{
	/// <summary>
	/// Collider type.
	/// </summary>
	public enum ColliderType
	{
		/// <summary>
		/// Do not create or destroy anything.
		/// </summary>
		Unset,
		
		/// <summary>
		/// If a collider exists, it will be destroyed. The sprite will be responsible in making sure there are no other colliders attached.
		/// </summary>
		None,
		
		/// <summary>
		/// Create a box collider.
		/// </summary>
		Box,
		
		/// <summary>
		/// Create a mesh collider.
		/// </summary>
		Mesh,
	}
	
	/// <summary>
	/// Name
	/// </summary>
	public string name;
	
	public Vector3[] boundsData;
	public Vector3[] untrimmedBoundsData;
	
	public Vector2 texelSize;
	
	/// <summary>
	/// Array of positions for sprite geometry.
	/// </summary>
    public Vector3[] positions;
	
	/// <summary>
	/// Array of normals for sprite geometry, zero length array if they dont exist
	/// </summary>
	public Vector3[] normals;
	
	/// <summary>
	/// Array of tangents for sprite geometry, zero length array if they dont exist
	/// </summary>
	public Vector4[] tangents;
	
	/// <summary>
	/// Array of UVs for sprite geometry, will match the position count.
	/// </summary>
    public Vector2[] uvs;
	/// <summary>
	/// Array of indices for sprite geometry.
	/// </summary>
    public int[] indices = new int[] { 0, 3, 1, 2, 3, 0 };
	/// <summary>
	/// The material used by this sprite. This is generally the same on all sprites in a colletion, but this is not
	/// true when multi-atlas spanning is enabled.
	/// </summary>
	public Material material;

	[System.NonSerialized]
	public Material materialInst;

	/// <summary>
	/// The material id used by this sprite. This is an index into the materials array and corresponds to the 
	/// material flag above.
	/// </summary>
	public int materialId;
	
	
	/// <summary>
	/// Source texture GUID - this is used by the inspector to find the source image without adding a unity dependency.
	/// </summary>
	public string sourceTextureGUID;
	/// <summary>
	/// Speficies if this texture is extracted from a larger texture source, for instance an atlas. This is used in the inspector.
	/// </summary>
	public bool extractRegion;
	public int regionX, regionY, regionW, regionH;
	
	/// <summary>
	/// Specifies if this texture is flipped to its side (rotated) in the atlas
	/// </summary>
	public bool flipped;
	
	/// <summary>
	/// Specifies if this texture has complex geometry
	/// </summary>
	public bool complexGeometry = false;
	
	/// <summary>
	/// Collider type
	/// </summary>
	public ColliderType colliderType = ColliderType.None;
	
	/// <summary>
	/// v0 and v1 are center and size respectively for box colliders when colliderType is Box.
	/// It is an array of vertices, and the geometry defined by indices when colliderType is Mesh.
	/// </summary>
	public Vector3[] colliderVertices; 
	public int[] colliderIndicesFwd;
	public int[] colliderIndicesBack;
	public bool colliderConvex;
	public bool colliderSmoothSphereCollisions;
	
	public bool Valid { get { return name.Length != 0; } }
}

[AddComponentMenu("2D Toolkit/Backend/tk2dSpriteCollectionData")]
/// <summary>
/// Sprite Collection Data.
/// </summary>
public class tk2dSpriteCollectionData : MonoBehaviour 
{
	public const int CURRENT_VERSION = 3;
	
	public int version;
	public bool materialIdsValid = false;
	public bool needMaterialInstance = false;
	public bool Transient { get; set; } // this should not get serialized

	/// <summary>
	/// An array of sprite definitions.
	/// </summary>
    public tk2dSpriteDefinition[] spriteDefinitions;
	
	/// <summary>
	/// Dictionary to look up sprite names. This will be initialized on first call to GetSpriteIdByName.
	/// </summary>
	Dictionary<string, int> spriteNameLookupDict = null;
	
	/// <summary>
	/// Whether premultiplied alpha is enabled on this sprite collection. This affects how tint colors are computed.
	/// </summary>
    public bool premultipliedAlpha;
	
	/// <summary>
	/// Only exists for backwards compatibility. Do not use or rely on this.
	/// </summary>
	public Material material;	
	
	/// <summary>
	/// An array of all materials used by this sprite collection.
	/// </summary>
	public Material[] materials;

	[System.NonSerialized]
	public Material[] materialInsts;

	
	/// <summary>
	/// An array of all textures used by this sprite collection.
	/// </summary>
	public Texture[] textures;
	
	/// <summary>
	/// Specifies if sprites span multiple atlases.
	/// </summary>
	public bool allowMultipleAtlases;
	
	/// <summary>
	/// The sprite collection GUI.
	/// </summary>
	public string spriteCollectionGUID;
	
	/// <summary>
	/// The name of the sprite collection.
	/// </summary>
	public string spriteCollectionName;

	/// <summary>
	/// Asset Name, used to load the asset
	/// </summary>
	public string assetName = "";	

	/// <summary>
	/// The size of the inv ortho size used to generate the sprite collection.
	/// </summary>
	public float invOrthoSize = 1.0f;
	
	/// <summary>
	/// Target height used to generate the sprite collection.
	/// </summary>
	public float halfTargetHeight = 1.0f;
	
	public int buildKey = 0;
	
	/// <summary>
	/// GUID of this object, used with <see cref="tk2dIndex"/>
	/// </summary>
	public string dataGuid = "";

	/// <summary>
	/// Returns the number of sprite definitions in this sprite collection.
	/// </summary>
    public int Count { get { return inst.spriteDefinitions.Length; } }

	/// <summary>
	/// When true, sprite collection will not be directly selectable
	/// </summary>
    public bool managedSpriteCollection = false;

	/// <summary>
	/// When true, spriteCollectionPlatforms & PlatformGUIDs are expected to have
	/// sensible data.
	/// </summary>
	public bool hasPlatformData = false;

	/// <summary>
	/// Returns an array of platform names.
	/// </summary>
    public string[] spriteCollectionPlatforms = null;

	/// <summary>
	/// Returns an array of GUIDs, each referring to an actual tk2dSpriteCollectionData object
	/// This object contains the actual sprite collection for the platform.
	/// </summary>
    public string[] spriteCollectionPlatformGUIDs = null;

	/// <summary>
	/// Resolves a sprite name and returns a unique id for the sprite.
	/// </summary>
	/// <returns>
	/// Unique Sprite Id. 0 if sprite isn't found.
	/// </returns>
	/// <param name='name'>Case sensitive sprite name, as defined in the sprite collection. This is usually the source filename excluding the extension</param>
	public int GetSpriteIdByName(string name)
	{
		inst.InitDictionary();
		int returnValue = 0;
		inst.spriteNameLookupDict.TryGetValue(name, out returnValue);
		return returnValue; // default to first sprite
	}
	
	/// <summary>
	/// Initializes the lookup dictionary
	/// </summary>
	public void InitDictionary()
	{
		if (spriteNameLookupDict == null)
		{
			spriteNameLookupDict = new Dictionary<string, int>(spriteDefinitions.Length);
			for (int i = 0; i < spriteDefinitions.Length; ++i)
			{
				spriteNameLookupDict[spriteDefinitions[i].name] = i;
			}
		}
	}
	
	/// <summary>
	/// Returns the first valid sprite definition
	/// </summary>
	public tk2dSpriteDefinition FirstValidDefinition
	{
		get 
		{
			foreach (var v in inst.spriteDefinitions)
			{
				if (v.Valid)
					return v;
			}
			return null;
		}
	}
	
	/// <summary>
	/// Returns the index of the first valid sprite definition
	/// </summary>
	public int FirstValidDefinitionIndex
	{
		get 
		{
			tk2dSpriteCollectionData data = inst;

			for (int i = 0; i < data.spriteDefinitions.Length; ++i)
				if (data.spriteDefinitions[i].Valid)
					return i;
			return -1;
		}
	}
	
	/// <summary>
	/// Internal function to make sure all material Ids are valid. Used in the tilemap editor
	/// </summary>
	public void InitMaterialIds()
	{
		if (inst.materialIdsValid)
			return;
		
		int firstValidIndex = -1;
		Dictionary<Material, int> materialLookupDict = new Dictionary<Material, int>();
		for (int i = 0; i < inst.materials.Length; ++i)
		{
			if (firstValidIndex == -1 && inst.materials[i] != null)
				firstValidIndex = i;
			materialLookupDict[materials[i]] = i;
		}
		if (firstValidIndex == -1)
		{
			Debug.LogError("Init material ids failed.");
		}
		else
		{
			foreach (var v in inst.spriteDefinitions)			
			{
				if (!materialLookupDict.TryGetValue(v.material, out v.materialId))
					v.materialId = firstValidIndex;
			}
			inst.materialIdsValid = true;
		}
	}

	tk2dSpriteCollectionData platformSpecificData = null;

	// Returns the active instance
	public tk2dSpriteCollectionData inst
	{
		get 
		{
			if (platformSpecificData == null)
			{
				if (hasPlatformData)
				{
					string systemPlatform = tk2dSystem.CurrentPlatform;
					string guid = "";

					for (int i = 0; i < spriteCollectionPlatforms.Length; ++i)
					{
						if (spriteCollectionPlatforms[i] == systemPlatform)
						{
							guid = spriteCollectionPlatformGUIDs[i];
							break;							
						}
					}
					if (guid.Length == 0)
						guid = spriteCollectionPlatformGUIDs[0]; // failed to find platform, pick the first one

					platformSpecificData = tk2dSystem.LoadResourceByGUID<tk2dSpriteCollectionData>(guid);
				}
				else
				{
					platformSpecificData = this;
				}
			}
			platformSpecificData.Init(); // awake is never called, so we initialize explicitly
			return platformSpecificData;
		}
	}
	
	void Init()
	{
		// check if already initialized
		if (materialInsts != null)
			return;

		if (spriteDefinitions == null) spriteDefinitions = new tk2dSpriteDefinition[0];
		if (materials == null) materials = new Material[0];

		materialInsts = new Material[materials.Length];
		if (needMaterialInstance)
		{
			for (int i = 0; i < materials.Length; ++i)
			{
				materialInsts[i] = Instantiate(materials[i]) as Material;
#if UNITY_EDITOR
				materialInsts[i].hideFlags = HideFlags.DontSave;
#endif
			}
			for (int i = 0; i < spriteDefinitions.Length; ++i)
			{
				tk2dSpriteDefinition def = spriteDefinitions[i];
				def.materialInst = materialInsts[def.materialId];
			}
		}
		else
		{
			for (int i = 0; i < spriteDefinitions.Length; ++i)
			{
				tk2dSpriteDefinition def = spriteDefinitions[i];
				def.materialInst = def.material;
			}
		}
	}

	/// <summary>
	/// Create a sprite collection at runtime from a texture and user specified regions.
	/// Please ensure that names, regions & anchor arrays have same dimension.
	/// Use <see cref="tk2dBaseSprite.CreateFromTexture"/> if you need to create only one sprite from a texture.
	/// </summary>
	public static tk2dSpriteCollectionData CreateFromTexture(Texture2D texture, tk2dRuntime.SpriteCollectionSize size, string[] names, Rect[] regions, Vector2[] anchors)
	{
		return tk2dRuntime.SpriteCollectionGenerator.CreateFromTexture(texture, size, names, regions, anchors);
	}

	public void ResetPlatformData()
	{
		if (hasPlatformData && platformSpecificData)
		{
			platformSpecificData = null;
		}
		
		materialInsts = null;
	}

	void OnDestroy()
	{
		if (Transient)
		{
			foreach (Material material in materials)
			{
				DestroyImmediate(material);
			}
		}
		else if (needMaterialInstance) // exclusive
		{
			foreach (Material material in materialInsts)
			{
				DestroyImmediate(material);
			}
		}

		ResetPlatformData();
	}
}
