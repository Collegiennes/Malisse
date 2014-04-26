using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlowActionData : FlowData
{
	#region Members and Properties
	// constants
	
	// enums
	
	// public
	
	// protected
	
	// private
	
	// properties
	public string ActionName
	{
		get; 
		private set;
	}

	public string ViewId
	{
		get; 
		private set;
	}
	
	public bool IsPopup
	{
		get; 
		private set;
	}
	
	public bool UseLoadingScreen
	{
		get; 
		private set;
	}
	
	public bool UseOverlay
	{
		get; 
		private set;
	}
	#endregion
	
	#region Class Methods
	public FlowActionData(string actionName, string viewId, bool isPopup, bool useLoadingScreen, bool useOverlay) : base()
	{
		ActionName = actionName;
		ViewId = viewId;
		IsPopup = isPopup;
		UseLoadingScreen = useLoadingScreen;
		UseOverlay = useOverlay;
	}
	#endregion
	
	#region Public Functions
	public FlowActionData Clone()
	{
		// Remember to always keep this function up to date.
		FlowActionData actionData = new FlowActionData(ActionName, ViewId, IsPopup, UseLoadingScreen, UseOverlay);
		foreach (string parameterKey in m_Parameters.Keys)
		{
			actionData.AddParameter(parameterKey, m_Parameters[parameterKey]);
		}

		return actionData;
	}
	#endregion
	
	#region Protected Functions
	#endregion
	
	#region Private Functions
	#endregion
}
