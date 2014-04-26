using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class tk2dSpriteGuiUtility 
{
    public static int NameCompare(string na, string nb)
    {
		if (na.Length == 0 && nb.Length != 0) return 1;
		else if (na.Length != 0 && nb.Length == 0) return -1;
		else if (na.Length == 0 && nb.Length == 0) return 0;

        int numStartA = na.Length - 1;

        // last char is not a number, compare as regular strings
        if (na[numStartA] < '0' || na[numStartA] > '9')
            return System.String.Compare(na, nb, true);

        while (numStartA > 0 && na[numStartA - 1] >= '0' && na[numStartA - 1] <= '9')
            numStartA--;

        int comp = System.String.Compare(na, 0, nb, 0, numStartA);

        if (comp == 0)
        {
            if (nb.Length > numStartA)
            {
                bool numeric = true;
                for (int i = numStartA; i < nb.Length; ++i)
                {
                    if (nb[i] < '0' || nb[i] > '9')
                    {
                        numeric = false;
                        break;
                    }
                }

                if (numeric)
                {
                    int numA = System.Convert.ToInt32(na.Substring(numStartA));
                    int numB = System.Convert.ToInt32(nb.Substring(numStartA));
                    return numA - numB;
                }
            }
        }

        return System.String.Compare(na, nb);
    }
	
	class SpriteCollectionLUT
	{
		public int buildKey;
		public string[] sortedSpriteNames;
		public int[] spriteIdToSortedList;
		public int[] sortedListToSpriteId;
	}
	static Dictionary<string, SpriteCollectionLUT> spriteSelectorLUT = new Dictionary<string, SpriteCollectionLUT>();
	
	public static int SpriteSelectorPopup(string label, int spriteId, tk2dSpriteCollectionData spriteCollection)
	{
		spriteCollection = spriteCollection.inst;
		int newSpriteId = spriteId;
		
		// cope with guid not existing
		if (spriteCollection.dataGuid == null || spriteCollection.dataGuid.Length == 0)
		{
			spriteCollection.dataGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(spriteCollection));
		}
		
		SpriteCollectionLUT lut = null; 
		spriteSelectorLUT.TryGetValue(spriteCollection.dataGuid, out lut);
		if (lut == null)
		{
			lut = new SpriteCollectionLUT();
			lut.buildKey = spriteCollection.buildKey - 1; // force mismatch
			spriteSelectorLUT[spriteCollection.dataGuid] = lut;
		}
		
		if (lut.buildKey != spriteCollection.buildKey)
		{
			var spriteDefs = spriteCollection.spriteDefinitions;
			string[] spriteNames = new string[spriteDefs.Length];
			int[] spriteLookupIndices = new int[spriteNames.Length];
			for (int i = 0; i < spriteDefs.Length; ++i)
			{
				if (tk2dPreferences.inst.showIds && spriteDefs[i].name != null && spriteDefs[i].name.Length > 0)
					spriteNames[i] = spriteDefs[i].name + "\t[" + i.ToString() + "]";
				else
					spriteNames[i] = spriteDefs[i].name;
				spriteLookupIndices[i] = i;
			}
			System.Array.Sort(spriteLookupIndices, (int a, int b) => tk2dSpriteGuiUtility.NameCompare((spriteDefs[a]!=null)?spriteDefs[a].name:"", (spriteDefs[b]!=null)?spriteDefs[b].name:""));
			
			lut.sortedSpriteNames = new string[spriteNames.Length];
			lut.sortedListToSpriteId = new int[spriteNames.Length];
			lut.spriteIdToSortedList = new int[spriteNames.Length];
			
			for (int i = 0; i < spriteLookupIndices.Length; ++i)
			{
				lut.spriteIdToSortedList[spriteLookupIndices[i]] = i;
				lut.sortedListToSpriteId[i] = spriteLookupIndices[i];
				lut.sortedSpriteNames[i] = spriteNames[spriteLookupIndices[i]];
			}
			
			lut.buildKey = spriteCollection.buildKey;
		}
		
		int spriteLocalIndex = lut.spriteIdToSortedList[spriteId];
		int newSpriteLocalIndex = (label == null)?EditorGUILayout.Popup(spriteLocalIndex, lut.sortedSpriteNames):EditorGUILayout.Popup(label, spriteLocalIndex, lut.sortedSpriteNames);
		if (newSpriteLocalIndex != spriteLocalIndex)
		{
			newSpriteId = lut.sortedListToSpriteId[newSpriteLocalIndex];
		}
		
		return newSpriteId;
	}
	
	static List<tk2dSpriteCollectionIndex> allSpriteCollections = new List<tk2dSpriteCollectionIndex>();
	static Dictionary<string, int> allSpriteCollectionLookup = new Dictionary<string, int>();
	static string[] spriteCollectionNames = new string[0];
	static string[] spriteCollectionNamesInclTransient = new string[0];

	public static tk2dSpriteCollectionData GetDefaultSpriteCollection()
	{
		BuildLookupIndex(false);
		
		foreach (tk2dSpriteCollectionIndex indexEntry in allSpriteCollections)
		{
			if (!indexEntry.managedSpriteCollection && indexEntry.spriteNames != null)
			{
				foreach (string name in indexEntry.spriteNames)
				{
					if (name != null && name.Length > 0)
					{
						GameObject scgo = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(indexEntry.spriteCollectionDataGUID), typeof(GameObject)) as GameObject;
						return scgo.GetComponent<tk2dSpriteCollectionData>();
					}
				}
			}
		}
		
		Debug.LogError("Unable to find any sprite collections.");
		return null;
	}
		
	static void BuildLookupIndex(bool force)
	{
		if (force)
			tk2dEditorUtility.ForceCreateIndex();
		
		allSpriteCollections = new List<tk2dSpriteCollectionIndex>();
		tk2dSpriteCollectionIndex[] mainIndex = tk2dEditorUtility.GetOrCreateIndex().GetSpriteCollectionIndex();
		foreach (tk2dSpriteCollectionIndex i in mainIndex)
		{
			if (!i.managedSpriteCollection)
				allSpriteCollections.Add(i);
		}

		allSpriteCollections.Sort( (a, b) => tk2dSpriteGuiUtility.NameCompare(a.name, b.name) );
		allSpriteCollectionLookup = new Dictionary<string, int>();
		
		spriteCollectionNames = new string[allSpriteCollections.Count];
		spriteCollectionNamesInclTransient = new string[allSpriteCollections.Count + 1];
		for (int i = 0; i < allSpriteCollections.Count; ++i)
		{
			allSpriteCollectionLookup[allSpriteCollections[i].spriteCollectionDataGUID] = i;
			spriteCollectionNames[i] = allSpriteCollections[i].name;
			spriteCollectionNamesInclTransient[i] = allSpriteCollections[i].name;
		}
		spriteCollectionNamesInclTransient[allSpriteCollections.Count] = "-"; // transient sprite collection
	}

	public static void ResetCache()
	{
		allSpriteCollections.Clear();
	}
	
	static tk2dSpriteCollectionData GetSpriteCollectionDataAtIndex(int index, tk2dSpriteCollectionData defaultValue)
	{
		if (index >= allSpriteCollections.Count) return defaultValue;
		GameObject go = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(allSpriteCollections[index].spriteCollectionDataGUID), typeof(GameObject)) as GameObject;
		if (go == null) return defaultValue;
		tk2dSpriteCollectionData data = go.GetComponent<tk2dSpriteCollectionData>();
		if (data == null) return defaultValue;
		return data;
	}
	
	public static string TransientGUID { get { return "transient"; } }
	
	public static tk2dSpriteCollectionData SpriteCollectionPopup(tk2dSpriteCollectionData currentValue)
	{
		return SpriteCollectionPopup("", currentValue, false, -1);
	}
	
	public static int GetValidSpriteId(tk2dSpriteCollectionData spriteCollection, int spriteId)
	{
		if (! (spriteId > 0 && spriteId < spriteCollection.spriteDefinitions.Length && 
			spriteCollection.spriteDefinitions[spriteId].Valid) )
		{
			spriteId = spriteCollection.FirstValidDefinitionIndex;
			if (spriteId == -1) spriteId = 0;
		}
		return spriteId;
	}
	
	public static tk2dSpriteCollectionData SpriteCollectionPopup(string label, tk2dSpriteCollectionData currentValue, bool allowEdit, int spriteId)
	{
		// Initialize guid if not present
		if (currentValue != null && (currentValue.dataGuid == null || currentValue.dataGuid.Length == 0))
		{
			string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(currentValue));
			currentValue.dataGuid = (guid.Length == 0)?TransientGUID:guid;
		}
		
		if (allSpriteCollections == null || allSpriteCollections.Count == 0)
			BuildLookupIndex(false);
		
		GUILayout.BeginHorizontal();
		if (label.Length > 0)
			EditorGUILayout.PrefixLabel(label);
		
		if (currentValue == null || currentValue.dataGuid == TransientGUID)
		{
			int currentSelection = allSpriteCollections.Count;
			int newSelection = EditorGUILayout.Popup(currentSelection, spriteCollectionNamesInclTransient);
			if (newSelection != currentSelection)
			{
				currentValue = GetSpriteCollectionDataAtIndex(newSelection, currentValue);
				GUI.changed = true;
			}
		}
		else
		{
			int currentSelection = -1;
			for (int iter = 0; iter < 2; ++iter) // 2 passes in worst case
			{
				for (int i = 0; i < allSpriteCollections.Count; ++i)
				{
					if (allSpriteCollections[i].spriteCollectionDataGUID == currentValue.dataGuid)
					{
						currentSelection = i;
						break;
					}
				}
				
				if (currentSelection != -1) break; // found something on first pass

				// we are missing a sprite collection, rebuild index
				BuildLookupIndex(true);
			}
			
			if (currentSelection == -1)
			{
				Debug.LogError("Unable to find sprite collection. This is a serious problem.");
				GUILayout.Label(currentValue.spriteCollectionName, EditorStyles.popup);
			}
			else
			{
				int newSelection = EditorGUILayout.Popup(currentSelection, spriteCollectionNames);
				if (newSelection != currentSelection)
				{
					currentValue = GetSpriteCollectionDataAtIndex(newSelection, currentValue);
					GUI.changed = true;					
				}
			}
			
			if ( allowEdit && GUILayout.Button( "Edit...", GUILayout.MaxWidth( 40f ), GUILayout.MaxHeight( 14f ) ) ) 
			{
				tk2dSpriteCollection gen = AssetDatabase.LoadAssetAtPath( AssetDatabase.GUIDToAssetPath(currentValue.spriteCollectionGUID), typeof(tk2dSpriteCollection) ) as tk2dSpriteCollection;
				if ( gen != null ) 
				{
					tk2dSpriteCollectionEditorPopup v = EditorWindow.GetWindow( typeof(tk2dSpriteCollectionEditorPopup), false, "Sprite Collection Editor" ) as tk2dSpriteCollectionEditorPopup;
					v.SetGeneratorAndSelectedSprite(gen, spriteId);
				}
			}  
		}	
		GUILayout.EndHorizontal();
		
		return currentValue;
	}
}
