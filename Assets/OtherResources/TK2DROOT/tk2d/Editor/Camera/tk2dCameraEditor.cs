using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(tk2dCamera))]
public class tk2dCameraEditor : Editor 
{
	struct Preset
	{
		public string name;
		public int width;
		public int height;
		public Preset(string name, int width, int height) { this.name = name; this.width = width; this.height = height; }
	}

	Preset[] presets = new Preset[] {
		new Preset("iOS/iPhone 3G Tall", 320, 480),
		new Preset("iOS/iPhone 3G Wide", 480, 320),
		new Preset("iOS/iPhone 5 Tall", 640, 1136),
		new Preset("iOS/iPhone 5 Wide", 1136, 640),
		new Preset("iOS/iPhone 4 Tall", 640, 960),
		new Preset("iOS/iPhone 4 Wide", 960, 640),
		new Preset("iOS/iPad Tall", 768, 1024),
		new Preset("iOS/iPad Wide", 1024, 768),
		new Preset("iOS/iPad 3 Tall", 1536, 2048),
		new Preset("iOS/iPad 3 Wide", 2048, 1536),

		new Preset("Android/HTC Legend Tall", 480, 320),
		new Preset("Android/HTC Legend Wide", 320, 480),
		new Preset("Android/Nexus One Tall", 480, 800),
		new Preset("Android/Nexus One Wide", 800, 480),
		new Preset("Android/MotorolaDroidX Tall", 480, 854),
		new Preset("Android/MotorolaDroidX Wide", 854, 480),
		new Preset("Android/MotorolaDroidX2 Tall", 540, 960),
		new Preset("Android/MotorolaDroidX2 Wide", 960, 540),
		new Preset("Android/Tegra Tablet Tall", 600, 1024),
		new Preset("Android/Tegra Tablet Wide", 1024, 600),
		new Preset("Android/Nexus7 Tall", 800, 1280),
		new Preset("Android/Nexus7 Wide", 1280, 800),

		new Preset("TV/720p", 1280, 720),
		new Preset("TV/1080p", 1920, 1080),

		new Preset("PC/4:3", 640, 480),
		new Preset("PC/4:3", 800, 600),
		new Preset("PC/4:3", 1024, 768),
	};

	string[] _presetListStr = null;
	string[] presetListStr {
		get {
			if (_presetListStr == null)
			{
				_presetListStr = new string[presets.Length + 1];
				_presetListStr[0] = "-";
				for (int i = 0; i < presets.Length; ++i)
					_presetListStr[i+1] = string.Format("{0} ({1} x {2})", presets[i].name, presets[i].width, presets[i].height);
			}
			return _presetListStr;
		}
	}

	public override void OnInspectorGUI()
	{
		//DrawDefaultInspector();

		tk2dCamera _target = (tk2dCamera)target;
		var frameBorderStyle = EditorStyles.textField;
		
		// sanity
		if (_target.resolutionOverride == null)
		{
			_target.resolutionOverride = new tk2dCameraResolutionOverride[0];
			GUI.changed = true;
		}
		
		_target.enableResolutionOverrides = EditorGUILayout.Toggle("Resolution overrides", _target.enableResolutionOverrides);
		if (_target.enableResolutionOverrides)
		{
			EditorGUILayout.LabelField("Native resolution", EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			_target.nativeResolutionWidth = EditorGUILayout.IntField("Width", _target.nativeResolutionWidth);
			_target.nativeResolutionHeight = EditorGUILayout.IntField("Height", _target.nativeResolutionHeight);
			EditorGUI.indentLevel--;

			// Overrides
			EditorGUILayout.LabelField("Overrides", EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			
			int deleteId = -1;
			for (int i = 0; i < _target.resolutionOverride.Length; ++i)
			{
				var ovr = _target.resolutionOverride[i];
				EditorGUILayout.BeginVertical(frameBorderStyle);
				GUILayout.Space(8);
				ovr.name = EditorGUILayout.TextField("Name", ovr.name);
				ovr.width = EditorGUILayout.IntField("Width", ovr.width);
				ovr.height = EditorGUILayout.IntField("Height", ovr.height);
				ovr.autoScaleMode = (tk2dCameraResolutionOverride.AutoScaleMode)EditorGUILayout.EnumPopup("Auto Scale", ovr.autoScaleMode);
				if (ovr.autoScaleMode == tk2dCameraResolutionOverride.AutoScaleMode.None)
				{
					EditorGUI.indentLevel++;
					ovr.scale = EditorGUILayout.FloatField("Scale", ovr.scale);
					EditorGUI.indentLevel--;
				}
				if (ovr.autoScaleMode == tk2dCameraResolutionOverride.AutoScaleMode.StretchToFit)
				{
					string msg = "The native resolution image will be stretched to fit the target display. " +
					"Image quality will suffer if non-uniform scaling occurs.";
					tk2dGuiUtility.InfoBox(msg, tk2dGuiUtility.WarningLevel.Info);
				}
				else
				{
					ovr.fitMode = (tk2dCameraResolutionOverride.FitMode)EditorGUILayout.EnumPopup("Fit Mode", ovr.fitMode);
					if (ovr.fitMode == tk2dCameraResolutionOverride.FitMode.Constant)
					{
						EditorGUI.indentLevel++;
						ovr.offsetPixels.x = EditorGUILayout.FloatField("X", ovr.offsetPixels.x);
						ovr.offsetPixels.y = EditorGUILayout.FloatField("Y", ovr.offsetPixels.y);
						EditorGUI.indentLevel--;
					}
				}
				GUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel(" ");
				if (GUILayout.Button("Delete", EditorStyles.miniButton))
					deleteId = i;
				GUILayout.EndHorizontal();
				GUILayout.Space(4);
				EditorGUILayout.EndVertical();
			}
			
			if (deleteId != -1)
			{
				List<tk2dCameraResolutionOverride> ovr = new List<tk2dCameraResolutionOverride>(_target.resolutionOverride);
				ovr.RemoveAt(deleteId);
				_target.resolutionOverride = ovr.ToArray();
				GUI.changed = true;
				Repaint();
			}
			
			EditorGUILayout.BeginVertical(frameBorderStyle);
			GUILayout.Space(32);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Add override", GUILayout.ExpandWidth(false)))
			{
				tk2dCameraResolutionOverride ovr = new tk2dCameraResolutionOverride();
				ovr.name = "Wildcard Override";
				ovr.width = -1;
				ovr.height = -1;
				ovr.autoScaleMode = tk2dCameraResolutionOverride.AutoScaleMode.FitVisible;
				ovr.fitMode = tk2dCameraResolutionOverride.FitMode.Center;
				System.Array.Resize(ref _target.resolutionOverride, _target.resolutionOverride.Length + 1);
				_target.resolutionOverride[_target.resolutionOverride.Length - 1] = ovr;
				GUI.changed = true;
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.Space(32);
			EditorGUILayout.EndVertical();
			EditorGUI.indentLevel--;
		}
		EditorGUILayout.Space();
		
		
		EditorGUILayout.LabelField("Camera resolution", EditorStyles.boldLabel);
		GUIContent toggleLabel = new GUIContent("Force Editor Resolution", 
			"When enabled, forces the resolution in the editor regardless of the size of the game window.");
		EditorGUI.indentLevel++;

		bool cameraOverrideChanged = false;

		tk2dGuiUtility.BeginChangeCheck();
		_target.forceResolutionInEditor = EditorGUILayout.Toggle(toggleLabel, _target.forceResolutionInEditor);
		if (tk2dGuiUtility.EndChangeCheck()) cameraOverrideChanged = true;

		if (_target.forceResolutionInEditor)
		{
			tk2dGuiUtility.BeginChangeCheck();

			int selectedResolution = EditorGUILayout.Popup("Preset", 0, presetListStr);
			if (selectedResolution != 0)
			{
				var preset = presets[selectedResolution - 1];
				_target.forceResolution.x = preset.width;
				_target.forceResolution.y = preset.height;
				GUI.changed = true;
			}

			_target.forceResolution.x = EditorGUILayout.IntField("Width", (int)_target.forceResolution.x);
			_target.forceResolution.y = EditorGUILayout.IntField("Height", (int)_target.forceResolution.y);

			// clamp to a sensible value
			_target.forceResolution.x = Mathf.Max(_target.forceResolution.x, 50);
			_target.forceResolution.y = Mathf.Max(_target.forceResolution.y, 50);

			Rect r = GUILayoutUtility.GetRect(1, 1, GUILayout.ExpandWidth(true), GUILayout.MinHeight(43));
			EditorGUI.HelpBox(new Rect(r.x + 4, r.y, r.width - 8, r.height), "Ensure that the the game view resolution is the same as the override chosen here, otherwise the game window will not display correctly.", MessageType.Warning);

			if (tk2dGuiUtility.EndChangeCheck())
				cameraOverrideChanged = true;
		}
		else
		{
			EditorGUILayout.FloatField("Width", _target.TargetResolution.x);
			EditorGUILayout.FloatField("Height", _target.TargetResolution.y);
		}
		EditorGUI.indentLevel--;

		if (cameraOverrideChanged)
		{
			// Propagate values to all tk2dCameras in scene
			tk2dCamera[] otherCameras = Resources.FindObjectsOfTypeAll(typeof(tk2dCamera)) as tk2dCamera[];
			foreach (tk2dCamera thisCamera in otherCameras)
			{
				thisCamera.forceResolutionInEditor = _target.forceResolutionInEditor;
				thisCamera.forceResolution = _target.forceResolution;
				thisCamera.UpdateCameraMatrix();
			}

			// Update all anchors after that
			tk2dCameraAnchor[] anchors = Resources.FindObjectsOfTypeAll(typeof(tk2dCameraAnchor)) as tk2dCameraAnchor[];
			foreach (var anchor in anchors)
				anchor.ForceUpdateTransform();
		}
		
		if (GUI.changed)
		{
			EditorUtility.SetDirty(target);
			tk2dCameraAnchor[] allAlignmentObjects = GameObject.FindObjectsOfType(typeof(tk2dCameraAnchor)) as tk2dCameraAnchor[];
			foreach (var v in allAlignmentObjects)
			{
				EditorUtility.SetDirty(v);
			}
		}
		
		GUILayout.Space(16.0f);
		
		EditorGUILayout.LabelField("Tools", EditorStyles.boldLabel);
		EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		if (GUILayout.Button("Create Anchor", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
		{
			tk2dCamera cam = (tk2dCamera)target;
			
			GameObject go = new GameObject("Anchor");
			go.transform.parent = cam.transform;
			tk2dCameraAnchor cameraAnchor = go.AddComponent<tk2dCameraAnchor>();
			cameraAnchor.tk2dCamera = cam;
			
			EditorGUIUtility.PingObject(go);
		}
		
		EditorGUILayout.EndHorizontal();
	}

	// Scene GUI handler - draws custom preview window, working around Unity bug
	tk2dEditor.tk2dCameraSceneGUI sceneGUIHandler = null;

	void OnDisable()
	{
		if (sceneGUIHandler != null)
		{
			sceneGUIHandler.Destroy();
			sceneGUIHandler = null;
		}
	}

	void OnSceneGUI()
	{
		if (sceneGUIHandler == null)
			sceneGUIHandler = new tk2dEditor.tk2dCameraSceneGUI();

		sceneGUIHandler.OnSceneGUI(target as tk2dCamera);
	}


	// Create tk2dCamera menu item
    [MenuItem("GameObject/Create Other/tk2d/Camera", false, 14905)]
    static void DoCreateCameraObject()
	{
		GameObject go = tk2dEditorUtility.CreateGameObjectInScene("tk2dCamera");
		go.transform.position = new Vector3(0, 0, -10.0f);
		Camera camera = go.AddComponent<Camera>();
		camera.orthographic = true;
		camera.orthographicSize = 480.0f; // arbitrary large number
		camera.farClipPlane = 1000.0f;
		go.AddComponent<tk2dCamera>();

		Selection.activeGameObject = go;
		Undo.RegisterCreatedObjectUndo(go, "Create tk2dCamera");
	}
}


// tk2dCameraSceneGUI - Enacapsulates the scene GUI implementation
// This is a workaround while Unity fixes the bug in the tk2dCamera code
// This is also the reason its in the same file - it will simply be defined
// when unity fix the bug and not leave an extra file in the file system
namespace tk2dEditor
{
	public class tk2dCameraSceneGUI
	{
		public void Destroy()
		{
			if (previewCamera != null)
			{
				Object.DestroyImmediate(previewCamera);
				previewCamera = null;
			}
		}

		void PreviewWindowFunc(int windowId) 
		{
			GUILayout.BeginVertical();
			Rect rs = GUILayoutUtility.GetRect(1.0f, 1.0f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

			switch (Event.current.type)
			{
				case EventType.Repaint:
				{
					int heightTweak = 19;
					Rect r = new Rect(previewWindowRect.x + rs.x, Camera.current.pixelHeight - (previewWindowRect.y + rs.y), rs.width, rs.height);
					Vector2 v = new Vector2(previewWindowRect.x + rs.x, (Camera.current.pixelHeight - previewWindowRect.y - rs.height - heightTweak) + rs.y);
					previewCamera.CopyFrom(target.camera);
					previewCamera.projectionMatrix = target.camera.projectionMatrix; // Work around a Unity bug
					previewCamera.pixelRect = new Rect(v.x, v.y, r.width, r.height);
					previewCamera.Render();
					break;
				}
			}

			GUILayout.EndVertical();
		}

		public void OnSceneGUI(tk2dCamera target)
		{
			this.target = target;

			if (previewCamera == null)
			{
				GameObject go = EditorUtility.CreateGameObjectWithHideFlags("tk2dCamera", UnityEngine.HideFlags.HideAndDontSave, new System.Type[] { typeof(Camera) } );
				previewCamera = go.camera;
				previewCamera.enabled = false;
			}

			Vector2 resolution = target.TargetResolution;

			float maxHeight = Screen.height / 5;
			float fWidth, fHeight;
			fHeight = maxHeight;
			fWidth = resolution.x / resolution.y * maxHeight;
			
			int windowDecorationWidth = 11;
			int windowDecorationHeight = 24;
			int width = (int)fWidth + windowDecorationWidth;
			int height = (int)fHeight + windowDecorationHeight;

			string windowCaption = "tk2dCamera";
			if (width > 200)
				windowCaption += string.Format(" ({0:0} x {1:0})", resolution.x, resolution.y);

			int viewportOffsetLeft = 10;
			int viewportOffsetBottom = -8;
			previewWindowRect = new Rect(viewportOffsetLeft, Camera.current.pixelHeight - height - viewportOffsetBottom, width, height);
			Handles.BeginGUI();
			GUI.Window("tk2dCamera Preview".GetHashCode(), previewWindowRect, PreviewWindowFunc, windowCaption);
			Handles.EndGUI();
		}

		tk2dCamera target;

		Camera previewCamera = null;
		Rect previewWindowRect;
	}	
}
