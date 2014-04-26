using UnityEngine;
using System.Collections;

public class AnimationController : MonoBehaviour
{
	public enum EaseInOutType
	{
		Linear, Quad, Expo, ExpoInverse, Cubique
	}
	
	public static float GetInterpolationRatio(float startValue, float endValue, float currentValue, EaseInOutType type) 
	{
		if (endValue < startValue) endValue = startValue;
		if (currentValue < startValue) currentValue = startValue;
		
		float ratio = (currentValue - startValue) / (endValue - startValue);
		
		switch (type)
		{
			case EaseInOutType.Quad:
				return Mathf.Clamp01(-(Mathf.Cos(Mathf.PI * Mathf.Clamp01(ratio)) / 2.0f) + 0.5f);
			case EaseInOutType.Expo:
				return Mathf.Clamp01(Mathf.Pow(Mathf.Clamp01(ratio), 2));
			case EaseInOutType.ExpoInverse:
				return -Mathf.Clamp01(Mathf.Pow(Mathf.Clamp01(ratio) - 1.0f, 2)) + 1.0f;
			case EaseInOutType.Cubique:
				return Mathf.Clamp01(Mathf.Pow(Mathf.Clamp01(ratio), 3));
		}
		
		return ratio; // Linear
	}
	
	public static Vector3 GetInterpolationVector3(Vector3 oldValue, Vector3 newValue, float ratio)
	{
		Vector3 currentValue = Vector3.zero;
		currentValue.x = Mathf.Lerp(oldValue.x, newValue.x, ratio);
		currentValue.y = Mathf.Lerp(oldValue.y, newValue.y, ratio);
		currentValue.z = Mathf.Lerp(oldValue.z, newValue.z, ratio);
		
		return currentValue;
	}
	
	public static AnimationController AnimateObject(GameObject animatedObject, Vector3 newPosition, Vector3 newAngle, float duration, EaseInOutType type, AnimateObjectCompleted animationDelegate, object delegateParameter)
	{
		GameObject newAnimationObj = new GameObject(animatedObject + "_animation");
		//newAnimationObj.transform.parent = animatedObject.transform;
		
		AnimationController newAnimation = newAnimationObj.AddComponent<AnimationController>();
		
		newAngle = MathfPlus.GetClosestAngle(animatedObject.transform.eulerAngles, newAngle);
		
		newAnimation.m_AnimationDelegate = animationDelegate;
		newAnimation.m_DelegateParameter = delegateParameter;
		
		newAnimation.m_AnimatedObject = animatedObject;
		newAnimation.m_NewPosition = newPosition;
		newAnimation.m_NewAngle = newAngle;
		newAnimation.m_Duration = duration;
		newAnimation.m_Type = type;
		
		return newAnimation;
	}
	
	public static AnimationController AnimateCamera(Camera animatedCamera, Vector3 newPosition, Vector3 newAngle, float newOrthographicSize, float duration, EaseInOutType type, AnimateObjectCompleted animationDelegate, object delegateParameter)
	{
		AnimationController newAnimation = null;
		if (animatedCamera != null)
		{
			newAnimation = AnimateObject(animatedCamera.gameObject, newPosition, newAngle, duration, type, animationDelegate, delegateParameter);
			newAnimation.m_AnimatedCamera = animatedCamera;
			newAnimation.m_NewOrthographicSize = Mathf.Clamp(newOrthographicSize, 1, newOrthographicSize);
		}
		
		return newAnimation;
	}
	
	public static AnimationController AnimateRigidbody(Rigidbody animatedRigidbody, Vector3 newPosition, Vector3 newAngle, float duration, EaseInOutType type, AnimateObjectCompleted animationDelegate, object delegateParameter)
	{
		AnimationController newAnimation = null;
		if (animatedRigidbody != null)
		{
			newAnimation = AnimateObject(animatedRigidbody.gameObject, newPosition, newAngle, duration, type, animationDelegate, delegateParameter);
			newAnimation.m_AnimatedRigidbody = animatedRigidbody;
		}
		
		return newAnimation;
	}
	
	/**
	 * Class Object stuff.
	 */
	public delegate void AnimateObjectCompleted(GameObject animatedObject, object delegateParameter);
	public AnimateObjectCompleted m_AnimationDelegate = null;
	
	public GameObject m_AnimatedObject;
	public Camera m_AnimatedCamera;
	public Rigidbody m_AnimatedRigidbody;
	
	public Vector3 m_NewPosition = Vector3.zero;
	public Vector3 m_NewAngle = Vector3.zero;
	public float m_NewOrthographicSize;
	public float m_Duration;
	public EaseInOutType m_Type = EaseInOutType.Quad;
	public bool m_IsLocal = true;
	public object m_DelegateParameter;
	
	private bool m_Paused;
	
	public void StartAnimation()
	{
		StopCoroutine("AnimateObjectCoroutine");
		if (gameObject != null && gameObject.activeInHierarchy)
		{
			if (m_AnimatedRigidbody != null)
			{
				if (m_IsLocal && m_NewPosition == m_AnimatedRigidbody.transform.localPosition && m_NewAngle == m_AnimatedRigidbody.transform.localEulerAngles)
				{
					StopAnimation();
					return;
				}
				else if (!m_IsLocal && m_NewPosition == m_AnimatedRigidbody.transform.position && m_NewAngle == m_AnimatedRigidbody.transform.eulerAngles)
				{
					StopAnimation();
					return;
				}
			}
			else if (m_AnimatedCamera != null)
			{
				if (m_IsLocal && m_NewPosition == m_AnimatedCamera.transform.localPosition && m_NewAngle == m_AnimatedCamera.transform.localEulerAngles && m_NewOrthographicSize == m_AnimatedCamera.orthographicSize)
				{
					StopAnimation();
					return;
				}
				else if (!m_IsLocal && m_NewPosition == m_AnimatedCamera.transform.position && m_NewAngle == m_AnimatedCamera.transform.eulerAngles && m_NewOrthographicSize == m_AnimatedCamera.orthographicSize)
				{
					StopAnimation();
					return;
				}
			}
			else if (m_AnimatedObject != null)
			{
				if (m_IsLocal && m_NewPosition == m_AnimatedObject.transform.localPosition && m_NewAngle == m_AnimatedObject.transform.localEulerAngles)
				{
					StopAnimation();
					return;
				}
				else if (!m_IsLocal && m_NewPosition == m_AnimatedObject.transform.position && m_NewAngle == m_AnimatedObject.transform.eulerAngles)
				{
					StopAnimation();
					return;
				}
			}
			
			StartCoroutine("AnimateObjectCoroutine");
		}
	}
	
	public void StopAnimation()
	{
		StopCoroutine("AnimateObjectCoroutine");
		AnimateObjectEnd(m_AnimatedObject, m_DelegateParameter);
	}
	
	public void Pause(bool paused)
	{
		m_Paused = paused;
	}
	
	private IEnumerator AnimateObjectCoroutine()
	{
		Vector3 oldPosition = Vector3.zero;
		Vector3 oldAngle = Vector3.zero;
		if (m_IsLocal)
		{
			oldPosition = m_AnimatedObject.transform.localPosition;
			oldAngle = m_AnimatedObject.transform.localEulerAngles;
		}
		else
		{
			oldPosition = m_AnimatedObject.transform.position;
			oldAngle = m_AnimatedObject.transform.eulerAngles;
		}
		
		float ratio = 0.0f;
		
		float currentDuration = m_Duration;
		float oldOrthographicSize = m_AnimatedCamera == null ? 0.0f : m_AnimatedCamera.orthographicSize;
		//Vector3 rigidbodyNewLocalPosition = Vector3.zero;
		while (currentDuration > 0.0f)
		{
			currentDuration -= Time.deltaTime;
			ratio = AnimationController.GetInterpolationRatio(0.0f, m_Duration, m_Duration-currentDuration, m_Type);
			
			if (m_AnimatedCamera != null)
			{
				m_AnimatedCamera.orthographicSize = Mathf.Lerp(oldOrthographicSize, m_NewOrthographicSize, ratio);
			}
			
			if (m_IsLocal)
			{
				/*if (m_AnimatedRigidbody != null)
				{
					rigidbodyNewLocalPosition = GetInterpolationVector3(oldPosition, m_NewPosition, ratio);
					m_AnimatedRigidbody.MovePosition(m_AnimatedRigidbody.transform.TransformPoint(rigidbodyNewLocalPosition));
				}
				else*/
				{
					m_AnimatedObject.transform.localPosition = GetInterpolationVector3(oldPosition, m_NewPosition, ratio);
				}
				m_AnimatedObject.transform.localEulerAngles = GetInterpolationVector3(oldAngle, m_NewAngle, ratio);
			}
			else
			{
				if (m_AnimatedRigidbody != null)
				{
					m_AnimatedRigidbody.MovePosition(GetInterpolationVector3(oldPosition, m_NewPosition, ratio));
				}
				else
				{
					m_AnimatedObject.transform.position = GetInterpolationVector3(oldPosition, m_NewPosition, ratio);
				}
				m_AnimatedObject.transform.eulerAngles = GetInterpolationVector3(oldAngle, m_NewAngle, ratio);
			}
			
			if (m_Paused)
			{
				yield return null;
			}
			yield return null;
		}
		
		if (m_AnimatedCamera != null)
		{
			m_AnimatedCamera.orthographicSize = m_NewOrthographicSize;
		}
		
		if (m_IsLocal)
		{
			m_AnimatedObject.transform.localPosition = m_NewPosition;
			m_AnimatedObject.transform.localEulerAngles = m_NewAngle;
		}
		else
		{
			if (m_AnimatedRigidbody != null)
			{
				m_AnimatedRigidbody.MovePosition(m_NewPosition);
			}
			else
			{
				m_AnimatedObject.transform.position = m_NewPosition;
			}
			m_AnimatedObject.transform.eulerAngles = m_NewAngle;
		}
		
		AnimateObjectEnd(m_AnimatedObject, m_DelegateParameter);
	}
	
	private void AnimateObjectEnd(GameObject animatedObject, object delegateParameter)
	{
		if (m_AnimationDelegate != null)
		{
			m_AnimationDelegate(animatedObject, delegateParameter);
		}
		
		Destroy(gameObject);
	}
}
