using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraController : MonoBehaviour 
{
	#region Members and properties
	// constants
	private const float CAMERA_MOVEMENT_DURATION = 2.0f;

	// delegates
	public delegate void OnCameraMovementEnded();
	
	// enums
	
	// public
	public Camera m_GUICamera = null;
	public Camera m_GroundCamera = null;
	public Camera m_HandCamera = null;
	public Camera m_MainCamera = null;
	public GameObject m_GameCameraContainer = null;
	
	// protected
	
	// private
	private static CameraController m_Instance = null;
	private Vector3 m_InitialGameCameraContainerPosition = Vector3.zero;
	private OnCameraMovementEnded m_MovementEndedDelegate = null;
	private float m_CameraMovement = 0.0f;
	
	// properties
	public static CameraController Instance
	{
		get
		{
			return m_Instance;
		}
	}
	#endregion
	
	#region Unity API
	public void Awake()
	{
		m_Instance = this;
		m_InitialGameCameraContainerPosition = m_GameCameraContainer.transform.position;
	}
	#endregion
	
	#region Public Methods
	public void Reset()
	{
		m_GameCameraContainer.transform.position = m_InitialGameCameraContainerPosition;
	}

	public void MoveGameCamera(float movement, OnCameraMovementEnded onEnded)
	{
		m_MovementEndedDelegate = onEnded;
		m_CameraMovement = movement;

		StopCoroutine("MoveGameCameraAnimation");
		StartCoroutine("MoveGameCameraAnimation");
	}
	#endregion
	
	#region Protected Methods
	#endregion
	
	#region Private Methods
	private IEnumerator MoveGameCameraAnimation()
	{
		yield return null;
		
		Vector3 oldPosition = m_GameCameraContainer.transform.position;
		Vector3 newPosition = oldPosition;
		newPosition.z += m_CameraMovement;
		
		float ratio = 0.0f;
		float duration = CAMERA_MOVEMENT_DURATION;
		while (ratio < 1.0f)
		{
			duration -= Time.deltaTime;
			
			ratio = AnimationController.GetInterpolationRatio(0.0f, CAMERA_MOVEMENT_DURATION, (CAMERA_MOVEMENT_DURATION - duration), AnimationController.EaseInOutType.Quad);
			m_GameCameraContainer.transform.position = Vector3.Lerp(oldPosition, newPosition, ratio);
			
			yield return null;
		}

		m_GameCameraContainer.transform.position = newPosition;
		
		m_MovementEndedDelegate();
		m_MovementEndedDelegate = null;
	}
	#endregion
}
