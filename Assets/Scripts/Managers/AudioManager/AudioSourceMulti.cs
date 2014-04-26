using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioSourceMulti : MonoBehaviour
{
	private List<AudioSource> m_AudioSourceList = new List<AudioSource>();
	
	private int m_MaxAudioSource = 10;
	private float m_DefaultVolume = 1.0f;
	private bool m_Looping = false;
	
	public int MaxAudioSource
	{
		get { return m_MaxAudioSource; }
		set { m_MaxAudioSource = value; }
	}
	
	public float Volume
	{
		get { return m_DefaultVolume; }
		set { m_DefaultVolume = value; }
	}
	
	public bool Looping
	{
		get { return m_Looping; }
		set 
		{
			m_Looping = value;
			
			foreach (AudioSource audioSource in m_AudioSourceList)
			{
				audioSource.loop = m_Looping;
			}
		}
	}
	
	public void Play(AudioClip clip)
	{
		Play(clip, m_DefaultVolume);
	}
	
	public void Play(AudioClip clip, float volume)
	{
		Play (clip, volume, 0.0f);
	}
	
	public void Play(AudioClip clip, float volume, float delay)
	{
		AudioSource audioSource = null;
		
		if (m_AudioSourceList.Count < m_MaxAudioSource)
		{
			audioSource = gameObject.AddComponent<AudioSource>();
			m_AudioSourceList.Add(audioSource);
		}
		else
		{
			foreach (AudioSource audioSourceTemp in m_AudioSourceList)
			{
				if (!audioSourceTemp.isPlaying)
				{
					audioSource = audioSourceTemp;
					break;
				}
			}
			
			if (audioSource == null && m_AudioSourceList.Count > 0)
			{
				audioSource = m_AudioSourceList[0];
			}
		}
		
		if (audioSource != null)
		{
			if (audioSource.clip != null)
			{
				audioSource.Stop();
			}
			
			// Start the clip.
			audioSource.clip = clip;
			audioSource.loop = m_Looping;
			audioSource.volume = volume;
			
			// Put the audio source at the end.
			m_AudioSourceList.Remove(audioSource);
			m_AudioSourceList.Add(audioSource);
			
			ulong newDelay = (ulong)(delay * 44100);
			audioSource.Play(newDelay);
		}
	}
	
	public void Pause(bool pause)
	{
		foreach (AudioSource audioSource in m_AudioSourceList)
		{
			if (pause)
			{
				audioSource.Pause();
			}
			else
			{
				if (IsPlaying(audioSource))
				{
					audioSource.loop = m_Looping;
					audioSource.Play();
				}
			}
		}
	}
	
	public void Stop()
	{
		foreach (AudioSource audioSource in m_AudioSourceList)
		{
			audioSource.Stop();
		}
	}
	
	public void Stop(AudioClip sfx)
	{
		foreach (AudioSource audioSource in m_AudioSourceList)
		{
			if (audioSource != null && audioSource.isPlaying && audioSource.clip == sfx)
			{
				audioSource.Stop();
			}
		}
	}
	
	private bool IsPlaying(AudioSource audioSource)
	{
		return audioSource.isPlaying || 
				(audioSource.clip != null && 
				audioSource.time > 0 && audioSource.time < audioSource.clip.length);
	}
}
