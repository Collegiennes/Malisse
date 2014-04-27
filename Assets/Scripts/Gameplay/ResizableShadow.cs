using UnityEngine;

class ResizableShadow : MonoBehaviour
{
    Collider parentCollider;

    Vector3 baseScale;

    void Awake()
    {
        parentCollider = transform.parent.collider;

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

            transform.localScale = new Vector3(length / transform.parent.localScale.x / transform.parent.parent.localScale.x,
                                               depth / transform.parent.localScale.y / transform.parent.parent.localScale.y,
                                               1);
        }

        baseScale = transform.localScale;
    }

    void Update()
    {
        transform.localPosition = Vector3.zero;
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);

        var height = Mathf.Max(transform.parent.position.y, 1);
        float distance = Mathf.Clamp01(1 - height / 475.0f);
        transform.localScale = baseScale * distance;

        renderer.material.color = new Color(1, 1, 1, Mathf.Pow(distance, 1.125f));

        if (transform.parent.GetComponent<tk2dAnimatedSprite>() == null)
            transform.rotation = Quaternion.Euler(90, 0, 0);
        else
            transform.rotation = Quaternion.identity;
    }
}
