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
        //Debug.Log(walker.CurrentTangent);

        if (walker.CurrentDirection.x > 0 && sprite.clipId != sprite.GetClipIdByName("walk_rf"))
        {
            Debug.Log("right");
            sprite.Play("walk_rf");
        }
        else if (walker.CurrentDirection.x < 0 && sprite.clipId != sprite.GetClipIdByName("walk_lf"))
        {
            Debug.Log("left");
            sprite.Play("walk_lf");
        }
    }
}
