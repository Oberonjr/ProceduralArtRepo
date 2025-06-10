using System;
using System.Collections.Generic;
using UnityEngine;

public enum FloorType
{
    GroundFloor,
    WindowFloor,
    Balcony,
    PlainFloor,
    GlassFloor,
    Roof
}

[System.Serializable]
public class Floor
{
    public FloorType FloorType;
    public GameObject[] WallStyle;
}

public class C17_FacadeParameters : MonoBehaviour
{
    public bool slantedRoof;
    public Floor[] wallStyle;
    public List<int> wallPattern;
    
    
    [HideInInspector] public bool hasFloor;
    [HideInInspector] public Floor groundFloor;
    
    [HideInInspector] public bool hasRoof;
    [HideInInspector] public Floor roofFloor;
    
    public void CheckContents()
    {
        Floor ground = null;
        foreach (Floor wall in wallStyle)
        {
            if (wall.FloorType == FloorType.GroundFloor)
            {
                hasFloor = true;
                ground = wall;
                break;
            }
        }
        if (hasFloor)
        {
            groundFloor = ground;
        }
        else
        {
            hasFloor = false;
            Debug.LogWarning("Facade: " + gameObject.name + " doesn't have a ground floor assigned");
        }

        Floor roof = null;
        foreach (Floor wall in wallStyle)
        {
            if (wall.FloorType == FloorType.Roof)
            {
                hasRoof = true;
                roof = wall;
                break;
            }
        }
        if (hasRoof)
        {
            roofFloor = roof;
        }
        else
        {
            hasRoof = false;
            Debug.LogWarning("Facade: " + gameObject.name + " doesn't have a roof assigned");
        }
    }

    public void CombineMeshes()
    {
        MeshFilter[] meshFilters = GetMeshesInChildren(this.gameObject.transform);
        if(gameObject.GetComponent<MeshFilter>() == null)
            gameObject.AddComponent<MeshFilter>();
        if(gameObject.GetComponent<MeshRenderer>() == null)
            gameObject.AddComponent<MeshRenderer>();
        //GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Diffuse"));
        
        Dictionary<Material, List<CombineInstance>> materialsToCombine = new Dictionary<Material, List<CombineInstance>>();
        foreach (MeshFilter mf in meshFilters)
        {
            MeshRenderer mr = mf.GetComponent<MeshRenderer>();
            if(mf.sharedMesh == null || mr == null) continue;
             Material mat = mr.sharedMaterial;
             if(!materialsToCombine.ContainsKey(mat))
                 materialsToCombine[mat] = new List<CombineInstance>();
             
             CombineInstance ci = new CombineInstance();
             ci.mesh = mf.sharedMesh;
             ci.transform = mf.transform.localToWorldMatrix;
             materialsToCombine[mat].Add(ci);
             
             mf.gameObject.SetActive(false);
             // if (mf != null && mf.gameObject != gameObject)
             //     DestroyImmediate(mf.gameObject);
             

        }
        
        List<Material> materials = new List<Material>();
        List<CombineInstance> finalCombinations = new List<CombineInstance>();
        Mesh finalMesh = new Mesh();
        
        int subMeshIndex = 0;
        foreach (var kvp in materialsToCombine)
        {
            Mesh subMesh = new Mesh();
            subMesh.CombineMeshes(kvp.Value.ToArray(), true, true);
            
            CombineInstance finalCI = new CombineInstance();
            finalCI.mesh = subMesh;
            finalCI.transform = Matrix4x4.identity * transform.localToWorldMatrix.inverse;
            
            finalCombinations.Add(finalCI);
            materials.Add(kvp.Key);
            subMeshIndex++;
        }
        
        finalMesh.CombineMeshes(finalCombinations.ToArray(), false);
        GetComponent<MeshFilter>().sharedMesh = finalMesh;
        GetComponent<MeshRenderer>().sharedMaterials = materials.ToArray();
        
        gameObject.SetActive(true);
    }

    MeshFilter[] GetMeshesInChildren(Transform parent)
    {
        List<MeshFilter> meshFilters = new List<MeshFilter>();
        if (parent.TryGetComponent<MeshFilter>(out MeshFilter meshFilter))
        {
            meshFilters.Add(meshFilter);
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            MeshFilter[] childFilters = GetMeshesInChildren(parent.GetChild(i));
            meshFilters.AddRange(childFilters);
        }
        return meshFilters.ToArray();
    }
    
}
