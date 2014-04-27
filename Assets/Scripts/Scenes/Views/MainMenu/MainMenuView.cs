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
	#endregion

	#region View Implementation
	public override void HandleAction(FlowActionData actionData)
	{
		if (actionData.ActionName == "CHOOSE_ONE_PLAYER")
		{
			GameUtils.m_GameMode = GameUtils.eGameMode.ONE_PLAYER;
			FlowManager.Instance.TriggerAction("GO_TO_MAIN_GAME");
		}
		else if (actionData.ActionName == "CHOOSE_TWO_PLAYER")
		{
			ControllerInputManager.Instance.AddMouseController();

			GameUtils.m_GameMode = GameUtils.eGameMode.TWO_PLAYER;
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
