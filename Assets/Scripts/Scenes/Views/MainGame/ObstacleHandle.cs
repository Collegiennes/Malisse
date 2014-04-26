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
	
	// properties
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
		NotifyObservers(true);
	}

	public void OnReleased()
	{
		NotifyObservers(false);
	}
	#endregion
	
	#region Protected Methods
	#endregion
	
	#region Private Methods
	#endregion
}
