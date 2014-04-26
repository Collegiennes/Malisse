using System;

[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public class ControllerAttribute : Attribute 
{
	public string m_DetectionName;
	public ControllerAttribute(string detectionName)
	{
		m_DetectionName = detectionName;
	}
}
