using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[AddComponentMenu("2D Toolkit/Text/tk2dTextMesh")]
/// <summary>
/// Text mesh
/// </summary>
public class tk2dTextMesh : MonoBehaviour, tk2dRuntime.ISpriteCollectionForceBuild
{
	[SerializeField] tk2dFontData _font;
	tk2dFontData _fontInst;
    [SerializeField] string _text = ""; 
	string _formattedText = "";
    [SerializeField] Color _color = Color.white; 
    [SerializeField] Color _color2 = Color.white; 
    [SerializeField] bool _useGradient = false; 
	[SerializeField] int _textureGradient = 0;
    [SerializeField] TextAnchor _anchor = TextAnchor.LowerLeft; 
    [SerializeField] Vector3 _scale = new Vector3(1.0f, 1.0f, 1.0f); 
	[SerializeField] bool _kerning = false; 
    [SerializeField] int _maxChars = 16; 
	[SerializeField] bool _inlineStyling = false;

	// Enable formatting
    [SerializeField] bool _formatting = false; 
    [SerializeField] int _wordWrapWidth = 0; 

	/// <summary>
	/// Specifies if this textMesh is kept pixel perfect
	/// </summary>
	public bool pixelPerfect = false;
	/// <summary>
	/// Deprecated: Use <see cref="Spacing"/> instead.
	/// Additional spacing between characters. This can be negative to bring characters closer together.
	/// This is in the font local space.
	/// </summary>
	public float spacing = 0.0f;
	/// <summary>
	/// Deprecated: Use <see cref="LineSpacing"/> instead.
	/// Additional line spacing for multiline text. This can be negative to bring lines closer together.
	/// This is in font local space.
	/// </summary>
	public float lineSpacing = 0.0f;
	
    Vector3[] vertices;
    Vector2[] uvs;
	Vector2[] uv2;
    Color[] colors;

	void FormatText()
	{
		if (formatting == false || wordWrapWidth == 0 || _fontInst.texelSize == Vector2.zero)
		{
			_formattedText = _text;
			return;
		}

		float lineWidth = _fontInst.texelSize.x * wordWrapWidth;

		System.Text.StringBuilder target = new System.Text.StringBuilder(_text.Length);
		float widthSoFar = 0.0f;
		float wordStart = 0.0f;
		int targetWordStartIndex = -1;
		int fmtWordStartIndex = -1;
		for (int i = 0; i < _text.Length; ++i)
		{
            char idx = _text[i];
			tk2dFontChar chr;
			
			if (_fontInst.useDictionary)
			{
				if (!_fontInst.charDict.ContainsKey(idx)) idx = (char)0;
				chr = _fontInst.charDict[idx];
			}
			else
			{
	            if (idx >= _fontInst.chars.Length) idx = (char)0; // should be space
	            chr = _fontInst.chars[idx];
			}

			if (idx == '\n') 
			{
				widthSoFar = 0.0f;
				wordStart = 0.0f;
				targetWordStartIndex = target.Length;
				fmtWordStartIndex = i;
			}
			else if (idx == ' ' || idx == '.' || idx == ',' || idx == ':' || idx == ';' || idx == '!')
			{
				if ((widthSoFar + chr.p1.x * _scale.x) > lineWidth)
				{
					target.Append('\n');
					widthSoFar = chr.advance * _scale.x;
				}
				else
				{
					widthSoFar += chr.advance * _scale.x;
				}

				wordStart = widthSoFar;
				targetWordStartIndex = target.Length;
				fmtWordStartIndex = i;
			}
			else
			{
				if ((widthSoFar + chr.p1.x * _scale.x) > lineWidth)
				{
					// If the last word started after the start of the line
					if (wordStart > 0.0f)
					{
						wordStart = 0.0f;
						widthSoFar = 0.0f;
						// rewind
						target.Remove(targetWordStartIndex + 1, target.Length - targetWordStartIndex - 1);
						target.Append('\n');
						i = fmtWordStartIndex;
						continue; // don't add this character
					}
					else
					{
						target.Append('\n');
						widthSoFar = chr.advance * _scale.x;
					}
				}
				else
				{
					widthSoFar += chr.advance * _scale.x;
				}
			}
			
			target.Append(idx);
		}
		_formattedText = target.ToString();
	}

	[System.FlagsAttribute]
	enum UpdateFlags
	{
		UpdateNone		= 0,
		UpdateText		= 1,	// update text vertices & uvs
		UpdateColors	= 2,	// only colors have changed
		UpdateBuffers	= 4,	// update buffers (maxchars has changed)
	};
	UpdateFlags updateFlags = UpdateFlags.UpdateBuffers;

    Mesh mesh;
	MeshFilter meshFilter;

	// accessors
	/// <summary>Gets or sets the font. Call <see cref="Commit"/> to commit changes.</summary>
	public tk2dFontData font 
	{ 
		get { return _font; } 
		set 
		{ 
			_font = value; 
			_fontInst = _font.inst;
			updateFlags |= UpdateFlags.UpdateText;

			UpdateMaterial();
		} 
	}

	/// <summary>Enables or disables formatting. Call <see cref="Commit"/> to commit changes.</summary>
	public bool formatting
	{
		get { return _formatting; }
		set
		{
			if (_formatting != value)
			{
				_formatting = value;
				updateFlags |= UpdateFlags.UpdateText;
			}
		}
	}

	/// <summary>Change word wrap width. This only works when formatting is enabled. 
	/// Call <see cref="Commit"/> to commit changes.</summary>
	public int wordWrapWidth
	{
		get { return _wordWrapWidth; }
		set { if (_wordWrapWidth != value) { _wordWrapWidth = value; updateFlags |= UpdateFlags.UpdateText; } }
	}

	/// <summary>Gets or sets the text. Call <see cref="Commit"/> to commit changes.</summary>
	public string text 
	{ 
		get { return _text; } 
		set 
		{
			_text = value;
			updateFlags |= UpdateFlags.UpdateText;
		}
	}

	/// <summary>Gets or sets the color. Call <see cref="Commit"/> to commit changes.</summary>
	public Color color { get { return _color; } set { _color = value; updateFlags |= UpdateFlags.UpdateColors; } }
	/// <summary>Gets or sets the secondary color (used in the gradient). Call <see cref="Commit"/> to commit changes.</summary>
	public Color color2 { get { return _color2; } set { _color2 = value; updateFlags |= UpdateFlags.UpdateColors; } }
	/// <summary>Use vertex vertical gradient. Call <see cref="Commit"/> to commit changes.</summary>
	public bool useGradient { get { return _useGradient; } set { _useGradient = value; updateFlags |= UpdateFlags.UpdateColors; } }
	/// <summary>Gets or sets the text anchor. Call <see cref="Commit"/> to commit changes.</summary>
	public TextAnchor anchor { get { return _anchor; } set { _anchor = value; updateFlags |= UpdateFlags.UpdateText; } }
	/// <summary>Gets or sets the scale. Call <see cref="Commit"/> to commit changes.</summary>
	public Vector3 scale { get { return _scale; } set { _scale = value; updateFlags |= UpdateFlags.UpdateText; } }
	/// <summary>Gets or sets kerning state. Call <see cref="Commit"/> to commit changes.</summary>
	public bool kerning { get { return _kerning; } set { _kerning = value; updateFlags |= UpdateFlags.UpdateText; } }
	/// <summary>Gets or sets maxChars. Call <see cref="Commit"/> to commit changes.
	/// NOTE: This will free & allocate memory, avoid using at runtime.
	/// </summary>
	public int maxChars { get { return _maxChars; } set { _maxChars = value; updateFlags |= UpdateFlags.UpdateBuffers; } }
	/// <summary>Gets or sets the default texture gradient. 
	/// You can also change texture gradient inline by using ^1 - ^9 sequences within your text.
	/// Call <see cref="Commit"/> to commit changes.</summary>
	public int textureGradient { get { return _textureGradient; } set { _textureGradient = value % font.gradientCount; updateFlags |= UpdateFlags.UpdateText; } }
	/// <summary>Enables or disables inline styling (texture gradient). Call <see cref="Commit"/> to commit changes.</summary>
	public bool inlineStyling { get { return _inlineStyling; } set { _inlineStyling = value; updateFlags |= tk2dTextMesh.UpdateFlags.UpdateText; } }
	/// <summary>Additional spacing between characters. 
	/// This can be negative to bring characters closer together.
	/// Call <see cref="Commit"/> to commit changes.</summary>
	public float Spacing { get { return spacing; } set { if (spacing != value) { spacing = value; updateFlags |= UpdateFlags.UpdateText; } } }
	/// <summary>Additional line spacing for multieline text. 
	/// This can be negative to bring lines closer together.
	/// Call <see cref="Commit"/> to commit changes.</summary>
	public float LineSpacing { get { return lineSpacing; } set { if (lineSpacing != value) { lineSpacing = value; updateFlags |= UpdateFlags.UpdateText; } } }
	
	void InitInstance()
	{
		if (_fontInst == null && _font != null)
			_fontInst = _font.inst;
	}

	// Use this for initialization
	void Awake() 
	{
		if (_font != null)
			_fontInst = _font.inst;

		if (pixelPerfect)
			MakePixelPerfect();
		
		// force rebuild when awakened, for when the object has been pooled, etc
		// this is probably not the best way to do it
		updateFlags = UpdateFlags.UpdateBuffers;
		
		if (_font != null)
		{
	        Init();
			UpdateMaterial();
		}
	}

	protected void OnDestroy()
	{
		if (meshFilter == null)
		{
			meshFilter = GetComponent<MeshFilter>();
		}
		if (meshFilter != null)
		{
			mesh = meshFilter.sharedMesh;
		}
		
		if (mesh)
		{
			DestroyImmediate(mesh, true);
			meshFilter.mesh = null;
		}
	}
	
	bool useInlineStyling { get { return inlineStyling && _fontInst.textureGradients; } }
	
	/// <summary>
	/// Returns the number of characters drawn for the currently active string.
	/// This may be less than string.Length - some characters are used as escape codes for switching texture gradient ^0-^9
	/// Also, there might be more characters in the string than have been allocated for the textmesh, in which case
	/// the string will be truncated.
	/// </summary>
	public int NumDrawnCharacters()
	{
		InitInstance();

		if ((updateFlags & (UpdateFlags.UpdateText | UpdateFlags.UpdateBuffers)) != 0)
			FormatText();

		bool _useInlineStyling = useInlineStyling;
		int charsDrawn = 0;
		for (int i = 0; i < _formattedText.Length && charsDrawn < _maxChars; ++i)
		{
            int idx = _formattedText[i];
			
			if (_fontInst.useDictionary)
			{
				if (!_fontInst.charDict.ContainsKey(idx)) idx = 0;
			}
			else
			{
	            if (idx >= _fontInst.chars.Length) idx = 0; // should be space
			}

			if (idx == '\n')
			{
				continue;
			}
			else if (_useInlineStyling)
			{
				if (idx == '^')
				{
					if (i+1 < _formattedText.Length)
					{
						i++;
						if (_formattedText[i] != '^')
						{
							continue;
						}
					}
				}
			}
			
			++charsDrawn;
		}
		return charsDrawn;
	}
	
	/// <summary>
	/// Returns the number of characters excluding texture gradient escape codes.
	/// </summary>
	public int NumTotalCharacters()
	{
		InitInstance();

		bool _useInlineStyling = useInlineStyling;
		int numChars = 0;
		for (int i = 0; i < _formattedText.Length; ++i)
		{
            int idx = _formattedText[i];

			if (_fontInst.useDictionary)
			{
				if (!_fontInst.charDict.ContainsKey(idx)) idx = 0;
			}
			else
			{
	            if (idx >= _fontInst.chars.Length) idx = 0; // should be space
			}

			if (idx == '\n')
			{
				continue;
			}
			else if (_useInlineStyling)
			{
				if (idx == '^')
				{
					if (i+1 < _formattedText.Length)
					{
						i++;
						if (_formattedText[i] != '^')
						{
							continue;
						}
					}
				}
			}
			
			++numChars;
		}
		return numChars;
	}
	
	void PostAlignTextData(int targetStart, int targetEnd, float offsetX)
	{
		for (int i = targetStart * 4; i < targetEnd * 4; ++i)
		{
			Vector3 v = vertices[i];
			v.x += offsetX;
			vertices[i] = v;
		}
	}
	
	int FillTextData()
	{
		Vector2 gradientOffset = new Vector2((float)_textureGradient / font.gradientCount, 0);
		
		Vector2 dims = GetMeshDimensionsForString(_formattedText);
		float offsetY = GetYAnchorForHeight(dims.y);
		
		bool _useInlineStyling = useInlineStyling;
        float cursorX = 0.0f;
		float cursorY = 0.0f;
		int target = 0;
		int alignStartTarget = 0;
		for (int i = 0; i < _formattedText.Length && target < _maxChars; ++i)
		{
            int idx = _formattedText[i];
			tk2dFontChar chr;
			
			if (_fontInst.useDictionary)
			{
				if (!_fontInst.charDict.ContainsKey(idx)) idx = 0;
				chr = _fontInst.charDict[idx];
			}
			else
			{
	            if (idx >= _fontInst.chars.Length) idx = 0; // should be space
				chr = _fontInst.chars[idx];
			}

			if (idx == '\n')
			{
				float lineWidth = cursorX;
				int alignEndTarget = target; // this is one after the last filled character
				if (alignStartTarget != target)
				{
					float xOffset = GetXAnchorForWidth(lineWidth);
					PostAlignTextData(alignStartTarget, alignEndTarget, xOffset);
				}
				
				
				alignStartTarget = target;
				cursorX = 0.0f;
				cursorY -= (_fontInst.lineHeight + lineSpacing) * _scale.y;
				continue;
			}
			else if (_useInlineStyling)
			{
				if (idx == '^')
				{
					if (i+1 < _formattedText.Length)
					{
						i++;
						if (_formattedText[i] != '^')
						{
							int data = _formattedText[i] - '0';
							gradientOffset = new Vector2((float)data / font.gradientCount, 0);
							continue;
						}
					}
				}
			}
			
            vertices[target * 4 + 0] = new Vector3(cursorX + chr.p0.x * _scale.x, offsetY + cursorY + chr.p0.y * _scale.y, 0);
            vertices[target * 4 + 1] = new Vector3(cursorX + chr.p1.x * _scale.x, offsetY + cursorY + chr.p0.y * _scale.y, 0);
            vertices[target * 4 + 2] = new Vector3(cursorX + chr.p0.x * _scale.x, offsetY + cursorY + chr.p1.y * _scale.y, 0);
            vertices[target * 4 + 3] = new Vector3(cursorX + chr.p1.x * _scale.x, offsetY + cursorY + chr.p1.y * _scale.y, 0);

			if (chr.flipped)
			{
	            uvs[target * 4 + 0] = new Vector2(chr.uv1.x, chr.uv1.y);
	            uvs[target * 4 + 1] = new Vector2(chr.uv1.x, chr.uv0.y);
	            uvs[target * 4 + 2] = new Vector2(chr.uv0.x, chr.uv1.y);
	            uvs[target * 4 + 3] = new Vector2(chr.uv0.x, chr.uv0.y);
			}
			else			
			{
	            uvs[target * 4 + 0] = new Vector2(chr.uv0.x, chr.uv0.y);
	            uvs[target * 4 + 1] = new Vector2(chr.uv1.x, chr.uv0.y);
	            uvs[target * 4 + 2] = new Vector2(chr.uv0.x, chr.uv1.y);
	            uvs[target * 4 + 3] = new Vector2(chr.uv1.x, chr.uv1.y);
			}
			
			if (_fontInst.textureGradients)
			{
				uv2[target * 4 + 0] = gradientOffset + chr.gradientUv[0];
				uv2[target * 4 + 1] = gradientOffset + chr.gradientUv[1];
				uv2[target * 4 + 2] = gradientOffset + chr.gradientUv[2];
				uv2[target * 4 + 3] = gradientOffset + chr.gradientUv[3];
			}

            cursorX += (chr.advance + spacing) * _scale.x;
			
			if (_kerning && i < _formattedText.Length - 1)
			{
				foreach (var k in _fontInst.kerning)
				{
					if (k.c0 == _formattedText[i] && k.c1 == _formattedText[i+1])
					{
						cursorX += k.amount * _scale.x;
						break;
					}
				}
			}				
			
			++target;
		}
		
		if (alignStartTarget != target)
		{
			float lineWidth = cursorX;
			int alignEndTarget = target;
			float xOffset = GetXAnchorForWidth(lineWidth);
			PostAlignTextData(alignStartTarget, alignEndTarget, xOffset);
		}
		
		return target;		
	}
	
	public void Init(bool force)
	{
		if (force)
		{
			updateFlags |= UpdateFlags.UpdateBuffers;
		}
		Init();
	}
	
	public void Init()
    {
        if (_fontInst && ((updateFlags & UpdateFlags.UpdateBuffers) != 0 || mesh == null))
        {
        	FormatText();
			_fontInst.InitDictionary();
			
            Color topColor = _color;
            Color bottomColor = _useGradient?_color2:_color;

            // volatile data
            vertices = new Vector3[_maxChars * 4];
            uvs = new Vector2[_maxChars * 4];
            colors = new Color[_maxChars * 4];
			if (_fontInst.textureGradients)
			{
				uv2 = new Vector2[_maxChars * 4];
			}
            int[] triangles = new int[_maxChars * 6];
			int target = FillTextData();
			
			for (int i = 0; i < target; ++i)
			{
                colors[i * 4 + 0] = colors[i * 4 + 1] = topColor;
                colors[i * 4 + 2] = colors[i * 4 + 3] = bottomColor;

                triangles[i * 6 + 0] = i * 4 + 0;
                triangles[i * 6 + 1] = i * 4 + 1;
                triangles[i * 6 + 2] = i * 4 + 3;
                triangles[i * 6 + 3] = i * 4 + 2;
                triangles[i * 6 + 4] = i * 4 + 0;
                triangles[i * 6 + 5] = i * 4 + 3;
			}
			
			for (int i = target; i < _maxChars; ++i)
			{
                vertices[i * 4 + 0] = vertices[i * 4 + 1] = vertices[i * 4 + 2] = vertices[i * 4 + 3] = Vector3.zero;
                uvs[i * 4 + 0] = uvs[i * 4 + 1] = uvs[i * 4 + 2] = uvs[i * 4 + 3] = Vector2.zero;
				if (_fontInst.textureGradients) 
				{
                    uv2[i * 4 + 0] = uv2[i * 4 + 1] = uv2[i * 4 + 2] = uv2[i * 4 + 3] = Vector2.zero;
				}				

				colors[i * 4 + 0] = colors[i * 4 + 1] = topColor;
                colors[i * 4 + 2] = colors[i * 4 + 3] = bottomColor;

                triangles[i * 6 + 0] = i * 4 + 0;
                triangles[i * 6 + 1] = i * 4 + 1;
                triangles[i * 6 + 2] = i * 4 + 3;
                triangles[i * 6 + 3] = i * 4 + 2;
                triangles[i * 6 + 4] = i * 4 + 0;
                triangles[i * 6 + 5] = i * 4 + 3;
			}

			if (mesh == null)
			{
				if (meshFilter == null)
					meshFilter = GetComponent<MeshFilter>();
				
				mesh = new Mesh();
				mesh.hideFlags = HideFlags.DontSave;
				meshFilter.mesh = mesh;
			}
			else
			{
				mesh.Clear();
			}
            mesh.vertices = vertices;
            mesh.uv = uvs;
			if (font.textureGradients)
			{
				mesh.uv1 = uv2;
			}
            mesh.triangles = triangles;
            mesh.colors = colors;
			mesh.RecalculateBounds();

			updateFlags = UpdateFlags.UpdateNone;
    	}
    }
	
	/// <summary>
	/// Call commit after changing properties to commit the changes.
	/// This is deffered to a commit call as more than one operation may require rebuilding the buffers, eg. scaling and changing text.
	/// This will be wasteful if performed multiple times.
	/// </summary>
    public void Commit()
    {
    	// Make sure instance is set up, might not be when calling from Awake.
		InitInstance();

		// make sure fonts dictionary is initialized properly before proceeding
		_fontInst.InitDictionary();
		
		// Can come in here without anything initalized when
		// instantiated in code
		if ((updateFlags & UpdateFlags.UpdateBuffers) != 0 || mesh == null)
		{
			Init();
		}
        else 
		{
			if ((updateFlags & UpdateFlags.UpdateText) != 0)
	        {
	        	FormatText();
				int target = FillTextData();
				for (int i = target; i < _maxChars; ++i)
				{
					// was/is unnecessary to fill anything else
                    vertices[i * 4 + 0] = vertices[i * 4 + 1] = vertices[i * 4 + 2] = vertices[i * 4 + 3] = Vector3.zero;
	            }
	
	            mesh.vertices = vertices;
	            mesh.uv = uvs;
				if (_fontInst.textureGradients)
				{
					mesh.uv1 = uv2;
				}
				
				// comment this in for game if it becomes a problem
				mesh.RecalculateBounds();
	        }
	
	        if ((updateFlags & UpdateFlags.UpdateColors) != 0)
	        {
	            Color topColor = _color;
	            Color bottomColor = _useGradient ? _color2 : _color;
	
	            for (int i = 0; i < colors.Length; i += 4)
	            {
	                colors[i + 0] = colors[i + 1] = topColor;
	                colors[i + 2] = colors[i + 3] = bottomColor;
	            }
	            mesh.colors = colors;
	        }
		}
		
		updateFlags = UpdateFlags.UpdateNone;
    }
	
	/// <summary>
	/// Calculates the mesh dimensions for the given string
	/// and returns a width and height.
	/// </summary>
	public Vector2 GetMeshDimensionsForString(string str)
	{
		bool _useInlineStyling = useInlineStyling;
		float maxWidth = 0.0f;
		
        float cursorX = 0.0f;
		float cursorY = 0.0f;
		
		int target = 0;
		for (int i = 0; i < str.Length && target < _maxChars; ++i)
		{
            int idx = str[i];
			if (idx == '\n')
			{
				maxWidth = Mathf.Max(cursorX, maxWidth);
				cursorX = 0.0f;
				cursorY -= (_fontInst.lineHeight + lineSpacing) * _scale.y;
				continue;
			}
			else if (_useInlineStyling)
			{
				if (idx == '^')
				{
					if (i+1 < str.Length)
					{
						i++;
						if (str[i] != '^')
						{
							continue;
						}
					}
				}
			}

			// Get the character from dictionary / array
			tk2dFontChar chr;
			if (_fontInst.useDictionary)
			{
				if (!_fontInst.charDict.ContainsKey(idx)) idx = 0;
				chr = _fontInst.charDict[idx];
			}
			else
			{
	            if (idx >= _fontInst.chars.Length) idx = 0; // should be space
				chr = _fontInst.chars[idx];
			}
			
            cursorX += (chr.advance + spacing) * _scale.x;
			if (_kerning && i < str.Length - 1)
			{
				foreach (var k in _fontInst.kerning)
				{
					if (k.c0 == str[i] && k.c1 == str[i+1])
					{
						cursorX += k.amount * _scale.x;
						break;
					}
				}
			}				
			
			++target;
		}
		
		maxWidth = Mathf.Max(cursorX, maxWidth);
		cursorY -= (_fontInst.lineHeight + lineSpacing) * _scale.y;
		
		return new Vector2(maxWidth, cursorY);
	}
	
	float GetYAnchorForHeight(float textHeight)
	{
		int heightAnchor = (int)_anchor / 3;
		float lineHeight = (_fontInst.lineHeight + lineSpacing) * _scale.y;
		switch (heightAnchor)
		{
            case 0: return -lineHeight;
            case 1:
            {
            	float y = -textHeight / 2.0f - lineHeight;
            	if (_fontInst.version >= 2) 
            	{
            		float ty = _fontInst.texelSize.y * _scale.y;
					return Mathf.Floor(y / ty) * ty;
            	}
            	else return y;
            }
            case 2: return -textHeight - lineHeight;
		}
		return -lineHeight;
	}
	
	float GetXAnchorForWidth(float lineWidth)
	{
		int widthAnchor = (int)_anchor % 3;
		switch (widthAnchor)
		{
			case 0: return 0.0f; // left
			case 1: // center
			{
				float x = -lineWidth / 2.0f;
				if (_fontInst.version >= 2) 
				{
					float tx = _fontInst.texelSize.x * _scale.x;
					return Mathf.Floor(x / tx) * tx;
				}
				return x;
			}
			case 2: return -lineWidth; // right
		}
		return 0.0f;
	}

	/// <summary>
	/// Makes the text mesh pixel perfect to the active camera.
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
		else if (tk2dCamera.inst != null)
		{
			if (_fontInst.version < 1)
			{
				Debug.LogError("Need to rebuild font.");
			}

			s = _fontInst.invOrthoSize * _fontInst.halfTargetHeight;
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
		scale = new Vector3(Mathf.Sign(scale.x) * s, Mathf.Sign(scale.y) * s, Mathf.Sign(scale.z) * s);
	}	
	
	// tk2dRuntime.ISpriteCollectionEditor
	public bool UsesSpriteCollection(tk2dSpriteCollectionData spriteCollection)
	{
		if (_font != null && _font.spriteCollection != null)
			return _font.spriteCollection == spriteCollection;
		
		// No easy way to identify this at this stage
		return true;
	}
	
	void UpdateMaterial()
	{
		if (renderer.sharedMaterial != _fontInst.materialInst)
			renderer.material = _fontInst.materialInst;
	}
	
	public void ForceBuild()
	{
		if (_font != null)
		{
			_fontInst = _font.inst;
			UpdateMaterial();
		}
		Init(true);
	}
}
