using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraManager : MonoBehaviour
{
	private Camera m_Camera = null;
	
	// Other stuff.
	private bool m_Initialized = false;
	
	// Use this for initialization
	protected virtual void Start ()
	{
		if (Application.isPlaying)
		{
			Initialization();
		}
	}
	
	protected virtual void OnDestroy()
	{
		if (TouchManager.Instance != null)
		{
			TouchManager.Instance.RemoveCamera(m_Camera);
		}
	}
	
	protected void Initialization()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			m_Camera = GetComponentInChildren<Camera>();
			TouchManager.Instance.AddCamera(m_Camera);
		}
	}
	
	public void SetClippingPanes(float near, float far)
	{
		Initialization();
		
		m_Camera.nearClipPlane = near;
		m_Camera.farClipPlane = far;
	}
}
