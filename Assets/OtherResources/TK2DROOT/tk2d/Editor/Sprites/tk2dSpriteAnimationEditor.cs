using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(tk2dSpriteAnimation))]
class tk2dSpriteAnimationEditor : Editor
{
	int currentClip = 0;
	Vector2 scrollPosition = Vector3.zero;
	
	bool initialized = false;
	string[] allSpriteCollectionNames = null;
	tk2dSpriteCollectionIndex[] spriteCollectionIndex = null;
	
	void InitializeInspector()
	{
		if (!initialized)
		{
			var index = tk2dEditorUtility.GetOrCreateIndex().GetSpriteCollectionIndex();
			if (index != null)
			{
				allSpriteCollectionNames = new string[index.Length];
				
				for (int i = 0; i < index.Length; ++i)
				{
					allSpriteCollectionNames[i] = index[i].name;
				}
			}
			spriteCollectionIndex = index;
			
			initialized = true;
		}
	}
	
	void OnDestroy()
	{
		tk2dSpriteThumbnailCache.ReleaseSpriteThumbnailCache();
	}
	
	Dictionary<tk2dSpriteCollectionData, int> indexLookup = new Dictionary<tk2dSpriteCollectionData, int>();
	int GetSpriteCollectionId(tk2dSpriteCollectionData data)
	{
		if (indexLookup.ContainsKey(data))
			return indexLookup[data];
		
		var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(data));	
		for (int i = 0; i < spriteCollectionIndex.Length; ++i)
		{
			if (spriteCollectionIndex[i].spriteCollectionDataGUID == guid)
			{
				indexLookup[data] = i;
				return i;
			}
		}
		return 0; // default
	}
	
    public override void OnInspectorGUI()
    {
		InitializeInspector();
		
		if (spriteCollectionIndex == null || allSpriteCollectionNames == null)
		{
			GUILayout.Label("data not found");
			if (GUILayout.Button("Refresh"))
			{
				initialized = false;
				InitializeInspector();
			}
			return;
		}
		
        tk2dSpriteAnimation anim = (tk2dSpriteAnimation)target;

		EditorGUI.indentLevel++;
		EditorGUILayout.BeginVertical();
		
		if (anim.clips.Length == 0)
		{
			if (GUILayout.Button("Add clip"))
			{
				anim.clips = new tk2dSpriteAnimationClip[1];
				anim.clips[0] = new tk2dSpriteAnimationClip();
				anim.clips[0].name = "New Clip 0";
				anim.clips[0].frames = new tk2dSpriteAnimationFrame[1];
				
				anim.clips[0].frames[0] = new tk2dSpriteAnimationFrame();
				var spriteCollection = tk2dSpriteGuiUtility.GetDefaultSpriteCollection();
				anim.clips[0].frames[0].spriteCollection = spriteCollection;
				anim.clips[0].frames[0].spriteId = spriteCollection.FirstValidDefinitionIndex;
			}
		}
		else // has anim clips
		{
			// All clips
			string[] allClipNames = new string[anim.clips.Length];
			for (int i = 0; i < anim.clips.Length; ++i)
				allClipNames[i] = anim.clips[i].name;
			currentClip = Mathf.Clamp(currentClip, 0, anim.clips.Length);
			
			#region AddAndDeleteClipButtons
			EditorGUILayout.BeginHorizontal();
			currentClip = EditorGUILayout.Popup("Clips", currentClip, allClipNames);
			
			// Add new clip
			if (GUILayout.Button("+", GUILayout.MaxWidth(28), GUILayout.MaxHeight(14)))
			{
				int previousClipId = currentClip;
				
				// try to find an empty slot
				currentClip = -1;
				for (int i = 0; i < anim.clips.Length; ++i)
				{
					if (anim.clips[i].name.Length == 0)
					{
						currentClip = i;
						break;
					}
				}
				
				if (currentClip == -1)
				{
					tk2dSpriteAnimationClip[] clips = new tk2dSpriteAnimationClip[anim.clips.Length + 1];
					for (int i = 0; i < anim.clips.Length; ++i)
						clips[i] = anim.clips[i];
					currentClip = anim.clips.Length;
					clips[currentClip] = new tk2dSpriteAnimationClip();
					anim.clips = clips;
				}
				
				string uniqueName = "New Clip ";
				int uniqueId = 0;
				for (int i = 0; i < anim.clips.Length; ++i)
				{
					string uname = uniqueName + uniqueId.ToString();
					if (anim.clips[i].name == uname)
					{
						uniqueId++;
						i = -1;
						continue;
					}
				}
				
				anim.clips[currentClip] = new tk2dSpriteAnimationClip();
				anim.clips[currentClip].name = uniqueName + uniqueId.ToString();
				anim.clips[currentClip].fps = 15;
				anim.clips[currentClip].wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop;
				anim.clips[currentClip].frames = new tk2dSpriteAnimationFrame[1];
				tk2dSpriteAnimationFrame frame = new tk2dSpriteAnimationFrame();
				if (previousClipId < anim.clips.Length
				    && anim.clips[previousClipId] != null 
				    && anim.clips[previousClipId].frames != null
				    && anim.clips[previousClipId].frames.Length != 0
				    && anim.clips[previousClipId].frames[anim.clips[previousClipId].frames.Length - 1] != null
				    && anim.clips[previousClipId].frames[anim.clips[previousClipId].frames.Length - 1].spriteCollection != null)
				{
					var previousClip = anim.clips[previousClipId];
					var lastFrame = previousClip.frames[previousClip.frames.Length - 1];
					frame.spriteCollection = lastFrame.spriteCollection;
					frame.spriteId = lastFrame.spriteId;
				}
				else
				{
					var spriteCollection = tk2dSpriteGuiUtility.GetDefaultSpriteCollection();
					frame.spriteCollection = spriteCollection;
					frame.spriteId = spriteCollection.FirstValidDefinitionIndex;
				}
				anim.clips[currentClip].frames[0] = frame;
				
				GUI.changed = true;
			}
			
			// Delete clip
			if (GUILayout.Button("-", GUILayout.MaxWidth(28), GUILayout.MaxHeight(14)))
			{
				anim.clips[currentClip].name = "";
				anim.clips[currentClip].frames = new tk2dSpriteAnimationFrame[0];
				
				currentClip = 0;
				// find first non zero clip
				for (int i = 0; i < anim.clips.Length; ++i)
				{
					if (anim.clips[i].name != "")
					{
						currentClip = i;
						break;
					}
				}
				
				GUI.changed = true;
			}
			EditorGUILayout.EndHorizontal();
			#endregion
			
			#region PruneClipList
			// Prune clip list
			int lastActiveClip = 0;
			for (int i = 0; i < anim.clips.Length; ++i)
			{
				if ( !(anim.clips[i].name == "" && anim.clips[i].frames != null && anim.clips[i].frames.Length == 0) ) lastActiveClip = i;
			}
			if (lastActiveClip != anim.clips.Length - 1)
			{
				System.Array.Resize<tk2dSpriteAnimationClip>(ref anim.clips, lastActiveClip + 1);
				GUI.changed = true;
			}
			#endregion
			
			// If anything has changed up to now, redraw
			if (GUI.changed)
			{
				EditorUtility.SetDirty(anim);
				Repaint();
				return;
			}
			
			EditorGUI.indentLevel = 2;
			tk2dSpriteAnimationClip clip = anim.clips[currentClip];

			// Clip properties
			
			// Name
			clip.name = EditorGUILayout.TextField("Name", clip.name);
			
			#region NumberOfFrames
			// Number of frames
			int clipNumFrames = (clip.frames != null)?clip.frames.Length:0;
			int newFrameCount = 0;
			if (clip.wrapMode == tk2dSpriteAnimationClip.WrapMode.Single)
			{
				newFrameCount = 1; // only one frame, no need to display
			}
			else
			{
				int maxFrameCount = 400;
				int startSection = Mathf.Max(clipNumFrames - 5, 1);
				int endSection = Mathf.Min(clipNumFrames + 5, maxFrameCount);
				
				string[] numFrameStr = new string[maxFrameCount - 1];
				int div = 20;
				
				int divStart = 0;
				int divEnd = 0;
				string section = "";
				for (int i = 1; i < startSection; ++i)
				{
					if (i > divEnd)
					{
						divStart = divEnd + 1;
						divEnd = Mathf.Min(startSection - 1, divStart + div - 1);
						section = divStart.ToString() + " .. " + divEnd.ToString();
					}
					numFrameStr[i-1] = section + "/" + i.ToString();
				}
				
				for (int i = startSection; i < endSection; ++i)
					numFrameStr[i-1] = i.ToString();
				
				divEnd = endSection - 1;
				for (int i = endSection; i < maxFrameCount; ++i)
				{
					if (i > divEnd)
					{
						divStart = Mathf.Max(divEnd + 1, endSection);
						divEnd = Mathf.Min(((divStart + div) / div) * div, maxFrameCount - 1); 
						section = divStart.ToString() + " .. " + divEnd.ToString();
					}
					numFrameStr[i-1] = section + "/" + i.ToString();
				}
				
				newFrameCount = EditorGUILayout.Popup("Num Frames", clipNumFrames - 1, numFrameStr) + 1;
				if (newFrameCount == 0) newFrameCount = 1; // minimum = 1
			}
			
			if (newFrameCount != clipNumFrames)
			{
				// Ungroup
				if (newFrameCount > clipNumFrames)
				{
					tk2dPreferences.inst.groupAnimDisplay = false;
					scrollPosition.y += 1000000.0f; // push to the end
				}
				
				tk2dSpriteAnimationFrame[] frames = new tk2dSpriteAnimationFrame[newFrameCount];
				
				int c1 = Mathf.Min(clipNumFrames, frames.Length);
				for (int i = 0; i < c1; ++i)
				{
					frames[i] = new tk2dSpriteAnimationFrame();
					frames[i].CopyFrom(clip.frames[i]);
				}
				if (c1 > 0)
				{
					for (int i = c1; i < frames.Length; ++i)
					{
						frames[i] = new tk2dSpriteAnimationFrame();
						frames[i].CopyFrom(clip.frames[c1-1]);
					}
				}
				else
				{
					for (int i = 0; i < frames.Length; ++i)
					{
						frames[i] = new tk2dSpriteAnimationFrame();
						var spriteCollection = tk2dSpriteGuiUtility.GetDefaultSpriteCollection();
						frames[i].spriteCollection = spriteCollection;
						frames[i].spriteId = spriteCollection.FirstValidDefinitionIndex;
					}
				}
				
				clip.frames = frames;
				clipNumFrames = newFrameCount;
			}
			#endregion
			
			// Frame rate
			if (clip.wrapMode != tk2dSpriteAnimationClip.WrapMode.Single)
				clip.fps = EditorGUILayout.FloatField("Frame rate", clip.fps);
			
			// Wrap mode
			clip.wrapMode = (tk2dSpriteAnimationClip.WrapMode)EditorGUILayout.EnumPopup("Wrap mode", clip.wrapMode);
			if (clip.wrapMode == tk2dSpriteAnimationClip.WrapMode.LoopSection)
			{
				clip.loopStart = EditorGUILayout.IntField("Loop start", clip.loopStart);
				clip.loopStart = Mathf.Clamp(clip.loopStart, 0, clip.frames.Length - 1);
			}
			
			#region DrawFrames
			EditorGUI.indentLevel = 0;
			
			GUILayout.Space(8);
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			GUILayout.Label("Frames");
			GUILayout.FlexibleSpace();
			
			// Reverse
			if (clip.wrapMode != tk2dSpriteAnimationClip.WrapMode.Single &&
			    GUILayout.Button("Reverse", EditorStyles.toolbarButton))
			{
				System.Array.Reverse(clip.frames);
				GUI.changed = true;
			}
			
			// Auto fill
			if (clip.wrapMode != tk2dSpriteAnimationClip.WrapMode.Single && clip.frames.Length >= 1)
			{
				AutoFill(clip);
			}
			
			if (GUILayout.Button(tk2dPreferences.inst.horizontalAnimDisplay?"H":"V", EditorStyles.toolbarButton, GUILayout.MaxWidth(24)))
			{
				tk2dPreferences.inst.horizontalAnimDisplay = !tk2dPreferences.inst.horizontalAnimDisplay;
				Repaint();
			}
			
			tk2dPreferences.inst.groupAnimDisplay = GUILayout.Toggle(tk2dPreferences.inst.groupAnimDisplay, "Group", EditorStyles.toolbarButton);
			
			EditorGUILayout.EndHorizontal();

			// Sanitize frame data
			for (int i = 0; i < clip.frames.Length; ++i)
			{
				if (clip.frames[i].spriteCollection == null || clip.frames[i].spriteCollection == null || clip.frames[i].spriteCollection.inst.spriteDefinitions.Length == 0)
				{
					EditorUtility.DisplayDialog("Warning", "Invalid sprite collection found.\nThis clip will now be deleted", "Ok");

					clip.name = "";
					clip.frames = new tk2dSpriteAnimationFrame[0];
					Repaint();
					return;
				}
				
				if (clip.frames[i].spriteId < 0 || clip.frames[i].spriteId >= clip.frames[i].spriteCollection.inst.Count)
				{
					EditorUtility.DisplayDialog("Warning", "Invalid frame found, resetting to frame 0", "Ok");
					clip.frames[i].spriteId = 0;
				}
			}
			
			// Warning when one of the frames has different poly count
			if (clipNumFrames > 0)
			{
				bool differentPolyCount = false;
				int polyCount = clip.frames[0].spriteCollection.inst.spriteDefinitions[clip.frames[0].spriteId].positions.Length;
				for (int i = 1; i < clipNumFrames; ++i)
				{
					int thisPolyCount = clip.frames[i].spriteCollection.inst.spriteDefinitions[clip.frames[i].spriteId].positions.Length;
					if (thisPolyCount != polyCount)
					{
						differentPolyCount = true;
						break;
					}
				}
				
				if (differentPolyCount)
				{
					Color bg = GUI.backgroundColor;
					GUI.backgroundColor = Color.red;
					GUILayout.TextArea("Sprites have different poly counts. Performance will be affected");
					GUI.backgroundColor = bg;
				}
			}
			
			// Draw frames
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space();
			
			DrawClipEditor(clip);
		
			EditorGUILayout.EndHorizontal();
			#endregion
		}				
		
		EditorGUILayout.EndVertical();
		EditorGUI.indentLevel--;
		
		if (GUI.changed)
		{
			EditorUtility.SetDirty(anim);
		}

		GUILayout.Space(64);
	}
	
	delegate void EditorCommitDelegate(tk2dSpriteAnimationFrame dest, tk2dSpriteAnimationFrame src);
	void PropogateFrameChange(tk2dSpriteAnimationClip clip, int frameId, int frameCount, EditorCommitDelegate commitDelegate)
	{
		var frame = clip.frames[frameId];
		for (int j = frameId + 1; j < frameId + frameCount; ++j)
			commitDelegate(clip.frames[j], frame);
	}
	
	delegate void DeferredFrameOperationDelegate(tk2dSpriteAnimationClip clip);
	DeferredFrameOperationDelegate deferredFrameOp = null;
	
	float cachedDurationFps = -1.0f;
	string[] durationTable = null;
	string[] GetDurationTableForClip(tk2dSpriteAnimationClip clip)
	{
		int numGroupedAnimationFrames = tk2dPreferences.inst.numGroupedAnimationFrames;
		if (durationTable == null || durationTable.Length == 0 || cachedDurationFps != clip.fps ||
			durationTable.Length != numGroupedAnimationFrames + 1)
		{
			durationTable = new string[numGroupedAnimationFrames + 1];
			for (int i = 0; i <= numGroupedAnimationFrames; ++i)
			{
				switch (i)
				{
				case 0: durationTable[i] = "Delete"; break;
				case 1: durationTable[i] = string.Format("1 frame, {0:0.00} sec", i / clip.fps); break;
				default: durationTable[i] = string.Format("{0} frames, {1:0.00} sec", i, i / clip.fps); break;
				}
			}
			cachedDurationFps = clip.fps;
		}
		return durationTable;
	}
	
	void AddFrame(tk2dSpriteAnimationClip clip)
	{
		System.Array.Resize(ref clip.frames, clip.frames.Length + 1);
		var newFrame = new tk2dSpriteAnimationFrame();
		newFrame.CopyFrom(clip.frames[clip.frames.Length - 2]); // previous "last" entry
		if (tk2dPreferences.inst.groupAnimDisplay)
		{
			// make sure the spriteId is something different, so it ends up adding a new entry
			var defs = newFrame.spriteCollection.inst.spriteDefinitions;
			for (int j = 0; j < defs.Length; ++j)
			{
				int i = (j + newFrame.spriteId + 1) % defs.Length; // start one after current frame, and work from there looping back
				if (i != newFrame.spriteId && defs[i].Valid)
				{
					newFrame.spriteId = i;
					break;
				}
			}
		}
		clip.frames[clip.frames.Length - 1] = newFrame;
		GUI.changed = true;
	}
	
	void DrawAddFrame(tk2dSpriteAnimationClip clip, bool vertical)
	{
		GUILayout.Space(32);
		if (vertical) GUILayout.BeginHorizontal();
		else GUILayout.BeginVertical();

		GUILayout.FlexibleSpace();
		if (clip.wrapMode != tk2dSpriteAnimationClip.WrapMode.Single && GUILayout.Button("Add frame", GUILayout.ExpandWidth(false)))
		{
			AddFrame(clip);
			scrollPosition.y += 200.0f; // allways bring the new frame into view
		}
		GUILayout.FlexibleSpace();
		
		if (vertical) GUILayout.EndHorizontal();
		else GUILayout.EndVertical();
		GUILayout.Space(32);
	}
	
	void DrawFrameEditor(tk2dSpriteAnimationClip clip, int frameId, int frameCount)
	{
		var frame = clip.frames[frameId];
		
		tk2dGuiUtility.BeginChangeCheck();
		frame.spriteCollection = tk2dSpriteGuiUtility.SpriteCollectionPopup(frame.spriteCollection);
		if (tk2dGuiUtility.EndChangeCheck())
		{
			frame.spriteId = tk2dSpriteGuiUtility.GetValidSpriteId(frame.spriteCollection.inst, frame.spriteId);
			PropogateFrameChange(clip, frameId, frameCount, 
			(dest, src) => { dest.spriteCollection = src.spriteCollection; dest.spriteId = src.spriteId; } );
		}
		
		tk2dGuiUtility.BeginChangeCheck();
		frame.spriteId = tk2dSpriteGuiUtility.SpriteSelectorPopup(null, frame.spriteId, frame.spriteCollection);
		if (tk2dGuiUtility.EndChangeCheck()) PropogateFrameChange(clip, frameId, frameCount, (dest, src) => dest.spriteId = src.spriteId );
		
		if (tk2dPreferences.inst.groupAnimDisplay)
		{
			int newFrameCount = EditorGUILayout.Popup(frameCount, GetDurationTableForClip(clip));
			if (newFrameCount != frameCount)
			{
				if (newFrameCount == 0)
				{
					deferredFrameOp = delegate(tk2dSpriteAnimationClip target)
					{
						if (frameCount == target.frames.Length) frameCount--; // don't delete last sprite
						if (frameCount > 0)
						{
							List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>(target.frames);
							frames.RemoveRange(frameId, frameCount);
							target.frames = frames.ToArray();
						}
					};
				}
				else if (newFrameCount < frameCount)
				{
					deferredFrameOp = delegate(tk2dSpriteAnimationClip target)
					{
						int toRemove = frameCount - newFrameCount;
						List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>(target.frames);
						frames.RemoveRange(frameId + frameCount - 1 - toRemove, toRemove);
						target.frames = frames.ToArray();
					};
				}
				else if (newFrameCount > frameCount)
				{
					deferredFrameOp = delegate(tk2dSpriteAnimationClip target)
					{
						int toAdd = newFrameCount - frameCount;
						List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>(target.frames);
						var source = target.frames[frameId + frameCount - 1]; // last valid one
						var framesToInsert = new List<tk2dSpriteAnimationFrame>(toAdd);
						for (int j = 0; j < toAdd; ++j)
						{
							tk2dSpriteAnimationFrame f = new tk2dSpriteAnimationFrame();
							f.CopyFrom(source, false);
							framesToInsert.Add(f);
						}
						frames.InsertRange(frameId + frameCount, framesToInsert);
						target.frames = frames.ToArray();
					};
				}
			}
			GUILayout.Space(8);
		}

		tk2dGuiUtility.BeginChangeCheck();
		frame.triggerEvent = EditorGUILayout.Toggle("Trigger", frame.triggerEvent);
		if (tk2dGuiUtility.EndChangeCheck()) PropogateFrameChange(clip, frameId, frameCount, (dest, src) => dest.triggerEvent = src.triggerEvent );
		if (frame.triggerEvent)
		{
			EditorGUI.indentLevel++;
			
			tk2dGuiUtility.BeginChangeCheck();
			frame.eventInfo = EditorGUILayout.TextField("Info", frame.eventInfo);
			if (tk2dGuiUtility.EndChangeCheck()) PropogateFrameChange(clip, frameId, frameCount, (dest, src) => dest.eventInfo = src.eventInfo );
			
			tk2dGuiUtility.BeginChangeCheck();
			frame.eventFloat = EditorGUILayout.FloatField("Float", frame.eventFloat);
			if (tk2dGuiUtility.EndChangeCheck()) PropogateFrameChange(clip, frameId, frameCount, (dest, src) => dest.eventFloat = src.eventFloat );

			tk2dGuiUtility.BeginChangeCheck();
			frame.eventInt = EditorGUILayout.IntField("Int", frame.eventInt);
			if (tk2dGuiUtility.EndChangeCheck()) PropogateFrameChange(clip, frameId, frameCount, (dest, src) => dest.eventInt = src.eventInt );

			GUILayout.Space(8);
			EditorGUI.indentLevel--;
		}		
	}
	
	int GetFrameCount(tk2dSpriteAnimationClip clip, int frameId)
	{
		int frameCount = 1;
		int clipNumFrames = clip.frames.Length;
		var frame = clip.frames[frameId];
		if (tk2dPreferences.inst.groupAnimDisplay)
		{
			for (int j = frameId + 1; j < clipNumFrames && frameCount < tk2dPreferences.inst.numGroupedAnimationFrames; ++j)
			{
				if (clip.frames[j].spriteCollection == frame.spriteCollection &&
					clip.frames[j].spriteId == frame.spriteId)
					frameCount++;
				else
					break;
			}
		}
		return frameCount;
	}
	
	void DrawClipEditor(tk2dSpriteAnimationClip clip)
	{
		EditorGUIUtility.LookLikeControls(80.0f, 50.0f);
		
		var frameBorderStyle = EditorStyles.textField;
		
		int clipNumFrames = clip.frames.Length;
		if (tk2dPreferences.inst.horizontalAnimDisplay)
		{
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(144.0f));
			EditorGUILayout.BeginHorizontal();

			for (int i = 0; i < clipNumFrames; ++i)
			{
				int frameCount = GetFrameCount(clip, i);
				EditorGUILayout.BeginHorizontal(frameBorderStyle);
					
				EditorGUILayout.BeginVertical();
				GUILayout.Label(new GUIContent(i.ToString(), "Frame"));
//				GUILayout.Label(new GUIContent((i / clip.fps).ToString("0.00" + "s"), "Time"));
				EditorGUILayout.EndVertical();
				DrawSpritePreview(clip.frames[i].spriteCollection, clip.frames[i].spriteId);
				
				EditorGUILayout.BeginVertical();
				DrawFrameEditor(clip, i, frameCount);
				EditorGUILayout.EndVertical();
				
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Space();
				EditorGUILayout.Space(); 
				
				i += (frameCount - 1);
			}
			
			DrawAddFrame(clip, false);
		
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndScrollView();
		}
		else
		{
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
			EditorGUILayout.BeginVertical();
			
			for (int i = 0; i < clipNumFrames; ++i)
			{
				int frameCount = GetFrameCount(clip, i);
				EditorGUILayout.BeginHorizontal(frameBorderStyle);
				
				EditorGUILayout.BeginVertical();
				GUILayout.Label(new GUIContent(i.ToString(), "Frame"));
//				GUILayout.Label(new GUIContent((i / clip.fps).ToString("0.00" + "s"), "Time"));
				EditorGUILayout.EndVertical();
				
				EditorGUILayout.BeginVertical();
				DrawFrameEditor(clip, i, frameCount);
				EditorGUILayout.EndVertical();
				
				DrawSpritePreview(clip.frames[i].spriteCollection, clip.frames[i].spriteId);
				
				EditorGUILayout.EndHorizontal();
				
				i += (frameCount - 1);
			}				
			
			DrawAddFrame(clip, true);
			
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndScrollView();
		}
	
		if (deferredFrameOp != null)
		{
			deferredFrameOp(clip);
			deferredFrameOp = null;
			
			GUI.changed = true;
		}
	}
	
	// Finds a sprite with the name and id
	// matches "baseName" [ 0..9 ]* as id
	int FindFrameIndex(tk2dSpriteDefinition[] spriteDefs, string baseName, int frameId)
	{
		for (int j = 0; j < spriteDefs.Length; ++j)
		{
			if (System.String.Compare(baseName, 0, spriteDefs[j].name, 0, baseName.Length, true) == 0)
			{
				int thisFrameId = 0;
				if (System.Int32.TryParse( spriteDefs[j].name.Substring(baseName.Length), out thisFrameId ) && 
				    thisFrameId == frameId)
				{
					return j;
				}
			}
		}
		return -1;
	}
	
	void AutoFill(tk2dSpriteAnimationClip clip)
	{
		int lastFrameId = clip.frames.Length - 1;
		if (clip.frames[lastFrameId].spriteCollection != null && clip.frames[lastFrameId].spriteId >= 0 && clip.frames[lastFrameId].spriteId < clip.frames[lastFrameId].spriteCollection.inst.Count)
		{
			string na = clip.frames[lastFrameId].spriteCollection.inst.spriteDefinitions[clip.frames[lastFrameId].spriteId].name;
			
			int numStartA = na.Length - 1;
			if (na[numStartA] >= '0' && na[numStartA] <= '9')
			{
				if (GUILayout.Button("AutoFill", EditorStyles.toolbarButton))
				{
			        while (numStartA > 0 && na[numStartA - 1] >= '0' && na[numStartA - 1] <= '9')
			            numStartA--;
					
					string baseName = na.Substring(0, numStartA).ToLower();
					int baseNo = System.Convert.ToInt32(na.Substring(numStartA));
					
					int maxAllowedMissing = 10;
					int allowedMissing = maxAllowedMissing;
					List<int> pendingFrames = new List<int>();
					for (int frameNo = baseNo + 1; ; ++frameNo)
					{
						int frameIdx = FindFrameIndex(clip.frames[lastFrameId].spriteCollection.inst.spriteDefinitions, baseName, frameNo);
						if (frameIdx == -1)
						{
							if (--allowedMissing <= 0)
								break;
						}
						else
						{
							pendingFrames.Add(frameIdx);
							allowedMissing = maxAllowedMissing; // reset
						}
					}
					
					if (pendingFrames.Count > 0)
					{
						int startFrame = clip.frames.Length;
						var collection = clip.frames[lastFrameId].spriteCollection;
						
						System.Array.Resize<tk2dSpriteAnimationFrame>(ref clip.frames, clip.frames.Length + pendingFrames.Count);
						for (int j = 0; j < pendingFrames.Count; ++j)
						{	
							clip.frames[startFrame + j] = new tk2dSpriteAnimationFrame();
							clip.frames[startFrame + j].spriteCollection = collection;
							clip.frames[startFrame + j].spriteId = pendingFrames[j];
						}
						
						GUI.changed = true;
					}
				}
			}
		}
	}
	
	void DrawSpritePreview(tk2dSpriteCollectionData spriteCollection, int spriteId)
	{
		if (!tk2dPreferences.inst.displayTextureThumbs)
			return;
		
		if (spriteCollection.version < 1 || spriteCollection.dataGuid == tk2dSpriteGuiUtility.TransientGUID)
		{
			GUILayout.Label("No thumbnail data.");
		}
		else		
		{
			Rect r = GUILayoutUtility.GetRect(64, 64, GUILayout.MaxWidth(64), GUILayout.MaxHeight(64));
			Texture2D tex = tk2dSpriteThumbnailCache.GetThumbnailTexture(spriteCollection, spriteId);
			if (tex)
			{
				if (tex.width < r.width && tex.height < r.height)
				{
					r.width = tex.width;
					r.height = tex.height;
				}
				else if (tex.width > tex.height)
				{
					r.height = r.width / tex.width * tex.height;
				}
				else
				{
					r.width = r.height / tex.height * tex.width;
				}
				
				GUI.DrawTexture(r, tex);
			}
		}
	}
	
	
	[MenuItem("Assets/Create/tk2d/Sprite Animation", false, 10001)]
    static void DoAnimationCreate()
    {
		string path = tk2dEditorUtility.CreateNewPrefab("SpriteAnimation");
        if (path.Length != 0)
        {
            GameObject go = new GameObject();
            go.AddComponent<tk2dSpriteAnimation>();
	        tk2dEditorUtility.SetGameObjectActive(go, false);

#if (UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4)
			Object p = EditorUtility.CreateEmptyPrefab(path);
            EditorUtility.ReplacePrefab(go, p, ReplacePrefabOptions.ConnectToPrefab);
#else
			Object p = PrefabUtility.CreateEmptyPrefab(path);
            PrefabUtility.ReplacePrefab(go, p, ReplacePrefabOptions.ConnectToPrefab);
#endif
            GameObject.DestroyImmediate(go);
			
			tk2dEditorUtility.GetOrCreateIndex().AddSpriteAnimation(AssetDatabase.LoadAssetAtPath(path, typeof(tk2dSpriteAnimation)) as tk2dSpriteAnimation);
			tk2dEditorUtility.CommitIndex();

			// Select object
			Selection.activeObject = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
        }
    }	
}

