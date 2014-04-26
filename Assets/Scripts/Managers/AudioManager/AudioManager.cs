using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
	private static bool MUSIC_CROSSFADE_FADEOUT_FADEIN = true;
	private static bool MUSIC_CROSSFADE_WITH_FADEIN = false;
	
	/**
	 * Singleton stuff.
	 */
	private static bool m_IsDestroyed = false;
	private static AudioManager m_Instance = null;
	
	public static AudioManager Instance
	{
		get
		{
			if (m_IsDestroyed)
			{
				return null;
			}
			
			if (m_Instance == null)
			{
				GameObject audioManager = new GameObject("AudioManager");
				m_Instance = audioManager.AddComponent<AudioManager>();
			}
			
			return m_Instance;
		}
	}
	
	/**
	 * Object class stuff.
	 */
	private float m_MusicVolume = 0.5f;
	private float m_SFXVolume = 0.5f;
	private float m_CrossFadeMusicDuration = 1.0f;
	private AudioClip m_MusicLoopClip;
	
	private AudioSource m_OldAudioSourceMusic = null;
	private AudioSource m_AudioSourceMusic = null;
	private AudioSourceMulti m_AudioSourceSFX = null;
	private AudioSourceMulti m_AudioSourceLoopingSFX = null;
	
	protected virtual void Start()
	{
		// Stays in every scenes.
		DontDestroyOnLoad(this);
	}
	
	protected virtual void OnDestroy()
	{
		m_IsDestroyed = true;
	}
	
	public void PlayMusic(AudioClip music)
	{
		PlayMusic(music, m_MusicVolume);
	}
	
	public void PlayMusic(AudioClip intro, AudioClip music)
	{
		PlayMusic(intro, music, m_MusicVolume);
	}
	
	public void PlayMusic(AudioClip music, float volume)
	{
		PlayMusic(null, music, volume);
	}
	
	public void PlayMusic(AudioClip intro, AudioClip music, float volume)
	{
		PlayMusic(intro, music, volume, m_CrossFadeMusicDuration);
	}
	
	public void PlayMusic(AudioClip intro, AudioClip music, float volume, float crossFadeDuration)
	{
		PlayMusic(intro, music, volume, crossFadeDuration, 0.0f);
	}
	
	public void PlayMusic(AudioClip intro, AudioClip music, float volume, float crossFadeDuration, float atSeconds)
	{
		if (music != null)
		{
			if (m_AudioSourceMusic != null && music == m_AudioSourceMusic.clip)
			{
				return;
			}
			
			m_MusicLoopClip = intro != null ? music : null;
			
			if (m_AudioSourceMusic == null)
			{
				GameObject audioSourceMusicObj = new GameObject("AudioSource_Music_" + music);
				audioSourceMusicObj.transform.parent = transform;
				m_AudioSourceMusic = audioSourceMusicObj.AddComponent<AudioSource>();
			
				m_AudioSourceMusic.clip = intro != null ? intro : music;
				m_AudioSourceMusic.volume = volume;
				m_AudioSourceMusic.loop = (intro == null);
				
		        m_AudioSourceMusic.time = atSeconds;
		        m_AudioSourceMusic.Play();
				
				StopAllCoroutines();
				StartCoroutine(PlayMusicIntro(volume));
			}
			else
			{
				StopAllCoroutines();
				if (m_OldAudioSourceMusic != null)
				{
					Destroy(m_OldAudioSourceMusic.gameObject);
					m_OldAudioSourceMusic = null;
				}
				StartCoroutine(CrossFadeMusic(intro != null ? intro : music, crossFadeDuration, volume, atSeconds));
			}
		}
	}
	
	public void StopMusic()
	{
		if (m_AudioSourceMusic != null)
		{
			StopAllCoroutines();
			
			m_AudioSourceMusic.Stop();
			GameObject.Destroy(m_AudioSourceMusic.gameObject);
			
			m_AudioSourceMusic = null;
		}
	}
	
	public void StopLoopingSFX()
	{
		if (m_AudioSourceLoopingSFX != null)
		{
			m_AudioSourceLoopingSFX.Stop();
		}
	}
	
	public void StopLoopingSFX(AudioClip sfx)
	{
		if (m_AudioSourceLoopingSFX != null)
		{
			m_AudioSourceLoopingSFX.Stop(sfx);
		}
	}
	
	public void PlaySFX(AudioClip sfx)
	{
		PlaySFX(sfx, m_SFXVolume);
	}
	
	public void PlaySFX(List<AudioClip> sfxList)
	{
		PlaySFX(sfxList, m_SFXVolume);
	}
	
	public void PlaySFX(List<AudioClip> sfxList, float volume)
	{
		if (sfxList.Count > 0)
		{
			PlaySFX(sfxList[Random.Range(0, sfxList.Count)], volume);
		}
	}
	
	public void PlaySFX(AudioClip sfx, float volume)
	{
		PlaySFX(sfx, volume, 0.0f);
	}
	
	public void PlaySFX(AudioClip sfx, float volume, float delay)
	{
		if (sfx != null)	
		{
			if (m_AudioSourceSFX == null)
			{
				GameObject audioSourceSfxObject = new GameObject("audioSource_SFX");
				audioSourceSfxObject.transform.parent = transform;
				m_AudioSourceSFX = audioSourceSfxObject.AddComponent<AudioSourceMulti>();
			}
			
			m_AudioSourceSFX.Looping = false;
			m_AudioSourceSFX.Play(sfx, volume, delay);
		}
	}
	
	public void PlayLoopingSFX(AudioClip sfx, float volume = 1.0f)
	{
		if (sfx != null)	
		{
			if (m_AudioSourceLoopingSFX == null)
			{
				GameObject audioSourceLoopingSfxObject = new GameObject("audioSource_LoopingSFX");
				audioSourceLoopingSfxObject.transform.parent = transform;
				m_AudioSourceLoopingSFX = audioSourceLoopingSfxObject.AddComponent<AudioSourceMulti>();
			}
			
			m_AudioSourceLoopingSFX.Looping = true;
			m_AudioSourceLoopingSFX.Play(sfx, volume);
		}
	}
	
	public void PauseSFX(bool paused)
	{
		if (m_AudioSourceSFX != null)
		{
			m_AudioSourceSFX.Pause(paused);
		}
	}
	
	public void StopSFX(AudioClip sfx)
	{
		if (m_AudioSourceSFX != null)
		{
			m_AudioSourceSFX.Stop(sfx);
		}
	}
	
	public void ResetMusicVolume()
	{
		if (m_AudioSourceMusic != null)
		{
			m_AudioSourceMusic.volume = m_MusicVolume;
		}
	}
	
	public void SetMusicVolume(float volume)
	{
		if (m_AudioSourceMusic != null)
		{
			m_AudioSourceMusic.volume = volume;
		}
	}
	
	private IEnumerator CrossFadeMusic(AudioClip newMusic, float duration, float volume, float atSeconds)
	{
		// Get the current music audio source.
		m_OldAudioSourceMusic = m_AudioSourceMusic;
		
		// Create a new Music Audio Source.
		GameObject audioSourceMusicObj = new GameObject("audioSource_music_" + newMusic);
		m_AudioSourceMusic = audioSourceMusicObj.AddComponent<AudioSource>();
		m_AudioSourceMusic.transform.parent = transform;
		m_AudioSourceMusic.clip = newMusic;
		m_AudioSourceMusic.volume = 0.0f;
		m_AudioSourceMusic.loop = (m_MusicLoopClip == null);
		m_AudioSourceMusic.time = atSeconds;
		
		yield return null;
		
		// Fade out the old music audio source.
		float ratio = 0.0f;
		float ratio2 = 0.0f;
		float startTime = Time.time;
		float oldVolume = m_OldAudioSourceMusic.volume;
		bool newMusicStarted = false;
		
		float newMusicDurationRatio = MUSIC_CROSSFADE_FADEOUT_FADEIN ? 1.0f : 3.0f;
		while ((ratio < 1.0f || ratio2 < 1.0f) && duration > 0.0f)
		{
			ratio = AnimationController.GetInterpolationRatio(startTime, startTime+duration, Time.time, AnimationController.EaseInOutType.Quad);
			if (MUSIC_CROSSFADE_WITH_FADEIN)
			{
				ratio2 = AnimationController.GetInterpolationRatio(startTime+(duration/newMusicDurationRatio), startTime+duration+(duration/newMusicDurationRatio), Time.time, AnimationController.EaseInOutType.Quad);
			}
			else
			{
				ratio2 = Time.time >= startTime+(duration/newMusicDurationRatio) ? 1.0f : 0.0f;
			}
			m_OldAudioSourceMusic.volume = Mathf.Lerp(oldVolume, 0.0f, ratio);
			m_AudioSourceMusic.volume = Mathf.Lerp(0.0f, volume, ratio2);
			if (!m_AudioSourceMusic.isPlaying && ratio2 >= 0.33f)
			{
				m_AudioSourceMusic.Play();
			}
			
			if (!newMusicStarted && m_AudioSourceMusic.volume > 0.0f)
			{
				newMusicStarted = true;
			}
			
			yield return null;
		}
		m_OldAudioSourceMusic.volume = 0.0f;
		m_AudioSourceMusic.volume = volume;
		if (!m_AudioSourceMusic.isPlaying)
		{
			m_AudioSourceMusic.Play();
		}
		
		// Destroy the old music audio source.
		m_OldAudioSourceMusic.Stop();
		GameObject.Destroy(m_OldAudioSourceMusic.gameObject);
		m_OldAudioSourceMusic = null;
		
		StartCoroutine(PlayMusicIntro(volume));
	}
	
	private IEnumerator PlayMusicIntro(float volume)
	{
		yield return null;
		
		if (m_MusicLoopClip != null && m_AudioSourceMusic != null && m_AudioSourceMusic.clip != null)
		{
			// Get the current music audio source.
			m_OldAudioSourceMusic = m_AudioSourceMusic;
			
			// Create a new Music Audio Source.
			GameObject audioSourceMusicObj = new GameObject("audioSource_music_" + m_MusicLoopClip);
			m_AudioSourceMusic = audioSourceMusicObj.AddComponent<AudioSource>();
			m_AudioSourceMusic.transform.parent = transform;
			m_AudioSourceMusic.clip = m_MusicLoopClip;
			m_AudioSourceMusic.volume = 0.0f;
			m_AudioSourceMusic.loop = true;
			m_AudioSourceMusic.time = 0.0f;
			m_AudioSourceMusic.Play();
			
			while (m_OldAudioSourceMusic.isPlaying && m_OldAudioSourceMusic.time < m_OldAudioSourceMusic.clip.length-0.05f)
			{
				yield return null;
			}
			
			m_AudioSourceMusic.Play();
			m_AudioSourceMusic.volume = volume;
		
			// Destroy the old music audio source.
			m_OldAudioSourceMusic.volume = 0.0f;
			m_OldAudioSourceMusic.Stop();
			GameObject.Destroy(m_OldAudioSourceMusic.gameObject);
			m_OldAudioSourceMusic = null;
		}
	}
}
