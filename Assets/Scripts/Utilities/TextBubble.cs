using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TextBubble : Button 
{
	#region Members and Properties
	// constants
	private const string SET_POSITION_TEST_CHARACTER = "O";

	// enums
	public enum eBubbleStates
	{
		BEGIN,
		READY,
		DONE
	}

	public enum eTextAlignment
	{
		TOP_LEFT,
		CENTER
	}

	// public
	public bool m_IsEnabled = true;
	public List<string> m_TextKeys = new List<string>();
	public TextMesh m_Text = null;
	public Collider m_TextBounds = null;
	public float m_CharacterDisplayDuration = 0.1f;
	public bool m_Skippable = true;
	public bool m_PlayOnStart = true;
	public Vector2 m_TextPadding = Vector2.zero;
	public bool m_CheckNextLineFits = true;
	public eTextAlignment m_TextAlignment = eTextAlignment.TOP_LEFT;

	// protected
	protected int m_NextTextIndex = 0;
	protected string m_CurrentText = "";
	protected bool m_IsDisplayingCharacters = false;
	protected float m_CurrentCharacterDisplayDuration = 0.0f;
	protected bool m_CurrentTextIsNext = false;
	protected bool m_IsInitialized;

	// private

	// properties
	#endregion
	
	#region Unity API
	// Use this for initialization
	protected override void Start() 
	{
		base.Start();
		
		Initialize();
		if (m_PlayOnStart)
		{
			LoadNextText();
		}
	}
	#endregion
	
	#region Public Functions
	public virtual void Reset()
	{
		Initialize();
		
		StopCoroutine("DisplayCharacters");
		
		if (m_Text != null)
		{
			m_Text.text = "";
		}
		m_NextTextIndex = 0;
		m_CurrentText = "";
		m_IsDisplayingCharacters = false;
		m_CurrentCharacterDisplayDuration = 0.0f;
		m_CurrentTextIsNext = false;
	}
	
	public virtual void SetEnabled(bool enabled)
	{
		Initialize();
		
		m_IsEnabled = enabled;
	}
	
	public void SetSingleText(string text)
	{
		m_TextKeys.Clear();
		m_TextKeys.Add(text);
		
		Reset();
		LoadNextText();
	}
	
	public void LoadNextText()
	{
		Initialize();
		
		if (m_IsDisplayingCharacters)
		{
			if (m_Skippable)
			{
				m_CurrentCharacterDisplayDuration = 0.0f;
			}
		}
		else if (m_NextTextIndex < m_TextKeys.Count || m_CurrentTextIsNext)
		{	
			StopCoroutine("DisplayCharacters");
			
			if (!m_CurrentTextIsNext)
			{
				m_CurrentText = GetText(m_TextKeys[m_NextTextIndex]);
				m_NextTextIndex++;
				
				NotifyObservers(eBubbleStates.BEGIN);
			}
			else
			{
				m_CurrentTextIsNext = false;
			}
			
			m_CurrentCharacterDisplayDuration = m_CharacterDisplayDuration;
			
			if (gameObject.activeInHierarchy)
			{
				StartCoroutine("DisplayCharacters");
			}
		}
	}
	#endregion

	#region Button Implementation
	public override void OnTouchUp(TouchEvent touchEvent) 
	{
		if (m_IsEnabled)
		{
			base.OnTouchUp(touchEvent);
			
			LoadNextText();
		}
	}
	#endregion
	
	#region Protected Functions	
	protected virtual string GetText(string textKey)
	{
		return textKey;
	}
	
	protected void Initialize()
	{
		if (!m_IsInitialized)
		{
			m_IsInitialized = true;
			
			if (m_CharacterDisplayDuration < 0.0f)
			{
				m_CharacterDisplayDuration = 0.0f;
			}
			
			if (m_Text == null)
			{
				m_Text = GetComponentInChildren<TextMesh>();
			}
			if (m_TextBounds == null)
			{
				m_TextBounds = GetComponentInChildren<Collider>();
			}
			
			PositionInitialText();
		}
	}
	#endregion
	
	#region Private Functions
	private IEnumerator DisplayCharacters()
	{
		m_IsDisplayingCharacters = true;
		
		if (m_Text != null)
		{
			m_Text.text = "";
			string[] words = m_CurrentText.Split(" "[0]);
			foreach (string word in words)
			{
				if (!IsNextWordFits(word))
				{
					m_Text.text += "\n";
					if (!IsNextLineFits(word))
					{
						m_CurrentTextIsNext = true;
						break;
					}
				}
				
				foreach (char character in word.ToCharArray())
				{
					if (m_CurrentCharacterDisplayDuration > 0.0f)
					{
						yield return new WaitForSeconds(m_CurrentCharacterDisplayDuration);
					}
					m_Text.text += character.ToString();
					m_CurrentText = RemoveCharacter(m_CurrentText, character);
				}
				
				if (m_CurrentCharacterDisplayDuration > 0.0f)
				{
					yield return new WaitForSeconds(m_CurrentCharacterDisplayDuration);
				}
				m_Text.text += " ";
				m_CurrentText = RemoveCharacter(m_CurrentText, ' ');
			}
		}
		
		m_IsDisplayingCharacters = false;
		if (m_NextTextIndex >= m_TextKeys.Count && !m_CurrentTextIsNext)
		{
			NotifyObservers(eBubbleStates.DONE);
		}
		else
		{
			NotifyObservers(eBubbleStates.READY);
		}
	}
	
	private void PositionInitialText()
	{
		if (m_TextBounds != null && m_Text != null)
		{
			m_Text.text = SET_POSITION_TEST_CHARACTER;

			switch (m_TextAlignment)
			{
			case eTextAlignment.TOP_LEFT:
				// Top left.
				Vector3 newPosition = m_Text.transform.position;
				newPosition.x += ((m_TextBounds.bounds.min.x + m_TextPadding.x) - m_Text.renderer.bounds.min.x);
				newPosition.y += ((m_TextBounds.bounds.max.y - m_TextPadding.y) - m_Text.renderer.bounds.max.y);
				m_Text.transform.position = newPosition;
				break;
			}

			m_Text.text = "";
		}
	}
	
	private string RemoveCharacter(string text, char character)
	{
		int index = text.IndexOf(character);
		if (index >= 0)
		{
			text = text.Remove(index, 1);
		}
		return text;
	}
	
	private bool IsNextWordFits(string nextWord)
	{
		bool nextWordOk = true;
		if (m_TextBounds != null && m_Text != null)
		{
			string oldText = m_Text.text;
			m_Text.text += nextWord;
			if (m_Text.renderer.bounds.max.x > (m_TextBounds.bounds.max.x - m_TextPadding.x))
			{
				nextWordOk = false;
			}
			
			m_Text.text = oldText;
		}
		
		return nextWordOk;
	}
	
	private bool IsNextLineFits(string nextWord)
	{
		bool nextWordOk = true;
		if (m_CheckNextLineFits && m_TextBounds != null && m_Text != null)
		{
			string oldText = m_Text.text;
			m_Text.text += nextWord;
			if (m_Text.renderer.bounds.min.y < (m_TextBounds.bounds.min.y + m_TextPadding.y))
			{
				nextWordOk = false;
			}
			
			m_Text.text = oldText;
		}
		
		return nextWordOk;
	}
	#endregion
}
