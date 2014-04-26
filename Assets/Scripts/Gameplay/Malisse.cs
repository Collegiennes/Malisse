using UnityEngine;

class Malisse : MonoBehaviour
{
    RoadWalker walker;
    tk2dAnimatedSprite sprite;

    void Awake()
    {
        var camGO = GameObject.Find("MainCamera");
        if (camGO)
            transform.rotation = camGO.transform.rotation;

        walker = GetComponent<RoadWalker>();
        sprite = GetComponent<tk2dAnimatedSprite>();
    }

    void Update()
    {
        var d = new Vector2(-walker.CurrentDirection.z, walker.CurrentDirection.x);
        //Debug.Log(d);

        // mostly right, some front
        if (d.x > 0 && Mathf.Abs(d.y) < 0.5 && d.y < 0 && sprite.clipId != sprite.GetClipIdByName("walk_rf"))
        {
            Debug.Log("rf");
            sprite.Play("walk_rf");
        }
        // mostly left, some front
        if (d.x < 0 && Mathf.Abs(d.y) < 0.5 && d.y < 0 && sprite.clipId != sprite.GetClipIdByName("walk_lf"))
        {
            Debug.Log("lf");
            sprite.Play("walk_lf");
        }

        // mostly back, some right
        if (d.y > 0.25 && Mathf.Abs(d.x) < 0.5 && d.x > 0 && sprite.clipId != sprite.GetClipIdByName("walk_rb"))
        {
            Debug.Log("rb");
            sprite.Play("walk_rb");
        }
        // mostly back, some left
        if (d.y > 0.25 && Mathf.Abs(d.x) < 0.5 && d.x < 0 && sprite.clipId != sprite.GetClipIdByName("walk_lb"))
        {
            Debug.Log("lb");
            sprite.Play("walk_lb");
        }
    }
}
