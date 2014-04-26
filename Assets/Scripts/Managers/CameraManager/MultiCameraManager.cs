using UnityEngine;
using System.Collections;

public class MultiCameraManager : MonoBehaviour 
{
	private static MultiCameraManager m_Instance;
	public static MultiCameraManager Instance
	{
		get 
		{
			if (m_Instance == null)
			{
				GameObject instanceObj = GameObject.Find("MultiCameraManager");
				if (instanceObj == null)
				{
					instanceObj = new GameObject("MultiCameraManager");
				}
				
				m_Instance = instanceObj.GetComponentInChildren<MultiCameraManager>();
				if (m_Instance == null)
				{
					m_Instance = instanceObj.AddComponent<MultiCameraManager>();
				}
			}
			
			return m_Instance; 
		}
	}
	
	private Camera m_GUICamera;
	public Camera GUICamera
	{
		get { return m_GUICamera; }
	}
	
	private Camera m_MainCamera;
	public Camera MainCamera
	{
		get { return m_MainCamera; }
	}
	
	// Use this for initialization
	private void Awake () 
	{
		if (m_Instance == null)
		{
			m_Instance = this;
		}
		
		if (m_GUICamera == null)
		{
			GameObject guiCameraObj = GameObject.Find("GUICamera");
			if (guiCameraObj != null)
			{
				m_GUICamera = guiCameraObj.GetComponentInChildren<Camera>();
			}
		}
		if (m_MainCamera == null)
		{
			GameObject mainCameraObj = GameObject.Find("MainCamera");
			if (mainCameraObj != null)
			{
				m_MainCamera = mainCameraObj.GetComponentInChildren<Camera>();
			}
		}
	}
}
