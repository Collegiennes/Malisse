using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainGameView : View 
{
	#region Members and properties
	// constants
	private const string HAND_PLAYER_PREFAB_PATH = "Prefabs/Hands/HandPlayer{0}";
	private const string LEVEL_NAME = "Level{0}";
	private const int NB_LEVELS = 3;
	private readonly Vector2 PLAYER_1_POSITION = new Vector2(-150.0f, 0.0f);
	private readonly Vector2 PLAYER_2_POSITION = new Vector2(150.0f, 0.0f);

	// delegates

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
#if UNITY_EDITOR
	private void OnGUI()
	{
		if (GUI.Button(new Rect(10, 10, 150, 100), "SKIP"))
		{
			if (!m_IsLoading)
			{
				LoadNextLevel();
			}
		}
	}
#endif

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

		LoadNextLevel();
	}
	#endregion
	
	#region Public Methods
	#endregion
	
	#region Protected Methods
	#endregion
	
	#region Private Methods
	private void LoadHand(int playerId, Vector2 localPosition)
	{
		GameObject handPrefab = Resources.Load(string.Format(HAND_PLAYER_PREFAB_PATH, playerId.ToString())) as GameObject;
		if (handPrefab != null)
		{
			GameObject handObj = GameObject.Instantiate(handPrefab) as GameObject;
			handObj.transform.parent = m_CurrentLevel.transform;
			handObj.transform.localPosition = new Vector3(localPosition.x, localPosition.y, handObj.transform.localPosition.z);
			handObj.transform.localEulerAngles = Vector3.zero;

			Hand hand = handObj.GetComponent<Hand>();
			if (hand != null)
			{
				hand.LevelBounds = m_CurrentLevel.m_Bounds;

				m_Hands.Add(playerId, hand);
			}
		}
	}

	private void UnloadHands()
	{
		foreach (Hand hand in m_Hands.Values)
		{
			Destroy(hand.gameObject);
		}
		m_Hands.Clear();
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

	private void LoadNextLevel()
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

		// Unload hands.
		UnloadHands();

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

		yield return Resources.UnloadUnusedAssets();
		System.GC.Collect();

		// Load new level.
		string levelName = string.Format(LEVEL_NAME, m_LevelIds[m_LevelIndex].ToString());
		yield return Application.LoadLevelAdditiveAsync(levelName);

		// Save new level.
		GameObject levelObj = GameObject.Find(levelName) as GameObject;
		if (levelObj != null)
		{
			levelObj.transform.parent = m_SceneAnchor;
			levelObj.name = "LEVEL_" + m_LevelIds[m_LevelIndex].ToString();

			m_CurrentLevel = levelObj.GetComponent<Level>();

			PositionNewLevel();
		}
	}

	private void PositionNewLevel()
	{
		if (m_PreviousLevel == null)
		{
			m_CurrentLevel.transform.localPosition = Vector3.zero;

			OnCameraMovementEnded();
		}
		else
		{
			m_CurrentLevel.transform.position = new Vector3(0.0f, 0.0f, m_PreviousLevel.m_Bounds.bounds.max.z + m_CurrentLevel.m_Bounds.bounds.extents.z);

			float offset = m_CurrentLevel.transform.position.z - m_PreviousLevel.transform.position.z;
			CameraController.Instance.MoveGameCamera(offset, OnCameraMovementEnded);
		}
	}

	private void OnCameraMovementEnded()
	{
		// Destroy previous.
		if (m_PreviousLevel != null)
		{
			Destroy(m_PreviousLevel.gameObject);
		}

		LoadHand(1, PLAYER_1_POSITION);
		LoadHand(2, PLAYER_2_POSITION);
		
		m_IsLoading = false;
	}
	#endregion
}
