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
	public List<AudioClip> m_SFXEfforts = new List<AudioClip>();
	public List<AudioClip> m_SFXHitFloor = new List<AudioClip>();
	
	private List<Rabbit> m_Rabbits = new List<Rabbit>();

	public class FloatHolder
	{
		public float Value;

		public static implicit operator float(FloatHolder d)
		{
			return d.Value;
		}
		public static implicit operator FloatHolder(float d)
		{
			return new FloatHolder { Value = d };
		}
	}
	public Dictionary<Collider, FloatHolder> walkableRefreshList = new Dictionary<Collider, FloatHolder>();
	
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

	    transform.Find("RotatedCollider").gameObject.layer = LayerMask.NameToLayer("Player");

        if (MainGameView.Instance)
        {
            Walker.Stop();
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

    void CheckForNextLevel()
    {
        bool done = true;
        foreach (var r in GetComponentsInChildren<Rabbit>())
            done &= r.Scattering || r.Walker.Done;

        if (done)
            MainGameView.Instance.LoadNextLevel(false);
    }

    Vector3 lastPosition;
    Vector3 lastDirection;

	List<Collider> toClear = new List<Collider>(); 

    void Update()
    {
        if (!Walker.Stopped)
            UpdateDirection();

        var r = renderer;
        r.enabled = Walker.DistanceFromStart > 0 && !Walker.Done;
        r = transform.Find("Shadow").renderer;
        r.enabled = Walker.DistanceFromStart > 0 && !Walker.Done;

		// time-expire walkables
	    toClear.Clear();
//		if (walkableRefreshList.Count > 0)
//			Debug.Log(walkableRefreshList.Count + " walkables");
		foreach (var k in walkableRefreshList.Keys)
		{
			var v = walkableRefreshList[k];

			v.Value += Time.deltaTime;
//			Debug.Log(k.gameObject.name + " has life of " + v);

			if (v > 0.25f)
				toClear.Add(k);
		}
	    foreach (var c in toClear)
			RevertWalkableCollider(c);

	    // readjust height based on ground raycast
		RaycastHit hit;
		if (Physics.SphereCast(new Ray(transform.position + Vector3.up * 1080, Vector3.down), 50, out hit, 2160, 1 << LayerMask.NameToLayer("WalkableObject")))
		{
			Walker.GroundHeight = hit.point.y;
			RefreshWalkableCollider(hit.collider);
		}
		else
			Walker.GroundHeight = Mathf.Max(Walker.GroundHeight - Time.deltaTime * 500.0f, 0.0f);
    }

	public void RefreshWalkableCollider(Collider c)
	{
		if (walkableRefreshList.ContainsKey(c))
			walkableRefreshList[c].Value = 0;
		else
		{
			Debug.Log("Could not find walkable collider " + c.gameObject.name);
		}
	}

	public void RevertWalkableCollider(Collider c)
	{
		Debug.Log("Reverting walkable object " + c.gameObject.name);
		c.gameObject.layer = LayerMask.NameToLayer("Default");
		walkableRefreshList.Remove(c);
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
//            RaycastHit preHit, postHit;
//            var preMove = Physics.Raycast(transform.position - Vector3.up * 1080, Vector3.down, out preHit);
//            var postMove = Physics.Raycast(destinationPosition - Vector3.up * 1080, Vector3.down, out postHit);

            //Debug.Log(info.contacts.Count() + " contacts");
            var point = info.contacts.First(x => x.otherCollider.transform.gameObject.layer != LayerMask.NameToLayer("Ground")).point;

            var postPoint = point + (point - transform.Find("RotatedCollider").position).normalized * 10;

			RaycastHit postHit;

			var postResults = Physics.RaycastAll(new Ray(postPoint + Vector3.up * 1080, Vector3.down), 2160,
	                                             (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("Ground")));

	        float heightDiff = 0;
	        if (postResults.Length > 0)
	        {
				var hits = postResults.Where(x => x.collider.GetComponent<Obstacle>()).OrderBy(x => x.distance);

		        if (hits.Any())
		        {
			        postHit = hits.First();

				    heightDiff = postHit.point.y - Walker.GroundHeight;
			        Debug.Log("Heightdiff for " + gameObject.name + ": " + heightDiff + " to " + postHit.collider.gameObject.name);

			        if (postHit.collider && heightDiff <= 25 && !walkableRefreshList.ContainsKey(postHit.collider))
			        {
				        if (postHit.collider.gameObject.GetComponent<Obstacle>().m_GrabbedHandleCount > 0)
					        heightDiff = 10000;
				        else
				        {
					        postHit.collider.gameObject.layer = LayerMask.NameToLayer("WalkableObject");
					        walkableRefreshList.Add(postHit.collider, 0);
				        }
			        }
		        }
				else
					Debug.Log("No hit!!");
	        }
	        else
		        Debug.Log("No hit!!");

			if (heightDiff > 25)
			{
				StartCoroutine(JumpBackAndStartle());
				AudioManager.Instance.PlaySFX(m_SFXEfforts);
			}
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

		AudioManager.Instance.PlaySFX(m_SFXHitFloor);

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
                rabbit.GetComponent<RoadWalker>().OnPathDone = CheckForNextLevel;
				m_Rabbits.Add(rabbit);
			}
		}
	}    
}
