using System.Collections.Generic;
using UnityEngine;

public class C17_RoundedCornerBuilding : C17_SimpleBuilding
{

    protected override void Execute()
    {
        Build();
        AddRoundedCorner();
    }

    void AddRoundedCorner()
    {
        int cornerIndex = Random.Range(0, 4);
        int numCurves = Random.Range(3, 13);
        
        Material baseMaterial = new Material(Shader.Find("Standard"));
        Material windowMaterial = null;
        
        Vector3 spawnPosition = new Vector3();
        switch (cornerIndex)
        {
            case 0:
                spawnPosition = transform.position + new Vector3(-Width / 2f, -Height/2f -1, -Depth / 2f);
                break;
            case 1:
                spawnPosition = transform.position + new Vector3(Width / 2f, -Height/2f -1, -Depth / 2f);
                break;
            case 2:
                spawnPosition = transform.position + new Vector3(Width / 2f, -Height/2f -1, Depth / 2f);
                break;
            case 3:
                spawnPosition = transform.position + new Vector3(-Width / 2f, -Height/2f -1, Depth / 2f);
                break;
        }

        for (int i = 0; i < Height - 1; i++)
        {
            GameObject lathe = new GameObject("Lathe" + i);
            lathe.transform.position = spawnPosition + new Vector3(0, transform.position.y + i, 0);
            lathe.transform.parent = transform;
            C17_Lathe latheComp = lathe.AddComponent<C17_Lathe>();
            
            
            bool hasWindow = false;
            if (i == 0)
            {
                foreach (Floor fl in cachedParam.wallStyle)
                {
                    if (fl.FloorType == FloorType.GroundFloor)
                    {
                        baseMaterial = fl.WallStyle[1].GetComponent<MeshRenderer>()
                            .sharedMaterial;
                        break;
                    }
                }
            }
            else
            {
                int floorIndex = buildingPattern[i - 1];
                Floor floor = cachedParam.wallStyle[floorIndex];
                switch (floor.FloorType)
                {
                    case FloorType.WindowFloor:
                        baseMaterial = cachedParam.wallStyle[floorIndex].WallStyle[0].GetComponent<MeshRenderer>()
                            .sharedMaterial;
                        GameObject roofObj = cachedParam.wallStyle[floorIndex].WallStyle[0].transform.GetChild(0).gameObject;
                        windowMaterial= roofObj.GetComponent<MeshRenderer>().sharedMaterial;
                        hasWindow = true;
                        break;
                    default:
                        baseMaterial = cachedParam.wallStyle[floorIndex].WallStyle[0].GetComponent<MeshRenderer>()
                            .sharedMaterial;
                        break;
                    
                }
            }
            latheComp.Initialize(numCurves, baseMaterial, false, hasWindow, windowMaterial);
            Curve curve = lathe.GetComponent<Curve>();
            curve.points = new List<Vector3>();
            curve.points.Add(new Vector3(1, 0, 0));
            curve.points.Add(new Vector3(1, 1, 0));
            curve.Apply();
            lathe.GetComponent<AutoUv>().UpdateUvs();
            latheComp.RecalculateMesh();
        }
    }
}
