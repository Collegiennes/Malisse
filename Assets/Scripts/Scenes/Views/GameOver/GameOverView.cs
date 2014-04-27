using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameOverView : AlisseView 
{
	#region Members and properties
	// constants
	
	// enums
	
	// public
	public float m_MinDelay = 0.5f;
	public AudioClip m_SFXClick = null;
	public AudioClip m_Music = null;
	
	// protected
	
	// private
	private float m_CurrentCountdown = 0.0f;
	
	// protected
	
	// private
	
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
				AudioManager.Instance.PlaySFX(m_SFXClick);

				FlowManager.Instance.TriggerAction("GO_TO_MAIN_MENU");
			}
		}
		else
		{
			m_CurrentCountdown += Time.deltaTime;
		}
	}
	#endregion

	#region View Implementation
	#endregion
	
	#region Public Methods
	#endregion
	
	#region Protected Methods
	#endregion
	
	#region Private Methods
	#endregion
}
