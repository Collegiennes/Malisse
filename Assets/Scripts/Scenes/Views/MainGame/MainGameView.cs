using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainGameView : View 
{
	#region Members and properties
	// constants
	private const string HAND_PLAYER_PREFAB_PATH = "Prefabs/Hands/HandPlayer{0}";
	private const string LEVEL_NAME = "Level{0}";
	private const int NB_LEVELS = 2;

	// enums
	
	// public
	public Transform m_SceneAnchor = null;
	
	// protected
	
	// private
	private Dictionary<int, Hand> m_Hands = new Dictionary<int, Hand>();
	private Level m_CurrentLevel = null;
	private Level m_PreviousLevel = null;
	private List<int> m_LevelIds = new List<int>();
	private int m_LevelIndex = -1;
	private bool m_IsLoading = false;
	
	// properties
	#endregion
	
	#region Unity API
	#endregion

	#region View Implementation
	public override void OpenView(FlowViewData viewData, FlowActionData actionData)
	{
		base.OpenView(viewData, actionData);

		object levelIdObj = actionData.GetParameterValue("LEVEL_ID");
		if (levelIdObj != null)
		{
			LoadLevelList(int.Parse(levelIdObj.ToString()));
		}
		else
		{
			LoadLevelList();
		}

		LoadLevel();
		
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

	private void LoadLevelList(int levelId)
	{
		m_LevelIds.Add(levelId);
	}

	private void LoadLevelList()
	{
		m_LevelIds.Clear();
		for (int i = 0; i < NB_LEVELS; ++i)
		{
			m_LevelIds.Insert(Random.Range(0, m_LevelIds.Count), i + 1);
		}
	}

	private void LoadLevel()
	{
		if (!m_IsLoading)
		{
			m_LevelIndex++;
			if (m_LevelIndex >= m_LevelIds.Count)
			{
				m_LevelIndex = 0;
			}

			StopCoroutine("LoadLevelAsync");
			StartCoroutine("LoadLevelAsync");
		}
	}

	private IEnumerator LoadLevelAsync()
	{
		m_IsLoading = true;

		// Destroy previous.
		if (m_PreviousLevel != null)
		{
			Destroy(m_PreviousLevel.gameObject);
		}

		// Swap old level.
		if (m_CurrentLevel != null)
		{
			m_PreviousLevel = m_CurrentLevel;
		}

		// Load new level.
		string levelName = string.Format(LEVEL_NAME, m_LevelIds[m_LevelIndex].ToString());
		yield return Application.LoadLevelAdditiveAsync(levelName);

		// Save new level.
		GameObject levelObj = GameObject.Find(levelName) as GameObject;
		if (levelObj != null)
		{
			levelObj.transform.parent = m_SceneAnchor;

			m_CurrentLevel = levelObj.GetComponent<Level>();
		}

		m_IsLoading = false;
	}
	#endregion
}
