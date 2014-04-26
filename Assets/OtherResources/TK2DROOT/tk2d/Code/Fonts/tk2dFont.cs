using UnityEngine;
using System.Collections;

[AddComponentMenu("2D Toolkit/Backend/tk2dFont")]
public class tk2dFont : MonoBehaviour 
{
	public Object bmFont;
	public Material material;
	public Texture texture;
	public Texture2D gradientTexture;
    public bool dupeCaps = false; // duplicate lowercase into uc, or vice-versa, depending on which exists
	public bool flipTextureY = false;
	
	[HideInInspector]
	public bool proxyFont = false;

	[HideInInspector]
	public bool useTk2dCamera = false;
	[HideInInspector]
	public int targetHeight = 640;
	[HideInInspector]
	public float targetOrthoSize = 1.0f;
	
	public int gradientCount = 1;
	
	public bool manageMaterial = false;
	
	public int charPadX = 0;
	
	public tk2dFontData data;
}
