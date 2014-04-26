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
	private eControllerId m_MouseControllerId = eControllerId.NONE;
	
	// properties
	public eControllerId MouseControllerId
	{
		get { return m_MouseControllerId; }
	}
	#endregion
	
	#region Unity API
	#endregion

	#region ControllerInputManager Implementation
	partial void PostInputDetection()
	{
		AddMouseController();

		// Aliases.
		SetAliases();
	}
	#endregion
	
	#region Public Functions
	#endregion
	
	#region Protected Functions
	#endregion
	
	#region Private Functions
	private void AddMouseController()
	{
		MouseController controller = new MouseController();
		m_MouseControllerId = ControllerInputManager.Instance.AddController(controller);
	}

	private void SetAliases()
	{
		AddButtonAlias((BaseController.eButtonId)Xbox360Controller.eXbox360ButtonId.A, eButtonAliases.GRAB.ToString());
	}
	#endregion
}
