using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterController : MonoBehaviour 
{
	#region Members and Properties
	// constants
	
	// enums
	public enum ePlayerId
	{
		PLAYER_1,
		PLAYER_2
	}
	
	// public
	private static CharacterController m_Instance = null;
	
	// protected
	
	// private
	
	// properties
	public static CharacterController Instance
	{
		get
		{
			if (m_Instance == null)
			{
				GameObject instanceObj = new GameObject("CharacterController");
				m_Instance = instanceObj.AddComponent<CharacterController>();
			}

			return m_Instance;
		}
	}
	#endregion
	
	#region Unity API
	private void Awake() 
	{
		if (m_Instance == null)
		{
			m_Instance = this;
		}
	}
	#endregion
	
	#region Public Functions
	public bool IsPlayerController(CharacterController.ePlayerId playerId, ControllerInputManager.eControllerId controllerId)
	{
		if (controllerId == ControllerInputManager.Instance.MouseControllerId)
		{
			return playerId == ePlayerId.PLAYER_1;
		}
		else if (controllerId == ControllerInputManager.Instance.Keyboard1ControllerId)
		{
			return playerId == ePlayerId.PLAYER_1;
		}
		else if (controllerId == ControllerInputManager.Instance.Keyboard2ControllerId)
		{
			return GameUtils.m_GameMode == GameUtils.eGameMode.TWO_PLAYER && playerId == ePlayerId.PLAYER_2;
		}
		else if ((playerId == ePlayerId.PLAYER_1 && controllerId == ControllerInputManager.eControllerId.CONTROLLER_01) ||
			    (playerId == ePlayerId.PLAYER_2 && controllerId == ControllerInputManager.eControllerId.CONTROLLER_02))
		{
			return true;
		}

		return false;
	}
	#endregion
	
	#region Protected Functions
	#endregion
	
	#region Private Functions
	#endregion
}
