using UnityEngine;
using System.Collections;

public class RatioCameraManager : CameraManager 
{
	public float m_ScreenGameRatio = 1.7778f;
	public bool m_DrawGizmos;
	public Color m_GizmosBorderColor = Color.white;
	
	private float m_LastWidth;
	private float m_LastHeight;

	// Use this for initialization
	protected override void Start () 
	{
		base.Start();
	}
	
	// Update is called once per frame
	protected virtual void Update () 
	{
		// Make sure the camera is always widescreen.
		if (camera != null && (m_LastWidth != Screen.width || m_LastHeight != Screen.height))
		{
			m_LastWidth = Screen.width;
			m_LastHeight = Screen.height;
			
			float currentWidth = m_LastHeight * m_ScreenGameRatio;
			float newHeightRatio = m_LastWidth / currentWidth;
			float topRatio = (1.0f - newHeightRatio) / 2.0f;
			
			float currentHeight = m_LastWidth / m_ScreenGameRatio;
			float newWidthRatio = m_LastHeight / currentHeight;
			float leftRatio = (1.0f - newWidthRatio) / 2.0f;
		
			camera.rect = new Rect(leftRatio, topRatio, newWidthRatio, newHeightRatio);
		}
	}
	
	public void OnDrawGizmos()
	{
		if (m_DrawGizmos && camera != null && camera.isOrthoGraphic)
		{
			if (m_ScreenGameRatio == 0.0f)
			{
				m_ScreenGameRatio = camera.aspect;
			}
			
			float extentY = camera.orthographicSize;
			
			// TODO: This is a temp. hack. To fix.
			Vector3 topLeft = Vector3.zero;
			Vector3 topRight = Vector3.zero;
			Vector3 bottomLeft = Vector3.zero;
			Vector3 bottomRight = Vector3.zero;
			if (transform.localEulerAngles.x == 90.0f)
			{
				topLeft = new Vector3(transform.position.x - (extentY*m_ScreenGameRatio), transform.position.y, transform.position.z + extentY);
				topRight = new Vector3(transform.position.x + (extentY*m_ScreenGameRatio), transform.position.y, transform.position.z + extentY);
				bottomLeft = new Vector3(transform.position.x - (extentY*m_ScreenGameRatio), transform.position.y, transform.position.z - extentY);
				bottomRight = new Vector3(transform.position.x + (extentY*m_ScreenGameRatio), transform.position.y, transform.position.z - extentY);
			}
			else
			{
				topLeft = new Vector3(transform.position.x - (extentY*m_ScreenGameRatio), transform.position.y + extentY, transform.position.z);
				topRight = new Vector3(transform.position.x + (extentY*m_ScreenGameRatio), transform.position.y + extentY, transform.position.z);
				bottomLeft = new Vector3(transform.position.x - (extentY*m_ScreenGameRatio), transform.position.y - extentY, transform.position.z);
				bottomRight = new Vector3(transform.position.x + (extentY*m_ScreenGameRatio), transform.position.y - extentY, transform.position.z);
			}
			
			/*topLeft = transform.TransformPoint(topLeft);
			topRight = transform.TransformPoint(topRight);
			bottomLeft = transform.TransformPoint(bottomLeft);
			bottomRight = transform.TransformPoint(bottomRight);*/
			
			Gizmos.color = m_GizmosBorderColor;
			//Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawLine(topLeft, topRight);
			Gizmos.DrawLine(topRight, bottomRight);
			Gizmos.DrawLine(bottomRight, bottomLeft);
			Gizmos.DrawLine(bottomLeft, topLeft);
		}
	}
}
