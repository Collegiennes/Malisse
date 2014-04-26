using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Observer : MonoBehaviour, IObserver
{
	public List<ISubject> m_SubjectList = new List<ISubject>();
	
	protected virtual void Awake()
	{
		if (Application.isPlaying)
		{
			RegisterSubjectList();
		}
	}
	
	public void RegisterSubjectList()
	{
		foreach (ISubject subject in m_SubjectList)
		{
			if (subject != null)
			{
				subject.AddObserver(this);
			}
		}
	}
	
	public void UnregisterSubjectList()
	{
		foreach (ISubject subject in m_SubjectList)
		{
			if (subject != null)
			{
				subject.RemoveObserver(this);
			}
		}
	}
	
	public virtual void OnNotify(ISubject subject, object args)
	{
	}
}
