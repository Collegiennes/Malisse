using System.Collections;
using UnityEngine;

class Malisse : MonoBehaviour
{
    RoadWalker walker;
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

        walker = GetComponent<RoadWalker>();
        sprite = GetComponent<tk2dAnimatedSprite>();

        sinceDirectionReevaluated = 0.1f;
    }

    Vector3 lastPosition;
    Vector3 lastDirection;
    float sinceDirectionReevaluated;

    void Update()
    {
        if (!walker.Stopped)
            UpdateDirection();
    }

    void UpdateDirection()
    {
        sinceDirectionReevaluated += Time.deltaTime;
        if (sinceDirectionReevaluated > 0.1f)
        {
            lastDirection = transform.position - lastPosition;
            lastPosition = transform.position;

            float angle = Mathf.Atan2(lastDirection.x, lastDirection.z);
            if (angle < 0) angle += Mathf.PI * 2;
            int index = Mathf.RoundToInt(angle / (Mathf.PI * 2) * 8);
            if (index == 8) index = 0;
            //Debug.Log("Angle : " + angle + " | Index = " + index);

            var animName = PerAngleAnimationMap[index];
            var lastName = sprite.CurrentClip.name;
            if (lastName != animName)
            {
                sprite.Play(animName);

                if (animName == "walk_rs" || lastName == "walk_rs")
                {
                    //Debug.Log("name = " + animName);
                    transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * (animName == "walk_rs" ? -1 : 1),
                                                       transform.localScale.y, transform.localScale.z);
                }
            }
        }
    }

    void OnCollisionEnter(Collision info)
    {
        if (!walker.Stopped && 
            (info.gameObject.layer == LayerMask.NameToLayer("Default") ||
             info.gameObject.layer == LayerMask.NameToLayer("Death")))
        {
            StartCoroutine(JumpBackAndStartle());
        }
    }

    IEnumerator JumpBackAndStartle()
    {
        var lastName = sprite.CurrentClip.name;

        walker.Stop();

        // undo flip just in case
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        sprite.Play("fall");

        float t = 0;
        while (t < 1)
        {
            float step = Mathf.Pow(1 - t, 1.25f);
            walker.DistanceFromStart -= Time.deltaTime * 30.0f * step;

            walker.HeightOffset = Mathf.Sin(t * Mathf.PI) * 200.0f - 75f;

            //transform.position = new Vector3();
            yield return new WaitForEndOfFrame();
            t += Time.deltaTime;
        }

        sprite.Play("timeout");
        walker.HeightOffset = -75.0f;

        yield return new WaitForSeconds(2.0f);

        walker.HeightOffset = 0.0f;
        sprite.Play(lastName);

        walker.Resume();
    }
}
