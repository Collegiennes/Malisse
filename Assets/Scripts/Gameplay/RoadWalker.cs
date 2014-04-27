using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class RoadWalker : MonoBehaviour
{
    public Spline RoadToWalk;
    public float Speed = 1;

    public float DistanceFromStart { get; set; }
    public bool Stopped { get; private set; }

    public float HeightOffset { get; set; }

    public bool Done { get; private set; }
    public Action OnPathDone;

    void Update()
    {
        if (!RoadToWalk) return;

        if (!Stopped)
            DistanceFromStart += Speed * Time.deltaTime * 5.0f;

        Vector3 curDir;
        bool done;
        var worldPos = RoadToWalk.RoadCenterAt(DistanceFromStart, out done, out curDir);

        if (done)
        {
            Done = true;
            if (OnPathDone != null)
                OnPathDone();
            return;
        }

        var destinationPosition = new Vector3(worldPos.x, worldPos.y + HeightOffset, worldPos.z);

        transform.position = destinationPosition;
        CurrentDirection = curDir.normalized;
    }

    public void Step()
    {
        Update();
    }

    public void Stop()
    {
        Stopped = true;
    }

    public void Resume()
    {
        Stopped = false;
    }

    public Vector3 CurrentDirection { get; private set; }
}
