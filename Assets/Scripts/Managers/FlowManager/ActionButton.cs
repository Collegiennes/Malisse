using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionButton : Button
{
	#region Members and Properties
	// constants
	
	// enums
	
	// public
	public string m_ActionName = "";
	
	// protected
	protected View m_AssociatedView = null;
	
	// private
	
	// properties
	#endregion
	
	#region Unity API
 	protected virtual void Awake()
	{
		m_AssociatedView = GameUtils.FindAssociatedView(transform.parent);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		m_AssociatedView = null;
	}
	#endregion

	#region Button Functions
	public override void OnTouchUp(TouchEvent touchEvent)
	{
		base.OnTouchUp(touchEvent);
		
		if (!string.IsNullOrEmpty(m_ActionName))
		{
			FlowManager.Instance.TriggerAction(m_ActionName);
		}
	}
	#endregion
	
	#region Public Functions
	#endregion
	
	#region Protected Functions
	#endregion
	
	#region Private Functions
	#endregion
}
