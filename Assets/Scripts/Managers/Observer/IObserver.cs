using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IObserver 
{
	void RegisterSubjectList();
	void UnregisterSubjectList();
	void OnNotify(ISubject subject, object args);
}
