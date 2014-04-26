using UnityEngine;
using UnityEditor;
using System.Collections;

public class tk2dEditorSkin
{
	static GUISkin skin;
	static bool isProSkin;
	
	// Sprite collection editor styles
	static GUIStyle sc_inspectorBG;
	static GUIStyle sc_inspectorHeaderBG;
	static GUIStyle sc_listBoxBG;
	static GUIStyle sc_listBoxItem;
	static GUIStyle sc_listBoxSectionHeader;
	static GUIStyle sc_bodyBackground;
	static GUIStyle sc_dropBox;
	static GUIStyle toolbarSearch;
	static GUIStyle toolbarSearchClear;
	static GUIStyle toolbarSearchRightCap;
	
	static string FindAsset(string name)
	{
		string[] files = System.IO.Directory.GetFiles("Assets", name, System.IO.SearchOption.AllDirectories);
		if (files.Length > 0)
		{
			if (Application.platform == RuntimePlatform.WindowsEditor)
				return files[0].Replace('\\', '/');
			else
				return files[0];
		}
			
		Debug.LogError("2D Toolkit - This is a serious error, the unity package has not been imported correctly.");
		return "";
	}
	
	static string GetSkinPath()
	{
		if (isProSkin)
		{
			string guid = "83a9c395d150f784e83608904bfb4ae2";
			string path = AssetDatabase.GUIDToAssetPath(guid);
			if (path.Length > 0) return path;
			else return FindAsset("tk2dpro.guiskin");
		}
		else
		{
			string guid = "e94c4ee922624114994b40051e97e72a";
			string path = AssetDatabase.GUIDToAssetPath(guid);
			if (path.Length > 0) return path;
			else return FindAsset("tk2dfree.guiskin");
		}
	}
	
	public static void Init()
	{
#if (UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4)
		if (isProSkin != tk2dPreferences.inst.isProSkin)
		{
			skin = null;
			isProSkin = tk2dPreferences.inst.isProSkin;
		}
#else
		if (isProSkin != EditorGUIUtility.isProSkin)
		{
			skin = null;
			isProSkin = EditorGUIUtility.isProSkin;
		}
#endif
		
		if (skin == null)
		{
			skin = AssetDatabase.LoadAssetAtPath(GetSkinPath(), typeof(GUISkin)) as GUISkin;
			if (skin != null)
			{
				sc_inspectorBG = skin.FindStyle("SC_InspectorBG");
				sc_inspectorHeaderBG = skin.FindStyle("SC_InspectorHeaderBG");
				sc_listBoxBG = skin.FindStyle("SC_ListBoxBG");
				sc_listBoxItem = skin.FindStyle("SC_ListBoxItem");
				sc_listBoxSectionHeader = skin.FindStyle("SC_ListBoxSectionHeader");
				sc_bodyBackground = skin.FindStyle("SC_BodyBackground");
				sc_dropBox = skin.FindStyle("SC_DropBox");
				
				toolbarSearch = skin.FindStyle("ToolbarSearch");
				toolbarSearchClear = skin.FindStyle("ToolbarSearchClear");
				toolbarSearchRightCap = skin.FindStyle("ToolbarSearchRightCap");
			}
		}
	}
	
	public static GUIStyle SC_InspectorBG { get { Init(); return sc_inspectorBG; } }
	public static GUIStyle SC_InspectorHeaderBG { get { Init(); return sc_inspectorHeaderBG; } }
	public static GUIStyle SC_ListBoxBG { get { Init(); return sc_listBoxBG; } }
	public static GUIStyle SC_ListBoxItem { get { Init(); return sc_listBoxItem; } }
	public static GUIStyle SC_ListBoxSectionHeader { get { Init(); return sc_listBoxSectionHeader; } }	
	public static GUIStyle SC_BodyBackground { get { Init(); return sc_bodyBackground; } }	
	public static GUIStyle SC_DropBox { get { Init(); return sc_dropBox; } }	
	
	public static GUIStyle ToolbarSearch { get { Init(); return toolbarSearch; } }
	public static GUIStyle ToolbarSearchClear { get { Init(); return toolbarSearchClear; } }
	public static GUIStyle ToolbarSearchRightCap { get { Init(); return toolbarSearchRightCap; } }
}
