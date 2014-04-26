using UnityEngine;
using System.Collections;

public class Button : Subject, ITouchable
{
	public enum State
	{
		DOWN,
		UP,
		FIRED,
		OVER
	}
	
	// Public
	public TouchPriority m_Priority = TouchPriority.Medium;

	// Protected
	protected State m_State = State.UP;
	
	// Private
	private TouchHandler m_TouchHandler;

	protected virtual void Start() 
	{
		m_TouchHandler = new TouchHandler(this);
		m_TouchHandler.KeepTouches = true;
	}
	
	protected virtual void OnDestroy()
	{
		if (m_TouchHandler != null)
		{
			m_TouchHandler.Release();
		}
	}
	
	// Public
	public bool IsFired(object subject, params object[] args)
	{
		return subject as Button == this && args != null && args.Length > 0 && args[0] is State && (State)args[0] == State.FIRED;
	}
	
	#region Events
	public virtual bool OnTouchDown(TouchEvent touchEvent)
	{
		m_State = State.DOWN;
		NotifyObservers(m_State);
		
		return true;
	}
	
	public virtual void OnTouchDrag(TouchEvent touchEvent)
	{
	}
	
	public virtual void OnTouchUp(TouchEvent touchEvent)
	{
		m_State = State.UP;
		NotifyObservers(m_State);
		
		m_State = State.FIRED;
		NotifyObservers(m_State);
	}
	#endregion
}
