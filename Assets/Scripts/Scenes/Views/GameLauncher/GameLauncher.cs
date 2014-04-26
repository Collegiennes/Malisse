using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameLauncher : MonoBehaviour 
{
	#region Members and Properties
	// constants
	
	// enums
	
	// public
	
	// protected
	
	// private
	
	// properties
	#endregion
	
	#region Unity API
	private void Start () 
	{
		FlowManager.Instance.TriggerAction("GO_TO_MAIN_GAME");

		ControllerInputManager.Instance.Init();
	}
	#endregion
	
	#region Public Functions
	#endregion
	
	#region Protected Functions
	#endregion
	
	#region Private Functions
	#endregion
}
