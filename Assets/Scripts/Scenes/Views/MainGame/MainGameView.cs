using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainGameView : View 
{
	#region Members and properties
	// constants
	private const string HAND_PLAYER_PREFAB_PATH = "Prefabs/Hands/HandPlayer{0}";

	// enums
	
	// public
	public Transform m_SceneAnchor = null;
	
	// protected
	
	// private
	private Dictionary<int, Hand> m_Hands = new Dictionary<int, Hand>();
	
	// properties
	#endregion
	
	#region Unity API
	#endregion

	#region View Implementation
	public override void OpenView(FlowViewData viewData, FlowActionData actionData)
	{
		base.OpenView(viewData, actionData);

		LoadHand(1);
		LoadHand(2);
	}
	#endregion
	
	#region Public Methods
	#endregion
	
	#region Protected Methods
	#endregion
	
	#region Private Methods
	private void LoadHand(int playerId)
	{
		GameObject handPrefab = Resources.Load(string.Format(HAND_PLAYER_PREFAB_PATH, playerId.ToString())) as GameObject;
		if (handPrefab != null)
		{
			GameObject handObj = GameObject.Instantiate(handPrefab) as GameObject;
			handObj.transform.parent = m_SceneAnchor != null ? m_SceneAnchor : transform;

			Hand hand = handObj.GetComponent<Hand>();

			m_Hands.Add(playerId, hand);
		}
	}
	#endregion
}
