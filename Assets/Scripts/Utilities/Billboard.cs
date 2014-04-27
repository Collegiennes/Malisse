using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class Billboard : MonoBehaviour
{
    public bool Inverse;

    void Awake()
    {
        var camGO = GameObject.Find("MainCamera");
        if (camGO)
        {
            if (Inverse)
                transform.localRotation = Quaternion.Inverse(camGO.transform.rotation);
            else
                transform.rotation = camGO.transform.rotation;
        }
    }
}
