using Demo;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class C17_Roof : Shape
{
    public int Width;
    public bool IsEdge;
    public int Depth;
    private MeshBuilder builder = null;

    public void Initialize(int pWidth, GameObject roofObject, bool pIsEdge = true, int pDepth = 0)
    {
        Width = pWidth;
        MeshRenderer mr = GetComponent<MeshRenderer>();
        mr.sharedMaterial = roofObject.GetComponent<MeshRenderer>().sharedMaterial;
        MeshFilter mf = GetComponent<MeshFilter>();
        mf.sharedMesh = roofObject.GetComponent<MeshFilter>().sharedMesh;
        IsEdge = pIsEdge;
        Depth = pDepth;
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
        if (IsEdge)
        {
            Vector3 offset = new Vector3(-Width / 2f, -0.5f, 0);
            for (int i = 0; i < Width; i++)
            {
                if (i == 0)
                {
                    int v1 = builder.AddVertex(offset + Vector3.zero, Vector2.zero);
                    int v2 = builder.AddVertex(offset + new Vector3(1,0,0), new Vector2(1,0));
                    int v3 = builder.AddVertex(offset + new Vector3(1,1,1), new Vector2(1,1));
                    builder.AddTriangle(v3, v2, v1);
                }
                else if (i == Width - 1)
                {
                    int v1 = builder.AddVertex(offset + new Vector3(Width, 0, 0), new Vector2(1,0));
                    int v2 = builder.AddVertex(offset + new Vector3(Width - 1, 0, 0), new Vector2(0,0));
                    int v3 = builder.AddVertex(offset + new Vector3(Width - 1,1,1), new Vector2(0,1));
                    builder.AddTriangle(v1, v2, v3);

                }
                else
                {
                    int v1 = builder.AddVertex(offset + new Vector3(i, 0, 0), Vector2.zero);
                    int v2 = builder.AddVertex(offset + new Vector3(i + 1, 0, 0), new Vector2(1,0));
                    int v3 = builder.AddVertex(offset + new Vector3(i + 1, 1, 1), new Vector2(1,1));
                    int v4 = builder.AddVertex(offset + new Vector3(i,1,1), new Vector2(0,1));
                    builder.AddTriangle (v3, v2, v4);
                    builder.AddTriangle (v2, v1, v4);
                }
            }
        }
        else
        {
            
            int v1 = builder.AddVertex(new Vector3(1,0,1), Vector2.zero);
            int v2 = builder.AddVertex(new Vector3(Width - 1,0,1), new Vector2(1,0));
            int v3 = builder.AddVertex(new Vector3(Width - 1,0,Depth - 1), new Vector2(1,1));
            int v4 = builder.AddVertex(new Vector3(1,0, Depth - 1), new Vector2(0,1));
            builder.AddTriangle (v3, v2, v4);
            builder.AddTriangle (v2, v1, v4);
        }
        

        GetComponent<MeshFilter>().mesh = builder.CreateMesh();
    }
}
