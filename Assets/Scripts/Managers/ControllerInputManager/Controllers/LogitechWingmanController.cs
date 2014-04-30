using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ControllerAttribute("WingMan")]
public class LogitechWingmanController : BaseController
{
	#region Members and Properties
	// constants
	
	// enums
	public enum eLogitechWingmanButtonId
	{
		NONE,
		A,
		B,
		X,
		Y,
		L,
		R,
		EMPTY_01,
		EMPTY_02,
		EMPTY_03,
		EMPTY_04,
		EMPTY_05,
		EMPTY_06,
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
		AddJoystickButtonMap(eLogitechWingmanButtonId.A, 0);		
		AddJoystickButtonMap(eLogitechWingmanButtonId.B, 1);	
		AddJoystickButtonMap(eLogitechWingmanButtonId.X, 3);	
		AddJoystickButtonMap(eLogitechWingmanButtonId.Y, 4);
		AddJoystickButtonMap(eLogitechWingmanButtonId.START, 8);
		AddJoystickButtonMap(eLogitechWingmanButtonId.L, 6);
		AddJoystickButtonMap(eLogitechWingmanButtonId.R, 7);
		AddJoystickButtonMap(eLogitechWingmanButtonId.C, 2);
		AddJoystickButtonMap(eLogitechWingmanButtonId.Z, 5);
		
		AddJoystickAxisMap(eAxisId.LEFT_JOYSTICK_HORIZONTAL, 1);
		AddJoystickAxisMap(eAxisId.LEFT_JOYSTICK_VERTICAL, 2);
		AddJoystickAxisMap(eAxisId.RIGHT_JOYSTICK_HORIZONTAL, 4);
		AddJoystickAxisMap(eAxisId.RIGHT_JOYSTICK_VERTICAL, 5);
		AddJoystickAxisMap(eAxisId.D_PAD_HORIZONTAL, 6);
		AddJoystickAxisMap(eAxisId.D_PAD_VERTICAL, 7);
		AddJoystickAxisMap(eAxisId.MOTION_HORIZONTAL, 3);
	}
	
	public override Vector2 GetDPad()
	{
		if (IsPlatformOSX())
		{
			return InvertAxis(base.GetDPad(), false, true);
		}

		return base.GetDPad();
	}

	public override Vector2 GetLeftJoystick()
	{
		return InvertAxis(base.GetLeftJoystick(), false, true);
	}
	
	public override Vector2 GetRightJoystick()
	{
		return InvertAxis(base.GetRightJoystick(), false, true);
	}
	
	public override Vector2 GetMotion()
	{
		return InvertAxis(base.GetMotion(), true, false);
	}
	#endregion
	
	#region Public Functions
	public bool GetButton(LogitechWingmanController.eLogitechWingmanButtonId buttonId)
	{
		return GetButton((BaseController.eButtonId)buttonId);
	}

	public bool GetButtonDown(LogitechWingmanController.eLogitechWingmanButtonId buttonId)
	{
		return GetButtonDown((BaseController.eButtonId)buttonId);
	}
	
	public bool GetButtonUp(LogitechWingmanController.eLogitechWingmanButtonId buttonId)
	{
		return GetButtonUp((BaseController.eButtonId)buttonId);
	}
	
	public override string GetButtonName(BaseController.eButtonId buttonId)
	{
		// For debug purpose only.
		return ((LogitechWingmanController.eLogitechWingmanButtonId)buttonId).ToString();
	}
	#endregion
	
	#region Protected Functions
	#endregion
	
	#region Private Functions	
	private void AddJoystickButtonMap(LogitechWingmanController.eLogitechWingmanButtonId buttonId, int inputKey)
	{
		AddJoystickButtonMap((BaseController.eButtonId)buttonId, inputKey);
	}
	#endregion
}
