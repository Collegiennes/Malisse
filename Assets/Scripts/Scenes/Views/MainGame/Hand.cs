using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Hand : MonoBehaviour 
{
	#region Members and properties
	// constants
	private const float GRABBED_OFFSET = -250f;
	private const float RELEASED_OFFSET = 0.0f;
	private const float RAISE_DURATION = 0.25f;
	private readonly Vector2 SPEED = new Vector2(800.0f, 1200.0f);
	
	// enums
	
	// public
	public ControllerInputManager.eControllerId m_ControllerId = ControllerInputManager.eControllerId.CONTROLLER_01;
	public tk2dSprite m_Asset = null;
	public string m_HandGrabbedAssetName = "";
	public string m_HandEmptyAssetName = "";
	public string m_HandReadyAssetName = "";

	// protected
	
	// private
	private ObstacleHandle m_GrabbedObstacleHandle = null;
	private HingeJoint m_GrabbedObstacleJoin = null;
	private Camera m_HandCamera = null;
	private float m_GoalHeight = 0.0f;
	private float m_CurrentHeight = 0.0f;
	
	// properties
	#endregion
	
	#region Unity API
	private void Awake()
	{
		m_HandCamera = GameObject.Find("HandCamera").GetComponent<Camera>();
	}

	private void FixedUpdate()
	{
		// Movements.
		Vector2 movement = ControllerInputManager.Instance.GetLeftJoystick(m_ControllerId);
		if (movement != Vector2.zero)
		{
			movement.x *= Time.deltaTime * SPEED.x;
			movement.y *= Time.deltaTime * SPEED.y;

			Vector3 newMovement = /*Quaternion.Euler(m_HandCamera.transform.eulerAngles) */ new Vector3(movement.x, 0.0f, movement.y);
			transform.position += newMovement;
		}

		// Buttons.
		if (ControllerInputManager.Instance.GetButton(m_ControllerId, ControllerInputManager.eButtonAliases.GRAB.ToString()))
		{
			GrabObstacle();
		}
		else
		{
			StartCoroutine("ReleaseObstacle");
		}
		
		// Height adjustment.
		transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, m_CurrentHeight);

		// Look at.
		transform.LookAt(-m_HandCamera.transform.position);
	}

	private void OnTriggerEnter(Collider other) 
	{
		ObstacleHandle handle = other.GetComponent<ObstacleHandle>();
		if (m_GrabbedObstacleHandle == null && handle != null)
		{
			m_Asset.spriteId = m_Asset.GetSpriteIdByName(m_HandReadyAssetName);
			m_GrabbedObstacleHandle = handle;
		}
	}

	private void OnTriggerExit(Collider other) 
	{
		ObstacleHandle handle = other.GetComponent<ObstacleHandle>();
		if (handle != null && handle == m_GrabbedObstacleHandle)
		{
			m_Asset.spriteId = m_Asset.GetSpriteIdByName(m_HandEmptyAssetName);
			m_GrabbedObstacleHandle = null;
		}
	}
	#endregion
	
	#region Public Methods
	#endregion
	
	#region Protected Methods
	#endregion
	
	#region Private Methods
	private void GrabObstacle()
	{
		m_Asset.spriteId = m_Asset.GetSpriteIdByName(m_HandGrabbedAssetName);

		if (m_GrabbedObstacleHandle != null && m_GrabbedObstacleJoin == null)
		{
			// Move hand.
			m_GoalHeight = GRABBED_OFFSET;
			StopCoroutine("RaiseHand");
			StartCoroutine("RaiseHand");

			// Setup obstacle.
			Vector3 newPosition = transform.position - (m_GrabbedObstacleHandle.transform.position - m_GrabbedObstacleHandle.m_Obstacle.transform.position);
			newPosition.y = m_GrabbedObstacleHandle.m_Obstacle.transform.position.y;
			m_GrabbedObstacleHandle.m_Obstacle.transform.position = newPosition;

			m_GrabbedObstacleJoin = gameObject.AddComponent<HingeJoint>();
			m_GrabbedObstacleJoin.connectedBody = m_GrabbedObstacleHandle.m_Obstacle.rigidbody;
			m_GrabbedObstacleJoin.anchor = m_GrabbedObstacleHandle.transform.localPosition;
		}
	}
	
	private IEnumerator ReleaseObstacle()
	{
		if (m_GrabbedObstacleJoin != null)
		{
			Destroy(m_GrabbedObstacleJoin);
			
			yield return null;

			m_GoalHeight = RELEASED_OFFSET;
			StopCoroutine("RaiseHand");
			StartCoroutine("RaiseHand");
		}
		
		m_Asset.spriteId = m_Asset.GetSpriteIdByName(m_HandEmptyAssetName);
	}

	private IEnumerator RaiseHand()
	{
		yield return null;

		float oldHeight = transform.localPosition.z;

		float ratio = 0.0f;
		float duration = RAISE_DURATION;
		while (ratio < 1.0f)
		{
			duration -= Time.deltaTime;

			ratio = AnimationController.GetInterpolationRatio(0.0f, RAISE_DURATION, (RAISE_DURATION - duration), AnimationController.EaseInOutType.Quad);
			m_CurrentHeight = Mathf.Lerp(oldHeight, m_GoalHeight, ratio);

			yield return null;
		}

	}
	#endregion
}
