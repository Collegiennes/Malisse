using UnityEngine;

class Malisse : MonoBehaviour
{
    Camera mainCamera;

    void Awake()
    {
        mainCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
        transform.rotation = mainCamera.transform.rotation;
    }

    void Update()
    {
        //transform.LookAt(mainCamera.transform.position);
    }
}
