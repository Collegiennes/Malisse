using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class View : Observer 
{
	#region Members and Properties
	// constants
	
	// enums
	public enum eState
	{
		OPENING,
		OPENED,
		CLOSING,
		CLOSED,
		GAINING_FOCUS,
		LOSING_FOCUS,
		FOCUS_LOST
	}
	
	// public
	
	// protected
	protected eState m_State = eState.OPENING;
	protected FlowViewData m_ViewData = null;
	
	// private
	
	// properties
	public eState State
	{
		get { return m_State; }
	}

	public FlowViewData ViewData
	{
		get { return m_ViewData; }
	}
	#endregion
	
	#region Unity API
	protected virtual void OnDestroy()
	{
		m_ViewData = null;
	}
	#endregion
	
	#region Public Functions
	public virtual void OpenView(FlowViewData viewData, FlowActionData actionData)
	{
		m_State = eState.OPENING;
		m_ViewData = viewData;

		OnViewOpened();
	}

	public virtual void CloseView(FlowActionData actionData)
	{
		m_State = eState.CLOSING;

		OnViewClosed();
	}

	public virtual void GainFocus(FlowActionData actionData)
	{
		m_State = eState.GAINING_FOCUS;

		OnFocusGained();
	}

	public virtual void LoseFocus(FlowActionData actionData)
	{
		m_State = eState.LOSING_FOCUS;

		OnFocusLost();
	}
	
	public virtual void HandleAction(FlowActionData actionData)
	{
	}
	#endregion
	
	#region Protected Functions
	protected virtual void OnViewOpened()
	{
		m_State = eState.OPENED;

		FlowManager.Instance.OnViewCompleted(this);
	}

	protected virtual void OnViewClosed()
	{
		m_State = eState.CLOSED;
		
		FlowManager.Instance.OnViewCompleted(this);
	}
	
	protected virtual void OnFocusGained()
	{
		m_State = eState.OPENED;
		
		FlowManager.Instance.OnViewCompleted(this);
	}
	
	protected virtual void OnFocusLost()
	{
		m_State = eState.FOCUS_LOST;
		
		FlowManager.Instance.OnViewCompleted(this);
	}
	#endregion
	
	#region Private Functions
	#endregion
}
