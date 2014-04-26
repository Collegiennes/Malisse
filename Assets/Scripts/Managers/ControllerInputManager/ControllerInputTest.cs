using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ControllerInputTest : MonoBehaviour 
{
	#region Members and Properties
	// constants
	
	// enums
	
	// public
	public bool m_IsActive = true;
	public bool m_OnlyKnownControllers = true;
	public bool m_TestContinuousButton = false;
	public bool m_TestMotion = true;
	
	// protected
	
	// private
	
	// properties
	#endregion
	
	#region Unity API
	private void Update()
	{
		if (m_IsActive)
		{
			if (!m_OnlyKnownControllers)
			{
				DebugButtonsAndAxes();
			}
			else
			{
				if (m_TestContinuousButton)
				{
					TestButton();
				}
				else
				{
					TestButtonDown();
					TestButtonUp();
				}

				TestLeftJoystick();
				TestRightJoystick();
				TestDPad();
				TestL2();
				TestR2();

				if (m_TestMotion)
				{
					TestMotion();
				}
			}
		}
	}
	#endregion
	
	#region Public Functions
	#endregion
	
	#region Protected Functions
	#endregion
	
	#region Private Functions
	private void TestButton()
	{
		Dictionary<ControllerInputManager.eControllerId, BaseController.eButtonId> buttons = ControllerInputManager.Instance.GetButton();
		foreach (ControllerInputManager.eControllerId controllerId in buttons.Keys)
		{
			BaseController controller = ControllerInputManager.Instance.GetController(controllerId);
			BaseController.eButtonId buttonId = buttons[controllerId];
			
			Debug.Log(controller.ControllerId.ToString() + " (" + controller.GetType().Name + "), Button: " + controller.GetButtonName(buttonId) + " (" + buttonId.ToString() + ")");
		}
	}

	private void TestButtonDown()
	{
		Dictionary<ControllerInputManager.eControllerId, BaseController.eButtonId> buttons = ControllerInputManager.Instance.GetButtonDown();
		foreach (ControllerInputManager.eControllerId controllerId in buttons.Keys)
		{
			BaseController controller = ControllerInputManager.Instance.GetController(controllerId);
			BaseController.eButtonId buttonId = buttons[controllerId];

			Debug.Log(controller.ControllerId.ToString() + " (" + controller.GetType().Name + "), Button Down: " + controller.GetButtonName(buttonId) + " (" + buttonId.ToString() + ")");
		}
	}

	private void TestButtonUp()
	{
		Dictionary<ControllerInputManager.eControllerId, BaseController.eButtonId> buttons = ControllerInputManager.Instance.GetButtonUp();
		foreach (ControllerInputManager.eControllerId controllerId in buttons.Keys)
		{
			BaseController controller = ControllerInputManager.Instance.GetController(controllerId);
			BaseController.eButtonId buttonId = buttons[controllerId];
			
			Debug.Log(controller.ControllerId.ToString() + " (" + controller.GetType().Name + "), Button Up: " + controller.GetButtonName(buttonId) + " (" + buttonId.ToString() + ")");
		}
	}

	private void TestLeftJoystick()
	{
		Dictionary<ControllerInputManager.eControllerId, Vector2> axises = ControllerInputManager.Instance.GetLeftJoystick();
		foreach (ControllerInputManager.eControllerId controllerId in axises.Keys)
		{
			BaseController controller = ControllerInputManager.Instance.GetController(controllerId);
			Vector2 axis = axises[controllerId];
			
			Debug.Log(controller.ControllerId.ToString() + " (" + controller.GetType().Name + "), Left Joystick: " + axis);
		}
	}
	
	private void TestRightJoystick()
	{
		Dictionary<ControllerInputManager.eControllerId, Vector2> axises = ControllerInputManager.Instance.GetRightJoystick();
		foreach (ControllerInputManager.eControllerId controllerId in axises.Keys)
		{
			BaseController controller = ControllerInputManager.Instance.GetController(controllerId);
			Vector2 axis = axises[controllerId];
			
			Debug.Log(controller.ControllerId.ToString() + " (" + controller.GetType().Name + "), Right Joystick: " + axis);
		}
	}
	
	private void TestDPad()
	{
		Dictionary<ControllerInputManager.eControllerId, Vector2> axises = ControllerInputManager.Instance.GetDPad();
		foreach (ControllerInputManager.eControllerId controllerId in axises.Keys)
		{
			BaseController controller = ControllerInputManager.Instance.GetController(controllerId);
			Vector2 axis = axises[controllerId];
			
			Debug.Log(controller.ControllerId.ToString() + " (" + controller.GetType().Name + "), D-Pad: " + axis);
		}
	}
	
	private void TestL2()
	{
		Dictionary<ControllerInputManager.eControllerId, float> buttons = ControllerInputManager.Instance.GetL2();
		foreach (ControllerInputManager.eControllerId controllerId in buttons.Keys)
		{
			BaseController controller = ControllerInputManager.Instance.GetController(controllerId);
			float button = buttons[controllerId];
			
			Debug.Log(controller.ControllerId.ToString() + " (" + controller.GetType().Name + "), L2: " + button);
		}
	}
	
	private void TestR2()
	{
		Dictionary<ControllerInputManager.eControllerId, float> buttons = ControllerInputManager.Instance.GetR2();
		foreach (ControllerInputManager.eControllerId controllerId in buttons.Keys)
		{
			BaseController controller = ControllerInputManager.Instance.GetController(controllerId);
			float button = buttons[controllerId];
			
			Debug.Log(controller.ControllerId.ToString() + " (" + controller.GetType().Name + "), R2: " + button);
		}
	}

	private void TestMotion()
	{
		Dictionary<ControllerInputManager.eControllerId, Vector2> motions = ControllerInputManager.Instance.GetMotion();
		foreach (ControllerInputManager.eControllerId controllerId in motions.Keys)
		{
			BaseController controller = ControllerInputManager.Instance.GetController(controllerId);
			Vector2 motion = motions[controllerId];
			
			Debug.Log(controller.ControllerId.ToString() + " (" + controller.GetType().Name + "), Motion: " + motion);
		}
	}

	public void DebugButtonsAndAxes()
	{
		// Debug buttons and axises.
		for (int joystickIndex = 1; joystickIndex <= ControllerInputManager.NB_CONTROLLERS; ++joystickIndex)
		{
			// There are 20 buttons detected by Unity.
			for (int buttonId = 0; buttonId < 20; ++buttonId)
			{
				string keyCode = string.Format("joystick {0} button {1}", joystickIndex, buttonId);
				if (Input.GetKeyDown(keyCode))
				{
					Debug.Log(string.Format("Controller: {0}, Button: {1}", joystickIndex, buttonId));
				}
			}
			
			for (int axisIndex = 1; axisIndex <= ControllerInputManager.NB_AXIS_PER_CONTROLLER; ++axisIndex)
			{
				float axisValue = Input.GetAxis(string.Format("Joystick{0}_Axis{1}", joystickIndex, axisIndex));
				if (axisValue != 0.0f && axisValue != -1.0f)
				{
					Debug.Log(string.Format("Controller: {0}, Axis: {1}", joystickIndex, axisIndex) + ", Value: " + axisValue);
				}
			}
		}
	}
	#endregion
}
