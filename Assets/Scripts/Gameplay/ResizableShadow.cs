using UnityEngine;

class ResizableShadow : MonoBehaviour
{
    Transform camTransform;
    Collider parentCollider;
    Transform actualParent;

    public Transform CustomParent;

    Vector3 baseScale;

    void Awake()
    {
        var camGO = GameObject.Find("MainCamera");
        if (camGO)
            camTransform = camGO.transform;

        actualParent = CustomParent ? CustomParent : transform.parent;
        parentCollider = actualParent.collider;

        if (parentCollider is BoxCollider)
        {
            var length = (parentCollider as BoxCollider).size.x;
            var depth = (parentCollider as BoxCollider).size.z;
            transform.localScale = new Vector3(length, depth, 1);
        }
        else if (parentCollider is MeshCollider)
        {
            var length = (parentCollider as MeshCollider).bounds.size.x;
            var depth = (parentCollider as MeshCollider).bounds.size.z;

            transform.localScale = new Vector3(length / actualParent.localScale.x / actualParent.parent.localScale.x,
                                               depth / actualParent.localScale.y / actualParent.parent.localScale.y,
                                               1);
        }

        baseScale = transform.localScale;
    }

    void Update()
    {
        var camFwd = camTransform ? camTransform.TransformDirection(Vector3.forward) : Vector3.zero;

        transform.localPosition = Vector3.zero;
        transform.position = new Vector3(transform.position.x, 0, transform.position.z) - camFwd * 200;

        var height = Mathf.Max(actualParent.position.y, 1);
        float distance = Mathf.Clamp01(1 - height / 475.0f);
        transform.localScale = baseScale * distance;

        renderer.material.color = new Color(1, 1, 1, Mathf.Pow(distance, 1.125f));

        if (actualParent.GetComponent<tk2dAnimatedSprite>() == null)
            transform.rotation = Quaternion.Euler(90, 0, 0);
        else
            transform.rotation = Quaternion.identity;
    }
}
