using UnityEngine;
using Demo;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class C17_Quad : Shape
{
    public Material material;
    [HideInInspector] public Vector3[] vertexPoints = new Vector3[4];

    [HideInInspector]public MeshBuilder builder = null;

    private MeshFilter mf = null;
    private MeshRenderer mr = null;
    
    
    public void Initialize(Material pMaterial, Vector3[] pVertexPoints)
    {
        if (pVertexPoints.Length != 4)
        {
            Debug.LogError("Trying to initialize a C17_Quad with invalid number of vertices!");
            return;
        }
        material = pMaterial;
        vertexPoints = pVertexPoints;
        
        mf = GetComponent<MeshFilter>();
        GameObject primitveQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Mesh quadMesh = primitveQuad.GetComponent<MeshFilter>().sharedMesh;
        mf.sharedMesh = quadMesh;
        DestroyImmediate(primitveQuad);
        
        mr = GetComponent<MeshRenderer>();
        mr.material = material;
    }

    public void Build()
    {
        Execute();
    }
    
    protected override void Execute()
    {
        if (builder == null)
        {
            builder = new MeshBuilder();
        }
        Vector3  v1 = vertexPoints[0];
        Vector3 v2 = vertexPoints[1];
        Vector3 v3 = vertexPoints[2];
        Vector3 v4 = vertexPoints[3];
        
        int w1 = builder.AddVertex(v1, Vector2.zero);
        int w2 = builder.AddVertex(v2, new Vector2(0,1));
        int w3 = builder.AddVertex(v3, new Vector2(1,1));
        int w4 = builder.AddVertex(v4, new Vector2(1,0));
        
        builder.AddTriangle(w1, w2, w3);
        builder.AddTriangle(w1, w3, w4);
        
        GetComponent<MeshFilter>().mesh = builder.CreateMesh();
    }
}
