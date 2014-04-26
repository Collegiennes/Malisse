using UnityEngine;
using System.Collections;

[AddComponentMenu("2D Toolkit/Sprite/tk2d9SliceSprite")]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
/// <summary>
/// Sprite implementation that implements 9-slice scaling. Doesn't support diced sprites.
/// The interface takes care of sprite unit conversions for border[Top|Bottom|Left|Right]
/// </summary>
public class tk2dSlicedSprite : tk2dBaseSprite
{
	/// <summary>
	/// Anchor.
	/// NOTE: The order in this enum is deliberate, to initialize at LowerLeft for backwards compatibility.
	/// This is also the reason it is local here. Other Anchor enums are NOT compatbile. Do not cast.
	/// </summary>
    public enum Anchor
    {
		/// <summary>Lower left</summary>
		LowerLeft,
		/// <summary>Lower center</summary>
		LowerCenter,
		/// <summary>Lower right</summary>
		LowerRight,
		/// <summary>Middle left</summary>
		MiddleLeft,
		/// <summary>Middle center</summary>
		MiddleCenter,
		/// <summary>Middle right</summary>
		MiddleRight,
		/// <summary>Upper left</summary>
		UpperLeft,
		/// <summary>Upper center</summary>
		UpperCenter,
		/// <summary>Upper right</summary>
		UpperRight,
    }
	
	Mesh mesh;
	Vector2[] meshUvs;
	Vector3[] meshVertices;
	Color[] meshColors;
	int[] meshIndices;
	
	[SerializeField]
	Vector2 _dimensions = new Vector2(50.0f, 50.0f);
	[SerializeField]
	Anchor _anchor = Anchor.LowerLeft;
	
	/// <summary>
	/// Legacy mode (uses scale to adjust overall size).
	/// Newly created sprites should have this set to false.
	/// </summary>
	public bool legacyMode = true;
	
	/// <summary>
	/// Gets or sets the dimensions.
	/// </summary>
	/// <value>
	/// Use this to change the dimensions of the sliced sprite in pixel units
	/// </value>
	public Vector2 dimensions
	{ 
		get { return _dimensions; } 
		set
		{
			if (value != _dimensions)
			{
				_dimensions = value;
				UpdateVertices();
#if UNITY_EDITOR
				EditMode__CreateCollider();
#endif
				UpdateCollider();
			}
		}
	}
	
	public Anchor anchor
	{
		get { return _anchor; }
		set
		{
			if (value != _anchor)
			{
				_anchor = value;
				UpdateVertices();
#if UNITY_EDITOR
				EditMode__CreateCollider();
#endif
				UpdateCollider();
			}
		}
	}
	
	/// <summary>
	/// Top border in sprite fraction (0 - Top, 1 - Bottom)
	/// </summary>
	public float borderTop = 0.2f;
	/// <summary>
	/// Bottom border in sprite fraction (0 - Bottom, 1 - Top)
	/// </summary>
	public float borderBottom = 0.2f;
	/// <summary>
	/// Left border in sprite fraction (0 - Left, 1 - Right)
	/// </summary>
	public float borderLeft = 0.2f;
	/// <summary>
	/// Right border in sprite fraction (1 - Right, 0 - Left)
	/// </summary>
	public float borderRight = 0.2f;
	/// <summary>
	/// The anchor position for this sliced sprite
	/// </summary>
	
	new void Awake()
	{
		base.Awake();
		
		// Create mesh, independently to everything else
		mesh = new Mesh();
		mesh.hideFlags = HideFlags.DontSave;
		GetComponent<MeshFilter>().mesh = mesh;
		
		// This will not be set when instantiating in code
		// In that case, Build will need to be called
		if (Collection)
		{
			// reset spriteId if outside bounds
			// this is when the sprite collection data is corrupt
			if (_spriteId < 0 || _spriteId >= Collection.Count)
				_spriteId = 0;
			
			Build();
			
			if (boxCollider == null)
				boxCollider = GetComponent<BoxCollider>();
		}
	}
	
	protected void OnDestroy()
	{
		if (mesh)
		{
#if UNITY_EDITOR
			DestroyImmediate(mesh);
#else
			Destroy(mesh);
#endif
		}
	}
	
	new protected void SetColors(Color[] dest)
	{
		Color c = _color;
        if (Collection.premultipliedAlpha) { c.r *= c.a; c.g *= c.a; c.b *= c.a; }
		for (int i = 0; i < dest.Length; ++i)
			dest[i] = c;
	}
	
	// Calculated center and extents
	Vector3 boundsCenter = Vector3.zero, boundsExtents = Vector3.zero;
	
	protected void SetGeometry(Vector3[] vertices, Vector2[] uvs)
	{
		var sprite = Collection.spriteDefinitions[spriteId];
		if (sprite.positions.Length == 4)
		{
			float colliderExtentZ = (sprite.colliderType == tk2dSpriteDefinition.ColliderType.Box)?(sprite.colliderVertices[1].z * _scale.z):0.1f;
			
			if (legacyMode)
			{
				// in legacy mode, scale is used to determine the total size of the sliced sprite
				float sx = _scale.x;
				float sy = _scale.y;
				
				Vector3[] srcVert = sprite.positions;
				Vector3 dx = srcVert[1] - srcVert[0];
				Vector3 dy = srcVert[2] - srcVert[0];
				
				Vector2[] srcUv = sprite.uvs;
				Vector2 duvx = sprite.uvs[1] - sprite.uvs[0];
				Vector2 duvy = sprite.uvs[2] - sprite.uvs[0];
				
				Vector3 origin = new Vector3(srcVert[0].x * sx, srcVert[0].y * sy, srcVert[0].z * _scale.z);
				
				Vector3[] originPoints = new Vector3[4] {
					origin,
					origin + dy * borderBottom,
					origin + dy * (sy - borderTop),
					origin + dy * sy
				};
				Vector2[] originUvs = new Vector2[4] {
					srcUv[0],
					srcUv[0] + duvy * borderBottom,
					srcUv[0] + duvy * (1 - borderTop),
					srcUv[0] + duvy,
				};
				
				for (int i = 0; i < 4; ++i)
				{
					meshVertices[i * 4 + 0] = originPoints[i];
					meshVertices[i * 4 + 1] = originPoints[i] + dx * borderLeft;
					meshVertices[i * 4 + 2] = originPoints[i] + dx * (sx - borderRight);
					meshVertices[i * 4 + 3] = originPoints[i] + dx * sx;
					meshUvs[i * 4 + 0] = originUvs[i];
					meshUvs[i * 4 + 1] = originUvs[i] + duvx * borderLeft;
					meshUvs[i * 4 + 2] = originUvs[i] + duvx * (1 - borderRight);
					meshUvs[i * 4 + 3] = originUvs[i] + duvx;
				}
			}
			else
			{
				float sx = sprite.texelSize.x;
				float sy = sprite.texelSize.y;
				
				Vector3[] srcVert = sprite.positions;
				float dx = (srcVert[1].x - srcVert[0].x);
				float dy = (srcVert[2].y - srcVert[0].y);
				
				float borderTopPixels = borderTop * dy;
				float borderBottomPixels = borderBottom * dy;
				float borderRightPixels = borderRight * dx;
				float borderLeftPixels = borderLeft * dx;
				
				float dimXPixels = dimensions.x * sx;
				float dimYPixels = dimensions.y * sy;
				
				float anchorOffsetX = 0.0f;
				float anchorOffsetY = 0.0f;
				switch (anchor)
				{
				case Anchor.LowerLeft: case Anchor.MiddleLeft: case Anchor.UpperLeft: 
					break;
				case Anchor.LowerCenter: case Anchor.MiddleCenter: case Anchor.UpperCenter: 
					anchorOffsetX = -(int)(dimensions.x / 2.0f); break;
				case Anchor.LowerRight: case Anchor.MiddleRight: case Anchor.UpperRight: 
					anchorOffsetX = -(int)(dimensions.x); break;
				}
				switch (anchor)
				{
				case Anchor.LowerLeft: case Anchor.LowerCenter: case Anchor.LowerRight:
					break;
				case Anchor.MiddleLeft: case Anchor.MiddleCenter: case Anchor.MiddleRight:
					anchorOffsetY = -(int)(dimensions.y / 2.0f); break;
				case Anchor.UpperLeft: case Anchor.UpperCenter: case Anchor.UpperRight:
					anchorOffsetY = -(int)dimensions.y; break;
				}
				
				// scale back to sprite coordinates
				// do it after the cast above, as we're trying to align to pixel
				anchorOffsetX *= sx;
				anchorOffsetY *= sy;
				
				boundsCenter = new Vector3(dimXPixels / 2.0f + anchorOffsetX, dimYPixels / 2.0f + anchorOffsetY, 0);
				boundsExtents = new Vector3(dimXPixels / 2.0f, dimYPixels / 2.0f, colliderExtentZ);
				
				Vector2[] srcUv = sprite.uvs;
				Vector2 duvx = sprite.uvs[1] - sprite.uvs[0];
				Vector2 duvy = sprite.uvs[2] - sprite.uvs[0];
				
				Vector3 origin = new Vector3(srcVert[0].x, srcVert[0].y, srcVert[0].z);
				origin = new Vector3(anchorOffsetX, anchorOffsetY, 0);
				
				Vector3[] originPoints = new Vector3[4] {
					origin,
					origin + new Vector3(0, borderBottomPixels, 0),
					origin + new Vector3(0, dimYPixels - borderTopPixels, 0),
					origin + new Vector3(0, dimYPixels, 0),
				};
				Vector2[] originUvs = new Vector2[4] {
					srcUv[0],
					srcUv[0] + duvy * borderBottom,
					srcUv[0] + duvy * (1 - borderTop),
					srcUv[0] + duvy,
				};
				
				for (int i = 0; i < 4; ++i)
				{
					meshVertices[i * 4 + 0] = originPoints[i];
					meshVertices[i * 4 + 1] = originPoints[i] + new Vector3(borderLeftPixels, 0, 0);
					meshVertices[i * 4 + 2] = originPoints[i] + new Vector3(dimXPixels - borderRightPixels, 0, 0);
					meshVertices[i * 4 + 3] = originPoints[i] + new Vector3(dimXPixels, 0, 0);
					
					for (int j = 0; j < 4; ++j)
					{
						Vector3 v = meshVertices[i * 4 + j];
						v.x = v.x * _scale.x;
						v.y = v.y * _scale.y;
						v.z = v.z * _scale.z;
						meshVertices[i * 4 + j] = v;
					}
					
					meshUvs[i * 4 + 0] = originUvs[i];
					meshUvs[i * 4 + 1] = originUvs[i] + duvx * borderLeft;
					meshUvs[i * 4 + 2] = originUvs[i] + duvx * (1 - borderRight);
					meshUvs[i * 4 + 3] = originUvs[i] + duvx;
				}
			}
		}
		else
		{
			for (int i = 0; i < vertices.Length; ++i)
				vertices[i] = Vector3.zero;
		}
	}
	
	void SetIndices()
	{
		meshIndices = new int[9 * 6] {
			0, 4, 1, 1, 4, 5,
			1, 5, 2, 2, 5, 6,
			2, 6, 3, 3, 6, 7,
			4, 8, 5, 5, 8, 9,
			5, 9, 6, 6, 9, 10,
			6, 10, 7, 7, 10, 11,
			8, 12, 9, 9, 12, 13,
			9, 13, 10, 10, 13, 14,
			10, 14, 11, 11, 14, 15
		};		
	}
	
	public override void Build()
	{
		meshUvs = new Vector2[16];
		meshVertices = new Vector3[16];
		meshColors = new Color[16];
		SetIndices();
		
		SetGeometry(meshVertices, meshUvs);
		SetColors(meshColors);
		
		if (mesh == null)
		{
			mesh = new Mesh();
			mesh.hideFlags = HideFlags.DontSave;
		}
		else
		{
			mesh.Clear();
		}
		mesh.vertices = meshVertices;
		mesh.colors = meshColors;
		mesh.uv = meshUvs;
		mesh.triangles = meshIndices;
		mesh.RecalculateBounds();
		
		GetComponent<MeshFilter>().mesh = mesh;
		
		UpdateCollider();
		UpdateMaterial();
	}
	
	protected override void UpdateGeometry() { UpdateGeometryImpl(); }
	protected override void UpdateColors() { UpdateColorsImpl(); }
	protected override void UpdateVertices() { UpdateGeometryImpl(); }
	
	
	protected void UpdateColorsImpl()
	{
#if UNITY_EDITOR
		// This can happen with prefabs in the inspector
		if (meshColors == null || meshColors.Length == 0)
			return;
#endif
		
		SetColors(meshColors);
		mesh.colors = meshColors;
	}

	protected void UpdateGeometryImpl()
	{
#if UNITY_EDITOR
		// This can happen with prefabs in the inspector
		if (mesh == null)
			return;
#endif
		SetGeometry(meshVertices, meshUvs);
		mesh.vertices = meshVertices;
		mesh.uv = meshUvs;
		mesh.RecalculateBounds();
		
		UpdateCollider();
	}
	
	new void UpdateCollider()
	{
		if (boxCollider)
		{
			boxCollider.center = boundsCenter;
			boxCollider.size = boundsExtents * 2.0f;
		}
	}
	
	protected override void UpdateMaterial()
	{
		if (renderer.sharedMaterial != Collection.spriteDefinitions[spriteId].materialInst)
			renderer.material = Collection.spriteDefinitions[spriteId].materialInst;
	}
	
	protected override int GetCurrentVertexCount()
	{
#if UNITY_EDITOR
		if (meshVertices == null)
			return 0;
#endif
		return 16;
	}
}
