using UnityEngine;
using System.Collections;

[AddComponentMenu("2D Toolkit/Backend/tk2dBaseSprite")]
/// <summary>
/// Sprite base class. Performs target agnostic functionality and manages state parameters.
/// </summary>
public abstract class tk2dBaseSprite : MonoBehaviour, tk2dRuntime.ISpriteCollectionForceBuild
{
	/// <summary>
	/// Holds a pointer to the sprite collection.
	/// </summary>
    public tk2dSpriteCollectionData collection;

	/// <summary>
	/// Holds a pointer to the sprite collection.
	/// </summary>
	public tk2dSpriteCollectionData Collection { get { return collection; } set { collection = value; collectionInst = collection.inst; } }
	
	public bool m_DontUpdateMaterial = false;

    // This is the active instance of the sprite collection
    protected tk2dSpriteCollectionData collectionInst;
	
	[SerializeField] protected Color _color = Color.white;
	[SerializeField] protected Vector3 _scale = new Vector3(1.0f, 1.0f, 1.0f);
	[SerializeField] protected int _spriteId = 0;
	
	/// <summary>
	/// Specifies if this sprite is kept pixel perfect
	/// </summary>
	public bool pixelPerfect = false;
	
	/// <summary>
	/// Internal cached version of the box collider created for this sprite, if present.
	/// </summary>
	public BoxCollider boxCollider = null;
	/// <summary>
	/// Internal cached version of the mesh collider created for this sprite, if present.
	/// </summary>
	public MeshCollider meshCollider = null;
	public Vector3[] meshColliderPositions = null;
	public Mesh meshColliderMesh = null;
	
	// This is unfortunate, but required due to the unpredictable script execution order in Unity.
	// The only problem happens in Awake(), where if another class is Awaken before this one, and tries to
	// modify this instance before it is initialized, very bad things could happen.
	void InitInstance()
	{
		if (collectionInst == null && collection != null)
			collectionInst = collection.inst;
	}

	/// <summary>
	/// Gets or sets the color.
	/// </summary>
	/// <value>
	/// Please note the range for a Unity Color is 0..1 and not 0..255.
	/// </value>
	public Color color 
	{ 
		get { return _color; } 
		set 
		{
			if (value != _color)
			{
				_color = value;
				InitInstance();
				UpdateColors();
			}
		} 
	}
	
	/// <summary>
	/// Gets or sets the scale.
	/// </summary>
	/// <value>
	/// Use the sprite scale method as opposed to transform.localScale to avoid breaking dynamic batching.
	/// </value>
	public Vector3 scale 
	{ 
		get { return _scale; } 
		set
		{
			if (value != _scale)
			{
				_scale = value;
				InitInstance();
				UpdateVertices();
#if UNITY_EDITOR
				EditMode__CreateCollider();
#else
				UpdateCollider();
#endif
			}
		}
	}
	
	/// <summary>
	/// Flips the sprite horizontally.
	/// </summary>
	public void FlipX()
	{
		scale = new Vector3(-_scale.x, _scale.y, _scale.z);
	}
	
	/// <summary>
	/// Flips the sprite vertically.
	/// </summary>
	public void FlipY()
	{
		scale = new Vector3(_scale.x, -_scale.y, _scale.z);
	}
	
	/// <summary>
	/// Gets or sets the sprite identifier.
	/// </summary>
	/// <value>
	/// The spriteId is a unique number identifying each sprite.
	/// Use <see cref="tk2dBaseSprite.GetSpriteIdByName">GetSpriteIdByName</see> to resolve an identifier from the current sprite collection.
	/// </value>
	public int spriteId 
	{ 
		get { return _spriteId; } 
		set 
		{
			if (value != _spriteId)
			{
				InitInstance();
				value = Mathf.Clamp(value, 0, collectionInst.spriteDefinitions.Length - 1);
				if (_spriteId < 0 || _spriteId >= collectionInst.spriteDefinitions.Length ||
					GetCurrentVertexCount() != collectionInst.spriteDefinitions[value].positions.Length ||
					collectionInst.spriteDefinitions[_spriteId].complexGeometry != collectionInst.spriteDefinitions[value].complexGeometry)
				{
					_spriteId = value;
					UpdateGeometry();
				}
				else
				{
					_spriteId = value;
					UpdateVertices();
				}
				UpdateMaterial();
				UpdateCollider();
			}
		} 
	}
	
	/// <summary>
	/// Switchs the sprite collection and sprite.
	/// Simply set the <see cref="tk2dBaseSprite.spriteId">spriteId</see> property when you don't need to switch the sprite collection.
	/// </summary>
	/// <param name='newCollection'>
	/// A reference to the sprite collection to switch to.
	/// </param>
	/// <param name='newSpriteId'>
	/// New sprite identifier.
	/// </param>
	public void SwitchCollectionAndSprite(tk2dSpriteCollectionData newCollection, int newSpriteId)
	{
		bool switchedCollection = false;
		if (Collection != newCollection)
		{
			Collection = newCollection;
			collectionInst = Collection.inst;
			_spriteId = -1; // force an update, but only when the collection has changed
			switchedCollection = true;
		}
		
		spriteId = newSpriteId;
		
		if (switchedCollection)
		{
			UpdateMaterial();
		}
	}
	
	/// <summary>
	/// Makes the sprite pixel perfect to the active camera.
	/// Automatically detects <see cref="tk2dCamera"/> if present
	/// Otherwise uses Camera.main
	/// </summary>
	public void MakePixelPerfect()
	{
		float s = 1.0f;
		tk2dPixelPerfectHelper pph = tk2dPixelPerfectHelper.inst;
		if (pph)
		{
			if (pph.CameraIsOrtho)
			{
				s = pph.scaleK;
			}
			else
			{
				s = pph.scaleK + pph.scaleD * transform.position.z;
			}
		}
		else if (tk2dCamera.inst)
		{
			if (Collection.version < 2)
			{
				Debug.LogError("Need to rebuild sprite collection.");
			}

			s = Collection.halfTargetHeight;
		}
		else if (Camera.main)
		{
			if (Camera.main.isOrthoGraphic)
			{
				s = Camera.main.orthographicSize;
			}
			else
			{
				float zdist = (transform.position.z - Camera.main.transform.position.z);
				s = tk2dPixelPerfectHelper.CalculateScaleForPerspectiveCamera(Camera.main.fieldOfView, zdist);
			}
		}
		else
		{
			Debug.LogError("Main camera not found.");
		}
		
		s *= Collection.invOrthoSize;
		
		scale = new Vector3(Mathf.Sign(scale.x) * s, Mathf.Sign(scale.y) * s, Mathf.Sign(scale.z) * s);
	}	
		
	
	protected abstract void UpdateMaterial(); // update material when switching spritecollection
	protected abstract void UpdateColors(); // reupload color data only
	protected abstract void UpdateVertices(); // reupload vertex data only
	protected abstract void UpdateGeometry(); // update full geometry (including indices)
	protected abstract int  GetCurrentVertexCount(); // return current vertex count
	
	/// <summary>
	/// Rebuilds the mesh data for this sprite. Not usually necessary to call this, unless some internal states are modified.
	/// </summary>
	public abstract void Build();
	
	/// <summary>
	/// Resolves a sprite name and returns a unique id for the sprite.
	/// Convenience alias of <see cref="tk2dSpriteCollectionData.GetSpriteIdByName"/>
	/// </summary>
	/// <returns>
	/// Unique Sprite Id.
	/// </returns>
	/// <param name='name'>Case sensitive sprite name, as defined in the sprite collection. This is usually the source filename excluding the extension</param>
	public int GetSpriteIdByName(string name)
	{
		InitInstance();
		return collectionInst.GetSpriteIdByName(name);
	}
	
	/// <summary>
	/// Adds a tk2dBaseSprite derived class as a component to the gameObject passed in, setting up necessary parameters
	/// and building geometry.
	/// </summary>
	public static T AddComponent<T>(GameObject go, tk2dSpriteCollectionData spriteCollection, int spriteId) where T : tk2dBaseSprite
	{
		T sprite = go.AddComponent<T>();
		sprite._spriteId = -1;
		sprite.SwitchCollectionAndSprite(spriteCollection, spriteId);
		sprite.Build();
		return sprite;
	}
	
	protected int GetNumVertices()
	{
		InitInstance();
		return collectionInst.spriteDefinitions[spriteId].positions.Length;
	}
	
	protected int GetNumIndices()
	{
		InitInstance();
		return collectionInst.spriteDefinitions[spriteId].indices.Length;
	}
	
	protected void SetPositions(Vector3[] positions, Vector3[] normals, Vector4[] tangents)	
	{
		var sprite = collectionInst.spriteDefinitions[spriteId];
		int numVertices = GetNumVertices();
		for (int i = 0; i < numVertices; ++i)
		{
			positions[i].x = sprite.positions[i].x * _scale.x;
			positions[i].y = sprite.positions[i].y * _scale.y;
			positions[i].z = sprite.positions[i].z * _scale.z;
		}
		
		// The secondary test sprite.normals != null must have been performed prior to this function call
		if (normals.Length > 0)
		{
			for (int i = 0; i < numVertices; ++i)
			{
				normals[i] = sprite.normals[i];
			}
		}

		// The secondary test sprite.tangents != null must have been performed prior to this function call
		if (tangents.Length > 0)
		{
			for (int i = 0; i < numVertices; ++i)
			{
				tangents[i] = sprite.tangents[i];
			}
		}
	}
	
	protected void SetColors(Color[] dest)
	{
		Color c = _color;
        if (collectionInst.premultipliedAlpha) { c.r *= c.a; c.g *= c.a; c.b *= c.a; }
		int numVertices = GetNumVertices();
		for (int i = 0; i < numVertices; ++i)
			dest[i] = c;
	}
	
	/// <summary>
	/// Gets the local space bounds of the sprite.
	/// </summary>
	/// <returns>
	/// Local space bounds
	/// </returns>
	public Bounds GetBounds()
	{
		InitInstance();
		var sprite = collectionInst.spriteDefinitions[_spriteId];
		return new Bounds(new Vector3(sprite.boundsData[0].x * _scale.x, sprite.boundsData[0].y * _scale.y, sprite.boundsData[0].z * _scale.z),
		                  new Vector3(sprite.boundsData[1].x * _scale.x, sprite.boundsData[1].y * _scale.y, sprite.boundsData[1].z * _scale.z));
	}
	
	/// <summary>
	/// Gets untrimmed local space bounds of the sprite. This is the size of the sprite before 2D Toolkit trims away empty space in the sprite.
	/// Use this when you need to position sprites in a grid, etc, when the trimmed bounds is not sufficient.
	/// </summary>
	/// <returns>
	/// Local space untrimmed bounds
	/// </returns>
	public Bounds GetUntrimmedBounds()
	{
		InitInstance();
		var sprite = collectionInst.spriteDefinitions[_spriteId];
		return new Bounds(new Vector3(sprite.untrimmedBoundsData[0].x * _scale.x, sprite.untrimmedBoundsData[0].y * _scale.y, sprite.untrimmedBoundsData[0].z * _scale.z),
		                  new Vector3(sprite.untrimmedBoundsData[1].x * _scale.x, sprite.untrimmedBoundsData[1].y * _scale.y, sprite.untrimmedBoundsData[1].z * _scale.z));
	}
	
	/// <summary>
	/// Gets the current sprite definition.
	/// </summary>
	/// <returns>
	/// <see cref="tk2dSpriteDefinition"/> for the currently active sprite.
	/// </returns>
	public tk2dSpriteDefinition GetCurrentSpriteDef()
	{
		InitInstance();
		return collectionInst.spriteDefinitions[_spriteId];
	}

	// Unity functions
	public void Start()
	{
		if (pixelPerfect)
			MakePixelPerfect();
	}	
	
	
	// Collider setup
	
	protected virtual bool NeedBoxCollider() { return false; }
	
	protected void UpdateCollider()
	{
		var sprite = collectionInst.spriteDefinitions[_spriteId];
		
		if (sprite.colliderType == tk2dSpriteDefinition.ColliderType.Box && boxCollider == null)
		{
			// Has the user created a box collider?
			boxCollider = gameObject.GetComponent<BoxCollider>();
			
			if (boxCollider == null)
			{
				// create box collider at runtime. this won't get removed from the object
				boxCollider = gameObject.AddComponent<BoxCollider>();
			}
		}

		
		if (boxCollider != null)
		{
			if (sprite.colliderType == tk2dSpriteDefinition.ColliderType.Box)
			{
				boxCollider.center = new Vector3(sprite.colliderVertices[0].x * _scale.x, sprite.colliderVertices[0].y * _scale.y, sprite.colliderVertices[0].z * _scale.z);
				boxCollider.size = new Vector3(sprite.colliderVertices[1].x * _scale.x, sprite.colliderVertices[1].y * _scale.y, sprite.colliderVertices[1].z * _scale.z) * 2.0f;
			}
			else if (sprite.colliderType == tk2dSpriteDefinition.ColliderType.Unset)
			{
				// Don't do anything here, for backwards compatibility
			}
			else // in all cases, if the collider doesn't match is requested, null it out
			{
				if (boxCollider != null)
				{
					// move the box far far away, boxes with zero extents still collide
					boxCollider.center = new Vector3(0, 0, -100000.0f);
					boxCollider.size = Vector3.zero;
				}
			}
		}
	}
	
	// This is separate to UpdateCollider, as UpdateCollider can only work with BoxColliders, and will NOT create colliders
	protected void CreateCollider()
	{
		var sprite = collectionInst.spriteDefinitions[_spriteId];
		if (sprite.colliderType == tk2dSpriteDefinition.ColliderType.Unset)
		{
			// do not attempt to create or modify anything if it is Unset
			return;
		}

		// User has created a collider
		if (collider != null)
		{
			boxCollider = GetComponent<BoxCollider>();
			meshCollider = GetComponent<MeshCollider>();
		}
		
		if ((NeedBoxCollider() || sprite.colliderType == tk2dSpriteDefinition.ColliderType.Box) && meshCollider == null)
		{
			if (boxCollider == null)
			{
				boxCollider = gameObject.AddComponent<BoxCollider>();
			}
		}
		else if (sprite.colliderType == tk2dSpriteDefinition.ColliderType.Mesh && boxCollider == null)
		{
			// this should not be updated again (apart from scale changes in the editor, where we force regeneration of colliders)
			if (meshCollider == null)
				meshCollider = gameObject.AddComponent<MeshCollider>();
			if (meshColliderMesh == null)
				meshColliderMesh = new Mesh();
			
			meshColliderMesh.Clear();
			
			meshColliderPositions = new Vector3[sprite.colliderVertices.Length];
			for (int i = 0; i < meshColliderPositions.Length; ++i)
				meshColliderPositions[i] = new Vector3(sprite.colliderVertices[i].x * _scale.x, sprite.colliderVertices[i].y * _scale.y, sprite.colliderVertices[i].z * _scale.z);
			meshColliderMesh.vertices = meshColliderPositions;
			
			float s = _scale.x * _scale.y * _scale.z;
			
			meshColliderMesh.triangles = (s >= 0.0f)?sprite.colliderIndicesFwd:sprite.colliderIndicesBack;
			meshCollider.sharedMesh = meshColliderMesh;
			meshCollider.convex = sprite.colliderConvex;
			
			// this is required so our mesh pivot is at the right point
			if (rigidbody) rigidbody.centerOfMass = Vector3.zero;
		}
		else if (sprite.colliderType != tk2dSpriteDefinition.ColliderType.None)
		{
			// This warning is not applicable in the editor
			if (Application.isPlaying)
			{
				Debug.LogError("Invalid mesh collider on sprite, please remove and try again.");
			}
		}
		
		UpdateCollider();
	}
	
#if UNITY_EDITOR
	public void EditMode__CreateCollider()
	{
		// Revert to runtime behaviour when the game is running
		if (Application.isPlaying)
		{
			UpdateCollider();
			return;
		}
		
		var sprite = collectionInst.spriteDefinitions[_spriteId];
		if (sprite.colliderType == tk2dSpriteDefinition.ColliderType.Unset)
			return;
		
		PhysicMaterial physicsMaterial = collider?collider.sharedMaterial:null;
		bool isTrigger = collider?collider.isTrigger:false;
		
		if (boxCollider)
		{
			DestroyImmediate(boxCollider, true);
		}
		if (meshCollider)
		{
			DestroyImmediate(meshCollider, true);
			if (meshColliderMesh)
				DestroyImmediate(meshColliderMesh, true);
		}

		CreateCollider();
		
		if (collider)
		{
			collider.isTrigger = isTrigger;
			collider.material = physicsMaterial;
		}
	}
#endif

	protected void Awake()
	{
		if (collection != null)
		{
			collectionInst = collection.inst;
		}
	}
	
	
	// tk2dRuntime.ISpriteCollectionEditor
	public bool UsesSpriteCollection(tk2dSpriteCollectionData spriteCollection)
	{
		return Collection == spriteCollection || collectionInst == spriteCollection;
	}
	
	public virtual void ForceBuild()
	{
		collectionInst = collection.inst;
		if (spriteId < 0 || spriteId >= collectionInst.spriteDefinitions.Length)
    		spriteId = 0;
		Build();
#if UNITY_EDITOR
		EditMode__CreateCollider();
#endif
	}

	/// <summary>
	/// Create a sprite (and gameObject) displaying the region of the texture specified.
	/// Use <see cref="tk2dSpriteCollectionData.CreateFromTexture"/> if you need to create a sprite collection
	/// with multiple sprites.
	/// </summary>
	public static GameObject CreateFromTexture<T>(Texture2D texture, tk2dRuntime.SpriteCollectionSize size, Rect region, Vector2 anchor) where T : tk2dBaseSprite
	{
		tk2dSpriteCollectionData data = tk2dRuntime.SpriteCollectionGenerator.CreateFromTexture(texture, size, region, anchor);
		if (data == null)
			return null;
		GameObject spriteGo = new GameObject();
		tk2dBaseSprite.AddComponent<T>(spriteGo, data, 0);
		return spriteGo;
	}
}
