using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class ControllerInputManager 
{
	#region Members and Properties
	// constants
	
	// enums
	public enum eButtonAliases
	{
		GRAB
	}
	
	// public
	
	// protected
	
	// private
	
	// properties
	#endregion
	
	#region Unity API
	#endregion

	#region ControllerInputManager Implementation
	partial void PostInputDetection()
	{
		// Aliases.
		SetAliases();
	}
	#endregion
	
	#region Public Functions
	#endregion
	
	#region Protected Functions
	#endregion
	
	#region Private Functions
	private void SetAliases()
	{
		AddButtonAlias((BaseController.eButtonId)Xbox360Controller.eXbox360ButtonId.A, eButtonAliases.GRAB.ToString());
	}
	#endregion
}
