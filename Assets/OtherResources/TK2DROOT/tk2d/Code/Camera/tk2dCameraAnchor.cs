using UnityEngine;
using System.Collections;

[AddComponentMenu("2D Toolkit/Camera/tk2dCameraAnchor")]
[ExecuteInEditMode]
/// <summary>
/// Anchors children to anchor position, offset by number of pixels
/// </summary>
public class tk2dCameraAnchor : MonoBehaviour 
{
	/// <summary>
	/// Anchor.
	/// </summary>
    public enum Anchor
    {
		/// <summary>Upper left</summary>
		UpperLeft,
		/// <summary>Upper center</summary>
		UpperCenter,
		/// <summary>Upper right</summary>
		UpperRight,
		/// <summary>Middle left</summary>
		MiddleLeft,
		/// <summary>Middle center</summary>
		MiddleCenter,
		/// <summary>Middle right</summary>
		MiddleRight,
		/// <summary>Lower left</summary>
		LowerLeft,
		/// <summary>Lower center</summary>
		LowerCenter,
		/// <summary>Lower right</summary>
		LowerRight,
    }
	
	/// <summary>
	/// Anchor location
	/// </summary>
	public Anchor anchor;
	/// <summary>
	/// Offset in pixels
	/// </summary>
	public Vector2 offset = Vector2.zero;
	
	public tk2dCamera tk2dCamera;
	
	Transform __transform; // cache transform locally
	Transform _transform {
		get {
			if (__transform == null) __transform = transform;
			return __transform;
		}
	}
	
	void Start()
	{
		UpdateTransform();
	}
	
	void UpdateTransform()
	{
		if (tk2dCamera != null)
		{
			Rect rect = tk2dCamera.ScreenExtents;

			float y_top = rect.yMin;
			float y_bot = rect.yMax;
			float y_ctr = (y_bot + y_top) * 0.5f;

			float x_lhs = rect.xMin;
			float x_rhs = rect.xMax;
			float x_ctr = (x_lhs + x_rhs) * 0.5f;

			Vector3 position = _transform.localPosition;	
			Vector3 anchoredPosition = Vector3.zero;

			switch (anchor)
			{
			case Anchor.UpperLeft: 		anchoredPosition = new Vector3(x_lhs, y_top, position.z); break;
			case Anchor.UpperCenter: 	anchoredPosition = new Vector3(x_ctr, y_top, position.z); break;
			case Anchor.UpperRight: 	anchoredPosition = new Vector3(x_rhs, y_top, position.z); break;
			case Anchor.MiddleLeft: 	anchoredPosition = new Vector3(x_lhs, y_ctr, position.z); break;
			case Anchor.MiddleCenter: 	anchoredPosition = new Vector3(x_ctr, y_ctr, position.z); break;
			case Anchor.MiddleRight: 	anchoredPosition = new Vector3(x_rhs, y_ctr, position.z); break;
			case Anchor.LowerLeft: 		anchoredPosition = new Vector3(x_lhs, y_bot, position.z); break;
			case Anchor.LowerCenter: 	anchoredPosition = new Vector3(x_ctr, y_bot, position.z); break;
			case Anchor.LowerRight: 	anchoredPosition = new Vector3(x_rhs, y_bot, position.z); break;
			}
			
			var newPosition = anchoredPosition + new Vector3(offset.x, offset.y, 0);
			var oldPosition = _transform.localPosition;
			if (oldPosition != newPosition)
				_transform.localPosition = newPosition;
		}
	}

	public void ForceUpdateTransform()
	{
		UpdateTransform();
	}
	
	// Update is called once per frame
	void Update () 
	{
		UpdateTransform();
	}
}
