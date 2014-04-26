using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public static class tk2dEditorUtility
{
	public static double version = 1.8;
	public static int releaseId = 3; // < -10000 = alpha, other negative = beta release, 0 = final, positive = final patch
	
	public static string ReleaseStringIdentifier(double _version, int _releaseId)
	{
		string id = _version.ToString();
		if (_releaseId == 0) id += " final";
		else if (_releaseId > 0) id += " final + patch " + _releaseId.ToString();
		else if (_releaseId < -10000) id += " alpha " + (-_releaseId - 10000).ToString();
		else if (_releaseId < 0) id += " beta " + (-_releaseId).ToString();
		return id;
	}
	
	/// <summary>
	/// Release filename for the current version
	/// </summary>
	public static string CurrentReleaseFileName()
	{
		string id = "2dtoolkit" + version.ToString();
		if (releaseId == 0) id += "final";
		else if (releaseId > 0) id += "final_patch" + releaseId.ToString();
		else if (releaseId < -10000) id += " alpha " + (-releaseId - 10000).ToString();
		else if (releaseId < 0) id += "beta" + (-releaseId).ToString();
		return id;
	}
	
	[MenuItem(tk2dMenu.root + "About", false, 10100)]
	public static void About2DToolkit()
	{
		EditorUtility.DisplayDialog("About 2D Toolkit",
		                            "2D Toolkit Version " + ReleaseStringIdentifier(version, releaseId) + "\n" +
 		                            "Copyright (c) 2011 Unikron Software Ltd",
		                            "Ok");
	}
	
	[MenuItem(tk2dMenu.root + "Documentation", false, 10098)]
	public static void LaunchWikiDocumentation()
	{
		Application.OpenURL("http://www.2dtoolkit.com/doc");
	}

	[MenuItem(tk2dMenu.root + "Forum", false, 10099)]
	public static void LaunchForum()
	{
		Application.OpenURL("http://www.2dtoolkit.com/forum");
	}

	[MenuItem(tk2dMenu.root + "Rebuild Index", false, 1)]
	public static void RebuildIndex()
	{
		AssetDatabase.DeleteAsset(indexPath);
		AssetDatabase.Refresh();
		CreateIndex();

		// Now rebuild system object
		tk2dSystemUtility.RebuildResources();
	}
	
	[MenuItem(tk2dMenu.root + "Preferences...", false, 1)]
	public static void ShowPreferences()
	{
		EditorWindow.GetWindow( typeof(tk2dPreferencesEditor), true, "2D Toolkit Preferences" );
	}	

	public static string CreateNewPrefab(string name) // name is the filename of the prefab EXCLUDING .prefab
	{
		Object obj = Selection.activeObject;
		string assetPath = AssetDatabase.GetAssetPath(obj);
		if (assetPath.Length == 0)
		{
			assetPath = tk2dGuiUtility.SaveFileInProject("Create...", "Assets/", name, "prefab");
		}
		else
		{
			// is a directory
			string path = System.IO.Directory.Exists(assetPath) ? assetPath : System.IO.Path.GetDirectoryName(assetPath);
			assetPath = AssetDatabase.GenerateUniqueAssetPath(path + "/" + name + ".prefab");
		}
		
		return assetPath;
	}
	
	
	const string indexPath = "Assets/-tk2d.asset";
	static tk2dIndex index = null;
	
	public static tk2dIndex GetExistingIndex()
	{
		if (index == null)
		{
			index = Resources.LoadAssetAtPath(indexPath, typeof(tk2dIndex)) as tk2dIndex;
		}
		return index;
	}
	
	public static tk2dIndex ForceCreateIndex()
	{
		CreateIndex();
		return GetExistingIndex();
	}
	
	public static tk2dIndex GetOrCreateIndex()
	{
		tk2dIndex thisIndex = GetExistingIndex();
		if (thisIndex == null || thisIndex.version != tk2dIndex.CURRENT_VERSION)
		{
			CreateIndex();
			thisIndex = GetExistingIndex();
		}
		
		return thisIndex;
	}
	
	public static void CommitIndex()
	{
		if (index)
		{
			EditorUtility.SetDirty(index);
			tk2dSpriteGuiUtility.ResetCache();
		}
	}
	
	static void CreateIndex()
	{
		tk2dIndex newIndex = ScriptableObject.CreateInstance<tk2dIndex>();
		newIndex.version = tk2dIndex.CURRENT_VERSION;
		
		List<string> rebuildSpriteCollectionPaths = new List<string>();
		
		// check all prefabs to see if we can find any objects we are interested in
		List<string> allPrefabPaths = new List<string>();
		Stack<string> paths = new Stack<string>();
		paths.Push(Application.dataPath);
		while (paths.Count != 0)
		{
			string path = paths.Pop();
			string[] files = Directory.GetFiles(path, "*.prefab");
			foreach (var file in files)
			{
				allPrefabPaths.Add(file.Substring(Application.dataPath.Length - 6));
			}
			
			foreach (string subdirs in Directory.GetDirectories(path)) 
				paths.Push(subdirs);
		}	
		
		// Check all prefabs
		int currPrefabCount = 1;
		foreach (string prefabPath in allPrefabPaths)
		{
			EditorUtility.DisplayProgressBar("Rebuilding Index", "Scanning project folder...", (float)currPrefabCount / (float)(allPrefabPaths.Count));
			
			GameObject iterGo = AssetDatabase.LoadAssetAtPath( prefabPath, typeof(GameObject) ) as GameObject;
			if (!iterGo) continue;

			tk2dSpriteCollection spriteCollection = iterGo.GetComponent<tk2dSpriteCollection>();
			tk2dSpriteCollectionData spriteCollectionData = iterGo.GetComponent<tk2dSpriteCollectionData>();
			tk2dFont font = iterGo.GetComponent<tk2dFont>();
			tk2dSpriteAnimation anim = iterGo.GetComponent<tk2dSpriteAnimation>();
			
			if (spriteCollection) 
			{
				tk2dSpriteCollectionData thisSpriteCollectionData = spriteCollection.spriteCollection;
				if (thisSpriteCollectionData)
				{
					if (thisSpriteCollectionData.version < 1)
					{
						rebuildSpriteCollectionPaths.Add( AssetDatabase.GetAssetPath(spriteCollection ));
					}
					newIndex.AddSpriteCollectionData( thisSpriteCollectionData );
				}
			}
			else if (spriteCollectionData)
			{
				string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(spriteCollectionData));
				bool present = false;
				foreach (var v in newIndex.GetSpriteCollectionIndex())
				{
					if (v.spriteCollectionDataGUID == guid)
					{
						present = true;
						break;
					}
				}
				if (!present && guid != "")
					newIndex.AddSpriteCollectionData(spriteCollectionData);
			}
			else if (font) 
			{
				newIndex.AddOrUpdateFont(font); // unfortunate but necessary
			}
			else if (anim) 
			{
				newIndex.AddSpriteAnimation(anim);
			}
			else
			{
				iterGo = null;
				System.GC.Collect();
			}

			tk2dEditorUtility.UnloadUnusedAssets();	
			++currPrefabCount;
		}
		EditorUtility.ClearProgressBar();
		
		// Create index
		AssetDatabase.CreateAsset(newIndex, indexPath);
		
		// unload all unused assets
		tk2dEditorUtility.UnloadUnusedAssets();
		
		// Rebuild invalid sprite collections
		if (rebuildSpriteCollectionPaths.Count > 0)
		{
			EditorUtility.DisplayDialog("Upgrade required",
			                            "Please wait while your sprite collection is upgraded.",
			                            "Ok");
			
			int count = 1;
			foreach (var scPath in rebuildSpriteCollectionPaths)
			{
				tk2dSpriteCollection sc = AssetDatabase.LoadAssetAtPath(scPath, typeof(tk2dSpriteCollection)) as tk2dSpriteCollection;
				EditorUtility.DisplayProgressBar("Rebuilding Sprite Collections", "Rebuilding Sprite Collection: " + sc.name, (float)count / (float)(rebuildSpriteCollectionPaths.Count));

				tk2dSpriteCollectionBuilder.Rebuild(sc);
				sc = null;
				
				tk2dEditorUtility.UnloadUnusedAssets();
				
				++count;
			}
			
			EditorUtility.ClearProgressBar();
		}
	
		index = newIndex;
		tk2dSpriteGuiUtility.ResetCache();
	}
	
	[System.ObsoleteAttribute]
	static T[] FindPrefabsInProjectWithComponent<T>() where T : Component
	// returns null if nothing is found
	{
		List<T> allGens = new List<T>();
		
		Stack<string> paths = new Stack<string>();
		paths.Push(Application.dataPath);
		while (paths.Count != 0)
		{
			string path = paths.Pop();
			string[] files = Directory.GetFiles(path, "*.prefab");
			foreach (var file in files)
			{
				GameObject go = AssetDatabase.LoadAssetAtPath( file.Substring(Application.dataPath.Length - 6), typeof(GameObject) ) as GameObject;
				if (!go) continue;
				
				T gen = go.GetComponent<T>();
				if (gen)
				{
					allGens.Add(gen);
				}
			}
			
			foreach (string subdirs in Directory.GetDirectories(path)) 
				paths.Push(subdirs);
		}
		
		if (allGens.Count == 0) return null;
		
		T[] allGensArray = new T[allGens.Count];
		for (int i = 0; i < allGens.Count; ++i)
			allGensArray[i] = allGens[i];
		return allGensArray;
	}
	
	public static GameObject CreateGameObjectInScene(string name)
	{
		string realName = name;
		int counter = 0;
		while (GameObject.Find(realName) != null)
		{
			realName = name + counter++;
		}
		
        GameObject go = new GameObject(realName);
		if (Selection.activeGameObject != null)
		{
			string assetPath = AssetDatabase.GetAssetPath(Selection.activeGameObject);
			if (assetPath.Length == 0) go.transform.parent = Selection.activeGameObject.transform;
		}
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;	
        return go;
	}
	
	public static void DrawMeshBounds(Mesh mesh, Transform transform, Color c)
	{
		var e = mesh.bounds.extents;
		Vector3[] boundPoints = new Vector3[] {
			mesh.bounds.center + new Vector3(-e.x, e.y, 0.0f),
			mesh.bounds.center + new Vector3( e.x, e.y, 0.0f),
			mesh.bounds.center + new Vector3( e.x,-e.y, 0.0f),
			mesh.bounds.center + new Vector3(-e.x,-e.y, 0.0f),
			mesh.bounds.center + new Vector3(-e.x, e.y, 0.0f) };
		
		for (int i = 0; i < boundPoints.Length; ++i)
			boundPoints[i] = transform.TransformPoint(boundPoints[i]);
		
		Handles.color = c;
		Handles.DrawPolyLine(boundPoints);
	}
	
	public static void UnloadUnusedAssets()
	{
		Object[] previousSelectedObjects = Selection.objects;
		Selection.objects = new Object[0];
		
		EditorUtility.UnloadUnusedAssets();
		System.GC.Collect();
		
		index = null;
		
		Selection.objects = previousSelectedObjects;
	}	

	public static void CollectAndUnloadUnusedAssets()
	{
		System.GC.Collect();
		System.GC.WaitForPendingFinalizers();
		EditorUtility.UnloadUnusedAssets();
	}

	public static bool IsPrefab(Object obj)
	{
#if (UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4)
		return AssetDatabase.GetAssetPath(obj).Length != 0;
#else
		return (PrefabUtility.GetPrefabType(obj) == PrefabType.Prefab);
#endif
	}

	public static void SetGameObjectActive(GameObject go, bool active)
	{
#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_3_8 || UNITY_3_9
		go.SetActiveRecursively(active);
#else
		go.SetActive(active);
#endif		
	}

	public static bool IsGameObjectActive(GameObject go)
	{
#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_3_8 || UNITY_3_9
		return go.active;
#else
		return go.activeSelf;
#endif		
	}
}
