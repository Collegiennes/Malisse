using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TouchHandler
{	
	private TouchPriority m_Priority = TouchPriority.Medium;
	private ITouchable m_Touchable;
	private bool m_KeepTouches = false;
	private GameObject m_Owner;
	private List<TouchRawType> m_ClaimedTouches = new List<TouchRawType>();
	
	public TouchPriority Priority
	{
		get
		{
			return m_Priority;
		}
		set
		{
			m_Priority = value;
		}
	}
	
	public ITouchable Touchable
	{
		get
		{
			return m_Touchable;
		}
	}
	
	public bool KeepTouches
	{
		get
		{
			return m_KeepTouches;
		}
		set
		{
			m_KeepTouches = value;
		}
	}
	
	public GameObject Owner
	{
		get
		{
			return m_Owner;
		}
	}
	
	public TouchHandler(ITouchable touchable) 
	{
		m_Touchable = touchable;
		m_Owner = (touchable as MonoBehaviour).gameObject;
		MouseTouchManager.Instance.AddHandler(this);
	}
	
	/*~TouchHandler()
	{
		Release();
	}*/
	
	public void Release()
	{
		if (!MouseTouchManager.IsInstanceNull)
		{
			MouseTouchManager.Instance.RemoveHandler(this);
		}
	}
	
	public bool OnTouchDown(TouchEvent touchEvent)
	{
		bool hasClaimedTouch = false;
		
		if (m_Touchable.OnTouchDown(touchEvent))
		{
			hasClaimedTouch = true;
			m_ClaimedTouches.Add(touchEvent.RawType);
		}
		
		return hasClaimedTouch;
	}
	
	public void OnTouchDrag(TouchEvent touchEvent)
	{
		m_Touchable.OnTouchDrag(touchEvent);
	}
	
	public void OnTouchUp(TouchEvent touchEvent)
	{
		m_Touchable.OnTouchUp(touchEvent);
		m_ClaimedTouches.Remove(touchEvent.RawType);
	}
	
	public void ClearClaimedTouches()
	{
		m_ClaimedTouches.Clear();
	}
	
	public bool HasClaimedTouch(TouchRawType rawType)
	{
		return m_ClaimedTouches.Contains(rawType);
	}
}

public interface ITouchable
{
	bool OnTouchDown(TouchEvent touchEvent);
	void OnTouchDrag(TouchEvent touchEvent);
	void OnTouchUp(TouchEvent touchEvent);
}
