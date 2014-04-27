using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TextureControllerButton : ControllerButton 
{
	#region Members and Properties
	// constants
	
	// enums
	
	// public
	public tk2dSprite m_Sprite = null;
	public string m_UpSpriteName = null;
	public string m_DownSpriteName = null;
	public string m_HoverSpriteName = null;
	public AudioClip m_SFXClick = null;
	
	// protected
	
	// private
	
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
		if (!string.IsNullOrEmpty(m_ActionName))
		{
			AudioManager.Instance.PlaySFX(m_SFXClick);
		}

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
