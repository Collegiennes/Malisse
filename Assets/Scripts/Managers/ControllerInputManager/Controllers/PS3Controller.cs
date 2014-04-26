using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ControllerAttribute("PLAYSTATION(R)3")]
public class PS3Controller : BaseController
{
	#region Members and Properties
	// constants
	
	// enums
	public enum ePS3ButtonId
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
		SELECT,
		START,
		EMPTY_01,
		UP,
		DOWN,
		LEFT,
		RIGHT
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
		AddJoystickButtonMap(ePS3ButtonId.X, 14);		
		AddJoystickButtonMap(ePS3ButtonId.CIRCLE, 13);	
		AddJoystickButtonMap(ePS3ButtonId.SQUARE, 15);	
		AddJoystickButtonMap(ePS3ButtonId.TRIANGLE, 12);
		AddJoystickButtonMap(ePS3ButtonId.SELECT, 0);
		AddJoystickButtonMap(ePS3ButtonId.START, 3);
		AddJoystickButtonMap(ePS3ButtonId.L1, 10);
		AddJoystickButtonMap(ePS3ButtonId.R1, 11);
		AddJoystickButtonMap(ePS3ButtonId.L2, 8);
		AddJoystickButtonMap(ePS3ButtonId.R2, 9);
		AddJoystickButtonMap(ePS3ButtonId.PS, 16);
		AddJoystickButtonMap(ePS3ButtonId.L3, 1);
		AddJoystickButtonMap(ePS3ButtonId.R3, 2);
		AddJoystickButtonMap(ePS3ButtonId.UP, 4);
		AddJoystickButtonMap(ePS3ButtonId.DOWN, 6);
		AddJoystickButtonMap(ePS3ButtonId.LEFT, 7);
		AddJoystickButtonMap(ePS3ButtonId.RIGHT, 5);

		AddJoystickAxisMap(eAxisId.LEFT_JOYSTICK_HORIZONTAL, 1);
		AddJoystickAxisMap(eAxisId.LEFT_JOYSTICK_VERTICAL, 2);
		AddJoystickAxisMap(eAxisId.RIGHT_JOYSTICK_HORIZONTAL, 3);
		AddJoystickAxisMap(eAxisId.RIGHT_JOYSTICK_VERTICAL, 4);
	}

	public override Vector2 GetDPad()
	{
		Vector2 axis = Vector2.zero;
		axis.x = GetButton(ePS3ButtonId.RIGHT) ? 1.0f : (GetButton(ePS3ButtonId.LEFT) ? -1.0f : 0.0f);
		axis.y = GetButton(ePS3ButtonId.UP) ? 1.0f : (GetButton(ePS3ButtonId.DOWN) ? -1.0f : 0.0f);

		return axis;
	}
	
	public override Vector2 GetLeftJoystick()
	{
		return InvertAxis(base.GetLeftJoystick(), false, true);
	}
	
	public override Vector2 GetRightJoystick()
	{
		return InvertAxis(base.GetRightJoystick(), false, true);
	}
	
	public override float GetL2()
	{
		return GetButtonDown(ePS3ButtonId.L2) ? 1.0f : -1.0f;
	}
	
	public override float GetR2()
	{
		return GetButtonDown(ePS3ButtonId.R2) ? 1.0f : -1.0f;
	}
	#endregion
	
	#region Public Functions
	public virtual bool GetButton(PS3Controller.ePS3ButtonId buttonId)
	{
		return GetButton((BaseController.eButtonId)buttonId);
	}

	public virtual bool GetButtonDown(PS3Controller.ePS3ButtonId buttonId)
	{
		return GetButtonDown((BaseController.eButtonId)buttonId);
	}
	
	public virtual bool GetButtonUp(PS3Controller.ePS3ButtonId buttonId)
	{
		return GetButtonUp((BaseController.eButtonId)buttonId);
	}
	
	public override string GetButtonName(BaseController.eButtonId buttonId)
	{
		// For debug purpose only.
		return ((PS3Controller.ePS3ButtonId)buttonId).ToString();
	}
	#endregion
	
	#region Protected Functions
	protected void AddJoystickButtonMap(PS3Controller.ePS3ButtonId buttonId, int inputKey)
	{
		AddJoystickButtonMap((BaseController.eButtonId)buttonId, inputKey);
	}
	#endregion
	
	#region Private Functions	
	#endregion
}
