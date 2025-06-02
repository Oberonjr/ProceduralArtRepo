using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GenerationManager))]
public class GenerationManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GenerationManager generationManager = (GenerationManager)target;
        
        GUILayout.Label("Generation Manager", EditorStyles.boldLabel);
        if (GUILayout.Button("Populate Areas"))
        {
            generationManager.GenerateCity();
        }

        if (GUILayout.Button("Destroy All Buildings"))
        {
            generationManager.DestroyCity();
        }

        if (GUILayout.Button("Generate Everything"))
        {
            generationManager.GenerateEverything();
        }
        DrawDefaultInspector();
    }
}
