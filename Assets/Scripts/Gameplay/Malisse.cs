using UnityEngine;

class Malisse : MonoBehaviour
{
    Camera mainCamera;

    void Awake()
    {
        var camGO = GameObject.Find("MainCamera");
        if (camGO)
        {
            mainCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
            transform.rotation = mainCamera.transform.rotation;
        }
    }
}
