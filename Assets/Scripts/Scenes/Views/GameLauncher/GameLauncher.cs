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
		Screen.showCursor = false;

		FlowManager.Instance.TriggerAction("BEGIN");

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
