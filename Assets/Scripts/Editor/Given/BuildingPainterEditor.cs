using UnityEngine;
using UnityEditor;

namespace Demo {
	[CustomEditor(typeof(BuildingPainter))]
	public class BuildingPainterEditor : Editor {

        private BuildingPainter painter;

        private void OnEnable()
        {
            painter = (BuildingPainter)target;
        }

        public void OnSceneGUI() {
			Event e = Event.current;
	        
			if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Space)
            {
				Debug.Log("Space");	
				Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
				if (Physics.Raycast(ray, out RaycastHit hit))
				{
					Undo.RecordObject(painter, "Add Building");
					if (hit.collider.tag == "Building")
					{
						DestroyImmediate(hit.collider.gameObject);
					}
					GameObject building = painter.CreateBuilding(hit.point);

					if (building != null)
					{
						Undo.RegisterCreatedObjectUndo(building, "Create Building");
					}
					
					EditorUtility.SetDirty(painter);
				}
				e.Use();
            }

            DrawBuildingTransforms();
		}


        private void DrawBuildingTransforms()
        {
            for (int i = 0; i < painter.createdBuildings.Count; i++)
            {
				// Take care of destroying buildings manually:
                GameObject building = painter.createdBuildings[i];
				if (building==null) {
					painter.createdBuildings.RemoveAt(i);
					i--;
					continue;
				}

				// TODO (Ex 2): Draw a handle at the position of this building.
				//  Try to draw a position, rotation and scale gizmo at the same time.
				//  Don't forget to record changes in the undo list, and mark the scene as dirty.
            }
        }
	}
}