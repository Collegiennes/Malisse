using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class FlowDataController 
{
	#region Members and Properties
	// constants
	private const string FLOW_XML_PATH = "XML/Flow";

	private const float VIEW_DEPTH = 100.0f;
	private const float OVERLAY_OFFSET = 1.0f;
	private const float LOADING_MIN_DURATION = 0.5f;
	
	// enums
	
	// public
	
	// protected
	
	// private
	private Dictionary<string, FlowViewData> m_ViewsData = new Dictionary<string, FlowViewData>();
	private Dictionary<string, Dictionary<string, FlowActionData>> m_ViewActionsData = new Dictionary<string, Dictionary<string, FlowActionData>>();
	private Dictionary<string, FlowActionData> m_ActionsData = new Dictionary<string, FlowActionData>();
	private float m_ViewDepth = VIEW_DEPTH;
	private float m_OverlayOffset = OVERLAY_OFFSET;
	private float m_LoadingMinDuration = LOADING_MIN_DURATION;
	
	// properties
	public float ViewDepth
	{
		get { return m_ViewDepth; }
	}
	public float OverlayOffset
	{
		get { return m_OverlayOffset; }
	}
	public float LoadingMinDuration
	{
		get { return m_LoadingMinDuration; }
	}
	#endregion
	
	#region Class Methods
	public FlowDataController()
	{
		LoadData();
	}

	~FlowDataController()
	{
		m_ViewsData.Clear();
		m_ViewsData = null;
		m_ViewActionsData.Clear();
		m_ViewActionsData = null;
		m_ActionsData.Clear();
		m_ActionsData = null;
	}
	#endregion
	
	#region Public Functions
	public FlowActionData GetActionData(string viewId, string actionName)
	{
		if (!string.IsNullOrEmpty(actionName))
		{
			// First look action node associated to the view.
			if (!string.IsNullOrEmpty(viewId) && m_ViewActionsData.ContainsKey(viewId) && m_ViewActionsData[viewId].ContainsKey(actionName))
			{
				return m_ViewActionsData[viewId][actionName];
			}
			// Then, find a global action.
			else if (m_ActionsData.ContainsKey(actionName))
			{
				return m_ActionsData[actionName];
			}
		}

		return null;
	}

	public FlowViewData GetViewData(string viewId)
	{
		if (m_ViewsData.ContainsKey(viewId))
		{
			return m_ViewsData[viewId];
		}

		return null;
	}
	#endregion
	
	#region Protected Functions
	#endregion
	
	#region Private Functions
	private void LoadData()
	{
		TextAsset xmlFile = Resources.Load(FLOW_XML_PATH) as TextAsset;
		if (xmlFile != null && xmlFile.text != "")
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xmlFile.text);
			
			// First, find the root.
			foreach (XmlNode baseNode in xmlDocument.GetElementsByTagName("flow"))
			{
				LoadFlowData(baseNode);

				// Find the action nodes.
				foreach (XmlNode viewNode in baseNode.ChildNodes)
				{
					// First look for the views.
					List<FlowActionData> viewActionsData = new List<FlowActionData>();
					FlowViewData viewData = LoadViewData(viewNode, ref viewActionsData);
					if (viewData != null)
					{
						m_ViewsData.Add(viewData.ViewId, viewData);

						// Add it to the view action data list.
						foreach (FlowActionData actionData in viewActionsData)
						{
							Dictionary<string, FlowActionData> actionsData = null;
							if (m_ViewActionsData.ContainsKey(viewData.ViewId))
							{
								actionsData = m_ViewActionsData[viewData.ViewId];
								if (!actionsData.ContainsKey(actionData.ActionName))
								{
									actionsData.Add(actionData.ActionName, actionData);
								}
							}
							else
							{
								actionsData = new Dictionary<string, FlowActionData>();
								actionsData.Add(actionData.ActionName, actionData);
								m_ViewActionsData.Add(viewData.ViewId, actionsData);
							}
						}
					}
					// Then, for global actions.
					else
					{
						FlowActionData actionData = LoadActionData(viewNode);

						// Add it to the action data list.
						if (actionData != null && !m_ActionsData.ContainsKey(actionData.ActionName))
						{
							m_ActionsData.Add(actionData.ActionName, actionData);
						}
					}
				}
			}
		}
	}

	private void LoadFlowData(XmlNode flowNode)
	{
		foreach (XmlAttribute flowAttributes in flowNode.Attributes)
		{
			if (flowAttributes.Name == "viewDepth")
			{
				float.TryParse(flowAttributes.Value, out m_ViewDepth);
				m_ViewDepth = Mathf.Abs(m_ViewDepth);
			}
			else if (flowAttributes.Name == "overlayOffset")
			{
				float.TryParse(flowAttributes.Value, out m_OverlayOffset);
				m_OverlayOffset = Mathf.Abs(m_OverlayOffset);
			}
			else if (flowAttributes.Name == "loadingMinDuration")
			{
				float.TryParse(flowAttributes.Value, out m_LoadingMinDuration);
				m_LoadingMinDuration = Mathf.Max(m_LoadingMinDuration, 0.0f);
			}
		}
	}

	private FlowViewData LoadViewData(XmlNode viewNode, ref List<FlowActionData> actionsData)
	{
		FlowViewData viewData = null;
		if (viewNode.Name == "view")
		{
			string viewId = "";
			string viewName = "";
			foreach (XmlAttribute viewAttributes in viewNode.Attributes)
			{
				if (viewAttributes.Name == "id")
				{
					viewId = viewAttributes.Value;
				}
				else if (viewAttributes.Name == "name")
				{
					viewName = viewAttributes.Value;
				}
			}
			
			// Keep the string item only if it has a key.
			if (!string.IsNullOrEmpty(viewId) && !string.IsNullOrEmpty(viewName))
			{
				viewData = new FlowViewData(viewId, viewName);
				viewData.AddParameters(LoadParameters(viewNode));
				
				foreach (XmlNode actionNode in viewNode.ChildNodes)
				{
					FlowActionData actionData = LoadActionData(actionNode);
					if (actionData != null)
					{
						actionsData.Add(actionData);
					}
				}
			}
		}

		return viewData;
	}

	private FlowActionData LoadActionData(XmlNode actionNode)
	{
		FlowActionData actionData = null;
		if (actionNode.Name == "action")
		{
			string actionName = "";
			string viewId = "";
			bool isPopup = false;
			bool useLoadingScreen = false;
			bool useOverlay = true;
			foreach (XmlAttribute actionAttributes in actionNode.Attributes)
			{
				if (actionAttributes.Name == "name")
				{
					actionName = actionAttributes.Value;
				}
				else if (actionAttributes.Name == "viewId")
				{
					viewId = actionAttributes.Value;
				}
				else if (actionAttributes.Name == "popup")
				{
					bool.TryParse(actionAttributes.Value, out isPopup);
				}
				else if (actionAttributes.Name == "loading")
				{
					bool.TryParse(actionAttributes.Value, out useLoadingScreen);
				}
				else if (actionAttributes.Name == "overlay")
				{
					bool.TryParse(actionAttributes.Value, out useOverlay);
				}
			}

			if (!string.IsNullOrEmpty(actionName))
			{
				actionData = new FlowActionData(actionName, viewId, isPopup, useLoadingScreen, useOverlay);
				actionData.AddParameters(LoadParameters(actionNode));
			}
		}

		return actionData;
	}

	private Dictionary<string, object> LoadParameters(XmlNode parentNode)
	{
		Dictionary<string, object> parameters = new Dictionary<string, object>();

		foreach (XmlNode parameterNode in parentNode.ChildNodes)
		{
			if (parameterNode.Name == "parameter")
			{
				string parameterName = "";
				string parameterValue = parameterNode.InnerText;
				foreach (XmlAttribute parameterAttribute in parameterNode.Attributes)
				{
					if (parameterAttribute.Name == "name")
					{
						parameterName = parameterAttribute.Value;
					}
				}

				if (!string.IsNullOrEmpty(parameterName) && !string.IsNullOrEmpty(parameterValue))
				{
					parameters.Add(parameterName, parameterValue);
				}
			}
		}

		return parameters;
	}
	#endregion
}
