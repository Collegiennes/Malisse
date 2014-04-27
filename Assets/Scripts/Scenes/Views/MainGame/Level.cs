using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Level : MonoBehaviour 
{
	#region Members and properties
	// constants
	private const float WALL_HEIGHT = 2000.0f;
	private const float WALL_DEPTH = 200.0f;
	
	// enums
	
	// public
	public Collider m_Bounds = null;
	
	// protected
	
	// private
	
	// properties
	#endregion
	
	#region Unity API
	private void Awake()
	{
		BuildWalls();
	}
	#endregion
	
	#region Public Methods
	#endregion

	#region Protected Methods
	#endregion
	
	#region Private Methods
	private void BuildWalls()
	{
		BuildWall("BottomWall", new Vector3(0.0f, WALL_HEIGHT * 0.5f, m_Bounds.bounds.min.z - (WALL_DEPTH * 0.5f)), new Vector3(m_Bounds.bounds.size.x, WALL_HEIGHT, WALL_DEPTH));
		BuildWall("TopWall", new Vector3(0.0f, WALL_HEIGHT * 0.5f, m_Bounds.bounds.max.z + (WALL_DEPTH * 0.5f)), new Vector3(m_Bounds.bounds.size.x, WALL_HEIGHT, WALL_DEPTH));
		BuildWall("LeftWall", new Vector3(m_Bounds.bounds.min.x - (WALL_DEPTH * 0.5f), WALL_HEIGHT * 0.5f, 0.0f), new Vector3(WALL_DEPTH, WALL_HEIGHT, m_Bounds.bounds.size.z));
		BuildWall("RightWall", new Vector3(m_Bounds.bounds.max.x + (WALL_DEPTH * 0.5f), WALL_HEIGHT * 0.5f, 0.0f), new Vector3(WALL_DEPTH, WALL_HEIGHT, m_Bounds.bounds.size.z));
	}

	private void BuildWall(string objName, Vector3 position, Vector3 size)
	{
		GameObject bottomWall = new GameObject(objName);
		bottomWall.transform.parent = transform;
		bottomWall.transform.position = position;
		
		BoxCollider wallCollider = bottomWall.AddComponent<BoxCollider>();
		wallCollider.size = size;
	}
	#endregion
}
