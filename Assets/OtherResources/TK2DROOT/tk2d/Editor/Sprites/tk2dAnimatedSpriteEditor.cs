using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(tk2dAnimatedSprite))]
class tk2dAnimatedSpriteEditor : tk2dSpriteEditor
{
	tk2dGenericIndexItem[] animLibs = null;
	string[] animLibNames = null;
	bool initialized = false;
	
	void Init()
	{
		if (!initialized)
		{
			animLibs = tk2dEditorUtility.GetOrCreateIndex().GetSpriteAnimations();
			if (animLibs != null)
			{
				animLibNames = new string[animLibs.Length];
				for (int i = 0; i < animLibs.Length; ++i)
				{
					animLibNames[i] = animLibs[i].AssetName;
				}
			}
			initialized = true;
		}
	}
	
	void OnDestroy()
	{
		tk2dSpriteThumbnailCache.ReleaseSpriteThumbnailCache();
	}
	
	static bool spriteUiVisible = false;
    public override void OnInspectorGUI()
    {
		spriteUiVisible = EditorGUILayout.Foldout(spriteUiVisible, "Sprite");
		if (spriteUiVisible)
			base.OnInspectorGUI();
		
		Init();
		if (animLibs == null)
		{
			GUILayout.Label("no libraries found");
			if (GUILayout.Button("Refresh"))
			{
				initialized = false;
				Init();
			}
		}
		else
		{
	        tk2dAnimatedSprite sprite = (tk2dAnimatedSprite)target;

			// NOTE ppoirier: Deprecated.
			//EditorGUIUtility.LookLikeInspector();
			EditorGUI.indentLevel = 1;

			if (sprite.anim == null)
			{
				sprite.anim = animLibs[0].GetAsset<tk2dSpriteAnimation>();
				GUI.changed = true;
			}
			
			// Display animation library
			int selAnimLib = 0;
			string selectedGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(sprite.anim));
			for (int i = 0; i < animLibs.Length; ++i)
			{
				if (animLibs[i].assetGUID == selectedGUID)
				{
					selAnimLib = i;
					break;
				}
			}
		
			int newAnimLib = EditorGUILayout.Popup("Anim Lib", selAnimLib, animLibNames);
			if (newAnimLib != selAnimLib)
			{
				sprite.anim = animLibs[newAnimLib].GetAsset<tk2dSpriteAnimation>();
				sprite.clipId = 0;
				
				if (sprite.anim.clips.Length > 0)
				{
					// automatically switch to the first frame of the new clip
					sprite.SwitchCollectionAndSprite(sprite.anim.clips[sprite.clipId].frames[0].spriteCollection,
					                                 sprite.anim.clips[sprite.clipId].frames[0].spriteId);
				}
			}
			
			// Everything else
			if (sprite.anim && sprite.anim.clips.Length > 0)
			{
				int clipId = sprite.clipId;

				// Sanity check clip id
				clipId = Mathf.Clamp(clipId, 0, sprite.anim.clips.Length - 1);
				if (clipId != sprite.clipId)
				{
					sprite.clipId = clipId;
					GUI.changed = true;
				}
				
				string[] clipNames = new string[sprite.anim.clips.Length];
				// fill names (with ids if necessary)
				if (tk2dPreferences.inst.showIds)
				{
					for (int i = 0; i < sprite.anim.clips.Length; ++i)
					{
						if (sprite.anim.clips[i].name != null && sprite.anim.clips[i].name.Length > 0)
							clipNames[i] = sprite.anim.clips[i].name + "\t[" + i.ToString() + "]";
						else
							clipNames[i] = sprite.anim.clips[i].name;
					}
				}
				else
				{
					for (int i = 0; i < sprite.anim.clips.Length; ++i)
						clipNames[i] = sprite.anim.clips[i].name;
				}
				
				int newClipId = EditorGUILayout.Popup("Clip", sprite.clipId, clipNames);
				if (newClipId != sprite.clipId)
				{
					sprite.clipId = newClipId;
					// automatically switch to the first frame of the new clip
					sprite.SwitchCollectionAndSprite(sprite.anim.clips[sprite.clipId].frames[0].spriteCollection,
					                                 sprite.anim.clips[sprite.clipId].frames[0].spriteId);
				}
			}

			// Play automatically
			sprite.playAutomatically = EditorGUILayout.Toggle("Play automatically", sprite.playAutomatically);
			bool oldCreateCollider = sprite.createCollider;
			sprite.createCollider = EditorGUILayout.Toggle("Create collider", sprite.createCollider);
			if (oldCreateCollider != sprite.createCollider)
			{
				sprite.EditMode__CreateCollider();
			}
			sprite.m_DontUpdateMaterial = EditorGUILayout.Toggle("Don't Update Material", sprite.m_DontUpdateMaterial);
			
			if (GUI.changed)
			{
				EditorUtility.SetDirty(sprite);
			}
		}
    }

    [MenuItem("GameObject/Create Other/tk2d/Animated Sprite", false, 12901)]
    static void DoCreateSpriteObject()
    {
		tk2dSpriteCollectionData sprColl = null;
		if (sprColl == null)
		{
			// try to inherit from other Sprites in scene
			tk2dSprite spr = GameObject.FindObjectOfType(typeof(tk2dSprite)) as tk2dSprite;
			if (spr)
			{
				sprColl = spr.Collection;
			}
		}
		
		if (sprColl == null)
		{
			tk2dSpriteCollectionIndex[] spriteCollections = tk2dEditorUtility.GetOrCreateIndex().GetSpriteCollectionIndex();
			foreach (var v in spriteCollections)
			{
				GameObject scgo = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(v.spriteCollectionDataGUID), typeof(GameObject)) as GameObject;
				var sc = scgo.GetComponent<tk2dSpriteCollectionData>();
				if (sc != null && sc.spriteDefinitions != null && sc.spriteDefinitions.Length > 0)
				{
					sprColl = sc;
					break;
				}
			}

			if (sprColl == null)
			{
				EditorUtility.DisplayDialog("Create Sprite", "Unable to create sprite as no SpriteCollections have been found.", "Ok");
				return;
			}
		}		
		
		tk2dGenericIndexItem[] animIndex = tk2dEditorUtility.GetOrCreateIndex().GetSpriteAnimations();
		tk2dSpriteAnimation anim = null;
		foreach (var animIndexItem in animIndex)
		{
			tk2dSpriteAnimation a = animIndexItem.GetAsset<tk2dSpriteAnimation>();
			if (a != null && a.clips != null && a.clips.Length > 0)
			{
				anim = a;
				break;
			}
		}
		
		if (anim == null)
		{
			EditorUtility.DisplayDialog("Create Animated Sprite", "Unable to create animated sprite as no SpriteAnimations have been found.", "Ok");
			return;
		}
		
		if (anim.clips[0].frames.Length == 0 || anim.clips[0].frames[0].spriteCollection == null)
		{
			EditorUtility.DisplayDialog("Create Animated Sprite", "Invalid SpriteAnimation has been found.", "Ok");
			return;
		}

		GameObject go = tk2dEditorUtility.CreateGameObjectInScene("AnimatedSprite");
		tk2dAnimatedSprite sprite = go.AddComponent<tk2dAnimatedSprite>();
		sprite.SwitchCollectionAndSprite(anim.clips[0].frames[0].spriteCollection, anim.clips[0].frames[0].spriteId);
		sprite.anim = anim;
		sprite.Build();
		
		Selection.activeGameObject = go;
		Undo.RegisterCreatedObjectUndo(go, "Create AnimatedSprite");
    }
}

