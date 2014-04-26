using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace tk2dEditor.SpriteCollectionEditor
{
	public class TextureEditor
	{
		public enum Mode
		{
			None,
			Texture,
			Anchor,
			Collider
		}
		
		int textureBorderPixels = 16;
		
		Mode mode = Mode.Texture;
		Vector2 textureScrollPos = new Vector2(0.0f, 0.0f);
		bool drawColliderNormals = false;
		
		float editorDisplayScale 
		{
			get { return SpriteCollection.editorDisplayScale; }
			set { SpriteCollection.editorDisplayScale = value; }
		}
		
		Color[] _handleInactiveColors = new Color[] { 
			new Color32(127, 201, 122, 255), // default
			new Color32(180, 0, 0, 255), // red
			new Color32(255, 255, 255, 255), // white
			new Color32(32, 32, 32, 255), // black
		};
		
		Color[] _handleActiveColors = new Color[] {
			new Color32(228, 226, 60, 255),
			new Color32(255, 0, 0, 255),
			new Color32(255, 0, 0, 255),
			new Color32(96, 0, 0, 255),
		};
		
		tk2dSpriteCollectionDefinition.ColliderColor currentColliderColor = tk2dSpriteCollectionDefinition.ColliderColor.Default;
		Color handleInactiveColor { get { return _handleInactiveColors[(int)currentColliderColor]; } }
		Color handleActiveColor { get { return _handleActiveColors[(int)currentColliderColor]; } }

		IEditorHost host;
		public TextureEditor(IEditorHost host)
		{
			this.host = host;
		}
		SpriteCollectionProxy SpriteCollection { get { return host.SpriteCollection; } }
		
		public void SetMode(Mode mode)
		{
			if (this.mode != mode)
			{
				this.mode = mode;
				HandleUtility.Repaint();
			}
		}

		Vector2 ClosestPointOnLine(Vector2 p, Vector2 p1, Vector2 p2)
		{
			float magSq = (p2 - p1).sqrMagnitude;
			if (magSq < float.Epsilon)
				return p1;
			
			float u = ((p.x - p1.x) * (p2.x - p1.x) + (p.y - p1.y) * (p2.y - p1.y)) / magSq;
			if (u < 0.0f || u > 1.0f)
				return p1;
			
			return p1 + (p2 - p1) * u;
		}
		
		void DrawPolygonColliderEditor(Rect r, ref tk2dSpriteColliderIsland[] islands, Texture2D tex, bool forceClosed)
		{
			Vector2 origin = new Vector2(r.x, r.y);
			Vector3 origin3 = new Vector3(r.x, r.y, 0);
			
			// Sanitize
			if (islands == null || islands.Length == 0 ||
				!islands[0].IsValid())
			{
				islands = new tk2dSpriteColliderIsland[1];
				islands[0] = new tk2dSpriteColliderIsland();
				islands[0].connected = true;
				int w = tex.width;
				int h = tex.height;
				
				Vector2[] p = new Vector2[4];
				p[0] = new Vector2(0, 0);
				p[1] = new Vector2(0, h);
				p[2] = new Vector2(w, h);
				p[3] = new Vector2(w, 0);
				islands[0].points = p;
			}
			
			Color previousHandleColor = Handles.color;
			bool insertPoint = false;
			
			if (Event.current.clickCount == 2 && Event.current.type == EventType.MouseDown)
			{
				insertPoint = true;
				Event.current.Use();
			}
			
			if (r.Contains(Event.current.mousePosition) && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.C)
			{
				Vector2 min = Event.current.mousePosition / editorDisplayScale - new Vector2(16.0f, 16.0f);
				Vector3 max = Event.current.mousePosition / editorDisplayScale + new Vector2(16.0f, 16.0f);
				
				min.x = Mathf.Clamp(min.x, 0, tex.width * editorDisplayScale);
				min.y = Mathf.Clamp(min.y, 0, tex.height * editorDisplayScale);
				max.x = Mathf.Clamp(max.x, 0, tex.width * editorDisplayScale);
				max.y = Mathf.Clamp(max.y, 0, tex.height * editorDisplayScale);
				
				tk2dSpriteColliderIsland island = new tk2dSpriteColliderIsland();
				island.connected = true;
				
				Vector2[] p = new Vector2[4];
				p[0] = new Vector2(min.x, min.y);
				p[1] = new Vector2(min.x, max.y);
				p[2] = new Vector2(max.x, max.y);
				p[3] = new Vector2(max.x, min.y);
				island.points = p;
				
				System.Array.Resize(ref islands, islands.Length + 1);
				islands[islands.Length - 1] = island;
				
				Event.current.Use();
			}
			
			// Draw outline lines
			int deletedIsland = -1;
			for (int islandId = 0; islandId < islands.Length; ++islandId)
			{
				float closestDistanceSq = 1.0e32f;
				Vector2 closestPoint = Vector2.zero;
				int closestPreviousPoint = 0;
				
				var island = islands[islandId];
		
				Handles.color = handleInactiveColor;
	
				Vector2 ov = (island.points.Length>0)?island.points[island.points.Length-1]:Vector2.zero;
				for (int i = 0; i < island.points.Length; ++i)
				{
					Vector2 v = island.points[i];
					
					// Don't draw last connection if its not connected
					if (!island.connected && i == 0)
					{
						ov = v;
						continue;
					}
					
					if (insertPoint)
					{
						Vector2 localMousePosition = (Event.current.mousePosition - origin) / editorDisplayScale;
						Vector2 closestPointToCursor = ClosestPointOnLine(localMousePosition, ov, v);
						float lengthSq = (closestPointToCursor - localMousePosition).sqrMagnitude;
						if (lengthSq < closestDistanceSq)
						{
							closestDistanceSq = lengthSq;
							closestPoint = closestPointToCursor;
							closestPreviousPoint = i;
						}
					}
					
					if (drawColliderNormals)
					{
						Vector2 l = (ov - v).normalized;
						Vector2 n = new Vector2(l.y, -l.x);
						Vector2 c = (v + ov) * 0.5f * editorDisplayScale + origin;
						Handles.DrawLine(c, c + n * 16.0f);
					}
					
					Handles.DrawLine(v * editorDisplayScale + origin, ov * editorDisplayScale + origin);
					ov = v;
				}
				Handles.color = previousHandleColor;
				
				if (insertPoint && closestDistanceSq < 16.0f)
				{
					var tmpList = new List<Vector2>(island.points);
					tmpList.Insert(closestPreviousPoint, closestPoint);
					island.points = tmpList.ToArray();
					HandleUtility.Repaint();
				}
				
				int deletedIndex = -1;
				bool flipIsland = false;
				bool disconnectIsland = false;
				
				for (int i = 0; i < island.points.Length; ++i)
				{
					Vector3 cp = island.points[i];
					KeyCode keyCode = KeyCode.None;
					int id = 16433 + i;
					cp = (tk2dGuiUtility.PositionHandle(id, cp * editorDisplayScale + origin3, 4.0f, handleInactiveColor, handleActiveColor, out keyCode) - origin) / editorDisplayScale;
					
					if (keyCode == KeyCode.Backspace || keyCode == KeyCode.Delete)
					{
						deletedIndex = i;
					}
					
					if (keyCode == KeyCode.X)
					{
						deletedIsland = islandId;
					}
					
					if (keyCode == KeyCode.T && !forceClosed)
					{
						disconnectIsland = true;
					}
					
					if (keyCode == KeyCode.F)
					{
						flipIsland = true;
					}
					
					cp.x = Mathf.Round(cp.x);
					cp.y = Mathf.Round(cp.y);
					
					// constrain
					cp.x = Mathf.Clamp(cp.x, 0.0f, tex.width);
					cp.y = Mathf.Clamp(cp.y, 0.0f, tex.height);

					tk2dGuiUtility.SetPositionHandleValue(id, new Vector2(cp.x, cp.y));
					
					island.points[i] = cp;
				}
				
				if (flipIsland)
				{
					System.Array.Reverse(island.points);
				}
				
				if (disconnectIsland)
				{
					island.connected = !island.connected;
					if (island.connected && island.points.Length < 3)
					{
						Vector2 pp = (island.points[1] - island.points[0]);
						float l = pp.magnitude;
						pp.Normalize();
						Vector2 nn = new Vector2(pp.y, -pp.x);
						nn.y = Mathf.Clamp(nn.y, 0, tex.height);
						nn.x = Mathf.Clamp(nn.x, 0, tex.width);
						System.Array.Resize(ref island.points, island.points.Length + 1);
						island.points[island.points.Length - 1] = (island.points[0] + island.points[1]) * 0.5f + nn * l * 0.5f;
					}
				}
				
				if (deletedIndex != -1 && 
				    ((island.connected && island.points.Length > 3) ||
				    (!island.connected && island.points.Length > 2)) )
				{
					var tmpList = new List<Vector2>(island.points);
					tmpList.RemoveAt(deletedIndex);
					island.points = tmpList.ToArray();
				}			
			}
			
			// Can't delete the last island
			if (deletedIsland != -1 && islands.Length > 1)
			{
				var tmpIslands = new List<tk2dSpriteColliderIsland>(islands);
				tmpIslands.RemoveAt(deletedIsland);
				islands = tmpIslands.ToArray();
			}
		}		
		
		void DrawCustomBoxColliderEditor(Rect r, tk2dSpriteCollectionDefinition param, Texture2D tex)
		{
			Vector2 origin = new Vector2(r.x, r.y);
			
			// sanitize
			if (param.boxColliderMin == Vector2.zero && param.boxColliderMax == Vector2.zero)
			{
				param.boxColliderMax = new Vector2(tex.width, tex.height);
			}
			
			Vector3[] pt = new Vector3[] {
				new Vector3(param.boxColliderMin.x * editorDisplayScale + origin.x, param.boxColliderMin.y * editorDisplayScale + origin.y, 0.0f),
				new Vector3(param.boxColliderMax.x * editorDisplayScale + origin.x, param.boxColliderMin.y * editorDisplayScale + origin.y, 0.0f),
				new Vector3(param.boxColliderMax.x * editorDisplayScale + origin.x, param.boxColliderMax.y * editorDisplayScale + origin.y, 0.0f),
				new Vector3(param.boxColliderMin.x * editorDisplayScale + origin.x, param.boxColliderMax.y * editorDisplayScale + origin.y, 0.0f),
			};
			Color32 transparentColor = handleInactiveColor;
			transparentColor.a = 10;
			Handles.DrawSolidRectangleWithOutline(pt, transparentColor, handleInactiveColor);
			
			// Draw grab handles
			Vector3 handlePos;
			
			int id = 16433;
			
			// Draw top handle
			handlePos = (pt[0] + pt[1]) * 0.5f;
			handlePos = (tk2dGuiUtility.PositionHandle(id + 0, handlePos, 4.0f, handleInactiveColor, handleActiveColor) - origin) / editorDisplayScale;
			param.boxColliderMin.y = handlePos.y;
			if (param.boxColliderMin.y > param.boxColliderMax.y) param.boxColliderMin.y = param.boxColliderMax.y;
	
			// Draw bottom handle
			handlePos = (pt[2] + pt[3]) * 0.5f;
			handlePos = (tk2dGuiUtility.PositionHandle(id + 1, handlePos, 4.0f, handleInactiveColor, handleActiveColor) - origin) / editorDisplayScale;
			param.boxColliderMax.y = handlePos.y;
			if (param.boxColliderMax.y < param.boxColliderMin.y) param.boxColliderMax.y = param.boxColliderMin.y;
	
			// Draw left handle
			handlePos = (pt[0] + pt[3]) * 0.5f;
			handlePos = (tk2dGuiUtility.PositionHandle(id + 2, handlePos, 4.0f, handleInactiveColor, handleActiveColor) - origin) / editorDisplayScale;
			param.boxColliderMin.x = handlePos.x;
			if (param.boxColliderMin.x > param.boxColliderMax.x) param.boxColliderMin.x = param.boxColliderMax.x;
	
			// Draw right handle
			handlePos = (pt[1] + pt[2]) * 0.5f;
			handlePos = (tk2dGuiUtility.PositionHandle(id + 3, handlePos, 4.0f, handleInactiveColor, handleActiveColor) - origin) / editorDisplayScale;
			param.boxColliderMax.x = handlePos.x;
			if (param.boxColliderMax.x < param.boxColliderMin.x) param.boxColliderMax.x = param.boxColliderMin.x;
	
			param.boxColliderMax.x = Mathf.Round(param.boxColliderMax.x);
			param.boxColliderMax.y = Mathf.Round(param.boxColliderMax.y);
			param.boxColliderMin.x = Mathf.Round(param.boxColliderMin.x);
			param.boxColliderMin.y = Mathf.Round(param.boxColliderMin.y);		
	
			// constrain
			param.boxColliderMax.x = Mathf.Clamp(param.boxColliderMax.x, 0.0f, tex.width);
			param.boxColliderMax.y = Mathf.Clamp(param.boxColliderMax.y, 0.0f, tex.height);
			param.boxColliderMin.x = Mathf.Clamp(param.boxColliderMin.x, 0.0f, tex.width);
			param.boxColliderMin.y = Mathf.Clamp(param.boxColliderMin.y, 0.0f, tex.height);

			tk2dGuiUtility.SetPositionHandleValue(id + 0, new Vector2(0, param.boxColliderMin.y));
			tk2dGuiUtility.SetPositionHandleValue(id + 1, new Vector2(0, param.boxColliderMax.y));
			tk2dGuiUtility.SetPositionHandleValue(id + 2, new Vector2(param.boxColliderMin.x, 0));
			tk2dGuiUtility.SetPositionHandleValue(id + 3, new Vector2(param.boxColliderMax.x, 0));
		}
		
		void HandleKeys()
		{
			Event evt = Event.current;
			if (evt.type == EventType.KeyUp && evt.shift)
			{
				Mode newMode = Mode.None;
				switch (evt.keyCode)
				{
					case KeyCode.Q: newMode = Mode.Texture; break;
					case KeyCode.W: newMode = Mode.Anchor; break;
					case KeyCode.E: newMode = Mode.Collider; break;
					case KeyCode.N: drawColliderNormals = !drawColliderNormals; HandleUtility.Repaint(); break;
				}
				if (newMode != Mode.None)
				{
					mode = newMode;
					evt.Use();
				}
			}
		}

		public void DrawTextureView(tk2dSpriteCollectionDefinition param, Texture2D texture)
		{
			HandleKeys();

			if (mode == Mode.None)
				mode = Mode.Texture;
			
			// sanity check
			if (editorDisplayScale <= 1.0f) editorDisplayScale = 1.0f;
			
			// mirror data
			currentColliderColor = param.colliderColor;
			
			GUILayout.BeginVertical(tk2dEditorSkin.SC_BodyBackground, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
	
			bool allowAnchor = param.anchor == tk2dSpriteCollectionDefinition.Anchor.Custom;
			bool allowCollider = (param.colliderType == tk2dSpriteCollectionDefinition.ColliderType.Polygon ||
				param.colliderType == tk2dSpriteCollectionDefinition.ColliderType.BoxCustom);
			if (mode == Mode.Anchor && !allowAnchor) mode = Mode.Texture;
			if (mode == Mode.Collider && !allowCollider) mode = Mode.Texture;
			
			Rect rect = GUILayoutUtility.GetRect(128.0f, 128.0f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
			
			// middle mouse drag and scroll zoom
			if (rect.Contains(Event.current.mousePosition))
			{
				if (Event.current.type == EventType.MouseDrag && Event.current.button == 2)
				{
					textureScrollPos -= Event.current.delta * editorDisplayScale;
					Event.current.Use();
					HandleUtility.Repaint();
				}
				if (Event.current.type == EventType.ScrollWheel)
				{
					editorDisplayScale -= Event.current.delta.y * 0.03f;
					Event.current.Use();
					HandleUtility.Repaint();
				}
			}
			
			bool alphaBlend = true;
			textureScrollPos = GUI.BeginScrollView(rect, textureScrollPos, 
				new Rect(0, 0, textureBorderPixels * 2 + (texture.width) * editorDisplayScale, textureBorderPixels * 2 + (texture.height) * editorDisplayScale));
			Rect textureRect = new Rect(textureBorderPixels, textureBorderPixels, texture.width * editorDisplayScale, texture.height * editorDisplayScale);
			texture.filterMode = FilterMode.Point;
			GUI.DrawTexture(textureRect, texture, ScaleMode.ScaleAndCrop, alphaBlend);

			if (mode == Mode.Collider)
			{
				if (param.colliderType == tk2dSpriteCollectionDefinition.ColliderType.BoxCustom)
					DrawCustomBoxColliderEditor(textureRect, param, texture);
				if (param.colliderType == tk2dSpriteCollectionDefinition.ColliderType.Polygon)
					DrawPolygonColliderEditor(textureRect, ref param.polyColliderIslands, texture, false);
			}
			
			if (mode == Mode.Texture && param.customSpriteGeometry)
			{
				DrawPolygonColliderEditor(textureRect, ref param.geometryIslands, texture, true);
			}
			
			// Anchor
			if (mode == Mode.Anchor)
			{
				Color handleColor = new Color(0,0,0,0.2f);
				Color lineColor = Color.white;
				Vector2 anchor = new Vector2(param.anchorX, param.anchorY);
				Vector2 origin = new Vector2(textureRect.x, textureRect.y);
				
				int id = 99999;
				anchor = (tk2dGuiUtility.PositionHandle(id, anchor * editorDisplayScale + origin, 12.0f, handleColor, handleColor ) - origin) / editorDisplayScale;
	
				Color oldColor = Handles.color;
				Handles.color = lineColor;
				float w = Mathf.Max(rect.width, texture.width * editorDisplayScale);
				float h = Mathf.Max(rect.height, texture.height * editorDisplayScale);
				
				Handles.DrawLine(new Vector3(textureRect.x, textureRect.y + anchor.y * editorDisplayScale, 0), new Vector3(textureRect.x + w, textureRect.y + anchor.y * editorDisplayScale, 0));
				Handles.DrawLine(new Vector3(textureRect.x + anchor.x * editorDisplayScale, textureRect.y + 0, 0), new Vector3(textureRect.x + anchor.x * editorDisplayScale, textureRect.y + h, 0));
				Handles.color = oldColor;
	
				// constrain
				param.anchorX = Mathf.Clamp(Mathf.Round(anchor.x), 0.0f, texture.width);
				param.anchorY = Mathf.Clamp(Mathf.Round(anchor.y), 0.0f, texture.height);
				
				tk2dGuiUtility.SetPositionHandleValue(id, new Vector2(param.anchorX, param.anchorY));
				
				HandleUtility.Repaint();			
			}
			GUI.EndScrollView();
			
			// Draw toolbar
			DrawToolbar(param);
			
			GUILayout.EndVertical();
		}
		
		public void DrawToolbar(tk2dSpriteCollectionDefinition param)
		{
			bool allowAnchor = param.anchor == tk2dSpriteCollectionDefinition.Anchor.Custom;
			bool allowCollider = (param.colliderType == tk2dSpriteCollectionDefinition.ColliderType.Polygon ||
				param.colliderType == tk2dSpriteCollectionDefinition.ColliderType.BoxCustom);

			GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
			mode = GUILayout.Toggle((mode == Mode.Texture), new GUIContent("Sprite", "Shift+Q"), EditorStyles.toolbarButton)?Mode.Texture:mode;
			if (allowAnchor)
				mode = GUILayout.Toggle((mode == Mode.Anchor), new GUIContent("Anchor", "Shift+W"), EditorStyles.toolbarButton)?Mode.Anchor:mode;
			if (allowCollider)
				mode = GUILayout.Toggle((mode == Mode.Collider), new GUIContent("Collider", "Shift+E"), EditorStyles.toolbarButton)?Mode.Collider:mode;
			GUILayout.FlexibleSpace();
			
			if (tk2dGuiUtility.HasActivePositionHandle)
			{
				string str = "X: " + tk2dGuiUtility.ActiveHandlePosition.x + " Y: " + tk2dGuiUtility.ActiveHandlePosition.y;
				GUILayout.Label(str, EditorStyles.toolbarTextField);
			}
			
			if ((mode == Mode.Collider && param.colliderType == tk2dSpriteCollectionDefinition.ColliderType.Polygon) ||
				(mode == Mode.Texture && param.customSpriteGeometry))
			{
				drawColliderNormals = GUILayout.Toggle(drawColliderNormals, new GUIContent("Show Normals", "Shift+N"), EditorStyles.toolbarButton);
			}
			GUILayout.EndHorizontal();			
		}
		
		public void DrawEmptyTextureView()
		{
			mode = Mode.None;
			GUILayout.FlexibleSpace();
		}
		
		public void DrawTextureInspector(tk2dSpriteCollectionDefinition param, Texture2D texture)
		{
			if (mode == Mode.Collider && param.colliderType == tk2dSpriteCollectionDefinition.ColliderType.Polygon)
			{
				param.colliderColor = (tk2dSpriteCollectionDefinition.ColliderColor)EditorGUILayout.EnumPopup("Display Color", param.colliderColor);
				
				tk2dGuiUtility.InfoBox("Points" +
										  "\nClick drag - move point" +
										  "\nClick hold + delete/bkspace - delete point" +
										  "\nDouble click on line - add point", tk2dGuiUtility.WarningLevel.Info);
	
				tk2dGuiUtility.InfoBox("Islands" +
										  "\nClick hold point + X - delete island" +
										  "\nPress C - create island at cursor" + 
							              "\nClick hold point + T - toggle connected" +
							              "\nClick hold point + F - flip island", tk2dGuiUtility.WarningLevel.Info);
			}
			if (mode == Mode.Texture && param.customSpriteGeometry)
			{
				param.colliderColor = (tk2dSpriteCollectionDefinition.ColliderColor)EditorGUILayout.EnumPopup("Display Color", param.colliderColor);

				tk2dGuiUtility.InfoBox("Points" +
										  "\nClick drag - move point" +
										  "\nClick hold + delete/bkspace - delete point" +
										  "\nDouble click on line - add point", tk2dGuiUtility.WarningLevel.Info);
	
				tk2dGuiUtility.InfoBox("Islands" +
										  "\nClick hold point + X - delete island" +
										  "\nPress C - create island at cursor" + 
							              "\nClick hold point + F - flip island", tk2dGuiUtility.WarningLevel.Info);
			}
		}
	}
	
}
