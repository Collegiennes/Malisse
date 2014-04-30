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
		CHARACTER_2_GRAB,
		START
	}
	
	// public
	
	// protected
	
	// private
	private eControllerId m_MouseControllerId = eControllerId.NONE;
	private eControllerId m_Keyboard1ControllerId = eControllerId.NONE;
	private eControllerId m_Keyboard2ControllerId = eControllerId.NONE;
	
	// properties
	public eControllerId MouseControllerId
	{
		get { return m_MouseControllerId; }
		set { m_MouseControllerId = value; }
	}
	
	public eControllerId Keyboard1ControllerId
	{
		get { return m_Keyboard1ControllerId; }
	}
	
	public eControllerId Keyboard2ControllerId
	{
		get { return m_Keyboard2ControllerId; }
		set { m_Keyboard2ControllerId = value; }
	}
	#endregion
	
	#region Unity API
	#endregion

	#region ControllerInputManager Implementation
	partial void PostInputDetection()
	{
		ControllerInputManager.Instance.AddKeyboard1Controller();
		ControllerInputManager.Instance.AddKeyboard2Controller();
		ControllerInputManager.Instance.AddMouseController();

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
	
	public void AddKeyboard1Controller()
	{
		Dictionary<KeyboardController.eSimulatedAxis, string> axis = new Dictionary<KeyboardController.eSimulatedAxis, string>();
		Dictionary<KeyboardController.eKeyboardButtonId, string> buttons = new Dictionary<KeyboardController.eKeyboardButtonId, string>();

		axis.Add(KeyboardController.eSimulatedAxis.UP_LEFT_JOYSTICK, "w");
		axis.Add(KeyboardController.eSimulatedAxis.LEFT_LEFT_JOYSTICK, "a");
		axis.Add(KeyboardController.eSimulatedAxis.DOWN_LEFT_JOYSTICK, "s");
		axis.Add(KeyboardController.eSimulatedAxis.RIGHT_LEFT_JOYSTICK, "d");
		
		axis.Add(KeyboardController.eSimulatedAxis.UP_RIGHT_JOYSTICK, "up");
		axis.Add(KeyboardController.eSimulatedAxis.LEFT_RIGHT_JOYSTICK, "left");
		axis.Add(KeyboardController.eSimulatedAxis.DOWN_RIGHT_JOYSTICK, "down");
		axis.Add(KeyboardController.eSimulatedAxis.RIGHT_RIGHT_JOYSTICK, "right");

		buttons.Add(KeyboardController.eKeyboardButtonId.ACTION, "space");
		buttons.Add(KeyboardController.eKeyboardButtonId.L1, "space");
		buttons.Add(KeyboardController.eKeyboardButtonId.R1, "return");

		KeyboardController controller = new KeyboardController(axis, buttons);
		m_Keyboard1ControllerId = ControllerInputManager.Instance.AddController(controller);
	}

	public void AddKeyboard2Controller()
	{
		Dictionary<KeyboardController.eSimulatedAxis, string> axis = new Dictionary<KeyboardController.eSimulatedAxis, string>();
		Dictionary<KeyboardController.eKeyboardButtonId, string> buttons = new Dictionary<KeyboardController.eKeyboardButtonId, string>();
		
		axis.Add(KeyboardController.eSimulatedAxis.UP_LEFT_JOYSTICK, "up");
		axis.Add(KeyboardController.eSimulatedAxis.LEFT_LEFT_JOYSTICK, "left");
		axis.Add(KeyboardController.eSimulatedAxis.DOWN_LEFT_JOYSTICK, "down");
		axis.Add(KeyboardController.eSimulatedAxis.RIGHT_LEFT_JOYSTICK, "right");
		
		buttons.Add(KeyboardController.eKeyboardButtonId.ACTION, "return");
		
		KeyboardController controller = new KeyboardController(axis, buttons);
		m_Keyboard2ControllerId = ControllerInputManager.Instance.AddController(controller);
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
		AddButtonAlias((BaseController.eButtonId)Xbox360Controller.eXbox360ButtonId.START, eButtonAliases.START.ToString());
	}
	#endregion
}
