using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// NOTE ppoirier: Isn't recognized on Mac because of Xbox360MacController: both empty string joystick names.
[ControllerAttribute("SideWinder")]
public class GenericController : BaseController
{
	#region Members and Properties
	// constants
	
	// enums
	public enum eMicrosoftButtonId
	{
		NONE,
		A,
		B,
		X,
		Y,
		L1,
		R1,
		EMPTY_01,
		EMPTY_02,
		EMPTY_03,
		EMPTY_04,
		EMPTY_05,
		SELECT,
		START,
		C,
		Z
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
		AddJoystickButtonMap(eMicrosoftButtonId.A, 0);		
		AddJoystickButtonMap(eMicrosoftButtonId.B, 1);	
		AddJoystickButtonMap(eMicrosoftButtonId.X, 3);	
		AddJoystickButtonMap(eMicrosoftButtonId.Y, 4);
		AddJoystickButtonMap(eMicrosoftButtonId.C, 2);
		AddJoystickButtonMap(eMicrosoftButtonId.Z, 5);
		AddJoystickButtonMap(eMicrosoftButtonId.START, 9);
		AddJoystickButtonMap(eMicrosoftButtonId.SELECT, 8);
		AddJoystickButtonMap(eMicrosoftButtonId.L1, 6);
		AddJoystickButtonMap(eMicrosoftButtonId.R1, 7);

		AddJoystickAxisMap(eAxisId.D_PAD_HORIZONTAL, 1);
		AddJoystickAxisMap(eAxisId.D_PAD_VERTICAL, 2); // inverted
	}

	public override Vector2 GetDPad()
	{
		return InvertAxis(base.GetDPad(), false, true);
	}
	
	public override string GetButtonName(BaseController.eButtonId buttonId)
	{
		// For debug purpose only.
		return ((GenericController.eMicrosoftButtonId)buttonId).ToString();
	}
	#endregion
	
	#region Public Functions
	#endregion
	
	#region Protected Functions
	protected void AddJoystickButtonMap(GenericController.eMicrosoftButtonId buttonId, int inputKey)
	{
		AddJoystickButtonMap((BaseController.eButtonId)buttonId, inputKey);
	}
	#endregion
	
	#region Private Functions
	#endregion
}
