using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Obstacle : Observer 
{
	#region Members and properties
	// constants
	
	// enums
	
	// public
	public float m_WeightFactorPerHandle = 1.0f;
	
	// protected
	
	// private
	private int m_GrabbedHandleCount = 0;
	
	// properties
	public float FullWeightFactor
	{
		get 
		{
			return m_WeightFactorPerHandle * m_GrabbedHandleCount;
		}
	}
	#endregion
	
	#region Unity API
	#endregion

	#region Observer Implementation
	public override void OnNotify(ISubject subject, object args)
	{
		base.OnNotify(subject, args);

		if (subject is ObstacleHandle && args is bool)
		{
			if ((bool)args)
			{
				m_GrabbedHandleCount++;
			}
			else
			{
				m_GrabbedHandleCount--;
			}
		}
	}
	#endregion
	
	#region Public Methods
	#endregion
	
	#region Protected Methods
	#endregion
	
	#region Private Methods
	#endregion
}
