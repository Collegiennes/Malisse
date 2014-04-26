using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TouchManager : MonoBehaviour 
{
	/**
	 * Singleton stuff.
	 */
	private static bool m_IsDestroyed = false;
	private static TouchManager m_Instance = null;
	
	public static TouchManager Instance
	{
		get
		{
			if (m_IsDestroyed)
			{
				return null;
			}
			
			if (m_Instance == null)
			{
				GameObject touchManager = GameObject.Find("TouchManager");
				if (touchManager == null)
				{
					touchManager = new GameObject("TouchManager");
				}
				m_Instance = touchManager.GetComponent<TouchManager>();
				if (m_Instance == null)
				{
					m_Instance = touchManager.AddComponent<TouchManager>();
				}
			}
			
			return m_Instance;
		}
	}
	
	/**
	 * Object class stuff.
	 */
	private List<Camera> m_CameraList = new List<Camera>();
	private Dictionary<int, List<GameObject>> m_OnFingerBeganMap;
	
	protected virtual void Start()
	{
		if (Application.isPlaying)
		{
			m_OnFingerBeganMap = new Dictionary<int, List<GameObject>>();
			
			if (m_CameraList.Count == 0)
			{
				m_CameraList.Add(Camera.main);
			}
			
			// Stays in every scenes.
			DontDestroyOnLoad(this);
		}
	}
	
	protected virtual void OnDestroy()
	{
		m_IsDestroyed = true;
	}
	
	protected virtual void Update()
	{
		// OnMouseDown doesn't automatically work for iOS.
		for (int i = 0; i < Input.touchCount; ++i) 
		{
			if (Input.GetTouch(i).phase.Equals(TouchPhase.Began)) 
			{
				BroadcastOnFingerBegan(i);
			}
			else if (Input.GetTouch(i).phase.Equals(TouchPhase.Moved))
			{
				BroadcastOnFingerMoved(i);
			}
			else if (Input.GetTouch(i).phase.Equals(TouchPhase.Canceled) ||
					Input.GetTouch(i).phase.Equals(TouchPhase.Ended)) 
			{
				BroadcastOnFingerEnded(i);
			}
		}
	}
	
	public void AddCamera(Camera camera)
	{
		if (!m_CameraList.Contains(camera))
		{
			m_CameraList.Add(camera);
		}
	}
	
	public void RemoveCamera(Camera camera)
	{
		m_CameraList.Remove(camera);
	}
	
	private void BroadcastOnFingerBegan(int touchId)
	{
		m_OnFingerBeganMap.Remove(touchId);
		List<GameObject> hitList = new List<GameObject>();
		
		foreach (Camera camera in m_CameraList)
		{
			// Construct a ray from the current touch coordinates
			Ray ray = camera.ScreenPointToRay(Input.GetTouch(touchId).position);
			
			// Broadcast the gameObjects that are newly tapped.
			foreach (RaycastHit hit in Physics.RaycastAll(ray))
			{
				if (!hitList.Contains(hit.transform.gameObject))
				{
	    			hit.transform.gameObject.SendMessage("OnMouseDown");
					hitList.Add(hit.transform.gameObject);
				}
			}
		}
		
		m_OnFingerBeganMap.Add(touchId, hitList);
	}
	
	private void BroadcastOnFingerEnded(int touchId)
	{
		List<GameObject> hitList = new List<GameObject>();
		m_OnFingerBeganMap.TryGetValue(touchId, out hitList);	
		
		foreach (Camera camera in m_CameraList)
		{
			// Construct a ray from the current touch coordinates
			Ray ray = camera.ScreenPointToRay(Input.GetTouch(touchId).position);
			
			// Broadcast the gameObjects that are fired.
			foreach (RaycastHit hit in Physics.RaycastAll(ray))
			{
				if (hitList.Contains(hit.transform.gameObject))
				{
	        		hit.transform.gameObject.SendMessage("OnMouseUpAsButton");
					hitList.Remove(hit.transform.gameObject);
				}
			}
		}
		
		// Broadcast the gameObjects that aren't tapped anymore.
		foreach (GameObject obj in hitList)
		{
        	obj.gameObject.SendMessage("OnMouseUp");
		}
		
		m_OnFingerBeganMap.Remove(touchId);
	}
	
	private void BroadcastOnFingerMoved(int touchId)
	{
		List<GameObject> hitList = new List<GameObject>();
		m_OnFingerBeganMap.TryGetValue(touchId, out hitList);
		
		// TODO: Necessary?
		List<GameObject> tempHitList = new List<GameObject>();
		tempHitList.AddRange(hitList);
		
		foreach (Camera camera in m_CameraList)
		{
			// Construct a ray from the current touch coordinates
			Ray ray = camera.ScreenPointToRay(Input.GetTouch(touchId).position);
			
			// Broadcast the gameObjects that are fired.
			foreach (RaycastHit hit in Physics.RaycastAll(ray))
			{
				if (tempHitList.Contains(hit.transform.gameObject))
				{
	        		hit.transform.gameObject.SendMessage("OnMouseOver");
					tempHitList.Remove(hit.transform.gameObject);
				}
			}
		}
		
		// Broadcast the gameObjects that aren't touched anymore, but the finger is still down.
		foreach (GameObject obj in tempHitList)
		{
        	obj.gameObject.SendMessage("OnMouseDrag");
		}
	}
	
	// TODO: Should also work with the editor.
	/*private List<RaycastHit> SortHits(List<RaycastHit> hitList)
	{
		// TODO: Add the Priority if necessary.
		
		int i=0;
		for (; i<hitList.Count; i++)
		{
			RaycastHit hit = hitList[i];
			BaseButton button = hit.transform.gameObject.GetComponentInChildren<BaseButton>();
			if (button != null && button.m_SwallowTouches)
			{
				break;
			}
		}
		
		hitList.RemoveRange(i+1, (hitList.Count)-(i+1));
	}*/
}
