using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BaseSubject : ISubject
{
	private List<IObserver> m_ObserverList = new List<IObserver>();
	
	public void AddObserver(IObserver observer)
	{
		if (observer != null && !m_ObserverList.Contains(observer))
		{
			m_ObserverList.Add(observer);
		}
	}
	
	public void RemoveObserver(IObserver observer)
	{
		if (observer != null && m_ObserverList.Contains(observer))
		{
			m_ObserverList.Remove(observer);
		}
	}
	
	public void NotifyObservers()
	{
		NotifyObservers(null);
	}
	
	public void NotifyObservers(object args)
	{
		foreach (IObserver observer in m_ObserverList)
		{
			if (observer != null)
			{
				observer.OnNotify(this, args);
			}
		}
	}
}
