using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class ControllerInputManager 
{
	#region Members and Properties
	// constants
	private readonly List<Type> SUPPORTED_CONTROLLERS = new List<Type>() {	typeof(LogitechWingmanController), 
																			typeof(PS3Controller), typeof(PS3MotioninJoyController), 
																			typeof(PS4Controller), 
																			typeof(SNESController), 
																			typeof(GenericController),
																			typeof(Xbox360Controller), typeof(Xbox360MacController) };
	public const int NB_CONTROLLERS = 6;
	public const int NB_AXIS_PER_CONTROLLER = 8;
	
	// enums
	public enum eControllerId
	{
		NONE,
		CONTROLLER_01,
		CONTROLLER_02,
		CONTROLLER_03,
		CONTROLLER_04,
		CONTROLLER_05,
		CONTROLLER_06,
		CONTROLLER_07,
		CONTROLLER_08,
		CONTROLLER_09,
		CONTROLLER_10,
		CONTROLLER_11
	}

	// public
	
	// protected

	// private
	private static ControllerInputManager m_Instance = null;
	private Dictionary<ControllerInputManager.eControllerId, BaseController> m_Controllers = new Dictionary<ControllerInputManager.eControllerId, BaseController>();
	private Dictionary<string, BaseController.eButtonId> m_ButtonAliases = new Dictionary<string, BaseController.eButtonId>();		
	
	// properties
	public static ControllerInputManager Instance
	{
		get
		{
			if (m_Instance == null)
			{
				m_Instance = new ControllerInputManager();
			}

			return m_Instance;
		}
	}

	public ICollection<ControllerInputManager.eControllerId> ControllerIds
	{
		get { return m_Controllers.Keys; }
	}
	#endregion
	
	#region Class API
	private ControllerInputManager()
	{
	}

	~ControllerInputManager()
	{
		m_Instance = null;
	}
	#endregion

	#region Partial Functions
	partial void PostInputDetection();
	#endregion
	
	#region Public Functions
	public void Init()
	{
		InitInputDetection();
		PostInputDetection();
	}

	public void AddButtonAlias(BaseController.eButtonId buttonId, string alias)
	{
		if (!m_ButtonAliases.ContainsKey(alias))
		{
			m_ButtonAliases.Add(alias, buttonId);
		}
	}

	public BaseController.eButtonId GetButtonAlias(string alias)
	{
		if (m_ButtonAliases.ContainsKey(alias))
		{
			return m_ButtonAliases[alias];
		}

		return BaseController.eButtonId.NONE;
	}

	public BaseController GetController(ControllerInputManager.eControllerId controllerId)
	{
		if (m_Controllers.ContainsKey(controllerId))
		{
			return m_Controllers[controllerId];
		}

		return null;
	}
	
	public ControllerInputManager.eControllerId AddController(BaseController controller)
	{
		// Save the detected controller.
		if (controller != null && (m_Controllers.Count + 1) < System.Enum.GetNames(typeof(eControllerId)).Length)
		{
			// First item starts at 1.
			eControllerId controllerId = (eControllerId)(m_Controllers.Count + 1);
			
			controller.SetKeyMapping();
			controller.ControllerId = controllerId;
			m_Controllers.Add(controllerId, controller);
			
			Debug.Log(controllerId.ToString() + " uses " + controller.GetType().Name + ": " + GetControllerDetectionName(controller.GetType()));

			return controllerId;
		}

		return eControllerId.NONE;
	}
	
	public Dictionary<ControllerInputManager.eControllerId, Vector2> GetDPad()
	{
		Dictionary<eControllerId, Vector2> controllers = new Dictionary<eControllerId, Vector2>();
		foreach (eControllerId controllerId in m_Controllers.Keys)
		{
			Vector2 axis = GetDPad(controllerId);
			if (axis != Vector2.zero)
			{
				controllers.Add(controllerId, axis);
			}
		}
		
		return controllers;
	}
	
	public Vector2 GetDPad(ControllerInputManager.eControllerId controllerId)
	{
		if (m_Controllers.ContainsKey(controllerId))
		{
			return m_Controllers[controllerId].GetDPad();
		}
		
		return Vector2.zero;
	}
	
	public Dictionary<ControllerInputManager.eControllerId, Vector2> GetLeftJoystick()
	{
		Dictionary<eControllerId, Vector2> controllers = new Dictionary<eControllerId, Vector2>();
		foreach (eControllerId controllerId in m_Controllers.Keys)
		{
			Vector2 axis = GetLeftJoystick(controllerId);
			if (axis != Vector2.zero)
			{
				controllers.Add(controllerId, axis);
			}
		}
		
		return controllers;
	}
	
	public Dictionary<ControllerInputManager.eControllerId, Vector2> GetRightJoystick()
	{
		Dictionary<eControllerId, Vector2> controllers = new Dictionary<eControllerId, Vector2>();
		foreach (eControllerId controllerId in m_Controllers.Keys)
		{
			Vector2 axis = GetRightJoystick(controllerId);
			if (axis != Vector2.zero)
			{
				controllers.Add(controllerId, axis);
			}
		}
		
		return controllers;
	}

	public Vector2 GetLeftJoystick(ControllerInputManager.eControllerId controllerId)
	{
		if (m_Controllers.ContainsKey(controllerId))
		{
			return m_Controllers[controllerId].GetLeftJoystick();
		}

		return Vector2.zero;
	}
	
	public Vector2 GetRightJoystick(ControllerInputManager.eControllerId controllerId)
	{
		if (m_Controllers.ContainsKey(controllerId))
		{
			return m_Controllers[controllerId].GetRightJoystick();
		}
		
		return Vector2.zero;
	}
	
	public Dictionary<eControllerId, float> GetL2()
	{
		Dictionary<eControllerId, float> controllers = new Dictionary<eControllerId, float>();
		foreach (eControllerId controllerId in m_Controllers.Keys)
		{
			float button = GetL2(controllerId);
			// NOTE ppoirier: This is a weird check that we could maybe manage in game instead.
			// 0.0f: When we lose focus, it keeps sending the 0.0f value.
			// -1.0f: When ingame, the idle position is -1.0f;
			if (button != 0.0f && button != -1.0f)
			{
				controllers.Add(controllerId, button);
			}
		}
		
		return controllers;
	}
	
	public Dictionary<eControllerId, float> GetR2()
	{
		Dictionary<eControllerId, float> controllers = new Dictionary<eControllerId, float>();
		foreach (eControllerId controllerId in m_Controllers.Keys)
		{
			float button = GetR2(controllerId);
			// NOTE ppoirier: This is a weird check that we could maybe manage in game instead.
			// 0.0f: When we lose focus, it keeps sending the 0.0f value.
			// -1.0f: When ingame, the idle position is -1.0f;
			if (button != 0.0f && button != -1.0f)
			{
				controllers.Add(controllerId, button);
			}
		}
		
		return controllers;
	}

	public float GetL2(ControllerInputManager.eControllerId controllerId)
	{
		if (m_Controllers.ContainsKey(controllerId))
		{
			return m_Controllers[controllerId].GetL2();
		}
		
		return -1.0f;
	}
	
	public float GetR2(ControllerInputManager.eControllerId controllerId)
	{
		if (m_Controllers.ContainsKey(controllerId))
		{
			return m_Controllers[controllerId].GetR2();
		}
		
		return -1.0f;
	}
	
	public Dictionary<ControllerInputManager.eControllerId, Vector2> GetMotion()
	{
		Dictionary<eControllerId, Vector2> controllers = new Dictionary<eControllerId, Vector2>();
		foreach (eControllerId controllerId in m_Controllers.Keys)
		{
			Vector2 motion = GetMotion(controllerId);
			if (motion != Vector2.zero)
			{
				controllers.Add(controllerId, motion);
			}
		}
		
		return controllers;
	}
	
	public Vector2 GetMotion(ControllerInputManager.eControllerId controllerId)
	{
		if (m_Controllers.ContainsKey(controllerId))
		{
			return m_Controllers[controllerId].GetMotion();
		}
		
		return Vector2.zero;
	}
	
	public Dictionary<eControllerId, BaseController.eButtonId> GetButton()
	{
		Dictionary<eControllerId, BaseController.eButtonId> buttons = new Dictionary<eControllerId, BaseController.eButtonId>();
		foreach (eControllerId controllerId in m_Controllers.Keys)
		{
			foreach (BaseController.eButtonId buttonId in System.Enum.GetValues(typeof(BaseController.eButtonId)))
			{
				// NOTE ppoirier: I had to do the containskey check for some reason.
				if (!buttons.ContainsKey(controllerId) && GetButton(controllerId, buttonId))
				{
					buttons.Add(controllerId, buttonId);
				}
			}
		}
		
		return buttons;
	}

	public Dictionary<eControllerId, BaseController.eButtonId> GetButtonDown()
	{
		Dictionary<eControllerId, BaseController.eButtonId> buttons = new Dictionary<eControllerId, BaseController.eButtonId>();
		foreach (eControllerId controllerId in m_Controllers.Keys)
		{
			foreach (BaseController.eButtonId buttonId in System.Enum.GetValues(typeof(BaseController.eButtonId)))
			{
				// NOTE ppoirier: I had to do the containskey check for some reason.
				if (!buttons.ContainsKey(controllerId) && GetButtonDown(controllerId, buttonId))
				{
					buttons.Add(controllerId, buttonId);
				}
			}
		}
		
		return buttons;
	}
	
	public Dictionary<eControllerId, BaseController.eButtonId> GetButtonUp()
	{
		Dictionary<eControllerId, BaseController.eButtonId> buttons = new Dictionary<eControllerId, BaseController.eButtonId>();
		foreach (eControllerId controllerId in m_Controllers.Keys)
		{
			foreach (BaseController.eButtonId buttonId in System.Enum.GetValues(typeof(BaseController.eButtonId)))
			{
				// NOTE ppoirier: I had to do the containskey check for some reason.
				if (!buttons.ContainsKey(controllerId) && GetButtonUp(controllerId, buttonId))
				{
					buttons.Add(controllerId, buttonId);
				}
			}
		}
		
		return buttons;
	}
	
	public List<eControllerId> GetButton(BaseController.eButtonId buttonId)
	{
		List<eControllerId> controllers = new List<eControllerId>();
		foreach (eControllerId controllerId in m_Controllers.Keys)
		{
			// NOTE ppoirier: I had to do the Contains check for some reason.
			if (!controllers.Contains(controllerId) && GetButton(controllerId, buttonId))
			{
				controllers.Add(controllerId);
			}
		}
		
		return controllers;
	}
	
	public List<eControllerId> GetButtonDown(BaseController.eButtonId buttonId)
	{
		List<eControllerId> controllers = new List<eControllerId>();
		foreach (eControllerId controllerId in m_Controllers.Keys)
		{
			// NOTE ppoirier: I had to do the Contains check for some reason.
			if (!controllers.Contains(controllerId) && GetButtonDown(controllerId, buttonId))
			{
				controllers.Add(controllerId);
			}
		}
		
		return controllers;
	}
	
	public List<eControllerId> GetButtonUp(BaseController.eButtonId buttonId)
	{
		List<eControllerId> controllers = new List<eControllerId>();
		foreach (eControllerId controllerId in m_Controllers.Keys)
		{
			// NOTE ppoirier: I had to do the Contains check for some reason.
			if (!controllers.Contains(controllerId) && GetButtonUp(controllerId, buttonId))
			{
				controllers.Add(controllerId);
			}
		}
		
		return controllers;
	}
	
	public List<eControllerId> GetButton(string alias)
	{
		List<eControllerId> controllers = new List<eControllerId>();
		foreach (eControllerId controllerId in m_Controllers.Keys)
		{
			// NOTE ppoirier: I had to do the Contains check for some reason.
			if (!controllers.Contains(controllerId) && GetButton(controllerId, alias))
			{
				controllers.Add(controllerId);
			}
		}
		
		return controllers;
	}
	
	public List<eControllerId> GetButtonDown(string alias)
	{
		List<eControllerId> controllers = new List<eControllerId>();
		foreach (eControllerId controllerId in m_Controllers.Keys)
		{
			// NOTE ppoirier: I had to do the Contains check for some reason.
			if (!controllers.Contains(controllerId) && GetButtonDown(controllerId, alias))
			{
				controllers.Add(controllerId);
			}
		}
		
		return controllers;
	}
	
	public List<eControllerId> GetButtonUp(string alias)
	{
		List<eControllerId> controllers = new List<eControllerId>();
		foreach (eControllerId controllerId in m_Controllers.Keys)
		{
			// NOTE ppoirier: I had to do the Contains check for some reason.
			if (!controllers.Contains(controllerId) && GetButtonUp(controllerId, alias))
			{
				controllers.Add(controllerId);
			}
		}
		
		return controllers;
	}
	
	public List<BaseController.eButtonId> GetButton(ControllerInputManager.eControllerId controllerId)
	{
		List<BaseController.eButtonId> buttons = new List<BaseController.eButtonId>();
		if (m_Controllers.ContainsKey(controllerId))
		{
			foreach (BaseController.eButtonId buttonId in System.Enum.GetValues(typeof(BaseController.eButtonId)))
			{
				// NOTE ppoirier: I had to do the Contains check for some reason.
				if (!buttons.Contains(buttonId) && GetButton(controllerId, buttonId))
				{
					buttons.Add(buttonId);
				}
			}
		}
		
		return buttons;
	}

	public List<BaseController.eButtonId> GetButtonDown(ControllerInputManager.eControllerId controllerId)
	{
		List<BaseController.eButtonId> buttons = new List<BaseController.eButtonId>();
		if (m_Controllers.ContainsKey(controllerId))
		{
			foreach (BaseController.eButtonId buttonId in System.Enum.GetValues(typeof(BaseController.eButtonId)))
			{
				// NOTE ppoirier: I had to do the Contains check for some reason.
				if (!buttons.Contains(buttonId) && GetButtonDown(controllerId, buttonId))
				{
					buttons.Add(buttonId);
				}
			}
		}
	
		return buttons;
	}
	
	public List<BaseController.eButtonId> GetButtonUp(ControllerInputManager.eControllerId controllerId)
	{
		List<BaseController.eButtonId> buttons = new List<BaseController.eButtonId>();
		if (m_Controllers.ContainsKey(controllerId))
		{
			foreach (BaseController.eButtonId buttonId in System.Enum.GetValues(typeof(BaseController.eButtonId)))
			{
				// NOTE ppoirier: I had to do the Contains check for some reason.
				if (!buttons.Contains(buttonId) && GetButtonUp(controllerId, buttonId))
				{
					buttons.Add(buttonId);
				}
			}
		}
		
		return buttons;
	}
	
	public bool GetButton(ControllerInputManager.eControllerId controllerId, string alias)
	{
		if (m_ButtonAliases.ContainsKey(alias))
		{
			return GetButton(controllerId, m_ButtonAliases[alias]);
		}
		
		return false;
	}
	
	public bool GetButtonDown(ControllerInputManager.eControllerId controllerId, string alias)
	{
		if (m_ButtonAliases.ContainsKey(alias))
		{
			return GetButtonDown(controllerId, m_ButtonAliases[alias]);
		}
		
		return false;
	}
	
	public bool GetButtonUp(ControllerInputManager.eControllerId controllerId, string alias)
	{
		if (m_ButtonAliases.ContainsKey(alias))
		{
			return GetButtonUp(controllerId, m_ButtonAliases[alias]);
		}
		
		return false;
	}
	
	public bool GetButton(ControllerInputManager.eControllerId controllerId, BaseController.eButtonId buttonId)
	{
		if (m_Controllers.ContainsKey(controllerId))
		{
			return m_Controllers[controllerId].GetButton(buttonId);
		}
		
		return false;
	}

	public bool GetButtonDown(ControllerInputManager.eControllerId controllerId, BaseController.eButtonId buttonId)
	{
		if (m_Controllers.ContainsKey(controllerId))
		{
			return m_Controllers[controllerId].GetButtonDown(buttonId);
		}

		return false;
	}
	
	public bool GetButtonUp(ControllerInputManager.eControllerId controllerId, BaseController.eButtonId buttonId)
	{
		if (m_Controllers.ContainsKey(controllerId))
		{
			return m_Controllers[controllerId].GetButtonUp(buttonId);
		}
		
		return false;
	}
	#endregion
	
	#region Protected Functions
	#endregion
	
	#region Private Functions
	private void InitInputDetection()
	{
		// Clean.
		m_Controllers.Clear();

		// Initialize the controller inputs.
		string[] joysticks =  Input.GetJoystickNames();
		for (int i = 0; i < joysticks.Length && i < NB_CONTROLLERS; ++i) 
		{
			Debug.Log("Joystick Name: " + joysticks[i]);

			// Detect and add the controller.
			AddController(CreateController(joysticks[i]));
		}
	}

	private BaseController CreateController(string joystickName)
	{
		BaseController controller = null;

		foreach (Type controllerType in SUPPORTED_CONTROLLERS)
		{
			string controllerName = GetControllerDetectionName(controllerType);

			// Special case a empty name controller.
			if ((string.IsNullOrEmpty(controllerName) && string.IsNullOrEmpty(joystickName)) || (!string.IsNullOrEmpty(controllerName) && joystickName.Contains(controllerName)))
			{
				controller = Activator.CreateInstance(controllerType) as BaseController;
				break;
			}
		}

		if (controller == null)
		{
			// Default configurations.
			controller = new GenericController();
		}

		return controller;
	}

	private string GetControllerDetectionName(Type controllerType)
	{
		return ((ControllerAttribute)Attribute.GetCustomAttribute(controllerType, typeof(ControllerAttribute))).m_DetectionName;
	}
	#endregion
}
