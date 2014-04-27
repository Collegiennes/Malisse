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
}
