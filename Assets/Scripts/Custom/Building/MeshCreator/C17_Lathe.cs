using UnityEngine;
using Handout;
using System.Collections.Generic;
using UnityEditor;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(Curve))]
[RequireComponent(typeof(AutoUv))]
public class C17_Lathe : MeshCreator {

	public int NumCurves = 10; //number of segments around the mesh
	public bool ModifySharedMesh = false;

	public float RoofHeight
	{
		get
		{
			return roofHeight;
		}
		set
		{
			if (value < 0)
			{
				roofHeight = 0;
			}
			else
			{
				roofHeight = value;
			}
		}
	}
	
	private Material baseMaterial;
	private Material windowMaterial;
	private Material roofMaterial;
	private bool hasWindow = false;
	private bool isRoof = false;
	private float roofHeight;

	//helper function to map the x,y location of a vertex to an index in a 1D array
	private int getIndex(int x, int y, int height) {
		return y + x * height;
	}

	public void Initialize(int pNumCurves, Material pBaseMaterial, bool pIsRoof = false, Material pRoofMaterial = null, bool pHasWindow = false, Material pWindowMaterial = null)
	{
		NumCurves = pNumCurves;
		baseMaterial = pBaseMaterial;
		isRoof = pIsRoof;
		roofMaterial = pRoofMaterial;
		hasWindow = pHasWindow;
		windowMaterial = pWindowMaterial;
		
		MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
		meshRenderer.material = baseMaterial;
	}
	
	
	public override void RecalculateMesh() {
		Curve curve = GetComponent<Curve>();
		if (curve==null)
			return;
		List<Vector3> _vertices = curve.points;

		MeshBuilder meshBuilder = new MeshBuilder();

		int vertexCount = _vertices.Count;
		int curveCount = NumCurves;

		//Go through all curves (vertical lines around mesh)
		for (int curveIndex = 0; curveIndex <= curveCount; curveIndex++) {
			//Create quaternion for rotating around y-axis (the curveIndex is used to determine the angle in degrees):
			Quaternion rotation = Quaternion.Euler(0, curveIndex * 360.0f / curveCount, 0);

			//Go through all vertices (all vertices per spline):
			for (int vertexIndex = 0; vertexIndex<vertexCount; vertexIndex++) {
				//create a Vector3 from a Vector2 (or: set the z-coordinate of the curve point to zero):
				Vector3 vertex = new Vector3(_vertices[vertexIndex].x, _vertices[vertexIndex].y, 0);
				float mama = vertexIndex / (float)vertexCount;
				float papa = curveIndex / (float)curveCount;
				Vector2 uv = new Vector2(papa, mama);
				//use quaternion to rotate the vertex into position:
				vertex = rotation * vertex;
				//add it to the mesh:
				meshBuilder.AddVertex(vertex, uv);
			}
		}

		List<Vector2Int> roofIndices = new List<Vector2Int>();
		//Generate quads:
		for (int curveIndex = 1; curveIndex<=curveCount; curveIndex++) { //start at 1, because we need to access spline at splineIndex-1
			for (int vertexIndex = 1; vertexIndex<vertexCount; vertexIndex++) { //start at 1, because we need to access vertex at vertexIndex-1

				//generate 4 vertices (quad):
				int v0 = getIndex(curveIndex - 1, vertexIndex - 1, vertexCount); //bL
				int v1 = getIndex(curveIndex, vertexIndex - 1, vertexCount);	   //bR
				int v2 = getIndex(curveIndex, vertexIndex, vertexCount);		   //tR
				int v3 = getIndex(curveIndex - 1, vertexIndex, vertexCount);	   //tL

				// Add two triangles (quad):
				meshBuilder.AddTriangle(v0, v1, v2);
				meshBuilder.AddTriangle(v0, v2, v3);

				if (isRoof)
				{
					roofIndices.Add(new Vector2Int(v2, v3));
				}
				
				if (hasWindow)
				{
					Vector3 p0 = meshBuilder.GetVertex(v0);
					Vector3 p1 = meshBuilder.GetVertex(v1);
					Vector3 p2 = meshBuilder.GetVertex(v2);
					Vector3 p3 = meshBuilder.GetVertex(v3);
					
					Vector3 center = (p0 + p1 + p2 + p3) / 4.0f;

					float scaleFactor = 0.1f;
					
					Vector3 w0 = Vector3.Lerp(p0, center, scaleFactor);
					Vector3 w1 = Vector3.Lerp(p1, center, scaleFactor);
					Vector3 w2 = Vector3.Lerp(p2, center, scaleFactor);
					Vector3 w3 = Vector3.Lerp(p3, center, scaleFactor);
					
					Vector3 normal = Vector3.Cross(p1 - p0, p3 - p0).normalized;

					float outwardOffset = 0.01f;
					w0 += normal * outwardOffset;
					w1 += normal * outwardOffset;
					w2 += normal * outwardOffset;
					w3 += normal * outwardOffset;
					Vector3[] vert = { w1, w2, w3, w0 };
					
					GameObject window = new GameObject("Window" + curveIndex);
					window.transform.SetParent(transform);
					window.transform.localPosition = Vector3.zero;
					window.gameObject.isStatic = true;
					C17_Quad windowQuad = window.AddComponent<C17_Quad>();
					windowQuad.Initialize(windowMaterial, vert);
					windowQuad.Build();
					
				}
			}
		}

		if (isRoof)
		{
			int roofIndex = 0; 
			foreach (Vector2Int roofSlab in roofIndices)
			{
				GameObject roof = new GameObject("Roof" + roofIndex);
				roof.transform.SetParent(transform);
				roof.transform.localPosition = Vector3.zero;
				roof.gameObject.isStatic = true;
				int v0 = meshBuilder.AddVertex(new Vector3(0, roofHeight, 0));
				Vector3 p0 = meshBuilder.GetVertex(v0);
				Vector3 p1 = meshBuilder.GetVertex(roofSlab.x);
				Vector3 p2 = meshBuilder.GetVertex(roofSlab.y);
				Vector3[] verts = { p2, p1, p0, p0 };
				C17_Quad roofQuad = roof.AddComponent<C17_Quad>();
				roofQuad.Initialize(roofMaterial, verts);
				roofQuad.Build();
			}
		}
		
		// Generate mesh and apply it to the meshfilter component:
		ReplaceMesh(meshBuilder.CreateMesh(), ModifySharedMesh);
	}
}
