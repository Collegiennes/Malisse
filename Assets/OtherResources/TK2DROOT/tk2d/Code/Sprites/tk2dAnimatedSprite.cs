using UnityEngine;
using System.Collections;

[AddComponentMenu("2D Toolkit/Sprite/tk2dAnimatedSprite")]
/// <summary>
/// Sprite implementation which plays and maintains animations
/// </summary>
public class tk2dAnimatedSprite : tk2dSprite
{
	/// <summary>
	/// <see cref="tk2dSpriteAnimation"/>
	/// </summary>
	public tk2dSpriteAnimation anim;
	/// <summary>
	/// Currently playing/active clip
	/// </summary>
	public int clipId = 0;
	/// <summary>
	/// Interface option to play the animation automatically when instantiated / game is started. Useful for background looping animations.
	/// </summary>
	public bool playAutomatically = false;
	
	// This is now an int so we'll be able to or bitmasks
	static State globalState = 0;

	/// <summary>
	/// Globally pause all animated sprites
	/// </summary>
	public static bool g_paused
	{
		get { return (globalState & State.Paused) != 0; }
		set { globalState = value?State.Paused:(State)0; }
	}

	/// <summary>
	/// Get or set pause state on this current sprite
	/// </summary>
	public bool Paused
	{
		get { return (state & State.Paused) != 0; }
		set 
		{ 
			if (value) state |= State.Paused;
			else state &= ~State.Paused;
		}
	}


	/// <summary>
	/// Interface option to create an animated box collider for this animated sprite
	/// </summary>
	public bool createCollider = false;
	
	/// <summary>
	/// Currently active clip
	/// </summary>
	tk2dSpriteAnimationClip currentClip = null;
	
	/// <summary>
	/// Time into the current clip. This is in clip local time (i.e. (int)clipTime = currentFrame)
	/// </summary>
    float clipTime = 0.0f;

	/// <summary>
	/// This is the frame rate of the current clip. Can be changed dynamicaly, as clipTime is accumulated time in real time.
	/// </summary>
    float clipFps = -1.0f;
	
	/// <summary>
	/// Previous frame identifier
	/// </summary>
	int previousFrame = -1;
	
	/// <summary>
	/// Animation complete delegate 
	/// </summary>
	public delegate void AnimationCompleteDelegate(tk2dAnimatedSprite sprite, int clipId);
	/// <summary>
	/// Animation complete event. This is called when the animation has completed playing. Will not trigger on looped animations
	/// </summary>
	public AnimationCompleteDelegate animationCompleteDelegate;
	
	/// <summary>
	/// Animation event delegate.
	/// </summary>
	public delegate void AnimationEventDelegate(tk2dAnimatedSprite sprite, tk2dSpriteAnimationClip clip, tk2dSpriteAnimationFrame frame, int frameNum);
	/// <summary>
	/// Animation event. This is called when the frame displayed has <see cref="tk2dSpriteAnimationFrame.triggerEvent"/> set.
	/// The triggering frame is passed to the delegate, and the eventInfo / Int / Float can be extracted from there.
	/// </summary>
	public AnimationEventDelegate animationEventDelegate;
	
	enum State 
	{
		Init = 0,
		Playing = 1,
		Paused = 2,
	}
	State state = State.Init; // init state. Do not use elsewhere
	
	new void Start()
	{
		base.Start();
		
		if (playAutomatically)
			Play(clipId);
	}
	
	/// <summary>
	/// Adds a tk2dAnimatedSprite as a component to the gameObject passed in, setting up necessary parameters and building geometry.
	/// </summary>
	public static tk2dAnimatedSprite AddComponent(GameObject go, tk2dSpriteAnimation anim, int clipId)
	{
		var clip = anim.clips[clipId];
		tk2dAnimatedSprite animSprite = go.AddComponent<tk2dAnimatedSprite>();
		animSprite.Collection = clip.frames[0].spriteCollection;
		animSprite.spriteId = clip.frames[0].spriteId;
		animSprite.anim = anim;
		return animSprite;
	}
	
	/// <summary>
	/// Play the active clip. Will restart the clip if called again.
	/// Will restart the clip at clipStartTime if called while the clip is playing.
	/// </summary>
	public void Play()
	{
		Play(clipId);
	}
	
	/// <summary>
	/// Play the active clip, starting "clipStartTime" seconds into the clip. 
	/// Will restart the clip at clipStartTime if called while the clip is playing.
	/// </summary>
	public void Play(float clipStartTime)
	{
		Play(clipId, clipStartTime);
	}
	
	/// <summary>
	/// Play the active clip, starting at the frame specified.
	/// Will restart the clip at frame if called while the clip is playing.
	/// </summary>
	public void PlayFromFrame(int frame)
	{
		PlayFromFrame(clipId, frame);
	}
	
	/// <summary>
	/// Play the specified clip.
	/// Will restart the clip at clipStartTime if called while the clip is playing.
	/// </summary>
	/// <param name='name'>
	/// Name of clip. Try to cache the animation clip Id and use that instead for performance.
	/// </param>
	public void Play(string name)
	{
		int id = anim?anim.GetClipIdByName(name):-1;
		Play(id);
	}
	
	/// <summary>
	/// Play the specified clip, starting at the frame specified.
	/// Will restart the clip at frame if called while the clip is playing.
	/// </summary>
	/// <param name='name'> Name of clip. Try to cache the animation clip Id and use that instead for performance. </param>
	/// <param name='frame'> Frame to start playing from. </param>
	public void PlayFromFrame(string name, int frame)
	{
		int id = anim?anim.GetClipIdByName(name):-1;
		PlayFromFrame(id, frame);
	}
	
	/// <summary>
	/// Play the specified clip, starting "clipStartTime" seconds into the clip.
	/// Will restart the clip at clipStartTime if called while the clip is playing.
	/// </summary>
	/// <param name='name'> Name of clip. Try to cache the animation clip Id and use that instead for performance. </param>
	/// <param name='clipStartTime'> Clip start time in seconds. </param>
	public void Play(string name, float clipStartTime)
	{
		int id = anim?anim.GetClipIdByName(name):-1;
		Play(id, clipStartTime);
	}
	
	/// <summary>
	/// The currently active or playing <see cref="tk2dSpriteAnimationClip"/>
	/// </summary>
	public tk2dSpriteAnimationClip CurrentClip
	{
		get { return currentClip; }
	}
	
	/// <summary>
	/// The current clip time in seconds
	/// </summary>
	public float ClipTimeSeconds
	{
		get { return (clipFps > 0.0f) ? (clipTime / clipFps) : (clipTime / currentClip.fps); }
	}
	
	/// <summary>
	/// Current frame rate of the playing clip. May have been overriden by the user.
	/// Set to 0 to default to the clips fps
	/// </summary>
	public float ClipFps
	{
		get { return clipFps; }
		set 
		{ 
			if (currentClip != null)
			{
				clipFps = (value > 0) ? value : currentClip.fps;
			}
		}
	}
	
	/// <summary>
	/// Stop the currently playing clip.
	/// </summary>
	public void Stop()
	{
		state &= ~State.Playing;
	}
	
	/// <summary>
	/// Stops the currently playing animation and reset to the first frame in the animation
	/// </summary>
	public void StopAndResetFrame()
	{
		if (currentClip != null)
		{
			SwitchCollectionAndSprite(currentClip.frames[0].spriteCollection, currentClip.frames[0].spriteId);
		}
		Stop();
	}
	
	/// <summary>
	/// Is a clip currently playing? Obselete, use <see cref="tk2dSpriteAnimation.Playing"/> instead.
	/// </summary>
	[System.Obsolete]
	public bool isPlaying()
	{
		return Playing;
	}

	/// <summary>
	/// Is a clip currently playing? 
	/// Will return true if the clip is playing, but is paused.
	/// </summary>
	public bool Playing
	{ 
		get { return (state & State.Playing) != 0; }
	}
	
	protected override bool NeedBoxCollider()
	{
		return createCollider;
	}
	
	/// <summary>
	/// Resolves an animation clip by name and returns a unique id.
	/// This is a convenient alias to <see cref="tk2dSpriteAnimation.GetClipIdByName"/>
	/// </summary>
	/// <returns>
	/// Unique Animation Clip Id.
	/// </returns>
	/// <param name='name'>Case sensitive clip name, as defined in <see cref="tk2dSpriteAnimationClip"/>. </param>
	public int GetClipIdByName(string name)
	{
		return anim?anim.GetClipIdByName(name):-1;
	}
	
	/// <summary>
	/// Play the clip specified by identifier.
	/// Will restart the clip at clipStartTime if called while the clip is playing.
	/// </summary>
	/// <param name='id'>
	/// Use <see cref="GetClipIdByName"/> to resolve a named clip id
	/// </param>
	public void Play(int id)
	{
		Play(id, 0.0f);
	}
	
	/// <summary>
	/// Play the clip specified by identifier, starting at the specified frame.
	/// Will restart the clip at clipStartTime if called while the clip is playing.
	/// </summary>
	/// <param name='id'>Use <see cref="GetClipIdByName"/> to resolve a named clip id</param>	
	/// <param name='frame'> Frame to start from. </param>
	public void PlayFromFrame(int id, int frame)
	{
		var clip = anim.clips[id];
		Play(id, (frame + 0.001f) / clip.fps); // offset ever so slightly to round down correctly
	}
	
	// Warps the current active frame to the local time (i.e. float frame number) specified. 
	// Ensure that time doesn't exceed the number of frames. Will warp silently otherwise
	void WarpClipToLocalTime(tk2dSpriteAnimationClip clip, float time)
	{
		clipTime = time;
		int frameId = (int)clipTime % clip.frames.Length;
		tk2dSpriteAnimationFrame frame = clip.frames[frameId];
		
		SwitchCollectionAndSprite(frame.spriteCollection, frame.spriteId);
		if (frame.triggerEvent)
		{
			if (animationEventDelegate != null)
				animationEventDelegate(this, clip, frame, frameId);
		}
		previousFrame = frameId;
	}

	/// <summary>
	/// Play the clip specified by identifier.
	/// Will restart the clip at clipStartTime if called while the clip is playing.
	/// </summary>
	/// <param name='id'>Use <see cref="GetClipIdByName"/> to resolve a named clip id</param>	
	/// <param name='clipStartTime'> Clip start time in seconds. </param>
	public void Play(int clipId, float clipStartTime)
	{
		this.clipId = clipId;
		Play(anim.clips[clipId], clipStartTime, DefaultFps);
	}

	public static float DefaultFps { get { return 0; } }

	/// <summary>
	/// Play the clip specified by identifier.
	/// Will restart the clip at clipStartTime if called while the clip is playing.
	/// </summary>
	/// <param name='clip'>The clip to play. </param>	
	/// <param name='clipStartTime'> Clip start time in seconds. A value of DefaultFps will start the clip from the beginning </param>
	public void Play(tk2dSpriteAnimationClip clip, float clipStartTime)
	{
		Play(clip, clipStartTime, DefaultFps);
	}

	/// <summary>
	/// Play the clip specified by identifier.
	/// Will restart the clip at clipStartTime if called while the clip is playing.
	/// No defaults to play nice with default MonoDevelop configuration.
	/// </summary>
	/// <param name='clip'>The clip to play. </param>	
	/// <param name='clipStartTime'> Clip start time in seconds. A value of DefaultFps will start the clip from the beginning </param>
	/// <param name='overrideFps'> Overriden framerate of clip. Set to 0 to use default </param>
	public void Play(tk2dSpriteAnimationClip clip, float clipStartTime, float overrideFps)
	{
		if (clip != null)
		{
			state |= State.Playing;
			currentClip = clip;
			clipFps = (overrideFps > 0.0f)?overrideFps:currentClip.fps;

			// Simply swap, no animation is played
			if (currentClip.wrapMode == tk2dSpriteAnimationClip.WrapMode.Single || currentClip.frames == null)
			{
				WarpClipToLocalTime(currentClip, 0.0f);
				state &= ~State.Playing;
			}
			else if (currentClip.wrapMode == tk2dSpriteAnimationClip.WrapMode.RandomFrame || currentClip.wrapMode == tk2dSpriteAnimationClip.WrapMode.RandomLoop)
			{
				int rnd = Random.Range(0, currentClip.frames.Length);
				WarpClipToLocalTime(currentClip, rnd);

				if (currentClip.wrapMode == tk2dSpriteAnimationClip.WrapMode.RandomFrame)
				{
					previousFrame = -1;
					state &= ~State.Playing;
				}
			}
			else
			{
				// clipStartTime is in seconds
				// clipTime is in clip local time (ignoring fps)
				float time = clipStartTime * clipFps;
				if (currentClip.wrapMode == tk2dSpriteAnimationClip.WrapMode.Once && time >= clipFps * currentClip.frames.Length)
				{
					// warp to last frame
					WarpClipToLocalTime(currentClip, currentClip.frames.Length - 1);
					state &= ~State.Playing;
				}
				else
				{
					WarpClipToLocalTime(currentClip, time);
					
					// force to the last frame
					clipTime = time;
				}
			}
		}
		else
		{
			OnCompleteAnimation();
			state &= ~State.Playing;
		}
	}
	
	/// <summary>
	/// Pause the currently playing clip. Will do nothing if the clip is currently paused.
	/// </summary>
	public void Pause()
	{
		state |= State.Paused;
	}
	
	/// <summary>
	/// Resume the currently paused clip. Will do nothing if the clip hasn't been paused.
	/// </summary>
	public void Resume()
	{
		state &= ~State.Paused;
	}
	
	void OnCompleteAnimation()
	{
		previousFrame = -1;
		if (animationCompleteDelegate != null)
			animationCompleteDelegate(this, clipId);
	}
	
	/// <summary>
	/// Sets the current frame. The animation will wrap if the selected frame exceeds the 
	/// number of frames in the clip.
	/// This variant WILL trigger an event if the current frame has a trigger defined.
	/// </summary>
	public void SetFrame(int currFrame)
	{
		SetFrame(currFrame, true);
	}

	/// <summary>
	/// Sets the current frame. The animation will wrap if the selected frame exceeds the 
	/// number of frames in the clip.
	/// </summary>
	public void SetFrame(int currFrame, bool triggerEvent)
	{
		if (currentClip == null && anim != null)
		{
			currentClip = anim.clips[clipId];
		}

		if (triggerEvent && currentClip != null && currentClip.frames.Length > 0 && currFrame >= 0)
		{
			int frame = currFrame % currentClip.frames.Length;
			SetFrameInternal(frame);
			ProcessEvents(frame - 1, frame, 1);
		}
	}
	
	void SetFrameInternal(int currFrame)
	{
		if (previousFrame != currFrame)
		{
			SwitchCollectionAndSprite( currentClip.frames[currFrame].spriteCollection, currentClip.frames[currFrame].spriteId );
			previousFrame = currFrame;
		}
	}
	
	void ProcessEvents(int start, int last, int direction)
	{
		if (animationEventDelegate == null || start == last) 
			return;
		int end = last + direction;
		var frames = currentClip.frames;
		for (int frame = start + direction; frame != end; frame += direction)
		{
			if (frames[frame].triggerEvent)
				animationEventDelegate(this, currentClip, frames[frame], frame);
		}
	}
	
	void LateUpdate() 
	{
#if UNITY_EDITOR
		// Don't play animations when not in play mode
		if (!Application.isPlaying)
			return;
#endif

		// Only process when clip is playing
		var localState = state | globalState;
		if (localState != State.Playing)
			return;

		// Current clip should not be null at this point
		clipTime += Time.deltaTime * clipFps;
		int _previousFrame = previousFrame;
		
		switch (currentClip.wrapMode)
		{
			case tk2dSpriteAnimationClip.WrapMode.Loop: 
			case tk2dSpriteAnimationClip.WrapMode.RandomLoop:
			{
				int currFrame = (int)clipTime % currentClip.frames.Length;
				SetFrameInternal(currFrame);
				if (currFrame < _previousFrame) // wrap around
				{
					ProcessEvents(_previousFrame, currentClip.frames.Length - 1, 1); // up to end of clip
					ProcessEvents(-1, currFrame, 1); // process up to current frame
				}
				else
				{
					ProcessEvents(_previousFrame, currFrame, 1);
				}
				break;
			}

			case tk2dSpriteAnimationClip.WrapMode.LoopSection:
			{
				int currFrame = (int)clipTime;
				int currFrameLooped = currentClip.loopStart + ((currFrame - currentClip.loopStart) % (currentClip.frames.Length - currentClip.loopStart));
				if (currFrame >= currentClip.loopStart)
				{
					SetFrameInternal(currFrameLooped);
					currFrame = currFrameLooped;
					if (_previousFrame < currentClip.loopStart)
					{
						ProcessEvents(_previousFrame, currentClip.loopStart - 1, 1); // processed up to loop-start
						ProcessEvents(currentClip.loopStart - 1, currFrame, 1); // to current frame, doesn't cope if already looped once
					}
					else 
					{
						if (currFrame < _previousFrame)
						{
							ProcessEvents(_previousFrame, currentClip.frames.Length - 1, 1); // up to end of clip
							ProcessEvents(currentClip.loopStart - 1, currFrame, 1); // up to current frame
						}
						else
						{
							ProcessEvents(_previousFrame, currFrame, 1); // this doesn't cope with multi loops within one frame
						}
					}
				}
				else
				{
					SetFrameInternal(currFrame);
					ProcessEvents(_previousFrame, currFrame, 1);
				}
				break;
			}

			case tk2dSpriteAnimationClip.WrapMode.PingPong:
			{
				int currFrame = (int)clipTime % (currentClip.frames.Length + currentClip.frames.Length - 2);
				int dir = 1;
				if (currFrame >= currentClip.frames.Length)
				{
					currFrame = 2 * currentClip.frames.Length - 2 - currFrame;
					dir = -1;
				}
				// This is likely to be buggy - this needs to be rewritten storing prevClipTime and comparing that rather than previousFrame
				// as its impossible to detect direction with this, when running at frame speeds where a transition occurs within a frame
				if (currFrame < _previousFrame) dir = -1;
				SetFrameInternal(currFrame);
				ProcessEvents(_previousFrame, currFrame, dir);
				break;
			}		

			case tk2dSpriteAnimationClip.WrapMode.Once:
			{
				int currFrame = (int)clipTime;
				if (currFrame >= currentClip.frames.Length)
				{
					SetFrameInternal(currentClip.frames.Length - 1); // set to last frame
					state &= ~State.Playing; // stop playing before calling event - the event could start a new animation playing here
					ProcessEvents(_previousFrame, currentClip.frames.Length - 1, 1);
					OnCompleteAnimation();
				}
				else
				{
					SetFrameInternal(currFrame);
					ProcessEvents(_previousFrame, currFrame, 1);
				}
				break;
			}
		}
	}
}
