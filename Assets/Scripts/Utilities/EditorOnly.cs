using UnityEngine;
using System.Collections;

public class EditorOnly : MonoBehaviour 
{
#if UNITY_EDITOR
	public bool m_KeepIfNoFlowManager = false;
#endif
	
	private void Start()
	{
#if UNITY_EDITOR
		if (!m_KeepIfNoFlowManager || !FlowManager.IsInstanceNull)
#endif
		{
			Destroy(gameObject);
		}
	}
}
