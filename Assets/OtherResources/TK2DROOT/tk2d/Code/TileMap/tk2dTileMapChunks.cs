using UnityEngine;
using System.Collections;

namespace tk2dRuntime.TileMap
{
	[System.Serializable]
	public class LayerSprites
	{
		public int[] spriteIds;
		public LayerSprites()
		{
			spriteIds = new int[0];
		}
	}
	
	[System.Serializable]
	public class SpriteChunk
	{
		bool dirty;
		public int[] spriteIds;
		public GameObject gameObject;
		public Mesh mesh;
		public MeshCollider meshCollider;
		public Mesh colliderMesh;
		public SpriteChunk() { spriteIds = new int[0]; }
		
		public bool Dirty
		{
			get { return dirty; }
			set { dirty = value; }
		}
		
		public bool IsEmpty
		{
			get { return spriteIds.Length == 0; }
		}
		
		public bool HasGameData
		{
			get { return gameObject != null || mesh != null || meshCollider != null ||  colliderMesh != null; }
		}
		
		public void DestroyGameData(tk2dTileMap tileMap)
		{
			if (mesh != null) tileMap.DestroyMesh(mesh);
			if (gameObject != null) GameObject.DestroyImmediate(gameObject);
			gameObject = null;
			mesh = null;
			
			DestroyColliderData(tileMap);
		}
		
		public void DestroyColliderData(tk2dTileMap tileMap)
		{
			if (colliderMesh != null) 
				tileMap.DestroyMesh(colliderMesh);
			if (meshCollider != null && meshCollider.sharedMesh != null && meshCollider.sharedMesh != colliderMesh) 
				tileMap.DestroyMesh(meshCollider.sharedMesh);
			if (meshCollider != null) GameObject.DestroyImmediate(meshCollider);
			meshCollider = null;
			colliderMesh = null;
		}
	}
	
	[System.Serializable]
	public class SpriteChannel
	{
		public SpriteChunk[] chunks;
		public SpriteChannel() { chunks = new SpriteChunk[0]; }
	}
	
	[System.Serializable]
	public class ColorChunk
	{
		public bool Dirty { get; set; }
		public bool Empty { get { return colors.Length == 0; } }
		public Color32[] colors;
		public ColorChunk() { colors = new Color32[0]; }
	}
	
	[System.Serializable]
	public class ColorChannel
	{
		public ColorChannel(int width, int height, int divX, int divY)
		{
			Init(width, height, divX, divY);	
		}
		
		public Color clearColor = Color.white;
		public ColorChunk[] chunks;
		public ColorChannel() { chunks = new ColorChunk[0]; }

		public void Init(int width, int height, int divX, int divY)
		{
			numColumns = (width + divX - 1) / divX;
			numRows = (height + divY - 1) / divY;
			chunks = new ColorChunk[0];
			this.divX = divX;
			this.divY = divY;
		}
		
		public ColorChunk FindChunkAndCoordinate(int x, int y, out int offset)
		{
			int cellX = Mathf.Max(x - 1, 0) / divX;
			int cellY = Mathf.Max(y - 1, 0) / divY;
			int idx = cellY * numColumns + cellX;
			var chunk = chunks[idx];
			int localX = x - cellX * divX;
			int localY = y - cellY * divY;
			offset = localY * (divX + 1) + localX;
			return chunk;
		}
		
		public Color GetColor(int x, int y)
		{
			if (IsEmpty)
				return clearColor;
			
			int offset;
			var chunk = FindChunkAndCoordinate(x, y, out offset);
			if (chunk.colors.Length == 0)
				return clearColor;
			else
				return chunk.colors[offset];
		}
		
		// create chunk if it doesn't already exist
		void InitChunk(ColorChunk chunk)
		{
			if (chunk.colors.Length == 0)
			{
				chunk.colors = new Color32[(divX + 1) * (divY + 1)];
				for (int i = 0; i < chunk.colors.Length; ++i)				
					chunk.colors[i] = clearColor;
			}
		}
		
		public void SetColor(int x, int y, Color color)
		{
			if (IsEmpty)
				Create();
			
			int cellLocalWidth = divX + 1;
			
			// at edges, set to all overlapping coordinates
			int cellX = Mathf.Max(x - 1, 0) / divX;
			int cellY = Mathf.Max(y - 1, 0) / divY;
			var chunk = GetChunk(cellX, cellY, true);
			int localX = x - cellX * divX;
			int localY = y - cellY * divY;
			chunk.colors[localY * cellLocalWidth + localX] = color;
			chunk.Dirty = true;
			
			// May need to set it to adjancent cells too
			bool needX = false, needY = false;
			if (x != 0 && (x % divX) == 0 && (cellX + 1) < numColumns)
				needX = true;
			if (y != 0 && (y % divY) == 0 && (cellY + 1) < numRows)
				needY = true;
			
			if (needX)
			{
				int cx = cellX + 1;
				chunk = GetChunk(cx, cellY, true);
				localX = x - cx * divX;
				localY = y - cellY * divY;
				chunk.colors[localY * cellLocalWidth + localX] = color;
			}
			if (needY)
			{
				int cy = cellY + 1;
				chunk = GetChunk(cellX, cy, true);
				localX = x - cellX * divX;
				localY = y - cy * divY;
				chunk.colors[localY * cellLocalWidth + localX] = color;
			}
			if (needX && needY)
			{
				int cx = cellX + 1;
				int cy = cellY + 1;
				chunk = GetChunk(cx, cy, true);
				localX = x - cx * divX;
				localY = y - cy * divY;
				chunk.colors[localY * cellLocalWidth + localX] = color;
			}
		}
		
		public ColorChunk GetChunk(int x, int y)
		{
			if (chunks == null || chunks.Length == 0)
				return null;
			
			return chunks[y * numColumns + x];
		}		
		
		public ColorChunk GetChunk(int x, int y, bool init)
		{
			if (chunks == null || chunks.Length == 0)
				return null;
			
			var chunk = chunks[y * numColumns + x];
			InitChunk(chunk);
			return chunk;
		}		
		
		public void ClearChunk(ColorChunk chunk)
		{
			for (int i = 0; i < chunk.colors.Length; ++i)
				chunk.colors[i] = clearColor;
		}
		
		public void ClearDirtyFlag()
		{
			foreach (var chunk in chunks)
				chunk.Dirty = false;
		}
		
		public void Clear(Color color)
		{
			clearColor = color;
			foreach (var chunk in chunks)
				ClearChunk(chunk);
			Optimize();
		}
		
		public void Delete()
		{
			chunks = new ColorChunk[0];
		}
		
		public void Create()
		{
			chunks = new ColorChunk[numColumns * numRows];
			for (int i = 0; i < chunks.Length; ++i)
				chunks[i] = new ColorChunk();
		}
		
		void Optimize(ColorChunk chunk)
		{
			bool empty = true;
			Color32 clearColor32 = this.clearColor;
			foreach (var c in chunk.colors)
			{
				if (c.r != clearColor32.r ||
					c.g != clearColor32.g ||
					c.b != clearColor32.b ||
					c.a != clearColor32.a)
				{
					empty = false;
					break;
				}
			}
			
			if (empty)
				chunk.colors = new Color32[0];
		}
		
		public void Optimize()
		{
			foreach (var chunk in chunks)
				Optimize(chunk);
		}
		
		public bool IsEmpty { get { return chunks.Length == 0; } }
		
		public int NumActiveChunks
		{
			get
			{
				int numActiveChunks = 0;
				foreach (var chunk in chunks)
					if (chunk != null && chunk.colors != null && chunk.colors.Length > 0)
						numActiveChunks++;
				return numActiveChunks;
			}
		}
		
		public int numColumns, numRows;
		public int divX, divY;
	}
	
	[System.Serializable]
	public class Layer
	{
		public int hash;
		public SpriteChannel spriteChannel;
		public Layer(int hash, int width, int height, int divX, int divY)
		{
			spriteChannel = new SpriteChannel();
			Init(hash, width, height, divX, divY);
		}
		
		public void Init(int hash, int width, int height, int divX, int divY)
		{
			this.divX = divX;
			this.divY = divY;
			this.hash = hash;
			this.numColumns = (width + divX - 1) / divX;
			this.numRows = (height + divY - 1) / divY;
			this.width = width;
			this.height = height;
			spriteChannel.chunks = new SpriteChunk[numColumns * numRows];
			for (int i = 0; i < numColumns * numRows; ++i)
				spriteChannel.chunks[i] = new SpriteChunk();
		}
		
		public bool IsEmpty { get { return spriteChannel.chunks.Length == 0; } }
		
		public void Create()
		{
			spriteChannel.chunks = new SpriteChunk[numColumns * numRows];
		}
		
		public int[] GetChunkData(int x, int y)
		{
			return GetChunk(x, y).spriteIds;
		}
		
		public SpriteChunk GetChunk(int x, int y)
		{
			return spriteChannel.chunks[y * numColumns + x];
		}
		
		SpriteChunk FindChunkAndCoordinate(int x, int y, out int offset)
		{
			int cellX = x / divX;
			int cellY = y / divY;
			var chunk = spriteChannel.chunks[cellY * numColumns + cellX];
			int localX = x - cellX * divX;
			int localY = y - cellY * divY;
			offset = localY * divX + localX;
			return chunk;
		}
		
		public void SetTile(int x, int y, int spriteId)
		{
			int offset;
			var chunk = FindChunkAndCoordinate(x, y, out offset);
			CreateChunk(chunk);
			chunk.spriteIds[offset] = spriteId;
			chunk.Dirty = true;
		}
		
		public int GetTile(int x, int y)
		{
			int offset;
			var chunk = FindChunkAndCoordinate(x, y, out offset);
			if (chunk.spriteIds == null || chunk.spriteIds.Length == 0)
				return -1;
			return chunk.spriteIds[offset];
		}
		
		void CreateChunk(SpriteChunk chunk)
		{
			if (chunk.spriteIds == null || chunk.spriteIds.Length == 0)
			{
				chunk.spriteIds = new int[divX * divY];
				for (int i = 0; i < divX * divY; ++i)
					chunk.spriteIds[i] = -1;
			}
		}
		
		void Optimize(SpriteChunk chunk)
		{
			bool empty = true;
			foreach (var v in chunk.spriteIds)
			{
				if (v != -1)
				{
					empty = false;
					break;
				}
			}
			if (empty)
				chunk.spriteIds = new int[0];
		}
		
		public void Optimize()
		{
			foreach (var chunk in spriteChannel.chunks)
				Optimize(chunk);
		}
		
		public void OptimizeIncremental()
		{
			foreach (var chunk in spriteChannel.chunks)
			{
				if (chunk.Dirty)				
					Optimize(chunk);
			}
		}
		
		public void ClearDirtyFlag()
		{
			foreach (var chunk in spriteChannel.chunks)
				chunk.Dirty = false;
		}
		
		public int NumActiveChunks
		{
			get
			{
				int numActiveChunks = 0;
				foreach (var chunk in spriteChannel.chunks)
					if (!chunk.IsEmpty)
						numActiveChunks++;
				return numActiveChunks;
			}
		}
		
		public int width, height;
		public int numColumns, numRows;
		public int divX, divY;
		public GameObject gameObject;
	}
}
