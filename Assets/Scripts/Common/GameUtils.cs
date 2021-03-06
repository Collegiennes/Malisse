﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameUtils 
{
	#region Members and Properties
	// constants
	
	// enums
	public enum eGameMode
	{
		ONE_PLAYER,
		TWO_PLAYER
	}

	// public
	public static eGameMode m_GameMode = eGameMode.ONE_PLAYER;
	
	// protected
	
	// private
	
	// properties
	#endregion
	
	#region Public Functions
	public static View FindAssociatedView(Transform current)
	{
		View view = null;
		if (current != null)
		{
			view = current.gameObject.GetComponent<View>();
			if (view == null)
			{
				view = FindAssociatedView(current.parent);
			}
		}
		
		return view;
	}
    public static Level FindAssociatedLevel(Transform current)
    {
        Level level = null;
        if (current != null)
        {
            level = current.gameObject.GetComponent<Level>();
            if (level == null)
            {
                level = FindAssociatedLevel(current.parent);
            }
        }

        return level;
    }
	#endregion
	
	#region Protected Functions
	#endregion
	
	#region Private Functions
	#endregion
}
