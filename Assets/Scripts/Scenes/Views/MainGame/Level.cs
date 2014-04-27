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
		BuildWall("BottomWall", new Vector3(0.0f, 0.0f, m_Bounds.bounds.min.z - (WALL_DEPTH * 0.5f)), new Vector3(m_Bounds.bounds.size.x, WALL_HEIGHT, WALL_DEPTH), new Vector3(0.0f, WALL_HEIGHT * 0.5f, 0.0f), true);
		BuildWall("TopWall", new Vector3(0.0f, 0.0f, m_Bounds.bounds.max.z + (WALL_DEPTH * 0.5f)), new Vector3(m_Bounds.bounds.size.x, WALL_HEIGHT, WALL_DEPTH), new Vector3(0.0f, WALL_HEIGHT * 0.5f, 0.0f));
		BuildWall("LeftWall", new Vector3(m_Bounds.bounds.min.x - (WALL_DEPTH * 0.5f), 0.0f, 0.0f), new Vector3(WALL_DEPTH, WALL_HEIGHT, m_Bounds.bounds.size.z), new Vector3(0.0f, WALL_HEIGHT * 0.5f, 0.0f));
		BuildWall("RightWall", new Vector3(m_Bounds.bounds.max.x + (WALL_DEPTH * 0.5f), 0.0f, 0.0f), new Vector3(WALL_DEPTH, WALL_HEIGHT, m_Bounds.bounds.size.z), new Vector3(0.0f, WALL_HEIGHT * 0.5f, 0.0f));
	}

	private void BuildWall(string objName, Vector3 position, Vector3 colliderSize, Vector3 colliderCenter, bool lookAt = false)
	{
		GameObject wall = new GameObject(objName);
		wall.transform.parent = transform;
		wall.transform.position = position;
		
		BoxCollider wallCollider = wall.AddComponent<BoxCollider>();
		wallCollider.size = colliderSize;
		wallCollider.center = colliderCenter;

		if (lookAt)
		{
			wall.transform.rotation = CameraController.Instance.m_MainCamera.transform.rotation * Quaternion.AngleAxis(-90, Vector3.right);
		}
	}
	#endregion
}
