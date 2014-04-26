using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class tk2dPreferences
{
	static tk2dPreferences _inst = null;	
	public static tk2dPreferences inst
	{
		get 
		{
			if (_inst == null)
			{
				_inst = new tk2dPreferences();
				_inst.Read();
			}
			return _inst;
		}
	}
	
	bool _displayTextureThumbs;
	bool _horizontalAnimDisplay;
	bool _groupAnimDisplay;
	bool _autoRebuild;
	bool _showIds;
	bool _isProSkin;
	int _numGroupedAnimationFrames;
	string _platform;

	public const int default_spriteCollectionListWidth = 200;
	int _spriteCollectionListWidth;
	public const int default_spriteCollectionInspectorWidth = 260;
	int _spriteCollectionInspectorWidth;
	public const string default_platform = "";

	public bool displayTextureThumbs { get { return _displayTextureThumbs; } set { if (_displayTextureThumbs != value) { _displayTextureThumbs = value; Write(); } } }
	public bool horizontalAnimDisplay { get { return _horizontalAnimDisplay; } set { if (_horizontalAnimDisplay != value) { _horizontalAnimDisplay = value; Write(); } } }
	public bool groupAnimDisplay { get { return _groupAnimDisplay; } set { if (_groupAnimDisplay != value) { _groupAnimDisplay = value; Write(); } } }
	public bool autoRebuild { get { return _autoRebuild; } set { if (_autoRebuild != value) { _autoRebuild = value; Write(); } } }
	public bool showIds { get { return _showIds; } set { if (_showIds != value) { _showIds = value; Write(); } } }
	public bool isProSkin { get { return _isProSkin; } set { if (_isProSkin != value) { _isProSkin = value; Write(); } } }
	public int numGroupedAnimationFrames { get { return _numGroupedAnimationFrames; } set { if (_numGroupedAnimationFrames != value) { _numGroupedAnimationFrames = value; Write(); } } }
	public int spriteCollectionInspectorWidth { get { return _spriteCollectionInspectorWidth; } set { if (_spriteCollectionInspectorWidth != value) { _spriteCollectionInspectorWidth = value; Write(); } } }
	public int spriteCollectionListWidth { get { return _spriteCollectionListWidth; } set { if (_spriteCollectionListWidth != value) { _spriteCollectionListWidth = value; Write(); } } }
	public string platform { 
		get { return _platform; } 
		set 
		{  
			if (_platform != value) 
			{ 
				_platform = value; 
				Write(); 

				// mirror to where it matters
				tk2dSystem.CurrentPlatform = _platform;

				// tell the editor things have changed
				tk2dSystemUtility.PlatformChanged();
			} 
		} 
	}

	const string prefix = "tk2d";

	void Read()
	{
		_displayTextureThumbs = EditorPrefs.GetBool(prefix + "_displayTextureThumbs", true);
		_horizontalAnimDisplay = EditorPrefs.GetBool(prefix + "_horizontalAnimDisplay", false);
		_autoRebuild = EditorPrefs.GetBool(prefix + "_autoRebuild", true);
		_showIds = EditorPrefs.GetBool(prefix + "_showIds", false);
		_isProSkin = EditorPrefs.GetBool(prefix + "_proSkin", false);
		_groupAnimDisplay = EditorPrefs.GetBool(prefix + "_groupAnimDisplay", false);

		_numGroupedAnimationFrames = EditorPrefs.GetInt(prefix + "_numGroupedAnimationFrames", 30);
		_spriteCollectionListWidth = EditorPrefs.GetInt(prefix + "_spriteCollectionListWidth", default_spriteCollectionListWidth);
		_spriteCollectionInspectorWidth = EditorPrefs.GetInt(prefix + "_spriteCollectionInspectorWidth", default_spriteCollectionInspectorWidth);
		_platform = EditorPrefs.GetString(prefix + "_platform", default_platform);
	}
	
	public void Write()
	{
		// sanitize values
		_spriteCollectionListWidth = Mathf.Clamp(_spriteCollectionListWidth, 120, 400);
		_spriteCollectionInspectorWidth = Mathf.Clamp(_spriteCollectionInspectorWidth, 260, 600);

		EditorPrefs.SetBool(prefix + "_displayTextureThumbs", _displayTextureThumbs);
		EditorPrefs.SetBool(prefix + "_horizontalAnimDisplay", _horizontalAnimDisplay);
		EditorPrefs.SetBool(prefix + "_autoRebuild", _autoRebuild);
		EditorPrefs.SetBool(prefix + "_showIds", _showIds);
		EditorPrefs.SetBool(prefix + "_proSkin", _isProSkin);
		EditorPrefs.SetBool(prefix + "_groupAnimDisplay", _groupAnimDisplay);

		EditorPrefs.SetInt(prefix + "_numGroupedAnimationFrames", _numGroupedAnimationFrames);
		EditorPrefs.SetInt(prefix + "_spriteCollectionListWidth", _spriteCollectionListWidth);
		EditorPrefs.SetInt(prefix + "_spriteCollectionInspectorWidth", _spriteCollectionInspectorWidth);

		EditorPrefs.SetString(prefix + "_platform", _platform);
	}
}

public class tk2dPreferencesEditor : EditorWindow
{
	GUIContent label_spriteThumbnails = new GUIContent("Sprite Thumbnails", "Turn off sprite thumbnails to save memory.");
	
	GUIContent label_animationFrames = new GUIContent("Animation Frame Display", "Select the direction of frames in the SpriteAnimation inspector.");
	GUIContent label_animFrames_Horizontal = new GUIContent("Horizontal");
	GUIContent label_animFrames_Vertical = new GUIContent("Vertical");
	
	GUIContent label_autoRebuild = new GUIContent("Auto Rebuild", "Auto rebuild sprite collections when source textures have changed.");
	GUIContent label_groupAnimDisplay = new GUIContent("Group Animation Display", "Group frames, and allow changing frame count in SpriteAnimation inspector.");

	GUIContent label_showIds = new GUIContent("Show Ids", "Show sprite and animation Ids.");
	
	GUIContent label_numGroupedAnimationFrames = new GUIContent("Grouped Frames", "Maximum number of frames to group.");
	
#if (UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4)
	GUIContent label_proSkin = new GUIContent("Pro Skin", "Select this to use the Dark skin.");
#endif	
	void OnGUI()
	{
		tk2dPreferences prefs = tk2dPreferences.inst;

		// NOTE ppoirier: Deprecated.
		//EditorGUIUtility.LookLikeControls(150.0f);
		
		prefs.displayTextureThumbs = EditorGUILayout.Toggle(label_spriteThumbnails, prefs.displayTextureThumbs);
		
		int had = EditorGUILayout.Popup(label_animationFrames, prefs.horizontalAnimDisplay?0:1, new GUIContent[] { label_animFrames_Horizontal, label_animFrames_Vertical } );
		prefs.horizontalAnimDisplay = (had == 0)?true:false;
		EditorGUILayout.Toggle(label_groupAnimDisplay, prefs.groupAnimDisplay);
		int newNumGroupedAnimationFrames = EditorGUILayout.IntField(label_numGroupedAnimationFrames, prefs.numGroupedAnimationFrames);
		prefs.numGroupedAnimationFrames = Mathf.Max(newNumGroupedAnimationFrames, 30); // sanity check

		prefs.autoRebuild = EditorGUILayout.Toggle(label_autoRebuild, prefs.autoRebuild);
		
		prefs.showIds = EditorGUILayout.Toggle(label_showIds, prefs.showIds);

		if (GUILayout.Button("Reset Sprite Collection Editor Sizes"))
		{
			prefs.spriteCollectionListWidth = tk2dPreferences.default_spriteCollectionListWidth;
			prefs.spriteCollectionInspectorWidth = tk2dPreferences.default_spriteCollectionInspectorWidth;
		}

		if (tk2dSystem.inst_NoCreate != null)
		{
			string newPlatform = tk2dGuiUtility.PlatformPopup(tk2dSystem.inst_NoCreate, "Platform", prefs.platform);
			if (newPlatform != prefs.platform)
			{
				prefs.platform = newPlatform;
				tk2dEditorUtility.UnloadUnusedAssets();
			}
		}

#if (UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4)
		prefs.isProSkin = EditorGUILayout.Toggle(label_proSkin, prefs.isProSkin);
#endif
	}
}
