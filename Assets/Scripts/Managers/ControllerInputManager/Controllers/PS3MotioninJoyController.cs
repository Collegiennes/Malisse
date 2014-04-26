using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ControllerAttribute("MotioninJoy")]
public class PS3MotioninJoyController : PS3Controller
{
	#region Members and Properties
	// constants
	
	// enums
	
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
		AddJoystickButtonMap(PS3Controller.ePS3ButtonId.X, 2);		
		AddJoystickButtonMap(PS3Controller.ePS3ButtonId.CIRCLE, 1);	
		AddJoystickButtonMap(PS3Controller.ePS3ButtonId.SQUARE, 3);	
		AddJoystickButtonMap(PS3Controller.ePS3ButtonId.TRIANGLE, 0);
		AddJoystickButtonMap(PS3Controller.ePS3ButtonId.SELECT, 8);
		AddJoystickButtonMap(PS3Controller.ePS3ButtonId.START, 11);
		AddJoystickButtonMap(PS3Controller.ePS3ButtonId.L1, 4);
		AddJoystickButtonMap(PS3Controller.ePS3ButtonId.R1, 5);
		AddJoystickButtonMap(PS3Controller.ePS3ButtonId.L2, 6);
		AddJoystickButtonMap(PS3Controller.ePS3ButtonId.R2, 7);
		AddJoystickButtonMap(PS3Controller.ePS3ButtonId.PS, 12);
		AddJoystickButtonMap(PS3Controller.ePS3ButtonId.L3, 9);
		AddJoystickButtonMap(PS3Controller.ePS3ButtonId.R3, 10);

		AddJoystickAxisMap(eAxisId.LEFT_JOYSTICK_HORIZONTAL, 1);
		AddJoystickAxisMap(eAxisId.LEFT_JOYSTICK_VERTICAL, 2);
		AddJoystickAxisMap(eAxisId.RIGHT_JOYSTICK_HORIZONTAL, 3);
		AddJoystickAxisMap(eAxisId.RIGHT_JOYSTICK_VERTICAL, 6);
		AddJoystickAxisMap(eAxisId.MOTION_HORIZONTAL, 4);
		AddJoystickAxisMap(eAxisId.MOTION_VERTICAL, 5);
	}
	
	public override Vector2 GetDPad()
	{
		// TODO ppoirier: Cannot find d-pad keys/axes.
		return Vector2.zero;
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
		return InvertAxis(base.GetMotion(), true, true);
	}
	
	public override float GetL2()
	{
		return GetButtonDown(PS3Controller.ePS3ButtonId.L2) ? 1.0f : -1.0f;
	}
	
	public override float GetR2()
	{
		return GetButtonDown(PS3Controller.ePS3ButtonId.R2) ? 1.0f : -1.0f;
	}
	#endregion
	
	#region Public Functions
	#endregion
	
	#region Protected Functions
	#endregion
	
	#region Private Functions
	#endregion
}
