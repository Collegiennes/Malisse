using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlowManager : MonoBehaviour 
{
	#region Members and Properties
	// constants
	private const string LOADING_PREFAB_PATH = "Prefabs/FlowManager/LoadingScreen";
	private const string OVERLAY_PREFAB_PATH = "Prefabs/FlowManager/Overlay";
	
	// enums

	// public
	
	// protected
	
	// private
	private static FlowManager m_Instance = null;
	private FlowDataController m_ActionController = null;
	private List<View> m_OpenedViews = new List<View>();
	private List<View> m_ClosedViews = new List<View>();
	private FlowViewData m_OpeningView = null;
	private List<View> m_ClosingViews = new List<View>();
	private View m_GainingFocusView = null;
	private View m_LosingFocusView = null;
	private FlowActionData m_CurrentActionData = null;
	private List<FlowActionData> m_QueuedActions = new List<FlowActionData>();
	private GameObject m_LoadingScreen = null;
	private GameObject m_Overlay = null;
	
	// properties
	public static FlowManager Instance
	{
		get
		{
			if (m_Instance == null)
			{
				GameObject instanceObj = new GameObject("FlowManager");
				m_Instance = instanceObj.AddComponent<FlowManager>();
			}

			return m_Instance;
		}
	}

	public static bool IsInstanceNull
	{
		get { return m_Instance == null; }
	}

	public bool IsBusy
	{
		get
		{
			return m_OpeningView != null || m_ClosingViews.Count > 0 || m_GainingFocusView != null || m_LosingFocusView != null;
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

		if (m_ActionController == null)
		{
			m_ActionController = new FlowDataController();
		}

		if (m_LoadingScreen == null)
		{
			GameObject loadingScreenPrefab = Resources.Load(LOADING_PREFAB_PATH) as GameObject;
			if (loadingScreenPrefab != null)
			{
				m_LoadingScreen = GameObject.Instantiate(loadingScreenPrefab) as GameObject;
				m_LoadingScreen.transform.parent = transform;
				m_LoadingScreen.SetActive(false);
			}
		}
		
		if (m_Overlay == null)
		{
			GameObject overlayPrefab = Resources.Load(OVERLAY_PREFAB_PATH) as GameObject;
			if (overlayPrefab != null)
			{
				m_Overlay = GameObject.Instantiate(overlayPrefab) as GameObject;
				m_Overlay.transform.parent = transform;
				m_Overlay.SetActive(false);
			}
		}
	}

	private void OnDestroy() 
	{
		StopAllCoroutines();

		if (m_LoadingScreen != null)
		{
			Destroy(m_LoadingScreen);
			m_LoadingScreen = null;
		}
		if (m_Overlay != null)
		{
			Destroy(m_Overlay);
			m_Overlay = null;
		}

		m_Instance = null;
		m_ActionController = null;
		m_OpenedViews.Clear();
		m_OpenedViews = null;
		m_ClosedViews.Clear();
		m_ClosedViews = null;
		m_OpeningView = null;
		m_ClosingViews.Clear();
		m_ClosingViews = null;
		m_GainingFocusView = null;
		m_LosingFocusView = null;
		m_CurrentActionData = null;
		m_QueuedActions.Clear();
		m_QueuedActions = null;
	}
	#endregion
	
	#region Public Functions
	public bool TriggerAction(string actionName, Dictionary<string, object> parameters = null)
	{
		View topView = GetTopView();
		FlowActionData actionData = m_ActionController.GetActionData(topView == null ? "" : topView.ViewData.ViewId, actionName);
		if (actionData != null)
		{
			// Make sure not to modify the original one.
			actionData = actionData.Clone();
			actionData.AddParameters(parameters);

			return TriggerAction(actionData);
		}
		else
		{
			Debug.LogError("FlowManager, TriggerAction: The action '" + actionName + "' didn't exist in the current view (" + (topView == null ? "" : topView.ViewData.ViewId) + ").");
		}

		return false;
	}

	public bool TriggerAction(FlowActionData actionData)
	{
		if (actionData != null)
		{
			m_QueuedActions.Add(actionData);

			TriggerNextAction();

			return true;
		}

		return false;
	}

	public void OnViewCompleted(View view)
	{
		switch (view.State)
		{
		case View.eState.OPENED:
			if (m_OpeningView == view.ViewData)
			{
				m_OpeningView = null;
				m_OpenedViews.Add(view);
			}
			else if (m_GainingFocusView == view)
			{
				m_GainingFocusView = null;

				// Put the view at the end of the queue.
				m_OpenedViews.Remove(view);
				m_OpenedViews.Add(view);
			}
			break;
		case View.eState.CLOSED:
			if (m_ClosingViews.Contains(view))
			{
				m_ClosingViews.Remove(view);
				m_OpenedViews.Remove(view);

				// Destroy immediately if using loading screen.
				if (m_CurrentActionData.UseLoadingScreen)
				{
					Destroy(view.gameObject);
				}
				// Postpone destroying to the end of the sequence.
				else
				{
					m_ClosedViews.Add(view);
				}
			}
			break;
		case View.eState.FOCUS_LOST:
			if (m_LosingFocusView == view)
			{
				m_LosingFocusView = null;
			}
			break;
		}

		if (!TriggerNextView())
		{
			 // Destroy when everything is loaded.
			foreach (View closedView in m_ClosedViews)
			{
				// Destroy it.
				Destroy(closedView.gameObject);
			}
			m_ClosedViews.Clear();

			// Hide the loading screen.
			m_LoadingScreen.SetActive(false);

			TriggerNextAction();
		}
	}
	#endregion
	
	#region Protected Functions
	#endregion
	
	#region Private Functions
	private View GetTopView()
	{
		if (m_OpenedViews.Count > 0)
		{
			return m_OpenedViews[m_OpenedViews.Count - 1];
		}
		
		return null;
	}
	private FlowViewData GetBeforeTopView()
	{
		if (m_OpenedViews.Count > 1)
		{
			return m_OpenedViews[m_OpenedViews.Count - 2].ViewData;
		}
		
		return null;
	}

	private void TriggerNextAction()
	{
		if (!IsBusy && m_QueuedActions.Count > 0)
		{
			// Get the first in action in queue.
			m_CurrentActionData = m_QueuedActions[0];
			FlowViewData viewData = m_ActionController.GetViewData(m_CurrentActionData.ViewId);

			m_QueuedActions.Remove(m_CurrentActionData);

			View topView = GetTopView();
			if (topView != null && viewData != null && viewData.ViewId == topView.ViewData.ViewId)
			{
				topView.HandleAction(m_CurrentActionData);

				TriggerNextAction();
			}
			else if (string.IsNullOrEmpty(m_CurrentActionData.ViewId) || viewData != null)
			{
				// Find if the view is already opened.
				for (int i = m_OpenedViews.Count - 1; i >= 0; --i)
				{
					View view = m_OpenedViews[i];
					
					// If already in the queue, it gains the focus.
					if ((string.IsNullOrEmpty(m_CurrentActionData.ViewId) && view.ViewData == GetBeforeTopView()) || 
					    (viewData != null && view.ViewData.ViewId == viewData.ViewId))
					{
						m_GainingFocusView = view;

						// If a popup, or we are closing the top view.
						if (string.IsNullOrEmpty(m_CurrentActionData.ViewId) || m_CurrentActionData.IsPopup)
						{
							// Close all popup on higher levels than one opening.
							for (int j = i + 1; j < m_OpenedViews.Count; ++j)
							{
								if (m_LosingFocusView == m_OpenedViews[j])
								{
									m_LosingFocusView = null;
								}
								m_ClosingViews.Add(m_OpenedViews[j]);
							}
						}
					}
					// If not in the queue, close/hide the other ones.
					// We don't want to close view in the case we're just closing the top one.
					else if (!string.IsNullOrEmpty(m_CurrentActionData.ViewId))
					{
						// If we are opening a popup, hide them.
						if (m_CurrentActionData.IsPopup)
						{
							// If it's the top view.
							if (topView != null && view.ViewData == topView.ViewData)
							{
								// Losing focus because we are opening a new view.
								m_LosingFocusView = view;
							}
						}
						// If we are not opening a popup, close everything.
						else
						{
							m_ClosingViews.Add(view);
						}
					}
				}
				
				// If nothing is gaining focus, we open a new view.
				if (viewData != null && m_GainingFocusView == null)
				{
					m_OpeningView = viewData;
				}

				// Display the loading screen if requested.
				if (m_OpeningView != null && m_CurrentActionData.UseLoadingScreen)
				{
					m_LoadingScreen.transform.position = new Vector3(0.0f, 0.0f, (m_OpenedViews.Count + 1) * -m_ActionController.ViewDepth);
					m_LoadingScreen.SetActive(true);
				}

				TriggerNextView();
			}
			else
			{
				Debug.LogError("FlowManager, TriggerNextAction: Cannot trigger '" + m_CurrentActionData.ActionName + "' in current scene (" + (viewData == null ? "NONE" : viewData.ViewId) + ").");
				TriggerNextAction();
			}
		}
	}

	private bool TriggerNextView()
	{
		// Trigger top focus view first.
		if (m_LosingFocusView != null)
		{
			m_LosingFocusView.LoseFocus(m_CurrentActionData);
			return true;
		}
		// Close all other views next.
		else if (m_ClosingViews.Count > 0)
		{
			m_ClosingViews[0].CloseView(m_CurrentActionData);
			return true;
		}
		// Show the gaining view.
		else if (m_GainingFocusView != null)
		{
			GainFocus(m_GainingFocusView, m_CurrentActionData);
			return true;
		}
		// Open the new view last.
		else if (m_OpeningView != null)
		{
			// TODO ppoirier: Coroutines are ugly.
			StartCoroutine(LoadView(m_OpeningView, m_CurrentActionData));
			return true;
		}

		return false;
	}

	private void GainFocus(View view, FlowActionData actionData)
	{
		// Display overlay if requested/necessary.
		m_Overlay.transform.position = new Vector3(0.0f, 0.0f, (m_OpenedViews.IndexOf(view) * -m_ActionController.ViewDepth) + m_ActionController.OverlayOffset);
		m_Overlay.SetActive(m_OpenedViews.Count > 1 && actionData.UseOverlay);

		m_GainingFocusView.GainFocus(m_CurrentActionData);
	}

	private IEnumerator LoadView(FlowViewData viewData, FlowActionData actionData)
	{
		// Let the loading screen appearing.
		if (actionData.UseLoadingScreen)
		{
			yield return new WaitForSeconds(m_ActionController.LoadingMinDuration);
		}

		// TODO ppoirier: Ideally, we should do an async call.
		Application.LoadLevelAdditive(viewData.ViewName);

		// TODO ppoirier: Seems necessary when not on a Pro licence.
		yield return null;

		GameObject viewObj = GameObject.Find(viewData.ViewName);
		if (viewObj != null)
		{
			viewObj.transform.parent = transform;
			viewObj.transform.position = new Vector3(0.0f, 0.0f, m_OpenedViews.Count * -m_ActionController.ViewDepth);

			View view = viewObj.GetComponent<View>();
			if (view != null)
			{
				// Display overlay if requested/necessary.
				m_Overlay.transform.position = new Vector3(0.0f, 0.0f, m_OpenedViews.Count * -m_ActionController.ViewDepth + m_ActionController.OverlayOffset);
				m_Overlay.SetActive(actionData.IsPopup && actionData.UseOverlay);

				view.name = viewData.ViewId;
				view.OpenView(viewData, actionData);
			}
			else
			{
				Destroy(viewObj);
				Debug.LogError("FlowManager, LoadView: The scene '" + viewData.ViewName + "' must have a 'View' script attached to it.");
			}
		}
		else
		{
			Debug.LogError("FlowManager, LoadView: Cannot load scene '" + viewData.ViewName + "'.");
		}
	}
	#endregion
}
