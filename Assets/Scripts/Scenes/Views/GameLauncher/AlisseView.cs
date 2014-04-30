using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AlisseView : View 
{
	#region Members and properties
	// constants
	
	// enums
	
	// public
	
	// protected
	
	// private
	
	// properties
	#endregion
	
	#region Unity API
	protected virtual void Update()
	{
		if (m_State != eState.OPENED)
		{
			return;
		}
		
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			FlowManager.Instance.TriggerAction("GO_TO_MAIN_MENU");
		}
	}
	#endregion
	
	#region Public Methods
	#endregion
	
	#region Protected Methods
	#endregion
	
	#region Private Methods
	#endregion
}
