using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Malisse : MonoBehaviour
{
	private const string RABBIT_PATH = "Prefabs/Malisse/Rabbit";
	
	public RoadWalker Walker;
	public float m_DistanceBetweenCharacters = 5.0f;
	
	private List<Rabbit> m_Rabbits = new List<Rabbit>();
	
	tk2dAnimatedSprite sprite;
	
	string[] PerAngleAnimationMap =
	{
		"walk_b",
		"walk_rb",
		"walk_rs", // walk_ls * -1
		"walk_rf", 
		"walk_f",
		"walk_lf", 
		"walk_ls", // walk_ls
		"walk_lb", 
	};
	
	void Awake()
	{
		var camGO = GameObject.Find("MainCamera");
		if (camGO)
			transform.rotation = camGO.transform.rotation;
		
		Walker = GetComponent<RoadWalker>();
		sprite = GetComponent<tk2dAnimatedSprite>();
		
		Walker.RoadToWalk = GameUtils.FindAssociatedLevel(transform).transform.Find("Road").GetComponent<Spline>();
		
		Walker.Step();
		lastPosition = transform.position;
		Walker.Step();
		UpdateDirection();
		
		if (MainGameView.Instance)
		{
			Walker.Stop();
			Walker.OnPathDone = MainGameView.Instance.LoadNextLevel;
			MainGameView.Instance.m_OnSceneReadyCallback += Walker.Resume;
		}
	}
	
	void OnDestroy()
	{
		if (MainGameView.Instance)
		{
			Walker.OnPathDone = null;
			MainGameView.Instance.m_OnSceneReadyCallback -= Walker.Resume;
		}
	}
	
	Vector3 lastPosition;
	Vector3 lastDirection;
	
	void Update()
	{
		if (!Walker.Stopped)
			UpdateDirection();
	}
	
	void UpdateDirection()
	{
		lastDirection = transform.position - lastPosition;
		lastPosition = transform.position;
		
		float angle = Mathf.Atan2(lastDirection.x, lastDirection.z);
		if (angle < 0) angle += Mathf.PI * 2;
		int index = Mathf.RoundToInt(angle / (Mathf.PI * 2) * 8);
		if (index == 8) index = 0;
		//Debug.Log("Angle : " + angle + " | Index = " + index);
		
		var animName = PerAngleAnimationMap[index];
		var lastName = sprite.CurrentClip == null ? "" : sprite.CurrentClip.name;
		if (lastName != animName)
		{
			sprite.Play(animName);
			
			if (animName == "walk_rs" || lastName == "walk_rs")
			{
				//Debug.Log("name = " + animName);
				sprite.FlipX();
			}
		}
	}
	
	void OnCollisionEnter(Collision info)
	{
		if (!Walker.Stopped && 
		    (info.gameObject.layer == LayerMask.NameToLayer("Default") ||
		 info.gameObject.layer == LayerMask.NameToLayer("Death")))
		{
			StartCoroutine(JumpBackAndStartle());
		}
	}
	
	IEnumerator JumpBackAndStartle()
	{
		var lastName = sprite.CurrentClip.name;
		if (lastName == "walk_rs")
			sprite.FlipX();
		
		Walker.Stop();
		
		sprite.Play("fall");
		
		// Scatter farthest one!
		var toScatter = GetComponentsInChildren<Rabbit>().OrderBy(x => x.DistanceToMalisse);
		if (toScatter.Any() && toScatter.Last().DistanceToMalisse > 0)
			toScatter.Last().Scatter();
		
		foreach (var b in GetComponentsInChildren<Rabbit>().Where(x => !x.Scattering))
		{
			if (b.GetComponent<tk2dAnimatedSprite>().CurrentClip.name.StartsWith("r"))
				b.GetComponent<tk2dAnimatedSprite>().FlipX();
			b.GetComponent<tk2dAnimatedSprite>().Play("fall");
			b.GetComponent<Rabbit>().Stunned = true;
		}
		
		var heights = GetComponentsInChildren<Rabbit>().Select(_ => Random.Range(50.0f, 150)).ToArray();
		var speeds = GetComponentsInChildren<Rabbit>().Select(_ => Random.Range(1, 2.0f)).ToArray();
		
		float t = 0;
		while (t < 1)
		{
			float step = Mathf.Pow(1 - t, 1.25f);
			Walker.DistanceFromStart -= Time.deltaTime * 30.0f * step;
			
			Walker.HeightOffset = Mathf.Sin(t * Mathf.PI) * 200.0f - 70f;
			
			int i = 0;
			foreach (var b in GetComponentsInChildren<Rabbit>().Where(x => !x.Scattering))
			{
				b.GetComponent<tk2dAnimatedSprite>().Play("timeout", Random.Range(0, 1.0f));
				b.Walker.HeightOffset = Mathf.Sin(Mathf.Clamp01(t * speeds[i]) * Mathf.PI) * heights[i++] - 5.0f;
			}
			
			yield return new WaitForEndOfFrame();
			t += Time.deltaTime;
		}
		
		sprite.Play("timeout");
		Walker.HeightOffset = -75.0f;
		
		foreach (var b in GetComponentsInChildren<Rabbit>().Where(x => !x.Scattering))
		{
			b.GetComponent<tk2dAnimatedSprite>().Play("timeout", Random.Range(0, 1.0f));
			b.Walker.HeightOffset = -12.0f;
		}

		yield return new WaitForSeconds(2.0f);
		
		if (GetComponentsInChildren<Rabbit>().Length > 0)
		{
			Walker.HeightOffset = 0.0f;
			foreach (var b in GetComponentsInChildren<Rabbit>().Where(x => !x.Scattering))
			{
				b.Walker.HeightOffset = 0.0f;
				b.GetComponent<Rabbit>().Stunned = false;
			}
			
			Walker.Resume();
		}
		else
		{
			FlowManager.Instance.TriggerAction("GAME_OVER");
		}
	}
	
	public void AddRabbit()
	{
		GameObject rabbitPrefab = Resources.Load(RABBIT_PATH) as GameObject;
		if (rabbitPrefab != null)
		{
			GameObject rabbitObj = GameObject.Instantiate(rabbitPrefab) as GameObject;
			rabbitObj.transform.parent = transform;
			
			Rabbit rabbit = rabbitObj.GetComponent<Rabbit>();
			if (rabbit != null)
			{
				rabbit.DistanceToMalisse = (m_DistanceBetweenCharacters * (m_Rabbits.Count + 1));
				m_Rabbits.Add(rabbit);
			}
		}
	}
}
