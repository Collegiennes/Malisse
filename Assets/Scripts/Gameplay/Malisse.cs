using UnityEngine;

class Malisse : MonoBehaviour
{
    RoadWalker walker;

    void Awake()
    {
        var camGO = GameObject.Find("MainCamera");
        if (camGO)
            transform.rotation = camGO.transform.rotation;

        walker = GetComponent<RoadWalker>();
    }

    void Update()
    {
        //walker.
    }
}
