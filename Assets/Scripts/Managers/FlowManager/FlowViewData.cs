using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlowViewData : FlowData 
{
	#region Members and Properties
	// constants
	
	// enums
	
	// public
	
	// protected
	
	// private
	
	// properties
	public string ViewId
	{
		get; 
		private set;
	}

	public string ViewName
	{
		get; 
		private set;
	}
	#endregion
	
	#region Class Methods
	public FlowViewData(string viewId, string viewName) : base()
	{
		ViewId = viewId;
		ViewName = viewName;
	}
	#endregion
	
	#region Public Functions
	public FlowViewData Clone()
	{
		// Remember to always keep this function up to date.
		FlowViewData viewData = new FlowViewData(ViewId, ViewName);
		foreach (string parameterKey in m_Parameters.Keys)
		{
			viewData.AddParameter(parameterKey, m_Parameters[parameterKey]);
		}
		
		return viewData;
	}
	#endregion
	
	#region Protected Functions
	#endregion
	
	#region Private Functions
	#endregion
}
