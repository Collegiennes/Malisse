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
	public List<AudioClip> m_SFXGrab = new List<AudioClip>();
	public List<AudioClip> m_SFXCantGrab = new List<AudioClip>();
	public AudioClip m_SFXHoldGrab = null;

	// protected
	
	// private
	private Collider m_Bounds = null;
	private ObstacleHandle m_HoveringObstacleHandle = null;
	private ObstacleHandle m_GrabbedObstacleHandle = null;
	private HingeJoint m_GrabbedObstacleJoin = null;
	private Camera m_HandCamera = null;
	private float m_GoalHeight = 0.0f;
	private float m_CurrentHeight = 0.0f;
	private bool m_IsGrabbing = false;
	
	// properties
	public Collider LevelBounds
	{
		set { m_Bounds = value; }
	}
	#endregion
	
	#region Unity API
	private void Awake()
	{
		m_HandCamera = GameObject.Find("HandCamera").GetComponent<Camera>();
	}

	private void OnDestroy()
	{
		AudioManager.Instance.StopLoopingSFX(m_SFXHoldGrab);
	}

	private void FixedUpdate()
	{
		// Height adjustment.
		transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, m_CurrentHeight);

		// TODO technobeanie: Quick fix related to the grabbing feature problems.
		if (m_CurrentHeight != GRABBED_OFFSET && m_CurrentHeight != RELEASED_OFFSET)
		{
			return;
		}

		if (GameUtils.m_GameMode == GameUtils.eGameMode.ONE_PLAYER)
		{
			HandleOnePlayerMode();
		}
		else
		{
			HandleTwoPlayerMode();
		}

		// Look at.
		transform.rotation = m_HandCamera.transform.rotation;

		// Clamp position.
		if (m_Bounds != null)
		{
			transform.position = new Vector3(Mathf.Clamp(transform.position.x, m_Bounds.bounds.min.x + (collider as CapsuleCollider).radius, m_Bounds.bounds.max.x - (collider as CapsuleCollider).radius), 
			                                 transform.position.y,
			                                 Mathf.Clamp(transform.position.z, m_Bounds.bounds.min.z + (collider as CapsuleCollider).radius, m_Bounds.bounds.max.z - (collider as CapsuleCollider).radius));
		}
	}

	private void OnTriggerEnter(Collider other) 
	{
		ObstacleHandle handle = other.GetComponent<ObstacleHandle>();
		if (m_HoveringObstacleHandle == null && handle != null)
		{
			if (!m_IsGrabbing)
			{
				m_Asset.spriteId = m_Asset.GetSpriteIdByName(m_HandReadyAssetName);
			}

			m_HoveringObstacleHandle = handle;
		}
	}

	private void OnTriggerStay(Collider other) 
	{
		// Same behaviour.
		OnTriggerEnter(other);
	}

	private void OnTriggerExit(Collider other) 
	{
		ObstacleHandle handle = other.GetComponent<ObstacleHandle>();
		if (handle != null && handle == m_HoveringObstacleHandle)
		{
			if (!m_IsGrabbing)
			{
				m_Asset.spriteId = m_Asset.GetSpriteIdByName(m_HandEmptyAssetName);
			}

			m_HoveringObstacleHandle = null;
		}
	}
	#endregion
	
	#region Public Methods
	public void SetDefaultAsset()
	{
		m_Asset.spriteId = m_Asset.GetSpriteIdByName(m_HandEmptyAssetName);
	}
	#endregion
	
	#region Protected Methods
	#endregion
	
	#region Private Methods
	private void GrabObstacle()
	{
		m_Asset.spriteId = m_Asset.GetSpriteIdByName(m_HandGrabbedAssetName);
		
		if (m_IsGrabbing)
		{
			return;
		}
		
		m_IsGrabbing = true;

		if (m_HoveringObstacleHandle != null && !m_HoveringObstacleHandle.IsGrabbed && m_GrabbedObstacleJoin == null)
		{
			AudioManager.Instance.PlaySFX(m_SFXGrab);
			AudioManager.Instance.PlayLoopingSFX(m_SFXHoldGrab);

			// Find hit point.
			Vector3 handHitPosition = transform.position - (CameraController.Instance.m_HandCamera.transform.TransformDirection(Vector3.forward) * 2000.0f);
			RaycastHit[] hits = Physics.SphereCastAll(handHitPosition, (collider as CapsuleCollider).radius, CameraController.Instance.m_HandCamera.transform.TransformDirection(Vector3.forward));

			Vector3 hitPoint = transform.position;
			bool isHit = false;
			if (hits.Length > 0)
			{
				foreach (RaycastHit hit in hits)
				{
					Obstacle obstacle = hit.collider.gameObject.GetComponent<Obstacle>();
					if (obstacle != null)
					{
						hitPoint = hit.point;
						isHit = true;
						break;
					}
				}
			}

			if (isHit)
			{
				m_GrabbedObstacleHandle = m_HoveringObstacleHandle;
				m_GrabbedObstacleHandle.OnGrabbed();
				
				GameObject joint = new GameObject("Join");
				joint.transform.position = hitPoint;
				joint.transform.parent = transform;
				
				m_GrabbedObstacleJoin = joint.AddComponent<HingeJoint>();
				m_GrabbedObstacleJoin.rigidbody.useGravity = false;
				m_GrabbedObstacleJoin.rigidbody.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
				m_GrabbedObstacleJoin.connectedBody = m_HoveringObstacleHandle.m_Obstacle.rigidbody;
				
				// Move hand.
				m_GoalHeight = GRABBED_OFFSET;
				StopCoroutine("RaiseHand");
				StartCoroutine("RaiseHand");
			}
		}
		else
		{
			AudioManager.Instance.PlaySFX(m_SFXCantGrab);
		}
	}
	
	private IEnumerator ReleaseObstacle()
	{
		if (m_IsGrabbing)
		{
			m_IsGrabbing = false;

			if (m_GrabbedObstacleJoin != null)
			{
				AudioManager.Instance.StopLoopingSFX(m_SFXHoldGrab);

				m_GrabbedObstacleHandle.OnReleased();
				m_GrabbedObstacleHandle = null;

				Destroy(m_GrabbedObstacleJoin.gameObject);
				m_GrabbedObstacleJoin = null;
				
				yield return null;

				m_GoalHeight = RELEASED_OFFSET;
				StopCoroutine("RaiseHand");
				StartCoroutine("RaiseHand");
			}

			m_Asset.spriteId = m_Asset.GetSpriteIdByName(m_HandEmptyAssetName);
		}
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

	private void HandleOnePlayerMode()
	{
		// Movements.
		Vector2 movement = Vector2.zero;

		if (m_ControllerId == ControllerInputManager.eControllerId.CONTROLLER_01)
		{
			Dictionary<ControllerInputManager.eControllerId, Vector2> movements = ControllerInputManager.Instance.GetLeftJoystick();
			foreach (Vector2 movementTemp in movements.Values)
			{
				movement = movementTemp;
				break;
			}
		}
		else
		{
			Dictionary<ControllerInputManager.eControllerId, Vector2> movements = ControllerInputManager.Instance.GetRightJoystick();
			foreach (Vector2 movementTemp in movements.Values)
			{
				movement = movementTemp;
				break;
			}
		}

		if (movement != Vector2.zero)
		{
			float weightFactor = 1.0f;
			if (m_GrabbedObstacleJoin != null && m_GrabbedObstacleHandle != null)
			{
				weightFactor = m_GrabbedObstacleHandle.m_Obstacle.FullWeightFactor;
			}
			
			movement.x *= Time.deltaTime * SPEED.x * weightFactor;
			movement.y *= Time.deltaTime * SPEED.y * weightFactor;
			
			Vector3 newMovement = /*Quaternion.Euler(m_HandCamera.transform.eulerAngles) */ new Vector3(movement.x, 0.0f, movement.y);
			transform.position += newMovement;
		}
		
		// Buttons (controllers)
		if ((m_ControllerId == ControllerInputManager.eControllerId.CONTROLLER_01 && ControllerInputManager.Instance.GetButton(ControllerInputManager.eButtonAliases.CHARACTER_1_GRAB.ToString()).Count > 0) || 
		    (m_ControllerId == ControllerInputManager.eControllerId.CONTROLLER_02 && ControllerInputManager.Instance.GetButton(ControllerInputManager.eButtonAliases.CHARACTER_2_GRAB.ToString()).Count > 0))
		{
			GrabObstacle();
		}
		else
		{
			StartCoroutine("ReleaseObstacle");
		}
	}

	private void HandleTwoPlayerMode()
	{
		// Movements.
		Vector2 movement = Vector2.zero;
		
		Dictionary<ControllerInputManager.eControllerId, Vector2> movements = ControllerInputManager.Instance.GetLeftJoystick();
		// Detect a controller.
		if (movements.ContainsKey(m_ControllerId))
		{
			movement = movements[m_ControllerId];
		}
		// Mouse is player 1.
		else if (m_ControllerId == ControllerInputManager.eControllerId.CONTROLLER_01 && 
		         ControllerInputManager.Instance.MouseControllerId != ControllerInputManager.eControllerId.CONTROLLER_02 && 
		         movements.ContainsKey(ControllerInputManager.Instance.MouseControllerId))
		{
			movement = movements[ControllerInputManager.Instance.MouseControllerId];
		}
		
		if (movement != Vector2.zero)
		{
			float weightFactor = 1.0f;
			if (m_GrabbedObstacleJoin != null && m_GrabbedObstacleHandle != null)
			{
				weightFactor = m_GrabbedObstacleHandle.m_Obstacle.FullWeightFactor;
			}
			
			movement.x *= Time.deltaTime * SPEED.x * weightFactor;
			movement.y *= Time.deltaTime * SPEED.y * weightFactor;
			
			Vector3 newMovement = /*Quaternion.Euler(m_HandCamera.transform.eulerAngles) */ new Vector3(movement.x, 0.0f, movement.y);
			transform.position += newMovement;
		}
		
		// Buttons (controllers and mouse)
		if (ControllerInputManager.Instance.GetButton(m_ControllerId, ControllerInputManager.eButtonAliases.GRAB.ToString()) || 
		    (m_ControllerId == ControllerInputManager.eControllerId.CONTROLLER_01 && 
			 ControllerInputManager.Instance.MouseControllerId != ControllerInputManager.eControllerId.CONTROLLER_02 && 
			 ControllerInputManager.Instance.GetButton(ControllerInputManager.Instance.MouseControllerId, ControllerInputManager.eButtonAliases.GRAB.ToString())))
		{
			GrabObstacle();
		}
		else
		{
			StartCoroutine("ReleaseObstacle");
		}
	}
	#endregion
}
