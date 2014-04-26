using UnityEngine;

class ResizableShadow : MonoBehaviour
{
    Collider parentCollider;

    void Awake()
    {
        parentCollider = transform.parent.collider;
    }

    void Update()
    {
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

        transform.localPosition = Vector3.zero;
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);

        var height = Mathf.Max(transform.parent.position.y, 1);
        transform.localScale *= Mathf.Min(100.0f / height, 1);

        if (transform.parent.GetComponent<tk2dAnimatedSprite>() == null)
            transform.rotation = Quaternion.Euler(90, 0, 0);
        else
            transform.rotation = Quaternion.identity;
    }
}
