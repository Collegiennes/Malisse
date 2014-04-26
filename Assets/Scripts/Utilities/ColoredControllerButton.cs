using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ColoredControllerButton : ControllerButton 
{
	#region Members and Properties
	// constants
	private readonly Color BUTTON_OUT_COLOR = new Color(0.5f, 0.5f, 0.5f, 1.0f);
	
	// enums
	
	// public
	public Renderer m_ButtonAsset = null;
	
	// protected
	
	// private
	private Color m_InitialColor = Color.white;
	
	// properties
	#endregion
	
	#region Unity API
	protected override void Awake()
	{
		base.Awake();
		
		if (m_ButtonAsset != null && m_ButtonAsset.material != null)
		{
			m_InitialColor = m_ButtonAsset.material.color;
		}
	}
	#endregion
	
	#region Public Functions
	#endregion

	#region ControllerButton
	public override void OnTouchOver(TouchEvent touchEvent)
	{
		base.OnTouchOver(touchEvent);
		
		if (m_ButtonAsset != null && m_ButtonAsset.material != null)
		{
			m_ButtonAsset.renderer.material.color = m_InitialColor;
		}
	}
	
	public override void OnTouchOut(TouchEvent touchEvent)
	{
		base.OnTouchOut(touchEvent);

		if (m_ButtonAsset != null && m_ButtonAsset.material != null)
		{
			m_ButtonAsset.renderer.material.color = BUTTON_OUT_COLOR;
		}
	}
	#endregion
	
	#region Protected Functions
	#endregion
	
	#region Private Functions
	#endregion
}
