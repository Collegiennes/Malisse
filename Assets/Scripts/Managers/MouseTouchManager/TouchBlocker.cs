using UnityEngine;
using System.Collections;

public class TouchBlocker : MonoBehaviour, ITouchable
{
	public TouchPriority m_Priority = TouchPriority.Medium;
	
	private TouchHandler m_TouchHandler;

	protected void Start() 
	{
		m_TouchHandler = new TouchHandler(this);
		m_TouchHandler.KeepTouches = true;
	}
	
	private void OnDestroy()
	{
		m_TouchHandler.Release();
	}
	
	#region Events
	public bool OnTouchDown(TouchEvent touchEvent)
	{
		return true;
	}
	
	public void OnTouchDrag(TouchEvent touchEvent)
	{
	}
	
	public void OnTouchUp(TouchEvent touchEvent)
	{
	}
	#endregion
}
