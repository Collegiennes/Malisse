using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;

public class MouseTouchManager : MonoBehaviour 
{
	private List<TouchHandler> m_Handlers = new List<TouchHandler>();
	private Camera m_Camera;
	private List<TouchHandler> m_HandlerHits = new List<TouchHandler>();
	
	private static MouseTouchManager m_Instance;
	
	public static MouseTouchManager Instance
	{
		get
		{
			if (m_Instance == null)
			{
				new GameObject("MouseTouchManager", typeof(MouseTouchManager));
			}
			
			return m_Instance;
		}
	}
	
	public static bool IsInstanceNull
	{
		get
		{
			return m_Instance == null;
		}
	}
	
	private void Awake()
	{
		// In case for some reason another TouchManager is initialized, the first one stays the singleton
		if (m_Instance != null)
		{
			return;
		}
		
		m_Instance = this;
		m_Camera = CameraController.Instance.m_GUICamera;
	}
	
	private void Update() 
	{
		// Clear some memory
		m_HandlerHits.Clear();
		
		// Raycast
		Vector3 mouseScreenPosition = Input.mousePosition;
		Vector3 mouseWorldPosition = m_Camera.ScreenToWorldPoint(mouseScreenPosition);
		Ray ray = m_Camera.ScreenPointToRay(mouseScreenPosition);
		RaycastHit[] hits = Physics.RaycastAll(ray);
		
		// Loop through the hits
		foreach (RaycastHit hit in hits.OrderBy(h => h.distance)) 
		{
			// Check if it matches any of the touch handlers
			foreach (TouchHandler handler in m_Handlers) 
			{
				if (hit.collider.gameObject == handler.Owner)
				{
					m_HandlerHits.Add(handler);
				}
			}
		}
		
		
		foreach (TouchRawType rawType in Enum.GetValues(typeof(TouchRawType))) 
		{
			TouchEvent touchEvent = new TouchEvent(mouseScreenPosition, mouseWorldPosition, rawType);
			
			// On touch down
			if (Input.GetMouseButtonDown((int)rawType))
			{
				foreach (TouchHandler handler in m_HandlerHits) 
				{
					handler.OnTouchDown(touchEvent);
					if (handler.KeepTouches)
					{
						break;
					}
				}
			}
			
			if (Input.GetMouseButton((int)rawType))
			{
				// On touch drag
				foreach (TouchHandler handler in m_Handlers) 
				{
					if (handler.HasClaimedTouch(rawType))
					{
						handler.OnTouchDrag(touchEvent);
					}
				}
			}
			
			if (Input.GetMouseButtonUp((int)rawType))
			{
				// On touch up
				foreach (TouchHandler handler in m_Handlers) 
				{
					if (handler.HasClaimedTouch(rawType))
					{
						handler.OnTouchUp(touchEvent);
					}
				}
			}
		}
	}
			
	private int HandlerComparison(TouchHandler x, TouchHandler y)
	{
		int comparison = 0;
		if ((int)x.Priority < (int)y.Priority)
		{
			comparison = -1;
		}
		else if ((int)x.Priority > (int)y.Priority)
		{
			comparison = 1;
		}
		
		return comparison;
	}
	
	public void AddHandler(TouchHandler touchHandler)
	{
		if (touchHandler != null && !m_Handlers.Contains(touchHandler))
		{
			m_Handlers.Add(touchHandler);
		}
	}
	
	public void RemoveHandler(TouchHandler touchHandler)
	{
		if (touchHandler != null && m_Handlers.Contains(touchHandler))
		{
			m_Handlers.Remove(touchHandler);
		}
	}
}

public struct TouchEvent
{
	public Vector3 ScreenPosition;
	public Vector3 WorldPosition;
	public TouchRawType RawType;
	
	public TouchEvent(Vector3 screenPosition, Vector3 worldPosition, TouchRawType rawType)
	{
		ScreenPosition = screenPosition;
		WorldPosition = worldPosition;
		RawType = rawType;
	}
}

public enum TouchRawType
{
	LeftClick = 0,
	RightClick = 1
}

public enum TouchPriority
{
	VeryHigh = 0,
	High = 10,
	Medium = 20,
	Low = 30,
	VeryLow = 40
}
