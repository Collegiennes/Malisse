using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainMenuView : View 
{
	#region Members and properties
	// constants
	
	// enums
	
	// public
	public float m_MinDelay = 0.5f;
	public AudioClip m_Music = null;
	public ControllerButtonManager m_ControllerButtonManager = null;
	public ControllerButton m_OnePlayerButton = null;
	public ControllerButton m_TwoPlayerButton = null;
	
	// protected
	
	// private
	private float m_CurrentCountdown = 0.0f;
	
	// protected
	
	// private
	private bool m_SceneReady = false;
	
	// properties
	#endregion
	
	#region Unity API
	private void Start()
	{
		// Initialize controllers.
		if (ControllerInputManager.Instance.Keyboard2ControllerId == ControllerInputManager.eControllerId.NONE)
		{
			ControllerInputManager.Instance.AddKeyboard2Controller();
		}
		if (ControllerInputManager.Instance.MouseControllerId == ControllerInputManager.eControllerId.NONE)
		{
			ControllerInputManager.Instance.AddMouseController();
		}

		// Initialize initial selected button.
		if (GameUtils.m_GameMode == GameUtils.eGameMode.ONE_PLAYER)
		{
			m_ControllerButtonManager.SelectNewButton(m_OnePlayerButton);
		}
		else if (GameUtils.m_GameMode == GameUtils.eGameMode.TWO_PLAYER)
		{
			m_ControllerButtonManager.SelectNewButton(m_TwoPlayerButton);
		}

		AudioManager.Instance.PlayMusic(m_Music);
	}

	protected virtual void Update()
	{
		if (m_State != eState.OPENED)
		{
			return;
		}
		
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			Application.Quit();
		}

		if (m_CurrentCountdown > m_MinDelay)
		{
			m_SceneReady = true;
		}
		else
		{
			m_CurrentCountdown += Time.deltaTime;
		}
	}
	#endregion

	#region View Implementation
	public override void HandleAction(FlowActionData actionData)
	{
		if (m_SceneReady)
		{
			if (actionData.ActionName == "CHOOSE_ONE_PLAYER")
			{
				// Remove if single player only.
				ControllerInputManager.Instance.RemoveController(ControllerInputManager.Instance.Keyboard2ControllerId);
				ControllerInputManager.Instance.Keyboard2ControllerId = ControllerInputManager.eControllerId.NONE;

				ControllerInputManager.Instance.RemoveController(ControllerInputManager.Instance.MouseControllerId);
				ControllerInputManager.Instance.MouseControllerId = ControllerInputManager.eControllerId.NONE;

				GameUtils.m_GameMode = GameUtils.eGameMode.ONE_PLAYER;
				FlowManager.Instance.TriggerAction("GO_TO_MAIN_GAME");
			}
			else if (actionData.ActionName == "CHOOSE_TWO_PLAYER")
			{
				GameUtils.m_GameMode = GameUtils.eGameMode.TWO_PLAYER;
				FlowManager.Instance.TriggerAction("GO_TO_MAIN_GAME");
			}
		}
	}
	#endregion
	
	#region Public Methods
	#endregion
	
	#region Protected Methods
	#endregion
	
	#region Private Methods
	#endregion
}
