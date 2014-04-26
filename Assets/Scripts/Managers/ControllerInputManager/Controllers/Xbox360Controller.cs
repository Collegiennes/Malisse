using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ControllerAttribute("360")]
public class Xbox360Controller : BaseController
{
	#region Members and Properties
	// constants
	
	// enums
	public enum eXbox360ButtonId
	{
		NONE,
		A,
		B,
		X,
		Y,
		L1,
		R1,
		L2,
		R2,
		L3,
		R3,
		XBOX,
		BACK,
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
#if UNITY_STANDALONE_OSX
		AddJoystickAxisMap(eAxisId.LEFT_JOYSTICK_HORIZONTAL, 1);
		AddJoystickAxisMap(eAxisId.LEFT_JOYSTICK_VERTICAL, 2); // inverted
		AddJoystickAxisMap(eAxisId.RIGHT_JOYSTICK_HORIZONTAL, 3);
		AddJoystickAxisMap(eAxisId.RIGHT_JOYSTICK_VERTICAL, 4); // inverted
		AddJoystickAxisMap(eAxisId.L2, 5);
		AddJoystickAxisMap(eAxisId.R2, 6);
		
		AddJoystickButtonMap(eXbox360ButtonId.A, 16);		
		AddJoystickButtonMap(eXbox360ButtonId.B, 17);	
		AddJoystickButtonMap(eXbox360ButtonId.X, 18);	
		AddJoystickButtonMap(eXbox360ButtonId.Y, 19);
		AddJoystickButtonMap(eXbox360ButtonId.BACK, 10);
		AddJoystickButtonMap(eXbox360ButtonId.START, 9);
		AddJoystickButtonMap(eXbox360ButtonId.L1, 13);
		AddJoystickButtonMap(eXbox360ButtonId.R1, 14);
		AddJoystickButtonMap(eXbox360ButtonId.L3, 11);
		AddJoystickButtonMap(eXbox360ButtonId.R3, 12);
		
		AddJoystickButtonMap(eXbox360ButtonId.UP, 5);
		AddJoystickButtonMap(eXbox360ButtonId.DOWN, 6);
		AddJoystickButtonMap(eXbox360ButtonId.LEFT, 7);
		AddJoystickButtonMap(eXbox360ButtonId.RIGHT, 8);
		AddJoystickButtonMap(eXbox360ButtonId.XBOX, 15);
#else
		AddJoystickAxisMap(eAxisId.LEFT_JOYSTICK_HORIZONTAL, 1);
		AddJoystickAxisMap(eAxisId.LEFT_JOYSTICK_VERTICAL, 2); // inverted
		AddJoystickAxisMap(eAxisId.RIGHT_JOYSTICK_HORIZONTAL, 4);
		AddJoystickAxisMap(eAxisId.RIGHT_JOYSTICK_VERTICAL, 5); // inverted
		AddJoystickAxisMap(eAxisId.D_PAD_HORIZONTAL, 6);
		AddJoystickAxisMap(eAxisId.D_PAD_VERTICAL, 7);
		AddJoystickAxisMap(eAxisId.L2, 3); // [0, 1]
		AddJoystickAxisMap(eAxisId.R2, 3); // [0, -1]

		AddJoystickButtonMap(eXbox360ButtonId.A, 0);		
		AddJoystickButtonMap(eXbox360ButtonId.B, 1);	
		AddJoystickButtonMap(eXbox360ButtonId.X, 2);	
		AddJoystickButtonMap(eXbox360ButtonId.Y, 3);
		AddJoystickButtonMap(eXbox360ButtonId.BACK, 6);
		AddJoystickButtonMap(eXbox360ButtonId.START, 7);
		AddJoystickButtonMap(eXbox360ButtonId.L1, 4);
		AddJoystickButtonMap(eXbox360ButtonId.R1, 5);
		AddJoystickButtonMap(eXbox360ButtonId.L3, 8);
		AddJoystickButtonMap(eXbox360ButtonId.R3, 9);
#endif
	}
	#endregion
	
	#region Public Functions
	public override Vector2 GetLeftJoystick()
	{ 
		return InvertAxis(base.GetLeftJoystick(), false, true);
	}
	
	public override Vector2 GetRightJoystick()
	{
		return InvertAxis(base.GetRightJoystick(), false, true);
	}
	
	public override Vector2 GetDPad()
	{
#if UNITY_STANDALONE_OSX
		Vector2 axis = Vector2.zero;
		axis.x = GetButton(eXbox360ButtonId.RIGHT) ? 1.0f : (GetButton(eXbox360ButtonId.LEFT) ? -1.0f : 0.0f);
		axis.y = GetButton(eXbox360ButtonId.UP) ? 1.0f : (GetButton(eXbox360ButtonId.DOWN) ? -1.0f : 0.0f);
		
		return axis;
#else
		return base.GetDPad();
#endif
	}
	
	public override float GetL2()
	{
#if UNITY_STANDALONE_OSX
		return base.GetL2();
#else
		return Mathf.Lerp(-1.0f, 1.0f, GetAxis(eAxisId.L2, 0.0f));
#endif
	}
	
	public override float GetR2()
	{
#if UNITY_STANDALONE_OSX
		return base.GetR2();
#else
		return Mathf.Lerp(-1.0f, 1.0f, -GetAxis(eAxisId.R2, 0.0f));
#endif
	}

	public virtual bool GetButton(Xbox360Controller.eXbox360ButtonId buttonId)
	{
		return GetButton((BaseController.eButtonId)buttonId);
	}

	public virtual bool GetButtonDown(Xbox360Controller.eXbox360ButtonId buttonId)
	{
		return GetButtonDown((BaseController.eButtonId)buttonId);
	}
	
	public virtual bool GetButtonUp(Xbox360Controller.eXbox360ButtonId buttonId)
	{
		return GetButtonUp((BaseController.eButtonId)buttonId);
	}
	
	public override string GetButtonName(BaseController.eButtonId buttonId)
	{
		// For debug purpose only.
		return ((Xbox360Controller.eXbox360ButtonId)buttonId).ToString();
	}
	#endregion
	
	#region Protected Functions
	protected void AddJoystickButtonMap(Xbox360Controller.eXbox360ButtonId buttonId, int inputKey)
	{
		AddJoystickButtonMap((BaseController.eButtonId)buttonId, inputKey);
	}
	#endregion
	
	#region Private Functions
	#endregion
}
