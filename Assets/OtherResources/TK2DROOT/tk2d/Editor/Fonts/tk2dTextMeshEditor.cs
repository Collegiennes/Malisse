using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(tk2dTextMesh))]
class tk2dTextMeshEditor : Editor
{
	tk2dGenericIndexItem[] allFonts = null;	// all generators
	string[] allFontNames = null;
	Vector2 gradientScroll;
	
	// Word wrap on text area - almost impossible to use otherwise
	GUIStyle _textAreaStyle = null;
	GUIStyle textAreaStyle
	{
		get {
			if (_textAreaStyle == null)
			{
				_textAreaStyle = new GUIStyle(EditorStyles.textField);
				_textAreaStyle.wordWrap = true;
			}
			return _textAreaStyle;
		}
	}

	// Draws the word wrap GUI
	void DrawWordWrapSceneGUI(tk2dTextMesh textMesh)
	{
		tk2dFontData font = textMesh.font;
		Transform transform = textMesh.transform;

		int px = textMesh.wordWrapWidth;

		Vector3 p0 = transform.position;
		float width = font.texelSize.x * px * transform.localScale.x;
		bool drawRightHandle = true;
		bool drawLeftHandle = false;
		switch (textMesh.anchor)
		{
			case TextAnchor.LowerCenter: case TextAnchor.MiddleCenter: case TextAnchor.UpperCenter:
				drawLeftHandle = true;
				p0 -= width * 0.5f * transform.right;
				break;
			case TextAnchor.LowerRight: case TextAnchor.MiddleRight: case TextAnchor.UpperRight:
				drawLeftHandle = true;
				drawRightHandle = false;
				p0 -= width * transform.right;
				break;
		}
		Vector3 p1 = p0 + width * transform.right;


		Handles.color = new Color32(255, 255, 255, 24);
		float subPin = font.texelSize.y * 2048;
		Handles.DrawLine(p0, p1);
		Handles.DrawLine(p0 - subPin * transform.up, p0 + subPin * transform.up);
		Handles.DrawLine(p1 - subPin * transform.up, p1 + subPin * transform.up);

		Handles.color = Color.white;
		Vector3 pin = transform.up * font.texelSize.y * 10.0f;
		Handles.DrawLine(p0 - pin, p0 + pin);
		Handles.DrawLine(p1 - pin, p1 + pin);

		if (drawRightHandle)
		{
			Vector3 newp1 = Handles.Slider(p1, transform.right, HandleUtility.GetHandleSize(p1), Handles.ArrowCap, 0.0f);
			if (newp1 != p1)
			{
				int newPx = (int)Mathf.Round((newp1 - p0).magnitude / (font.texelSize.x * transform.localScale.x));
				newPx = Mathf.Max(newPx, 0);
				textMesh.wordWrapWidth = newPx;
				textMesh.Commit();
			}
		}

		if (drawLeftHandle)
		{
			Vector3 newp0 = Handles.Slider(p0, -transform.right, HandleUtility.GetHandleSize(p0), Handles.ArrowCap, 0.0f);
			if (newp0 != p0)
			{
				int newPx = (int)Mathf.Round((p1 - newp0).magnitude / (font.texelSize.x * transform.localScale.x));
				newPx = Mathf.Max(newPx, 0);
				textMesh.wordWrapWidth = newPx;
				textMesh.Commit();
			}
		}
	}

	public void OnSceneGUI()
	{
		tk2dTextMesh textMesh = (tk2dTextMesh)target;
		if (textMesh.formatting && textMesh.wordWrapWidth > 0)
		{
			DrawWordWrapSceneGUI(textMesh);
		}
	}

    public override void OnInspectorGUI()
    {
        tk2dTextMesh textMesh = (tk2dTextMesh)target;
        EditorGUIUtility.LookLikeControls(80, 50);
		
		// maybe cache this if its too slow later
		if (allFonts == null || allFontNames == null) 
		{
			tk2dGenericIndexItem[] indexFonts = tk2dEditorUtility.GetOrCreateIndex().GetFonts();
			List<tk2dGenericIndexItem> filteredFonts = new List<tk2dGenericIndexItem>();
			foreach (var f in indexFonts)
				if (!f.managed) filteredFonts.Add(f);

			allFonts = filteredFonts.ToArray();
			allFontNames = new string[allFonts.Length];
			for (int i = 0; i < allFonts.Length; ++i)
				allFontNames[i] = allFonts[i].AssetName;
		}
		
		if (allFonts != null)
        {
			if (textMesh.font == null)
			{
				textMesh.font = allFonts[0].GetAsset<tk2dFont>().data;
			}
			
			int currId = -1;
			string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(textMesh.font));
			for (int i = 0; i < allFonts.Length; ++i)
			{
				if (allFonts[i].dataGUID == guid)
				{
					currId = i;
				}
			}
			
			int newId = EditorGUILayout.Popup("Font", currId, allFontNames);
			if (newId != currId)
			{
				textMesh.font = allFonts[newId].GetAsset<tk2dFont>().data;
			}
			
			EditorGUILayout.BeginHorizontal();
			textMesh.maxChars = EditorGUILayout.IntField("Max Chars", textMesh.maxChars);
			if (textMesh.maxChars < 1) textMesh.maxChars = 1;
			if (textMesh.maxChars > 16000) textMesh.maxChars = 16000;
			if (GUILayout.Button("Fit", GUILayout.MaxWidth(32.0f)))
			{
				textMesh.maxChars = textMesh.NumTotalCharacters();
				GUI.changed = true;
			}
			EditorGUILayout.EndHorizontal();

			textMesh.formatting = EditorGUILayout.BeginToggleGroup("Formatting", textMesh.formatting);
			GUILayout.BeginHorizontal();
			if (textMesh.wordWrapWidth == 0)
			{
				EditorGUILayout.PrefixLabel("Word Wrap");
				if (GUILayout.Button("Enable", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
				{
					textMesh.wordWrapWidth = 500;
					GUI.changed = true;
				}
			}
			else
			{
				textMesh.wordWrapWidth = EditorGUILayout.IntField("Word Wrap", textMesh.wordWrapWidth);
				if (GUILayout.Button("Disable", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
				{
					textMesh.wordWrapWidth = 0;
					GUI.changed = true;
				}
			}
			GUILayout.EndHorizontal();
			EditorGUILayout.EndToggleGroup();
			
			GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Text");
			textMesh.text = EditorGUILayout.TextArea(textMesh.text, textAreaStyle, GUILayout.Height(64));
			GUILayout.EndHorizontal();
			
			textMesh.anchor = (TextAnchor)EditorGUILayout.EnumPopup("Anchor", textMesh.anchor);
			textMesh.kerning = EditorGUILayout.Toggle("Kerning", textMesh.kerning);
			textMesh.spacing = EditorGUILayout.FloatField("Spacing", textMesh.spacing);
			textMesh.lineSpacing = EditorGUILayout.FloatField("Line Spacing", textMesh.lineSpacing);
			textMesh.scale = EditorGUILayout.Vector3Field("Scale", textMesh.scale);
			
			if (textMesh.font.textureGradients && textMesh.font.gradientCount > 0)
			{
				//textMesh.textureGradient = EditorGUILayout.IntSlider("Gradient", textMesh.textureGradient, 0, textMesh.font.gradientCount - 1);
				
				GUILayout.BeginHorizontal();
				
				EditorGUILayout.PrefixLabel("TextureGradient");
				
				// Draw gradient scroller
				bool drawGradientScroller = true;
				if (drawGradientScroller)
				{
					textMesh.textureGradient = textMesh.textureGradient % textMesh.font.gradientCount;
					
					gradientScroll = EditorGUILayout.BeginScrollView(gradientScroll, GUILayout.ExpandHeight(false));
					Rect r = GUILayoutUtility.GetRect(textMesh.font.gradientTexture.width, textMesh.font.gradientTexture.height, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
					GUI.DrawTexture(r, textMesh.font.gradientTexture);
					
					Rect hr = r;
					hr.width /= textMesh.font.gradientCount;
					hr.x += hr.width * textMesh.textureGradient;
					float ox = hr.width / 8;
					float oy = hr.height / 8;
					Vector3[] rectVerts = { new Vector3(hr.x + 0.5f + ox, hr.y + oy, 0), new Vector3(hr.x + hr.width - ox, hr.y + oy, 0), new Vector3(hr.x + hr.width - ox, hr.y + hr.height -  0.5f - oy, 0), new Vector3(hr.x + ox, hr.y + hr.height - 0.5f - oy, 0) };
					Handles.DrawSolidRectangleWithOutline(rectVerts, new Color(0,0,0,0.2f), new Color(0,0,0,1));
					
					if (GUIUtility.hotControl == 0 && Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
					{
						textMesh.textureGradient = (int)(Event.current.mousePosition.x / (textMesh.font.gradientTexture.width / textMesh.font.gradientCount));
						GUI.changed = true;
					}
	
					EditorGUILayout.EndScrollView();
				}
				
				
				GUILayout.EndHorizontal();
				
				textMesh.inlineStyling = EditorGUILayout.Toggle("Inline Styling", textMesh.inlineStyling);
				if (textMesh.inlineStyling)
				{
					Color bg = GUI.backgroundColor;
					GUI.backgroundColor = new Color32(154, 176, 203, 255);
					GUILayout.TextArea("Inline style commands\n" +
					                   "^0-9 - select gradient\n" +
									   "^^ - print ^");
					GUI.backgroundColor = bg;						
				}
			}
			
			EditorGUILayout.BeginHorizontal();
			
			if (GUILayout.Button("HFlip"))
			{
				Vector3 s = textMesh.scale;
				s.x *= -1.0f;
				textMesh.scale = s;
				GUI.changed = true;
			}
			if (GUILayout.Button("VFlip"))
			{
				Vector3 s = textMesh.scale;
				s.y *= -1.0f;
				textMesh.scale = s;
				GUI.changed = true;
			}			

			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			
			if (GUILayout.Button("Bake Scale"))
			{
				tk2dScaleUtility.Bake(textMesh.transform);
				GUI.changed = true;
			}
			
			GUIContent pixelPerfectButton = new GUIContent("1:1", "Make Pixel Perfect");
			if ( GUILayout.Button(pixelPerfectButton ))
			{
				if (tk2dPixelPerfectHelper.inst) tk2dPixelPerfectHelper.inst.Setup();
				textMesh.MakePixelPerfect();
				GUI.changed = true;
			}
			textMesh.pixelPerfect = GUILayout.Toggle(textMesh.pixelPerfect, "Always", GUILayout.Width(60.0f));
			
			EditorGUILayout.EndHorizontal();
			
			textMesh.useGradient = EditorGUILayout.Toggle("Use Gradient", textMesh.useGradient);
			if (textMesh.useGradient)
			{
				textMesh.color = EditorGUILayout.ColorField("Top Color", textMesh.color);
				textMesh.color2 = EditorGUILayout.ColorField("Bottom Color", textMesh.color2);
			}
			else
			{
				textMesh.color = EditorGUILayout.ColorField("Color", textMesh.color);
			}
			
			if (GUI.changed)
			{
				textMesh.Commit();
				EditorUtility.SetDirty(textMesh);
			}
		}
	}

    [MenuItem("GameObject/Create Other/tk2d/TextMesh", false, 13905)]
    static void DoCreateTextMesh()
    {
		tk2dFontData fontData = null;
		
		// Find reference in scene
        tk2dTextMesh dupeMesh = GameObject.FindObjectOfType(typeof(tk2dTextMesh)) as tk2dTextMesh;
		if (dupeMesh) 
			fontData = dupeMesh.font;
		
		// Find in library
		if (fontData == null)
		{
			tk2dGenericIndexItem[] allFontEntries = tk2dEditorUtility.GetOrCreateIndex().GetFonts();
			foreach (var v in allFontEntries)
			{
				if (v.managed) continue;
				tk2dFontData data = v.GetData<tk2dFontData>();
				if (data != null)
				{
					fontData = data;
					break;
				}
			}
		}
		
		if (fontData == null)
		{
			EditorUtility.DisplayDialog("Create TextMesh", "Unable to create text mesh as no Fonts have been found.", "Ok");
			return;
		}

		GameObject go = tk2dEditorUtility.CreateGameObjectInScene("TextMesh");
        tk2dTextMesh textMesh = go.AddComponent<tk2dTextMesh>();
		textMesh.font = fontData;
		textMesh.text = "New TextMesh";
		textMesh.Commit();
		
		Selection.activeGameObject = go;
		Undo.RegisterCreatedObjectUndo(go, "Create TextMesh");
    }
}
