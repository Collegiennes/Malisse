using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObstacleHandle : Subject 
{
	#region Members and properties
	// constants
	
	// enums
	
	// public
	public Obstacle m_Obstacle = null;
	
	// protected
	
	// private
	private bool m_IsGrabbed = false;
	
	// properties
	public bool IsGrabbed
	{
		get { return m_IsGrabbed; }
	}
	#endregion
	
	#region Unity API
	public void Awake()
	{
		if (m_Obstacle != null)
		{
			AddObserver(m_Obstacle);
		}
	}
	#endregion
	
	#region Public Methods
	public void OnGrabbed()
	{
		m_IsGrabbed = true;
		NotifyObservers(m_IsGrabbed);
	}

	public void OnReleased()
	{
		m_IsGrabbed = false;
		NotifyObservers(m_IsGrabbed);
	}
	#endregion
	
	#region Protected Methods
	#endregion
	
	#region Private Methods
	#endregion
}
