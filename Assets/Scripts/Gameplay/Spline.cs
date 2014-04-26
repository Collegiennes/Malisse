using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[ExecuteInEditMode]
class Spline : MonoBehaviour
{
    public float SegmentRate = 0.5f;
    public float Width = 0.5f;

    public Mesh SplineMesh;
    public SplineNode[] CachedNodes;

    void Awake()
    {
        var meshFilter = GetComponent<MeshFilter>();
        SplineMesh = (meshFilter.sharedMesh = new Mesh());
        CachedNodes = GetComponentsInChildren<SplineNode>().OrderBy(x => x.name).ToArray();
        Rebuild();
    }

    public void Update()
    {
        TryRebuild();
    }

    float cacheChecksum;
    void TryRebuild()
    {
        if (!Application.isEditor)
            return;

        // safeguard if it's deleted from the editor
        if (GetComponent<MeshFilter>().sharedMesh == null)
            Awake();

        // go through child nodes and check if checksum matches
        float newChecksum = 0;
        CachedNodes = GetComponentsInChildren<SplineNode>().OrderBy(x => x.name).ToArray();
        foreach (var node in CachedNodes)
            newChecksum += Vector3.Dot(node.transform.position, Vector3.one);
        cacheChecksum += Width;
        cacheChecksum += SegmentRate;

        if (cacheChecksum != newChecksum)
        {
            // only rebuild mesh if checksum changed
            Rebuild();
            //Debug.Log("Rebuilt!");
            cacheChecksum = newChecksum;
        }
    }

    public Vector3 RoadCenterAt(float distanceFromStart)
    {
        var segments = Math.Round(CachedNodes.Length * 100 * SegmentRate);

        var nodePositions = CachedNodes.Select(x => x.transform.localPosition).ToArray();

        float lengthSeen = 0;
        for (int i = 0; i <= segments; i++)
        {
            var s = i / (float)segments;

            var center = MathfPlus.BSpline(nodePositions, s);
            Vector3 nextCenter = MathfPlus.BSpline(nodePositions, (i + 1) / (float)segments);

            var segmentLength = Vector3.Distance(nextCenter, center);
            lengthSeen += segmentLength;

            if (lengthSeen > distanceFromStart)
            {
                float distanceAtLastSegment = lengthSeen - segmentLength;
                return transform.localToWorldMatrix * MathfPlus.PadVector3(Vector3.Lerp(center, nextCenter, (distanceFromStart - distanceAtLastSegment) / (lengthSeen - distanceAtLastSegment)));
            }
        }

        // fallback : at distance = 0
        return RoadCenterAt(0);
    }

    public float RoadTotalLength { get; private set; }

    void Rebuild()
    {
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var uv = new List<Vector2>();

        var nodePositions = CachedNodes.Select(x => x.transform.localPosition).ToArray();

        SplineMesh.Clear();
        SplineMesh.subMeshCount = 1;

        RoadTotalLength = 0;

        var segments = Math.Round(CachedNodes.Length * 100 * SegmentRate);

        float accumulatedTexCoord = 0;
        for (int i = 0; i <= segments; i++)
        {
            var s = i / (float)segments;

            var center = MathfPlus.BSpline(nodePositions, s);
            Vector3 nextCenter = MathfPlus.BSpline(nodePositions, (i + 1) / (float)segments);

            var diff = Vector3.Normalize(nextCenter - center);
            var tangent = Vector3.Cross(diff, Vector3.up);

            var c = vertices.Count;
            var segmentLength = Vector3.Distance(nextCenter, center);
            RoadTotalLength += segmentLength;
            accumulatedTexCoord += segmentLength * 0.1f;

            vertices.Add(center - tangent * Width); uv.Add(new Vector2(0, accumulatedTexCoord));
            vertices.Add(center + tangent * Width); uv.Add(new Vector2(1, accumulatedTexCoord));

            if (i != segments)
            {
                triangles.Add(c); triangles.Add(c + 1); triangles.Add(c + 2);
                triangles.Add(c + 2); triangles.Add(c + 1); triangles.Add(c + 3);

                triangles.Add(c); triangles.Add(c + 2); triangles.Add(c + 1);
                triangles.Add(c + 2); triangles.Add(c + 3); triangles.Add(c + 1);
            }
        }

        if (vertices.Count > 0)
        {
            try
            {
                SplineMesh.vertices = vertices.ToArray();
                SplineMesh.uv = uv.ToArray();
                SplineMesh.SetTriangles(triangles.ToArray(), 0);

                SplineMesh.RecalculateNormals();
                SplineMesh.RecalculateBounds();
                SplineMesh.Optimize();
            }
            catch (Exception exception)
            {
                Debug.Log(exception.Message);
            }
        }
    }

}
