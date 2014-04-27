using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainMenuView : AlisseView 
{
	#region Members and properties
	// constants
	
	// enums
	
	// public
	public float m_MinDelay = 0.5f;
	public AudioClip m_Music = null;
	
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
		AudioManager.Instance.PlayMusic(m_Music);
	}

	protected override void Update()
	{
		base.Update();
		
		if (m_State != eState.OPENED)
		{
			return;
		}

		if (m_CurrentCountdown > m_MinDelay)
		{
			if (ControllerInputManager.Instance.GetButtonDown(ControllerInputManager.eButtonAliases.GRAB.ToString()).Count > 0 || 
			    ControllerInputManager.Instance.GetButtonDown(ControllerInputManager.eButtonAliases.START.ToString()).Count > 0)
			{
				m_SceneReady = true;
			}
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
				GameUtils.m_GameMode = GameUtils.eGameMode.ONE_PLAYER;
				FlowManager.Instance.TriggerAction("GO_TO_MAIN_GAME");
			}
			else if (actionData.ActionName == "CHOOSE_TWO_PLAYER")
			{
				ControllerInputManager.Instance.AddMouseController();

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
