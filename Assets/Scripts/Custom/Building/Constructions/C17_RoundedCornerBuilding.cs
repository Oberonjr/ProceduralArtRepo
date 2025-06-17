using System.Collections.Generic;
using UnityEngine;

public class C17_RoundedCornerBuilding : C17_SimpleBuilding
{
    #region Rounded Corner Parameters
    [Header("Rounded Corner Parameters")]
    [Range(4,12)]
    public int numCurves = 4;
    public bool RandomizeCurveNumbers;
    public Vector2Int numCurveRange = new Vector2Int(4, 12);

    [Header("Rounded Corner Roof Parameters")]
    public Vector2 roofHeightRange = new Vector2(1, 2);
    #endregion
    protected override void Execute()
    {
        Build();
    }

    public override void Build()
    {
        base.Build();
    }

    protected override void BuildRemainingFacades()
    {
        base.BuildRemainingFacades();
        AddRoundedCorner();
    }
    
    void AddRoundedCorner()
    {
        int cornerIndex = Random.Range(0, 4);
        if (RandomizeCurveNumbers)
        {
            numCurves = Random.Range(numCurveRange.x, numCurveRange.y);
        }

        if (numCurves % 2 == 1)
        {
            numCurves++;
        }
        Material baseMaterial = new Material(Shader.Find("Standard"));
        Material windowMaterial = null;
        Material roofMaterial = null;
        GameObject roundedCorner = new GameObject("Rounded Corner");
        Vector3 spawnPosition = new Vector3();
        switch (cornerIndex)
        {
            case 0:
                spawnPosition = transform.position + new Vector3(-Width / 2f, -1, -Depth / 2f);
                break;
            case 1:
                spawnPosition = transform.position + new Vector3(Width / 2f, -1, -Depth / 2f);
                break;
            case 2:
                spawnPosition = transform.position + new Vector3(Width / 2f, -1, Depth / 2f);
                break;
            case 3:
                spawnPosition = transform.position + new Vector3(-Width / 2f, -1, Depth / 2f);
                break;
        }
        roundedCorner.transform.SetParent(transform);
        roundedCorner.transform.position = spawnPosition;
        roundedCorner.transform.rotation = Quaternion.identity;
        for (int i = 0; i < Height - 1; i++)
        {
            GameObject lathe = new GameObject("Lathe" + i);
            lathe.transform.parent = roundedCorner.transform;
            lathe.transform.position = spawnPosition + new Vector3(0, transform.position.y + i, 0);
            lathe.gameObject.isStatic = true;
            C17_Lathe latheComp = lathe.AddComponent<C17_Lathe>();
            
            
            bool hasWindow = false;
            bool isRoof = false;
            if (i == Height - 2)
            {
                isRoof = true;
                foreach (Floor fl in cachedParam.wallStyle)
                {
                    if (fl.FloorType == FloorType.Roof)
                    {
                        roofMaterial = fl.WallStyle[0].GetComponent<MeshRenderer>()
                            .sharedMaterial;
                        break;
                    }
                }
            }
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
                    case FloorType.GlassFloor:
                        foreach (Floor fl in cachedParam.wallStyle)
                        {
                            if (fl.FloorType == FloorType.GroundFloor)
                            {
                                baseMaterial = fl.WallStyle[1].GetComponent<MeshRenderer>()
                                    .sharedMaterial;
                                break;
                            }
                        }
                        windowMaterial = floor.WallStyle[0].GetComponent<MeshRenderer>()
                            .sharedMaterial;
                        hasWindow = true;
                        break;
                    default:
                        baseMaterial = cachedParam.wallStyle[floorIndex].WallStyle[0].GetComponent<MeshRenderer>()
                            .sharedMaterial;
                        break;
                    
                }
            }
            float rH = Random.Range(roofHeightRange.x, roofHeightRange.y + 1);
            latheComp.RoofHeight = rH;
            latheComp.Initialize(numCurves, baseMaterial, isRoof, roofMaterial, hasWindow, windowMaterial);
            Curve curve = lathe.GetComponent<Curve>();
            curve.points = new List<Vector3>();
            curve.points.Add(new Vector3(1, 0, 0));
            curve.points.Add(new Vector3(1, 1, 0));
            curve.Apply();
            latheComp.RecalculateMesh();
            lathe.GetComponent<AutoUv>().UpdateUvs();
        }

        if (ToCombineMeshes)
        {
            CombineMeshes(roundedCorner.transform);
        }
    }
}
