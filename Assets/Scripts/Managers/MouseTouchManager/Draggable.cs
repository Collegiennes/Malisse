using UnityEngine;
using System.Collections;
using System.IO;

public class Draggable : Subject , ITouchable
{	
	private TouchHandler m_TouchHandler;
	
	protected Vector3 m_MouseOffset = Vector3.zero;
	protected Camera m_Camera;
	protected bool m_IsBeingDragged = false;	
	protected static bool m_IsDragging = false;
	
	public static bool IsDragging
	{
		get
		{
			return m_IsDragging;
		}
	}
	
	protected void Start() 
	{
		m_Camera = Camera.main;
		
		m_TouchHandler = new TouchHandler(this);
		m_TouchHandler.KeepTouches = true;
	}
	
	private void OnDestroy()
	{
		m_TouchHandler.Release();
	}
	
	#region Events
	public virtual bool OnTouchDown(TouchEvent touchEvent)
	{
		if (!m_IsDragging && touchEvent.RawType == TouchRawType.LeftClick)
		{
			m_IsDragging = true;
			m_IsBeingDragged = true;
			m_MouseOffset = transform.position - m_Camera.ScreenToWorldPoint(Input.mousePosition);
			return true;
		}
		
		return false;
	}
	
	public virtual void OnTouchDrag(TouchEvent touchEvent)
	{
		if (m_IsBeingDragged && touchEvent.RawType == TouchRawType.LeftClick)
		{
			Vector3 position = m_Camera.ScreenToWorldPoint(Input.mousePosition) + m_MouseOffset;
			transform.position = new Vector3(position.x, position.y, transform.position.z);
		}
	}
	
	public virtual void OnTouchUp(TouchEvent touchEvent)
	{
		if (m_IsBeingDragged && touchEvent.RawType == TouchRawType.LeftClick)
		{
			m_IsDragging = false;
			m_IsBeingDragged = false;
		}
	}
	#endregion
}
