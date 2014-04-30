using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ControllerAttribute("")]
public abstract class BaseController : IController 
{
	#region Members and Properties
	// constants
	public const string JOYSTICK_AXIS_KEY = "Joystick#_Axis{0}";
	public const string JOYSTICK_BUTTON_KEY = "joystick # button {0}";
	
	// enums
	public enum eButtonId
	{
		NONE,
		BUTTON_01,
		BUTTON_02,
		BUTTON_03,
		BUTTON_04,
		BUTTON_05,
		BUTTON_06,
		BUTTON_07,
		BUTTON_08,
		BUTTON_09,
		BUTTON_10,
		BUTTON_11,
		BUTTON_12,
		BUTTON_13,
		BUTTON_14,
		BUTTON_15,
		BUTTON_16,
		BUTTON_17,
		BUTTON_18,
		BUTTON_19,
		BUTTON_20
	}

	public enum eAxisId
	{
		LEFT_JOYSTICK_HORIZONTAL,
		LEFT_JOYSTICK_VERTICAL,
		RIGHT_JOYSTICK_HORIZONTAL,
		RIGHT_JOYSTICK_VERTICAL,
		D_PAD_HORIZONTAL,
		D_PAD_VERTICAL,
		L2,
		R2,
		MOTION_HORIZONTAL,
		MOTION_VERTICAL
	}
	
	// public
	
	// protected
	protected Dictionary<BaseController.eButtonId, string> m_KeyCodeMaps = new Dictionary<BaseController.eButtonId, string>();
	protected Dictionary<BaseController.eAxisId, string> m_AxixMaps = new Dictionary<BaseController.eAxisId, string>();
	
	// private
	
	// properties
	public ControllerInputManager.eControllerId ControllerId
	{
		get; set;
	}
	#endregion
	
	#region Class API
	#endregion

	#region IController Implementation
	public abstract void SetKeyMapping();

	public virtual Vector2 GetDPad()
	{
		return GetAxis(eAxisId.D_PAD_HORIZONTAL, eAxisId.D_PAD_VERTICAL);
	}

	public virtual Vector2 GetLeftJoystick()
	{
		return GetAxis(eAxisId.LEFT_JOYSTICK_HORIZONTAL, eAxisId.LEFT_JOYSTICK_VERTICAL);
	}

	public virtual Vector2 GetRightJoystick()
	{
		return GetAxis(eAxisId.RIGHT_JOYSTICK_HORIZONTAL, eAxisId.RIGHT_JOYSTICK_VERTICAL);
	}

	public virtual float GetL2()
	{
		return GetAxis(eAxisId.L2, -1.0f);
	}

	public virtual float GetR2()
	{
		return GetAxis(eAxisId.R2, -1.0f);
	}

	public virtual Vector2 GetMotion()
	{
		return GetAxis(eAxisId.MOTION_HORIZONTAL, eAxisId.MOTION_VERTICAL);
	}
	
	public virtual bool GetButton(BaseController.eButtonId buttonId)
	{
		string keyCode = GetKeyCode(buttonId);
		if (!string.IsNullOrEmpty(keyCode))
		{
			return Input.GetKey(keyCode);
		}
		
		return false;
	}

	public virtual bool GetButtonDown(BaseController.eButtonId buttonId)
	{
		string keyCode = GetKeyCode(buttonId);
		if (!string.IsNullOrEmpty(keyCode))
		{
			return Input.GetKeyDown(keyCode);
		}

		return false;
	}

	public virtual bool GetButtonUp(BaseController.eButtonId buttonId)
	{
		string keyCode = GetKeyCode(buttonId);
		if (!string.IsNullOrEmpty(keyCode))
		{
			return Input.GetKeyUp(keyCode);
		}
		
		return false;
	}

	public virtual Vector2 GetAxis(BaseController.eAxisId horizontalAxisId, BaseController.eAxisId verticalAxisId, float defaultValue = 0.0f)
	{
		Vector2 axis = Vector2.zero;
		axis.x = GetAxis(horizontalAxisId);
		axis.y = GetAxis(verticalAxisId);
		
		return axis;
	}
	
	public virtual float GetAxis(BaseController.eAxisId axisId, float defaultValue = 0.0f)
	{
		string axisCode = GetAxisCode(axisId);
		if (!string.IsNullOrEmpty(axisCode))
		{
			return Input.GetAxis(axisCode);
		}
		
		return defaultValue;
	}
	#endregion
	
	#region Public Functions
	public void AddButtonMap(BaseController.eButtonId buttonId, string inputKey)
	{
		if (!m_KeyCodeMaps.ContainsKey(buttonId))
		{
			m_KeyCodeMaps.Add(buttonId, inputKey);
		}
		else
		{
			m_KeyCodeMaps[buttonId] = inputKey;
		}
	}

	public void AddJoystickButtonMap(BaseController.eButtonId buttonId, int inputKey)
	{
		if (!m_KeyCodeMaps.ContainsKey(buttonId))
		{
			m_KeyCodeMaps.Add(buttonId, string.Format(JOYSTICK_BUTTON_KEY, inputKey));
		}
		else
		{
			m_KeyCodeMaps[buttonId] = string.Format(JOYSTICK_BUTTON_KEY, inputKey);
		}
	}
	
	public void AddAxisMap(BaseController.eAxisId axisId, string inputKey)
	{
		if (!m_AxixMaps.ContainsKey(axisId))
		{
			m_AxixMaps.Add(axisId, inputKey);
		}
		else
		{
			m_AxixMaps[axisId] = inputKey;
		}
	}
	
	public void AddJoystickAxisMap(BaseController.eAxisId axisId, int inputKey)
	{
		if (!m_AxixMaps.ContainsKey(axisId))
		{
			m_AxixMaps.Add(axisId, string.Format(JOYSTICK_AXIS_KEY, inputKey));
		}
		else
		{
			m_AxixMaps[axisId] = string.Format(JOYSTICK_AXIS_KEY, inputKey);
		}
	}

	public virtual string GetButtonName(BaseController.eButtonId buttonId)
	{
		// For debug purpose only.
		return buttonId.ToString();
	}
	#endregion
	
	#region Protected Functions
	protected string GetKeyCode(BaseController.eButtonId buttonId)
	{
		string keyCode = "";
		if (m_KeyCodeMaps.ContainsKey(buttonId))
		{
			keyCode = m_KeyCodeMaps[buttonId];
			keyCode = keyCode.Replace("#", ((int)ControllerId).ToString());
		}
		
		return keyCode;
	}

	protected string GetAxisCode(BaseController.eAxisId axisId)
	{
		string axisCode = "";
		if (m_AxixMaps.ContainsKey(axisId))
		{
			axisCode = m_AxixMaps[axisId];
			axisCode = axisCode.Replace("#", ((int)ControllerId).ToString());
		}
		
		return axisCode;
	}

	protected Vector2 InvertAxis(Vector2 axis, bool invertHorizontal, bool invertVertical)
	{
		return new Vector2(InvertAxis(axis.x, invertHorizontal), InvertAxis(axis.y, invertVertical));
	}

	protected float InvertAxis(float axis, bool invert)
	{
		return invert ? axis * -1.0f : axis;
	}

	protected bool IsPlatformOSX()
	{
		return Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor || 
			Application.platform == RuntimePlatform.OSXWebPlayer || Application.platform == RuntimePlatform.OSXDashboardPlayer;
	}
	#endregion
	
	#region Private Functions
	#endregion
}
