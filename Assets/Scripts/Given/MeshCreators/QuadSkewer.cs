using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Curve))]
public class QuadSkewer : MeshCreator
{
    [SerializeField] private Mesh InputMesh;
    [SerializeField] private Vector3 offset;
    public override void RecalculateMesh()
    {
        Curve spline = GetComponent<Curve>();
        if (spline == null)
        {
            Debug.LogWarning("QuadSkewer: No Curve on this game object");
            return;
        }
        List<Vector3> points = spline.points;
        if (points.Count != 4)
        {
            Debug.LogWarning("QuadSkewer: Invalid number of points. Needs to have exactly 4 points.");
            return;
        }
        Vector3[] warpedVertices = new Vector3[InputMesh.vertices.Length];

        for (int i = 0; i < InputMesh.vertices.Length; i++)
        {
            //Do the actual warping
            warpedVertices[i] = WarpVertexUsingQuad(InputMesh.vertices[i], points);
        }
        
        Mesh newMesh = new Mesh();
        newMesh.vertices = warpedVertices;
        newMesh.uv = InputMesh.uv; //Maybe:clone?
        newMesh.subMeshCount = InputMesh.subMeshCount;
        
        //Just copy the submeshes and triangles
        for (int i = 0; i < InputMesh.subMeshCount; i++)
        {
            newMesh.SetTriangles(InputMesh.GetTriangles(i), i);
        }
        newMesh.RecalculateNormals();
        newMesh.RecalculateTangents();
        
        GetComponent<MeshFilter>().mesh = newMesh;
    }

    Vector3 WarpVertexUsingQuad(Vector3 point, List<Vector3> points)
    {
        float x = point.x + offset.x;
        float z = point.z + offset.z;
        
        float c1 = (1 - x) * (1 - z);
        float c2 = (1 - x) * (z);
        float c3 = (x) * (z);
        float c4 = (x) * (1 - z);
        Vector3 newPosition = new Vector3();
        newPosition = 
            c1 * points[0] + c2 * points[1] + c3 * points[2] + c4 * points[3];
        newPosition.y = point.y;
        return newPosition;
    }
}
