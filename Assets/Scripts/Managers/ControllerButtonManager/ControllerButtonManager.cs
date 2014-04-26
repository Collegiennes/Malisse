using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ControllerButtonManager : Observer 
{
	#region Members & Properties
	// constants
	private const float DOT_PRODUCT_MIN = 0.75f;

	// public

	// protected
	protected ControllerButton m_SelectedButton;
	protected bool m_MoveAvailable = true;

	// private
	private bool m_IsActive = true;
	private ControllerInputManager.eControllerId m_ControllerInput = ControllerInputManager.eControllerId.NONE;

	// properties
	public bool IsActive
	{
		get { return m_IsActive; }
		set 
		{
			m_IsActive = value; 
			if (!m_IsActive)
			{
				ClearButtons();
			}
			else
			{
				Reset();
			}
		}
	}
	
	public ControllerInputManager.eControllerId ControllerInput
	{
		set { m_ControllerInput = value; }
	}
	#endregion

	#region Unity API
	protected virtual void Update()
	{
		if (m_IsActive)
		{
			SelectNewButton();
		}
	}
	#endregion

	#region Public Functions
	public void Reset()
	{
		m_MoveAvailable = true;
		m_SelectedButton = null;
		SelectNewButton();
	}
	#endregion

	#region Observer Implementation
	public override void OnNotify(ISubject subject, object args)
	{
	}
	#endregion
	
	#region Protected Functions
	protected void SelectNewButton()
	{
		ControllerButton newSelectedButton = GetSelectedButton();
		if (m_MoveAvailable && newSelectedButton != null)
		{
			m_MoveAvailable = false;
			
			SelectNewButton(newSelectedButton);
		}
	}
	
	protected void SelectNewButton(ControllerButton button)
	{
		if (button != null)
		{
			// Clear the button.
			ClearButtons();
			
			// Select the new button.
			m_SelectedButton = button;
			m_SelectedButton.ButtonOver();
		}
	}
	#endregion
	
	#region Private Functions
	private void ClearButtons()
	{
		// Clear the button.
		foreach (Subject subject in m_SubjectList)
		{
			if (subject != null && subject is ControllerButton /*&& subject.gameObject.active*/)
			{
				((ControllerButton)subject).ButtonOut();
			}
		}
	}
	
	private ControllerButton GetSelectedButton()
	{
		ControllerButton newSelectedButton = null;
		if (m_SelectedButton != null)
		{
			// TODO: Find a way not to be dependant of PlayerInputManager.
			Vector2 leftJoystick = Vector2.zero;
			if (m_ControllerInput != ControllerInputManager.eControllerId.NONE)
			{
				leftJoystick = ControllerInputManager.Instance.GetLeftJoystick(m_ControllerInput);
			}
			else
			{
				Dictionary<ControllerInputManager.eControllerId, Vector2> leftJoysticks = ControllerInputManager.Instance.GetLeftJoystick();
				leftJoystick = leftJoysticks.Count > 0 ? leftJoysticks[0] : Vector2.zero;
			}
			
			if (leftJoystick == Vector2.zero)
			{
				m_MoveAvailable = true;
			}
			
			float dotProduct = DOT_PRODUCT_MIN;
			float distance = -1.0f;
			foreach (Subject subject in m_SubjectList)
			{
				if (subject != null && subject is ControllerButton && subject.gameObject.activeSelf)
				{
					ControllerButton button = subject as ControllerButton;
					if (button != m_SelectedButton)
					{
						// Find the closest button related to the joystick direction.
						Vector2 buttonVector = new Vector2(button.transform.position.x - m_SelectedButton.transform.position.x, button.transform.position.y - m_SelectedButton.transform.position.y);
						float dotProductTemp = Vector2.Dot(leftJoystick.normalized, buttonVector.normalized);
						float newDistance = Vector3.Distance(button.transform.position, m_SelectedButton.transform.position);
						if (dotProductTemp > dotProduct && (distance == -1.0f || newDistance < distance))
						{
							distance = newDistance;
							newSelectedButton = button;
						}
					}
				}
			}
		}
		else if (m_SubjectList.Count > 0)
		{
			// Set a default button.
			foreach (Subject subject in m_SubjectList)
			{
				if (subject != null && subject is ControllerButton && subject.gameObject.activeSelf)
				{
					newSelectedButton = subject as ControllerButton;
					break;
				}
			}
		}
		
		return newSelectedButton;
	}
	#endregion
}
