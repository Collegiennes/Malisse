using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(tk2dSlicedSprite))]
class tk2dSlicedSpriteEditor : tk2dSpriteEditor
{
	public override void OnInspectorGUI()
    {
        tk2dSlicedSprite sprite = (tk2dSlicedSprite)target;
		base.OnInspectorGUI();
		
		if (sprite.Collection == null)
			return;

		
		EditorGUILayout.BeginVertical();
		
		var spriteData = sprite.GetCurrentSpriteDef();
		
		// need raw extents (excluding scale)
		Vector3 extents = spriteData.boundsData[1];
		
		// this is the size of one texel
		Vector3 spritePixelMultiplier = new Vector3(0, 0);
		
		// if either of these are zero, the division to rescale to pixels will result in a
		// div0, so display the data in fractions to avoid this situation
		bool editBorderInFractions = true;
		if (spriteData.texelSize.x != 0.0f && spriteData.texelSize.y != 0.0f && extents.x != 0.0f && extents.y != 0.0f)
		{
			spritePixelMultiplier = new Vector3(extents.x / spriteData.texelSize.x, extents.y / spriteData.texelSize.y, 1);
			editBorderInFractions = false;
		}
		
		if (!editBorderInFractions)
		{
			if (sprite.transform.localScale == Vector3.one && sprite.legacyMode)
			{
				Vector2 scalePixelUnits = new Vector2(spritePixelMultiplier.x * sprite.scale.x, spritePixelMultiplier.y * sprite.scale.y);
				Vector2 scalePixelUnitsChanged = EditorGUILayout.Vector2Field("Scale (Pixel Units)", scalePixelUnits);
				if (scalePixelUnits != scalePixelUnitsChanged)
				{
					sprite.scale = new Vector3(scalePixelUnitsChanged.x / spritePixelMultiplier.x, scalePixelUnitsChanged.y / spritePixelMultiplier.y, sprite.scale.z);
				}
			}
			else
			{
				Vector2 newDimensions = EditorGUILayout.Vector2Field("Dimensions (Pixel Units)", sprite.dimensions);
				if (newDimensions != sprite.dimensions)
					sprite.dimensions = newDimensions;
				
				sprite.anchor = (tk2dSlicedSprite.Anchor)EditorGUILayout.EnumPopup("Anchor", sprite.anchor);
			}
			
			EditorGUILayout.PrefixLabel("Border");
			
			sprite.borderLeft = EditorGUILayout.FloatField("Left", sprite.borderLeft * spritePixelMultiplier.x) / spritePixelMultiplier.x;
			sprite.borderRight = EditorGUILayout.FloatField("Right", sprite.borderRight * spritePixelMultiplier.x) / spritePixelMultiplier.x;
			sprite.borderTop = EditorGUILayout.FloatField("Top", sprite.borderTop * spritePixelMultiplier.y) / spritePixelMultiplier.y;
			sprite.borderBottom = EditorGUILayout.FloatField("Bottom", sprite.borderBottom * spritePixelMultiplier.y) / spritePixelMultiplier.y;
		}
		else
		{
			GUILayout.Label("Border (Displayed as Fraction).\nSprite Collection needs to be rebuilt.", "textarea");
			sprite.borderLeft = EditorGUILayout.FloatField("Left", sprite.borderLeft);
			sprite.borderRight = EditorGUILayout.FloatField("Right", sprite.borderRight);
			sprite.borderTop = EditorGUILayout.FloatField("Top", sprite.borderTop);
			sprite.borderBottom = EditorGUILayout.FloatField("Bottom", sprite.borderBottom);
		}

		// One of the border valus has changed, so simply rebuild mesh data here		
		if (GUI.changed)
		{
			sprite.Build();
		}

		if (GUI.changed || GUILayout.Button("Commit"))
		{
			Vector3 scl = sprite.transform.localScale;
			Vector3 newScale = new Vector3(sprite.scale.x * scl.x, sprite.scale.y * scl.y, sprite.scale.z);
			sprite.scale = newScale;
			sprite.transform.localScale = Vector3.one;
			sprite.Build();
		}
		
		
		EditorGUILayout.EndVertical();
    }

    [MenuItem("GameObject/Create Other/tk2d/Sliced Sprite", false, 12901)]
    static void DoCreateSlicedSpriteObject()
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
				EditorUtility.DisplayDialog("Create Sliced Sprite", "Unable to create sliced sprite as no SpriteCollections have been found.", "Ok");
				return;
			}
		}

		GameObject go = tk2dEditorUtility.CreateGameObjectInScene("Sliced Sprite");
		tk2dSlicedSprite sprite = go.AddComponent<tk2dSlicedSprite>();
		sprite.legacyMode = false;
		sprite.Collection = sprColl;
		sprite.renderer.material = sprColl.FirstValidDefinition.material;
		sprite.Build();
		Selection.activeGameObject = go;
		Undo.RegisterCreatedObjectUndo(go, "Create Sliced Sprite");
    }
}

