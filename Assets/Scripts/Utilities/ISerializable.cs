using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface ISerializable 
{
	string Serialize();
	void Deserialize(string serialization);
}
