using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class Billboard : MonoBehaviour
{
    void Awake()
    {
        var camGO = GameObject.Find("MainCamera");
        if (camGO)
            transform.rotation = camGO.transform.rotation;
    }
}
