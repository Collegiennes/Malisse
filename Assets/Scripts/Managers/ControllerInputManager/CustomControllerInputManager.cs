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
		NONE,
		GRAB,
		CHARACTER_1_GRAB,
		CHARACTER_2_GRAB
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
		// Aliases.
		SetAliases();
	}
	#endregion
	
	#region Public Functions
	public void AddMouseController()
	{
		MouseController controller = new MouseController();
		m_MouseControllerId = ControllerInputManager.Instance.AddController(controller);
	}
	#endregion
	
	#region Protected Functions
	#endregion
	
	#region Private Functions
	private void SetAliases()
	{
		AddButtonAlias((BaseController.eButtonId)Xbox360Controller.eXbox360ButtonId.A, eButtonAliases.GRAB.ToString());
		AddButtonAlias((BaseController.eButtonId)Xbox360Controller.eXbox360ButtonId.L1, eButtonAliases.CHARACTER_1_GRAB.ToString());
		AddButtonAlias((BaseController.eButtonId)Xbox360Controller.eXbox360ButtonId.R1, eButtonAliases.CHARACTER_2_GRAB.ToString());
	}
	#endregion
}
