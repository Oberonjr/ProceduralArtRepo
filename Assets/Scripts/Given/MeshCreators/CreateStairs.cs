using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Handout {
	public class CreateStairs : MonoBehaviour {
		public int numberOfSteps = 10;
		// The dimensions of a single step of the staircase:
		public float width=3;
		public float height=1;
		public float depth=1;

		MeshBuilder builder;

		void Start () {
			builder = new MeshBuilder ();
			CreateShape ();
			GetComponent<MeshFilter> ().mesh = builder.CreateMesh (true);
		}

		/// <summary>
		/// Creates a stairway shape in [builder].
		/// </summary>
		void CreateShape() {
			builder.Clear ();
			
			// V2, with for loop:
			for (int i = 0; i < numberOfSteps; i++) {
				Vector3 offset = new Vector3 (0, height * i, depth * i); 

				// TODO 1: use the width and height parameters from the inspector to change the step width and height
				Vector3 scale = new Vector3 (width, height, depth);
				
				// TODO 4: Fix the uvs:
				// bottom:
				int v1 = builder.AddVertex (offset + Vector3.Scale(new Vector3 (2, 0, 0),scale), new Vector2 (1, 0));	
				int v2 = builder.AddVertex (offset + Vector3.Scale(new Vector3 (-2, 0, 0),scale), new Vector2 (0, 0));
				// top front:
				int v3 = builder.AddVertex (offset + Vector3.Scale(new Vector3 (2, 1, 0),scale), new Vector2 (1, 0.5f));	
				int v4 = builder.AddVertex (offset + Vector3.Scale(new Vector3 (-2, 1, 0),scale), new Vector2 (0, 0.5f));
				// top back:
				int v5 = builder.AddVertex (offset + Vector3.Scale(new Vector3 (2, 1, 1),scale), new Vector2 (1, 1));	
				int v6 = builder.AddVertex (offset + Vector3.Scale(new Vector3 (-2, 1, 1),scale), new Vector2 (0, 1));

				// TODO 2: Fix the winding order (everything clockwise):
				// builder.AddTriangle (v1, v2, v3);
				// builder.AddTriangle (v2, v3, v4);
				// builder.AddTriangle (v3, v4, v5);
				// builder.AddTriangle (v4, v6, v5);
				
				builder.AddTriangle (v6, v3, v4);
				builder.AddTriangle (v3, v6, v5);
				builder.AddTriangle (v3, v2, v4);
				builder.AddTriangle (v3, v1, v2);

				// TODO 3: make the mesh solid by adding left, right and back side.
				//Sides
				int v7 = builder.AddVertex (offset + Vector3.Scale(new Vector3 (2, 0, 0),scale), new Vector2 (1, 0));	
				int v8 = builder.AddVertex (offset + Vector3.Scale(new Vector3 (2, 1, 0),scale), new Vector2 (1, 1));
				int v9 = builder.AddVertex (offset + Vector3.Scale(new Vector3 (2, 1, 1),scale), new Vector2 (0, 1));	
				int v10 = builder.AddVertex (offset + Vector3.Scale(new Vector3 (-2, 0, 0),scale), new Vector2 (0, 0));
				int v11 = builder.AddVertex (offset + Vector3.Scale(new Vector3 (-2, 1, 0),scale), new Vector2 (0, 1));	
				int v12 = builder.AddVertex (offset + Vector3.Scale(new Vector3 (-2, 1, 1),scale), new Vector2 (1, 1));
				
				builder.AddTriangle (v8, v9, v7);
				builder.AddTriangle (v10, v12, v11);
				//Back
				int v13 = builder.AddVertex (offset + Vector3.Scale(new Vector3 (2, 0, 0),scale), new Vector2 (0, 0));	
				int v14 = builder.AddVertex (offset + Vector3.Scale(new Vector3 (-2, 0, 0),scale), new Vector2 (1, 0));
				int v15 = builder.AddVertex (offset + Vector3.Scale(new Vector3 (2, 1, 1),scale), new Vector2 (0, 1));	
				int v16 = builder.AddVertex (offset + Vector3.Scale(new Vector3 (-2, 1, 1),scale), new Vector2 (1, 1));
				
				builder.AddTriangle (v15, v14, v13);
				builder.AddTriangle (v15, v16, v14);
				
				// TODO 5: Fix the normals by *not* reusing a single vertex in multiple triangles with different normals (solve it by creating more vertices at the same position)
			}
			
		}
		
	}
}