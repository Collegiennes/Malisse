using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ControllerAttribute("2Axes 11Keys")]
public class SNESController : BaseController
{
	#region Members and Properties
	// constants
	
	// enums
	public enum eSNESButtonId
	{
		NONE,
		B,
		A,
		Y,
		X,
		L,
		R,
		EMPTY_01,
		EMPTY_02,
		EMPTY_03,
		EMPTY_04,
		EMPTY_05,
		SELECT,
		START
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
		AddJoystickButtonMap(eSNESButtonId.B, 2);		
		AddJoystickButtonMap(eSNESButtonId.A, 1);	
		AddJoystickButtonMap(eSNESButtonId.Y, 3);	
		AddJoystickButtonMap(eSNESButtonId.X, 0);
		AddJoystickButtonMap(eSNESButtonId.SELECT, 8);
		AddJoystickButtonMap(eSNESButtonId.START, 9);
		AddJoystickButtonMap(eSNESButtonId.L, 4);
		AddJoystickButtonMap(eSNESButtonId.R, 5);
		
		AddJoystickAxisMap(eAxisId.D_PAD_HORIZONTAL, 1);
		AddJoystickAxisMap(eAxisId.D_PAD_VERTICAL, 2);
	}
	
	public override Vector2 GetDPad()
	{
		return InvertAxis(base.GetDPad(), false, true);
	}
	#endregion

	#region Public Functions
	public bool GetButton(SNESController.eSNESButtonId buttonId)
	{
		return GetButton((BaseController.eButtonId)buttonId);
	}

	public bool GetButtonDown(SNESController.eSNESButtonId buttonId)
	{
		return GetButtonDown((BaseController.eButtonId)buttonId);
	}
	
	public bool GetButtonUp(SNESController.eSNESButtonId buttonId)
	{
		return GetButtonUp((BaseController.eButtonId)buttonId);
	}
	
	public override string GetButtonName(BaseController.eButtonId buttonId)
	{
		// For debug purpose only.
		return ((SNESController.eSNESButtonId)buttonId).ToString();
	}
	#endregion
	
	#region Protected Functions
	#endregion
	
	#region Private Functions
	private void AddJoystickButtonMap(SNESController.eSNESButtonId buttonId, int inputKey)
	{
		AddJoystickButtonMap((BaseController.eButtonId)buttonId, inputKey);
	}
	#endregion
}
