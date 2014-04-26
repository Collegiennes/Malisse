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

        distanceFromStart += 0.1f;

        transform.position = RoadToWalk.RoadCenterAt(distanceFromStart);
    }
}
