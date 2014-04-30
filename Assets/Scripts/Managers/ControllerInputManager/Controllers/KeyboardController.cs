using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ControllerAttribute("Keyboard")]
public class KeyboardController : BaseController
{
	#region Members and Properties
	// constants
	
	// enums
	public enum eKeyboardButtonId
	{
		NONE,
		ACTION,
		BACK,
		EMPTY_01,
		EMPTY_02,
		L1,
		R1,
		L2,
		R2,
		EMPTY_07,
		EMPTY_08,
		EMPTY_09,
		EMPTY_10,
		START,
		EMPTY_11,
		UP,
		DOWN,
		LEFT,
		RIGHT
	}

	public enum eSimulatedAxis
	{
		UP_LEFT_JOYSTICK,
		DOWN_LEFT_JOYSTICK,
		LEFT_LEFT_JOYSTICK,
		RIGHT_LEFT_JOYSTICK,
		UP_RIGHT_JOYSTICK,
		DOWN_RIGHT_JOYSTICK,
		LEFT_RIGHT_JOYSTICK,
		RIGHT_RIGHT_JOYSTICK
	}
	
	// public
	
	// protected
	
	// private
	private Dictionary<eSimulatedAxis, string> m_SimulatedAxisMaps = new Dictionary<eSimulatedAxis, string>();
	
	// properties
	#endregion
	
	#region Class API
	public KeyboardController(Dictionary<eSimulatedAxis, string> axisMap, Dictionary<eKeyboardButtonId, string> buttonIds) : base()
	{
		m_SimulatedAxisMaps = axisMap;

		foreach (eKeyboardButtonId keyboardButtonId in buttonIds.Keys)
		{
			AddButtonMap(keyboardButtonId, buttonIds[keyboardButtonId]);
		}
	}
	#endregion

	#region IController Implementation
	public override void SetKeyMapping()
	{
		// Nothing to do here.
	}
	
	public override Vector2 GetDPad()
	{
		Vector2 axis = Vector2.zero;
		axis.x = GetButton(eKeyboardButtonId.RIGHT) ? 1.0f : (GetButton(eKeyboardButtonId.LEFT) ? -1.0f : 0.0f);
		axis.y = GetButton(eKeyboardButtonId.UP) ? 1.0f : (GetButton(eKeyboardButtonId.DOWN) ? -1.0f : 0.0f);
		
		return axis;
	}

	public override Vector2 GetLeftJoystick()
	{
		Vector2 axis = Vector2.zero;
		axis.x = GetButton(eSimulatedAxis.RIGHT_LEFT_JOYSTICK) ? 1.0f : (GetButton(eSimulatedAxis.LEFT_LEFT_JOYSTICK) ? -1.0f : 0.0f);
		axis.y = GetButton(eSimulatedAxis.UP_LEFT_JOYSTICK) ? 1.0f : (GetButton(eSimulatedAxis.DOWN_LEFT_JOYSTICK) ? -1.0f : 0.0f);
		
		return axis;
	}
	
	public override Vector2 GetRightJoystick()
	{
		Vector2 axis = Vector2.zero;
		axis.x = GetButton(eSimulatedAxis.RIGHT_RIGHT_JOYSTICK) ? 1.0f : (GetButton(eSimulatedAxis.LEFT_RIGHT_JOYSTICK) ? -1.0f : 0.0f);
		axis.y = GetButton(eSimulatedAxis.UP_RIGHT_JOYSTICK) ? 1.0f : (GetButton(eSimulatedAxis.DOWN_RIGHT_JOYSTICK) ? -1.0f : 0.0f);
		
		return axis;
	}
	
	public override float GetL2()
	{
		return GetButtonDown(eKeyboardButtonId.L2) ? 1.0f : -1.0f;
	}
	
	public override float GetR2()
	{
		return GetButtonDown(eKeyboardButtonId.R2) ? 1.0f : -1.0f;
	}
	#endregion
	
	#region Public Functions
	public bool GetButton(KeyboardController.eKeyboardButtonId buttonId)
	{
		return GetButton((BaseController.eButtonId)buttonId);
	}
	
	public bool GetButtonDown(KeyboardController.eKeyboardButtonId buttonId)
	{
		return GetButtonDown((BaseController.eButtonId)buttonId);
	}
	
	public bool GetButtonUp(KeyboardController.eKeyboardButtonId buttonId)
	{
		return GetButtonUp((BaseController.eButtonId)buttonId);
	}
	
	public bool GetButton(KeyboardController.eSimulatedAxis simulatedAxisId)
	{
		string keyCode = GetKeyCode(simulatedAxisId);
		if (!string.IsNullOrEmpty(keyCode))
		{
			return Input.GetKey(keyCode);
		}
		
		return false;
	}
	
	public bool GetButtonDown(KeyboardController.eSimulatedAxis simulatedAxisId)
	{
		string keyCode = GetKeyCode(simulatedAxisId);
		if (!string.IsNullOrEmpty(keyCode))
		{
			return Input.GetKeyDown(keyCode);
		}
		
		return false;
	}
	
	public override string GetButtonName(BaseController.eButtonId buttonId)
	{
		// For debug purpose only.
		return ((KeyboardController.eKeyboardButtonId)buttonId).ToString();
	}
	#endregion
	
	#region Protected Functions
	#endregion
	
	#region Private Functions	
	private void AddButtonMap(KeyboardController.eKeyboardButtonId buttonId, string inputKey)
	{
		AddButtonMap((BaseController.eButtonId)buttonId, inputKey);
	}

	private string GetKeyCode(KeyboardController.eSimulatedAxis simulatedAxisId)
	{
		string keyCode = "";
		if (m_SimulatedAxisMaps.ContainsKey(simulatedAxisId))
		{
			keyCode = m_SimulatedAxisMaps[simulatedAxisId];
		}
		
		return keyCode;
	}
	#endregion
}
