using UnityEngine;
using UnityEditor;

namespace Demo {
	// If the second parameter is true, this will also be applied to subclasses.
	// If you want a custom inspector for a subclass, just add it, and this one will be ignored.
	[CustomEditor(typeof(Shape), true)] 
	public class ShapeEditor : Editor
	{
		private bool isC17Facade;
		public override void OnInspectorGUI() {
			Shape targetShape = (Shape)target;

			if (targetShape as C17_Facade)
			{
				isC17Facade = true;
			}
			
			GUILayout.Label("Generated objects: "+targetShape.NumberOfGeneratedObjects);
			if (GUILayout.Button("Generate")) {
				if (isC17Facade)
				{
					C17_Facade targetFacade = targetShape as C17_Facade;
					targetFacade.Root.GetComponent<C17_FacadeParameters>().CheckContents();
				}
				targetShape.Generate(0.1f);
			}
			DrawDefaultInspector();
		}
	}
}