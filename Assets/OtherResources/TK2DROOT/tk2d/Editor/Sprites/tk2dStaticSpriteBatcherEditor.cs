using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[CustomEditor(typeof(tk2dStaticSpriteBatcher))]
class tk2dStaticSpriteBatcherEditor : Editor
{
	tk2dStaticSpriteBatcher batcher { get { return (tk2dStaticSpriteBatcher)target; } }
	
	void DrawEditorGUI()
	{
		if (GUILayout.Button("Commit"))
		{
			// Select all children, EXCLUDING self
			Transform[] allTransforms = batcher.transform.GetComponentsInChildren<Transform>();
			allTransforms = (from t in allTransforms where t != batcher.transform select t).ToArray();
			
			tk2dSpriteCollectionData scd = null;
			foreach (Transform t in allTransforms)
			{
				tk2dSprite s = t.GetComponent<tk2dSprite>();
				if (s)
				{
					if (scd == null) scd = s.Collection;
					if (scd != s.Collection)
					{
						EditorUtility.DisplayDialog("StaticSpriteBatcher", "Error: Multiple sprite collections found", "Ok");
						return;
					}
				}
			}
			
			// sort sprites, smaller to larger z
			allTransforms = (from t in allTransforms orderby t.position.z descending select t).ToArray();
			
			// and within the z sort by material
			if (allTransforms.Length == 0)
			{
				EditorUtility.DisplayDialog("StaticSpriteBatcher", "Error: No child objects found", "Ok");
				return;
			}
		
			Dictionary<Transform, int> batchedSpriteLookup = new Dictionary<Transform, int>();
			batchedSpriteLookup[batcher.transform] = -1;
			
			batcher.spriteCollection = scd;
			batcher.batchedSprites = new tk2dBatchedSprite[allTransforms.Length];
			int currBatchedSprite = 0;
			foreach (var t in allTransforms)
			{
				tk2dBatchedSprite bs = new tk2dBatchedSprite();
				bs.name = t.gameObject.name;
				tk2dSprite s = t.GetComponent<tk2dSprite>();
				if (s)
				{
					bs.color = s.color;
					bs.localScale = new Vector3(s.scale.x * t.localScale.x, s.scale.y * t.localScale.y, s.scale.z * t.localScale.z);
					bs.spriteId = s.spriteId;
					bs.alwaysPixelPerfect = s.pixelPerfect;
				}
				else
				{
					bs.spriteId = -1;
					bs.localScale = t.localScale;
				}
				
				batchedSpriteLookup[t] = currBatchedSprite;
				batcher.batchedSprites[currBatchedSprite++] = bs;
			}
			
			int idx = 0;
			foreach (var t in allTransforms)
			{
				var bs = batcher.batchedSprites[idx];

				bs.parentId = batchedSpriteLookup[t.parent];
				t.parent = batcher.transform; // unparent

				bs.position = t.localPosition;
				bs.rotation = t.localRotation;
				
				++idx;
			}
			
			Transform[] directChildren = (from t in allTransforms where t.parent == batcher.transform select t).ToArray();
			foreach (var t in directChildren)
			{
				GameObject.DestroyImmediate(t.gameObject);
			}
			
			batcher.scale = batcher.transform.localScale;
			batcher.transform.localScale = Vector3.one;
			batcher.Build();
			EditorUtility.SetDirty(target);
		}
	}
	
	void DrawInstanceGUI()
	{
		if (GUILayout.Button("Edit"))
	    {
			Vector3 batcherPos = batcher.transform.position;
			Quaternion batcherRotation = batcher.transform.rotation;
			batcher.transform.position = Vector3.zero;
			batcher.transform.rotation = Quaternion.identity;
			
			Dictionary<int, Transform> parents = new Dictionary<int, Transform>();
			List<Transform> children = new List<Transform>();
			
			int id = 0;
			foreach (var v in batcher.batchedSprites)
			{
				GameObject go = new GameObject(v.name);
				
				go.transform.localPosition = v.position;
				go.transform.localRotation = v.rotation;

				if (v.spriteId != -1)
				{
					tk2dSprite s = tk2dSprite.AddComponent<tk2dSprite>(go, batcher.spriteCollection, v.spriteId);
	
					s.scale = v.localScale;
					s.pixelPerfect = v.alwaysPixelPerfect;
					s.color = v.color;
				}
				
				parents[id++] = go.transform;
				children.Add(go.transform);
			}
			
			int idx = 0;
			foreach (var v in batcher.batchedSprites)
			{
				Transform parent = batcher.transform;
				if (v.parentId != -1)
					parents.TryGetValue(v.parentId, out parent);
				
				children[idx].parent = parent;
				++idx;
			}
			
			batcher.transform.localScale = batcher.scale;
			
			batcher.batchedSprites = null;
			batcher.Build();
			EditorUtility.SetDirty(target);

			batcher.transform.position = batcherPos;
			batcher.transform.rotation = batcherRotation;
		}

		batcher.scale = EditorGUILayout.Vector3Field("Scale", batcher.scale);		
	}
	
    public override void OnInspectorGUI()
    {
		if (batcher.batchedSprites == null || batcher.batchedSprites.Length == 0)
			DrawEditorGUI();
		else
			DrawInstanceGUI();
    }
	
    [MenuItem("GameObject/Create Other/tk2d/Static Sprite Batcher", false, 12907)]
    static void DoCreateSpriteObject()
    {
		GameObject go = tk2dEditorUtility.CreateGameObjectInScene("Static Sprite Batcher");
		tk2dStaticSpriteBatcher batcher = go.AddComponent<tk2dStaticSpriteBatcher>();
		batcher.version = tk2dStaticSpriteBatcher.CURRENT_VERSION;
		
		Selection.activeGameObject = go;
		Undo.RegisterCreatedObjectUndo(go, "Create Static Sprite Batcher");
    }
}

