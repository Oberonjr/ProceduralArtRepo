using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridManager))]
public class GridManagerEditor : Editor
{
    private GridManager gridManager;
    private Vector3 scale;
    private SerializedProperty gridSizeProp;
    private SerializedProperty nodeSizeProp;

    public override void OnInspectorGUI()
    {
        gridManager = (GridManager)target;
        gridSizeProp = serializedObject.FindProperty("gridSize");
        nodeSizeProp = serializedObject.FindProperty("nodeSize");
        if (GUILayout.Button("Generate Grid"))
        {
            gridManager.GenerateGrid();
        }

        if (GUILayout.Button("Snap all zones to grid"))
        {
            gridManager.SnapAllToGrid();
        }
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(gridSizeProp);
        EditorGUILayout.PropertyField(nodeSizeProp);
        string[] excludeProps = new string[] { "gridSize", "nodeSize" };
        DrawPropertiesExcluding(serializedObject, excludeProps);
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            Undo.RecordObject(gridManager, "Change Grid Size");
            gridManager.GenerateGrid();
            EditorUtility.SetDirty(gridManager);
            
        }
    }

    private void OnSceneGUI()
    {
        gridManager = (GridManager)target;
        Vector3 position = gridManager.transform.position;
        Vector2Int gridSize = gridManager.gridSize;

        EditorGUI.BeginChangeCheck();
        position = Handles.PositionHandle(position, Quaternion.identity);
        scale = Handles.ScaleHandle(new Vector3(gridSize.x, 0, gridSize.y), gridManager.transform.position,
            Quaternion.identity);
        gridSize = new Vector2Int((int)(scale.x), (int)(scale.z));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(gridManager, "Change Grid Size");
            gridManager.transform.position = position;
            gridManager.gridSize = gridSize;
            gridManager.GenerateGrid();
            EditorUtility.SetDirty(gridManager);
        }
    }
}
