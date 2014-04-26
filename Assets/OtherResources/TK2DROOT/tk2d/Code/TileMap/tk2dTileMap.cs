using UnityEngine;
using System.Collections.Generic;

using tk2dRuntime.TileMap;

[ExecuteInEditMode]
[AddComponentMenu("2D Toolkit/TileMap/TileMap")]
/// <summary>
/// Tile Map
/// </summary>
public class tk2dTileMap : MonoBehaviour, tk2dRuntime.ISpriteCollectionForceBuild
{
	/// <summary>
	/// This is a link to the editor data object (tk2dTileMapEditorData).
	/// It contains presets, and other data which isn't really relevant in game.
	/// </summary>
	public string editorDataGUID = "";
	
	/// <summary>
	/// Tile map data, stores shared parameters for tilemaps
	/// </summary>
	public tk2dTileMapData data;
	
	/// <summary>
	/// Tile map render and collider object
	/// </summary>
	public GameObject renderData;
	
	/// <summary>
	/// The sprite collection used by the tilemap
	/// </summary>
	[SerializeField]
	private tk2dSpriteCollectionData spriteCollection = null;
	public tk2dSpriteCollectionData Editor__SpriteCollection 
	{ 
		get 
		{ 
			return spriteCollection; 
		} 
		set
		{
			_spriteCollectionInst = null;
			spriteCollection = value;
			if (spriteCollection != null)
				_spriteCollectionInst = spriteCollection.inst;
		}
	}
	
	tk2dSpriteCollectionData _spriteCollectionInst = null;
	public tk2dSpriteCollectionData SpriteCollectionInst
	{
		get 
		{
			if (_spriteCollectionInst == null && spriteCollection != null)
				_spriteCollectionInst = spriteCollection.inst;
			return _spriteCollectionInst;
		}
	}
	
	[SerializeField]
	int spriteCollectionKey;
	

	/// <summary>Width of the tilemap</summary>
	public int width = 128;
	/// <summary>Height of the tilemap</summary>
	public int height = 128;
	
	/// <summary>X axis partition size for this tilemap</summary>
	public int partitionSizeX = 32;
	/// <summary>Y axis partition size for this tilemap</summary>
	public int partitionSizeY = 32;

	[SerializeField]
	Layer[] layers;
	
	[SerializeField]
	ColorChannel colorChannel;
	
	public int buildKey;
	[SerializeField]
	bool _inEditMode = false;
	public bool AllowEdit { get { return _inEditMode; } }
	
	// do we need to retain meshes?
	public bool serializeRenderData = false;
	
	// holds a path to a serialized mesh, uses this to work out dump directory for meshes
	public string serializedMeshPath;
	
	void Awake()
	{
		if (spriteCollection != null)
			_spriteCollectionInst = spriteCollection.inst;
		
		bool spriteCollectionKeyMatch = true;
		if (SpriteCollectionInst && SpriteCollectionInst.buildKey != spriteCollectionKey) spriteCollectionKeyMatch = false;

		if (Application.platform == RuntimePlatform.WindowsEditor ||
			Application.platform == RuntimePlatform.OSXEditor)
		{
			if ((Application.isPlaying && _inEditMode == true) || !spriteCollectionKeyMatch)
			{
				// Switched to edit mode while still in edit mode, rebuild
				Build(BuildFlags.ForceBuild);
			}
		}
		else
		{
			if (_inEditMode == true)
			{
				Debug.LogError("Tilemap " + name + " is still in edit mode. Please fix." +
					"Building overhead will be significant.");
				Build(BuildFlags.ForceBuild);
			}
			else if (!spriteCollectionKeyMatch)
			{
				Build(BuildFlags.ForceBuild);
			}
		}
	}
	
	[System.Flags]
	public enum BuildFlags {
		Default = 0,
		EditMode = 1,
		ForceBuild = 2
	};
	
	public void Build() { Build(BuildFlags.Default); }
	public void ForceBuild() { Build(BuildFlags.ForceBuild); }
	
	// Clears all spawned instances, but retains the renderData object
	void ClearSpawnedInstances()
	{
		if (layers == null)
			return;

		for (int layerIdx = 0; layerIdx < layers.Length; ++layerIdx)
		{
			Layer layer = layers[layerIdx];
			for (int chunkIdx = 0; chunkIdx < layer.spriteChannel.chunks.Length; ++chunkIdx)
			{
				var chunk = layer.spriteChannel.chunks[chunkIdx];

				if (chunk.gameObject == null)
					continue;
				
				var transform = chunk.gameObject.transform;
				List<Transform> children = new List<Transform>();
				for (int i = 0; i < transform.childCount; ++i)
					children.Add(transform.GetChild(i));
				for (int i = 0; i < children.Count; ++i)
					DestroyImmediate(children[i].gameObject);
			}
		}
	}
	
	public void Build(BuildFlags buildFlags)
	{
		if (spriteCollection != null)
			_spriteCollectionInst = spriteCollection.inst;
		
		
#if UNITY_EDITOR || !UNITY_FLASH
		// Sanitize tilePrefabs input, to avoid branches later
		if (data != null)
		{
			if (data.tilePrefabs == null)
				data.tilePrefabs = new Object[SpriteCollectionInst.Count];
			else if (data.tilePrefabs.Length != SpriteCollectionInst.Count)
				System.Array.Resize(ref data.tilePrefabs, SpriteCollectionInst.Count);
			
			// Fix up data if necessary
			BuilderUtil.InitDataStore(this);
		}
		else
		{
			return;
		}

		// Sanitize sprite collection material ids
		if (SpriteCollectionInst)
			SpriteCollectionInst.InitMaterialIds();
			
		
		bool editMode = (buildFlags & BuildFlags.EditMode) != 0;
		bool forceBuild = (buildFlags & BuildFlags.ForceBuild) != 0;

		// When invalid, everything needs to be rebuilt
		if (SpriteCollectionInst && SpriteCollectionInst.buildKey != spriteCollectionKey)
			forceBuild = true;

		if (forceBuild)
			ClearSpawnedInstances();

		BuilderUtil.CreateRenderData(this, editMode);
		
		RenderMeshBuilder.Build(this, editMode, forceBuild);
		
		if (!editMode)
		{
			ColliderBuilder.Build(this);
			BuilderUtil.SpawnPrefabs(this);
		}
		
		// Clear dirty flag on everything
		foreach (var layer in layers)
			layer.ClearDirtyFlag();
		if (colorChannel != null)
			colorChannel.ClearDirtyFlag();
		
		// One random number to detect undo
		buildKey = Random.Range(0, int.MaxValue);
		
		// Update sprite collection key
		if (SpriteCollectionInst)
			spriteCollectionKey = SpriteCollectionInst.buildKey;
#endif
	}
	
	/// <summary>
	/// Gets the tile coordinate at position. This can be used to obtain tile or color data explicitly from layers
	/// Returns true if the position is within the tilemap bounds
	/// </summary>
	public bool GetTileAtPosition(Vector3 position, out int x, out int y)
	{
		float ox, oy;
		bool b = GetTileFracAtPosition(position, out ox, out oy);
		x = (int)ox;
		y = (int)oy;
		return b;
	}
	
	/// <summary>
	/// Gets the tile coordinate at position. This can be used to obtain tile or color data explicitly from layers
	/// The fractional value returned is the fraction into the current tile
	/// Returns true if the position is within the tilemap bounds
	/// </summary>
	public bool GetTileFracAtPosition(Vector3 position, out float x, out float y)
	{
		switch (data.tileType)
		{
		case tk2dTileMapData.TileType.Rectangular:
		{
			Vector3 localPosition = transform.worldToLocalMatrix.MultiplyPoint(position);
			x = (localPosition.x - data.tileOrigin.x) / data.tileSize.x;
			y = (localPosition.y - data.tileOrigin.y) / data.tileSize.y;
			return (x >= 0 && x <= width && y >= 0 && y <= height);
		}
		case tk2dTileMapData.TileType.Isometric:
		{
			if (data.tileSize.x == 0.0f)
				break;

			float tileAngle = Mathf.Atan2(data.tileSize.y, data.tileSize.x / 2.0f);
			
			Vector3 localPosition = transform.worldToLocalMatrix.MultiplyPoint(position);
			x = (localPosition.x - data.tileOrigin.x) / data.tileSize.x;
			y = ((localPosition.y - data.tileOrigin.y) / (data.tileSize.y));
			
			float fy = y * 0.5f;
			int iy = (int)fy;
			
			float fry = fy - iy;
			float frx = x % 1.0f;
			
			x = (int)x;
			y = iy * 2;
			
			if (frx > 0.5f)
			{
				if (fry > 0.5f && Mathf.Atan2(1.0f - fry, (frx - 0.5f) * 2) < tileAngle)
					y += 1;
				else if (fry < 0.5f && Mathf.Atan2(fry, (frx - 0.5f) * 2) < tileAngle)
					y -= 1;
			}
			else if (frx < 0.5f)
			{
				if (fry > 0.5f && Mathf.Atan2(fry - 0.5f, frx * 2) > tileAngle)
				{
					y += 1;
					x -= 1;
				}
				
				if (fry < 0.5f && Mathf.Atan2(fry, (0.5f - frx) * 2) < tileAngle)
				{
					y -= 1;
					x -= 1;
				}
			}
			
			return (x >= 0 && x <= width && y >= 0 && y <= height);
		}
		}
		
		x = 0.0f;
		y = 0.0f;
		return false;
	}
	
	/// <summary>
	/// Returns the tile position in world space
	/// </summary>
	public Vector3 GetTilePosition(int x, int y)
	{
		Vector3 localPosition = new Vector3(
			x * data.tileSize.x + data.tileOrigin.x,
			y * data.tileSize.y + data.tileOrigin.y,
			0);
		return transform.localToWorldMatrix.MultiplyPoint(localPosition);
	}
	
	/// <summary>
	/// Gets the tile at position. This can be used to obtain tile data, etc
	/// -1 = no data or empty tile
	/// </summary>
	public int GetTileIdAtPosition(Vector3 position, int layer)
	{
		if (layer < 0 || layer >= layers.Length)
			return -1;
		
		int x, y;
		if (!GetTileAtPosition(position, out x, out y))
			return -1;
		
		return layers[layer].GetTile(x, y);
	}
	
	/// <summary>
	/// Returns the tile info chunk for the tile. Use this to store additional metadata
	/// </summary>
	public tk2dRuntime.TileMap.TileInfo GetTileInfoForTileId(int tileId)
	{
		return data.GetTileInfoForSprite(tileId);
	}
	
	/// <summary>
	/// Gets the tile at position. This can be used to obtain tile data, etc
	/// -1 = no data or empty tile
	/// </summary>
	public Color GetInterpolatedColorAtPosition(Vector3 position)
	{
		Vector3 localPosition = transform.worldToLocalMatrix.MultiplyPoint(position);
		int x = (int)((localPosition.x - data.tileOrigin.x) / data.tileSize.x);
		int y = (int)((localPosition.y - data.tileOrigin.y) / data.tileSize.y);
	
		if (colorChannel == null || colorChannel.IsEmpty)
			return Color.white;
		
		if (x < 0 || x >= width ||
			y < 0 || y >= height)
		{
			return colorChannel.clearColor;
		}
		
		int offset;
		ColorChunk colorChunk = colorChannel.FindChunkAndCoordinate(x, y, out offset);
		
		if (colorChunk.Empty)
		{
			return colorChannel.clearColor;
		}
		else
		{
			int colorChunkRowOffset = partitionSizeX + 1;
			Color tileColorx0y0 = colorChunk.colors[offset];
			Color tileColorx1y0 = colorChunk.colors[offset + 1];
			Color tileColorx0y1 = colorChunk.colors[offset + colorChunkRowOffset];
			Color tileColorx1y1 = colorChunk.colors[offset + colorChunkRowOffset + 1];
			
			float wx = x * data.tileSize.x + data.tileOrigin.x;
			float wy = y * data.tileSize.y + data.tileOrigin.y;
			
			float ix = (localPosition.x - wx) / data.tileSize.x;
			float iy = (localPosition.y - wy) / data.tileSize.y;
			
			Color cy0 = Color.Lerp(tileColorx0y0, tileColorx1y0, ix);
			Color cy1 = Color.Lerp(tileColorx0y1, tileColorx1y1, ix);
			return Color.Lerp(cy0, cy1, iy);
		}
	}
	
	// ISpriteCollectionBuilder
	public bool UsesSpriteCollection(tk2dSpriteCollectionData spriteCollection)
	{
		return spriteCollection == this.spriteCollection || _spriteCollectionInst == spriteCollection;
	}
	
#if UNITY_EDITOR
	public void BeginEditMode()
	{
		_inEditMode = true;
		
		// Destroy all children
		if (layers == null)
			return;

		ClearSpawnedInstances();
		
		Build(BuildFlags.EditMode | BuildFlags.ForceBuild);
	}

	public void EndEditMode()
	{
		_inEditMode = false;
		Build(BuildFlags.ForceBuild);
		
		if (serializeRenderData && renderData != null)
		{
#if (UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4)
			Debug.LogError("Prefab needs to be updated");
#else
			GameObject go = UnityEditor.PrefabUtility.FindValidUploadPrefabInstanceRoot(renderData);
			Object obj = UnityEditor.PrefabUtility.GetPrefabParent(go);
			if (obj != null)
				UnityEditor.PrefabUtility.ReplacePrefab(go, obj, UnityEditor.ReplacePrefabOptions.ConnectToPrefab);
#endif
		}
	}
	
	public bool AreSpritesInitialized()
	{
		return layers != null;
	}
	
	public bool HasColorChannel()
	{
		return (colorChannel != null && !colorChannel.IsEmpty);
	}
	
	public void CreateColorChannel()
	{
		colorChannel = new ColorChannel(width, height, partitionSizeX, partitionSizeY);
		colorChannel.Create();
	}
	
	public void DeleteColorChannel()
	{
		colorChannel.Delete();
	}
	
	public void DeleteSprites(int layerId, int x0, int y0, int x1, int y1)
	{
		int numTilesX = x1 - x0 + 1;
		int numTilesY = y1 - y0 + 1;
		var layer = layers[layerId];
		for (int y = 0; y < numTilesY; ++y)
		{
			for (int x = 0; x < numTilesX; ++x)
			{
				layer.SetTile(x0 + x, y0 + y, -1);
			}
		}
		
		layer.OptimizeIncremental();
	}
#endif
	
	// used by util functions
	public Mesh GetOrCreateMesh()
	{
#if UNITY_EDITOR
		Mesh mesh = new Mesh();
		if (serializeRenderData && renderData)
		{
			if (serializedMeshPath == null) serializedMeshPath = "";
			
			if (serializedMeshPath.Length == 0)
			{
				// find one serialized mesh
				MeshFilter[] meshFilters = renderData.gameObject.GetComponentsInChildren<MeshFilter>();
				foreach (var v in meshFilters)
				{
					Mesh m = v.sharedMesh;
					serializedMeshPath = UnityEditor.AssetDatabase.GetAssetPath(m);
					if (serializedMeshPath.Length > 0) break;
				}
			}
			if (serializedMeshPath.Length == 0)
			{
				MeshCollider[] meshColliders = renderData.gameObject.GetComponentsInChildren<MeshCollider>();
				foreach (var v in meshColliders)
				{
					Mesh m = v.sharedMesh;
					serializedMeshPath = UnityEditor.AssetDatabase.GetAssetPath(m);
					if (serializedMeshPath.Length > 0) break;
				}				
			}
			
			if (serializedMeshPath.Length == 0)
			{
				Debug.LogError("Unable to serialize meshes - please resave.");
				serializeRenderData = false; 
			}
			else
			{
				// save the mesh
				string path = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(serializedMeshPath);
				UnityEditor.AssetDatabase.CreateAsset(mesh, path);
			}
		}
		return mesh;
#else
		return new Mesh();
#endif
	}
	
	public void TouchMesh(Mesh mesh)
	{
#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(mesh);
#endif
	}
	
	public void DestroyMesh(Mesh mesh)
	{
#if UNITY_EDITOR
		if (UnityEditor.AssetDatabase.GetAssetPath(mesh).Length != 0)
		{
			mesh.Clear();
			UnityEditor.AssetDatabase.DeleteAsset(UnityEditor.AssetDatabase.GetAssetPath(mesh));
		}
		else
		{
			DestroyImmediate(mesh);
		}
#else
		DestroyImmediate(mesh);
#endif
	}
	
	public Layer[] Layers
	{
		get { return layers; }
		set { layers = value; }
	}
	
	public ColorChannel ColorChannel
	{
		get { return colorChannel; }
		set { colorChannel = value; }
	}
}
