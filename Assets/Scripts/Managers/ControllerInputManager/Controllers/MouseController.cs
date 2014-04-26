using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ControllerAttribute("Mouse")]
public class MouseController : BaseController
{
	#region Members and Properties
	// constants
	
	// enums
	public enum eMouseButtonId
	{
		NONE,
		LEFT_CLICK,
		RIGHT_CLICK
	}
	
	// public
	
	// protected
	
	// private
	
	// properties
	#endregion
	
	#region Class API
	#endregion

	#region IController Implementation
	public override void SetKeyMapping()
	{
		// NOTE ppoirier: Maybe motion isn't appropriate for mouse movement.
		AddAxisMap(eAxisId.MOTION_HORIZONTAL, "MouseX");
		AddAxisMap(eAxisId.MOTION_VERTICAL, "MouseY");
	}
	
	public override bool GetButton(BaseController.eButtonId buttonId)
	{
		switch ((MouseController.eMouseButtonId)buttonId)
		{
		case MouseController.eMouseButtonId.LEFT_CLICK:
			return Input.GetMouseButton(0);
		case MouseController.eMouseButtonId.RIGHT_CLICK:
			return Input.GetMouseButton(1);
		}
		
		return base.GetButton(buttonId);
	}

	public override bool GetButtonDown(BaseController.eButtonId buttonId)
	{
		switch ((MouseController.eMouseButtonId)buttonId)
		{
		case MouseController.eMouseButtonId.LEFT_CLICK:
			return Input.GetMouseButtonDown(0);
		case MouseController.eMouseButtonId.RIGHT_CLICK:
			return Input.GetMouseButtonDown(1);
		}
		
		return base.GetButtonDown(buttonId);
	}
	
	public override bool GetButtonUp(BaseController.eButtonId buttonId)
	{
		switch ((MouseController.eMouseButtonId)buttonId)
		{
		case MouseController.eMouseButtonId.LEFT_CLICK:
			return Input.GetMouseButtonUp(0);
		case MouseController.eMouseButtonId.RIGHT_CLICK:
			return Input.GetMouseButtonUp(1);
		}
		
		return base.GetButtonUp(buttonId);
	}
	#endregion
	
	#region Public Functions
	public bool GetButton(MouseController.eMouseButtonId buttonId)
	{
		return GetButton((BaseController.eButtonId)buttonId);
	}

	public bool GetButtonDown(MouseController.eMouseButtonId buttonId)
	{
		return GetButtonDown((BaseController.eButtonId)buttonId);
	}
	
	public bool GetButtonUp(MouseController.eMouseButtonId buttonId)
	{
		return GetButtonUp((BaseController.eButtonId)buttonId);
	}
	
	public override string GetButtonName(BaseController.eButtonId buttonId)
	{
		// For debug purpose only.
		return ((MouseController.eMouseButtonId)buttonId).ToString();
	}
	#endregion
	
	#region Protected Functions
	#endregion
	
	#region Private Functions
	#endregion
}
