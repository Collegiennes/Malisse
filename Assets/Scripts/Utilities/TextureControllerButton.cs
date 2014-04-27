using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TextureControllerButton : ControllerButton 
{
	#region Members and Properties
	// constants
	private readonly Color BUTTON_OUT_COLOR = new Color(0.5f, 0.5f, 0.5f, 1.0f);
	
	// enums
	
	// public
	public tk2dSprite m_Sprite = null;
	public string m_UpSpriteName = null;
	public string m_DownSpriteName = null;
	public string m_HoverSpriteName = null;
	
	// protected
	
	// private
	private Color m_InitialColor = Color.white;
	
	// properties
	#endregion
	
	#region Unity API
	#endregion
	
	#region Public Functions
	#endregion

	#region ControllerButton
	public override void OnTouchOver(TouchEvent touchEvent)
	{
		base.OnTouchOver(touchEvent);
		
		if (m_Sprite != null)
		{
			m_Sprite.spriteId = m_Sprite.GetSpriteIdByName(m_HoverSpriteName);
		}
	}
	
	public override void OnTouchOut(TouchEvent touchEvent)
	{
		base.OnTouchOut(touchEvent);
		
		if (m_Sprite != null)
		{
			m_Sprite.spriteId = m_Sprite.GetSpriteIdByName(m_UpSpriteName);
		}
	}
	
	public override bool OnTouchDown(TouchEvent touchEvent)
	{
		if (base.OnTouchDown(touchEvent))
		{
			if (m_Sprite != null)
			{
				m_Sprite.spriteId = m_Sprite.GetSpriteIdByName(m_DownSpriteName);
			}

			return true;
		}

		return false;
	}
	
	public override void OnTouchUp(TouchEvent touchEvent)
	{
		base.OnTouchUp(touchEvent);

		if (m_IsOver)
		{
			if (m_Sprite != null)
			{
				m_Sprite.spriteId = m_Sprite.GetSpriteIdByName(m_HoverSpriteName);
			}
		}
		else
		{
			if (m_Sprite != null)
			{
				m_Sprite.spriteId = m_Sprite.GetSpriteIdByName(m_UpSpriteName);
			}
		}
	}
	#endregion
	
	#region Protected Functions
	#endregion
	
	#region Private Functions
	#endregion
}
