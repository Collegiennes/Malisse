using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface ISubject 
{
	void AddObserver(IObserver observer);
	void RemoveObserver(IObserver observer);
	void NotifyObservers();
	void NotifyObservers(object args);
}
