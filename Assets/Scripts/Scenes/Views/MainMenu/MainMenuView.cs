using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainMenuView : AlisseView 
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
	protected override void Update()
	{
		base.Update();

		if (m_State != eState.OPENED)
		{
			return;
		}

		if (ControllerInputManager.Instance.GetButton(ControllerInputManager.eButtonAliases.GRAB.ToString()).Count > 0)
		{
			FlowManager.Instance.TriggerAction("GO_TO_MAIN_GAME");
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
