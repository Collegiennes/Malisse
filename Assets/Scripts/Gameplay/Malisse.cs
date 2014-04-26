using UnityEngine;

class Malisse : MonoBehaviour
{
    void Awake()
    {
        if (Camera.main)
            transform.rotation = Camera.main.transform.rotation;
    }
}
