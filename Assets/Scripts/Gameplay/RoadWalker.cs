using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class RoadWalker : MonoBehaviour
{
    public Spline RoadToWalk;
    public float Speed = 1;
    public float Delay = 0;

    float distanceFromStart;

    void Update()
    {
        if (!RoadToWalk) return;

        if (Delay > 0)
        {
            Delay -= Time.deltaTime;
            return;
        }

        distanceFromStart += 0.1f * Speed;

        Vector3 curTan;
        bool done;
        var worldPos = RoadToWalk.RoadCenterAt(distanceFromStart, out done, out curTan);

        if (done)
        {
            // finished walking the path! change level?
            // for now, wrap around
            distanceFromStart = 0;
            RoadToWalk.RoadCenterAt(distanceFromStart, out done, out curTan);
        }

        transform.position = new Vector3(worldPos.x, 0, worldPos.z);
        CurrentTangent = curTan.normalized;
    }

    public Vector3 CurrentTangent { get; private set; }
}
