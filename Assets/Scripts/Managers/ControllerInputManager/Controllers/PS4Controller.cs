using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ControllerAttribute("Wireless Controller")]
public class PS4Controller : BaseController
{
	#region Members and Properties
	// constants
	
	// enums
	public enum ePS4ButtonId
	{
		NONE,
		X,
		CIRCLE,
		SQUARE,
		TRIANGLE,
		L1,
		R1,
		L2,
		R2,
		L3,
		R3,
		PS,
		SHARE,
		OPTIONS,
		TOUCH_PAD
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
		AddJoystickButtonMap(ePS4ButtonId.X, 1);		
		AddJoystickButtonMap(ePS4ButtonId.CIRCLE, 2);	
		AddJoystickButtonMap(ePS4ButtonId.SQUARE, 0);	
		AddJoystickButtonMap(ePS4ButtonId.TRIANGLE, 3);
		AddJoystickButtonMap(ePS4ButtonId.SHARE, 8);
		AddJoystickButtonMap(ePS4ButtonId.OPTIONS, 9);
		AddJoystickButtonMap(ePS4ButtonId.L1, 4);
		AddJoystickButtonMap(ePS4ButtonId.R1, 5);
		AddJoystickButtonMap(ePS4ButtonId.L2, 6);
		AddJoystickButtonMap(ePS4ButtonId.R2, 7);
		AddJoystickButtonMap(ePS4ButtonId.PS, 12);
		AddJoystickButtonMap(ePS4ButtonId.L3, 10);
		AddJoystickButtonMap(ePS4ButtonId.R3, 11);
		AddJoystickButtonMap(ePS4ButtonId.TOUCH_PAD, 13);
		
		AddJoystickAxisMap(eAxisId.LEFT_JOYSTICK_HORIZONTAL, 1);
		AddJoystickAxisMap(eAxisId.LEFT_JOYSTICK_VERTICAL, 2);
		AddJoystickAxisMap(eAxisId.RIGHT_JOYSTICK_HORIZONTAL, 3);
		AddJoystickAxisMap(eAxisId.D_PAD_HORIZONTAL, 7);
		AddJoystickAxisMap(eAxisId.D_PAD_VERTICAL, 8);
		
#if UNITY_STANDALONE_OSX
		AddJoystickAxisMap(eAxisId.RIGHT_JOYSTICK_VERTICAL, 4);
		AddJoystickAxisMap(eAxisId.L2, 5);
		AddJoystickAxisMap(eAxisId.R2, 6);
#else
		AddJoystickAxisMap(eAxisId.RIGHT_JOYSTICK_VERTICAL, 6);
		AddJoystickAxisMap(eAxisId.L2, 4);
		AddJoystickAxisMap(eAxisId.R2, 5);
#endif
	}
	
	public override Vector2 GetDPad()
	{
#if UNITY_STANDALONE_OSX
		return InvertAxis(base.GetDPad(), false, true);
#else
		return base.GetDPad();
#endif
	}

	public override Vector2 GetLeftJoystick()
	{
		return InvertAxis(base.GetLeftJoystick(), false, true);
	}
	
	public override Vector2 GetRightJoystick()
	{
		return InvertAxis(base.GetRightJoystick(), false, true);
	}
	#endregion
	
	#region Public Functions
	public bool GetButton(PS4Controller.ePS4ButtonId buttonId)
	{
		return GetButton((BaseController.eButtonId)buttonId);
	}

	public bool GetButtonDown(PS4Controller.ePS4ButtonId buttonId)
	{
		return GetButtonDown((BaseController.eButtonId)buttonId);
	}
	
	public bool GetButtonUp(PS4Controller.ePS4ButtonId buttonId)
	{
		return GetButtonUp((BaseController.eButtonId)buttonId);
	}
	
	public override string GetButtonName(BaseController.eButtonId buttonId)
	{
		// For debug purpose only.
		return ((PS4Controller.ePS4ButtonId)buttonId).ToString();
	}
	#endregion
	
	#region Protected Functions
	#endregion
	
	#region Private Functions	
	private void AddJoystickButtonMap(PS4Controller.ePS4ButtonId buttonId, int inputKey)
	{
		AddJoystickButtonMap((BaseController.eButtonId)buttonId, inputKey);
	}
	#endregion
}
