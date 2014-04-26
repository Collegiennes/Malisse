using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlowData 
{
	#region Members and Properties
	// constants
	
	// enums
	
	// public
	
	// protected
	protected Dictionary<string, object> m_Parameters = null;
	
	// private
	
	// properties
	public Dictionary<string, object> Parameters
	{
		get { return m_Parameters; }
	}
	#endregion
	
	#region Class Methods
	public FlowData()
	{
		m_Parameters = new Dictionary<string, object>();
	}
	#endregion
	
	#region Public Functions
	public bool AddParameters(Dictionary<string, object> parameters)
	{
		bool bOk = true;
		if (parameters != null)
		{
			foreach (string parameterKey in parameters.Keys)
			{
				if (!string.IsNullOrEmpty(parameterKey))
				{
					bOk = AddParameter(parameterKey, parameters[parameterKey]) && bOk;
				}
			}
		}
		
		return bOk;
	}
	
	public bool AddParameter(string key, object value)
	{
		if (!m_Parameters.ContainsKey(key))
		{
			m_Parameters.Add(key, value);
			return true;
		}
		
		return false;
	}
	
	public object GetParameterValue(string key)
	{
		if (m_Parameters.ContainsKey(key))
		{
			return m_Parameters[key];
		}
		
		return null;
	}
	#endregion
	
	#region Protected Functions
	#endregion
	
	#region Private Functions
	#endregion
}
