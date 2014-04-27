using System;
using System.Collections;
using UnityEngine;

class Rabbit : MonoBehaviour
{
    Malisse parent;
    RoadWalker walker;
    tk2dAnimatedSprite sprite;

    public float DistanceToMalisse;

    string[] PerAngleAnimationMap =
    {
        "b", 
        "rb", 
        "rs", 
        "rf", 
        "f", 
        "lf", 
        "ls", 
        "lb", 
    };

    void Awake()
    {
        transform.localRotation = Quaternion.identity;

        sprite = GetComponent<tk2dAnimatedSprite>();
        parent = transform.parent.GetComponent<Malisse>();
        walker = GetComponent<RoadWalker>();

        walker.Stop();

        sinceDirectionReevaluated = 0.1f;
    }

    Vector3 lastPosition;
    Vector3 lastDirection;
    float sinceDirectionReevaluated;

    void Update()
    {
        walker.DistanceFromStart = parent.Walker.DistanceFromStart - DistanceToMalisse;

        UpdateDirection();
    }

    void UpdateDirection()
    {
        sinceDirectionReevaluated += Time.deltaTime;
        if (sinceDirectionReevaluated > 0.1f)
        {
            sinceDirectionReevaluated = 0;

            lastDirection = transform.position - lastPosition;
            lastPosition = transform.position;

            float angle = Mathf.Atan2(lastDirection.x, lastDirection.z);
            if (angle < 0) angle += Mathf.PI * 2;
            int index = Mathf.RoundToInt(angle / (Mathf.PI * 2) * 8);
            if (index == 8) index = 0;
            //Debug.Log("Angle : " + angle + " | Index = " + index);

            var animName = PerAngleAnimationMap[index];
            var lastName = sprite.CurrentClip == null ? " " : sprite.CurrentClip.name;
            if (lastName != animName)
            {
                sprite.Play(animName);

                if ((animName.StartsWith("r") && !lastName.StartsWith("r")) || (lastName.StartsWith("r") && !animName.StartsWith("r")))
                {
                    sprite.FlipX();
                }
            }
        }
    }
}
