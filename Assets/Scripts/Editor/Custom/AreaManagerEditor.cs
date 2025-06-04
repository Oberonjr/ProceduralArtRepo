using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(AreaManager))]
public class AreaManagerEditor : Editor
{

    public override void OnInspectorGUI()
    {
        AreaManager areaManager = (AreaManager)target;
        GUILayout.Label("Area Manager", EditorStyles.boldLabel);
        if (GUILayout.Button("Handle Preplaced Locations"))
        {
            areaManager.HandlePreplacedLocations();
        }
        if (GUILayout.Button("Generate Areas"))
        {
            areaManager.GenerateNewAreas();
        }

        if (GUILayout.Button("Destroy Generated Areas"))
        {
            areaManager.ClearAllGeneratedAreas();
        }

        if (GUILayout.Button("Clear All Areas"))
        {
            areaManager.ClearAllAreas();
        }
        DrawDefaultInspector();
    }
    private void OnSceneGUI()
    {
        AreaManager example = (AreaManager)target;
        
        for (int i = 0; i < example.Locations.Count; i++)
        {
            Vector3 position = example.Locations[i].position;
            Vector3 scale = example.Locations[i].size;

            EditorGUI.BeginChangeCheck();
            if (example.DrawHandles)
            {
                position = Handles.PositionHandle(position, Quaternion.identity);
                scale = Handles.ScaleHandle(scale, position, Quaternion.identity);
            }
            

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(example, "Updated location");
                example.Locations[i].position = position;
                example.Locations[i].size = scale;
                EditorUtility.SetDirty(example);
            }
        }
    }
}