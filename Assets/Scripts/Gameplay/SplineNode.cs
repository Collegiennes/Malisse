using UnityEngine;
using System.Collections;

public class SplineNode : MonoBehaviour 
{
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0x72 / 256.0f, 0xDE / 256.0f, 0xC2 / 256.0f);
        Gizmos.DrawSphere(transform.position, 1);
    }
}
