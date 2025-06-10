using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using Random = UnityEngine.Random;

public class C17_SimpleBuilding : C17_Building
{
    #region GenerationParameters

    [Header("Building Generation Parameters")]
    public bool randomRoofType;
    public bool slanted;
    public bool ToCombineMeshes = true;
    [SerializeField] private List<C17_Facade> facades;
    [SerializeField, Range(3,30)] private int width;
    [SerializeField, Range(3,30)] private int depth;
    [SerializeField, Range(3,30)] private int height;
    #endregion
    
    #region Getters and Setters
    public int Width
    {
        get{return width;}
        set
        {
            if (value < 3)
            {
                width = 3;
            }
            else if (value > 30)
            {
                width = 30;
            }
            else
            {
                width = value;
            }
        }
    }

    public int Depth
    {
        get{return depth;}
        set
        {
            if (value < 3)
            {
                depth = 3;
            }
            else if (value > 30)
            {
                depth = 30;
            }
            else
            {
                depth = value;
            }
        }
    }

    public int Height
    {
        get{return height;}
        set
        {
            if (value < 3)
            {
                height = 3;
            }
            else if (value > 30)
            {
                height = 30;
            }
            else
            {
                height = value;
            }
        }
    }
    #endregion
    
    //[SerializeField]
    protected List<int> buildingPattern;
    private int facadeIndex;

    
    //REMINDER FOR SELF: DO NOT USE START OR AWAKE. YOU WANT TO BE DOING THINGS IN EDITOR. THAT STUFF DON'T WORK THERE
    //heresy
    private void Start()
    {
        Execute();
    }
    
    #region Build functions
    protected override void Execute()
    {
        Build();
    }
    void Build(int pWidth, int pDepth, int pHeight, int pFacadeIndex)
    {
        
        BuildFirstFacade(pWidth, pDepth, pHeight, pFacadeIndex);
        
    }
    public void Build(int pWidth, int pDepth, int pHeight)
    {
        Build(pWidth, pDepth, pHeight, facadeIndex);
    }
    public virtual void Build()
    {
        facadeIndex = Random.Range(0, facades.Count);
        Build(width, depth, height, facadeIndex);
    }
    #endregion
    private int cachedWidth, cachedDepth, cachedHeight, cachedFacadeIndex, cachedDoorFacade;
    protected C17_FacadeParameters cachedParam;
    
    void BuildFirstFacade(int pWidth, int pDepth, int pHeight, int pFacadeIndex)
    {
        cachedFacadeIndex = pFacadeIndex;
        
        if (transform.childCount > 0)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
        cachedWidth = pWidth;
        cachedDepth = pDepth;
        cachedHeight = pHeight;
        
        if (pWidth != pDepth)
        {
            int widthSide = Random.Range(0, 2) * 2;
            int depthSide = Random.Range(0, 2) * 2 + 1;
            cachedDoorFacade = (pWidth > pDepth) ? widthSide : depthSide; // 0: Front (Z+), 1: Side (X+)
        }
        else
        {
            cachedDoorFacade = Random.Range(0, 4);
        }
        
        C17_Facade original = facades[pFacadeIndex];
        C17_Facade facadeInstance = Instantiate(original, transform);

        facadeInstance.OnFacadeFinished += (List<int> pattern) =>
        {
            buildingPattern = pattern;
            BuildRemainingFacades();
            if (ToCombineMeshes)
            {
                CombineMeshes(cachedParam.transform);
            }
        };
        
        bool isDoorSide = (0 == cachedDoorFacade);
        int facadeWidth = cachedWidth;
        
        cachedParam = facadeInstance.GetComponent<C17_FacadeParameters>();
        if (randomRoofType)
        {
            slanted = Random.value < 0.5f;
        }
        cachedParam.slantedRoof = slanted;
        facadeInstance.Initialize(cachedHeight, facadeWidth);
        facadeInstance.DoorHaver = isDoorSide;
        facadeInstance.transform.localPosition = new Vector3(0, 0, cachedDepth / 2f);
        facadeInstance.transform.localEulerAngles = new Vector3(0, 180, 0);
        facadeInstance.transform.name = "BuildingFacade0";
        facadeInstance.Build();
        
    }

    protected virtual void BuildRemainingFacades()
    {
        for (int i = 1; i < 4; i++)
        {
            C17_Facade original = facades[cachedFacadeIndex];
            C17_Facade facadeInstance = Instantiate(original, transform);
            
            bool isDoorSide = (i == cachedDoorFacade);
            int facadeWidth = (i % 2 == 0) ? cachedWidth : cachedDepth;

            C17_FacadeParameters parameters = facadeInstance.GetComponent<C17_FacadeParameters>();
            parameters.slantedRoof = slanted;
            facadeInstance.Initialize(cachedHeight, facadeWidth, true, true, buildingPattern);
            facadeInstance.DoorHaver = isDoorSide;
            Vector3 positionOffset = Vector3.zero;
            Vector3 rotation = Vector3.zero;

            switch (i)
            {
                case 1: // Right (X+)
                    positionOffset = new Vector3(cachedWidth / 2f, 0, 0);
                    rotation = new Vector3(0, 270, 0);
                    break;
                case 2: // Back (Z-)
                    positionOffset = new Vector3(0, 0, -cachedDepth / 2f);
                    rotation = new Vector3(0, 0, 0);
                    break;
                case 3: // Left (X-)
                    positionOffset = new Vector3(-cachedWidth / 2f, 0, 0);
                    rotation = new Vector3(0, 90, 0);
                    break;
            }

            facadeInstance.transform.localPosition = positionOffset;
            facadeInstance.transform.localEulerAngles = rotation;
            facadeInstance.transform.name = "BuildingFacade" + i;
            facadeInstance.Build();
            facadeInstance.OnFacadeFinished += (List<int> pattern) =>
            {
                if (ToCombineMeshes)
                {
                    CombineMeshes(parameters.transform);
                }
            };

        }
        
        GameObject roofObj = null;
        if (cachedParam.hasRoof)
        {
            roofObj = cachedParam.roofFloor.WallStyle[0];
        }
        Vector3 cOffset = new Vector3(-cachedWidth / 2f, -0.5f, -cachedDepth / 2f);
        int index = 0;
        int coverSize;
        if (slanted)
        {
            coverSize = 2;
        }
        else
        {
            coverSize = 0;
        }
        GameObject roof = new GameObject("Roof");
        roof.transform.SetParent(transform);
        roof.transform.localPosition = Vector3.zero;
        roof.transform.localEulerAngles = Vector3.zero;
        for (int x = 0; x < cachedWidth - coverSize; x++)
        {
            for (int z = 0; z < cachedDepth - coverSize; z++)
            {
                C17_Roof ceilingTile = CreateSymbol<C17_Roof>("CeilingTile " + index, Vector3.zero, Quaternion.identity, roof.transform);
                ceilingTile.Initialize(1, roofObj, false, 1); // 1x1 tile
                ceilingTile.transform.localPosition = new Vector3(x + coverSize/2, cachedHeight, z + coverSize/2) + cOffset;
                ceilingTile.gameObject.isStatic = true;
                ceilingTile.Build();
                index++;
            }
        }

        if (ToCombineMeshes)
        {
            CombineMeshes(roof.transform);
        }
    }
    
    public void CombineMeshes(Transform target)
    {
        MeshFilter[] meshFilters = GetMeshesInChildren(target);
        if(target.gameObject.GetComponent<MeshFilter>() == null)
            target.gameObject.AddComponent<MeshFilter>();
        if(target.gameObject.GetComponent<MeshRenderer>() == null)
            target.gameObject.AddComponent<MeshRenderer>();
        
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
            finalCI.transform = Matrix4x4.identity * target.localToWorldMatrix.inverse;
            
            finalCombinations.Add(finalCI);
            materials.Add(kvp.Key);
            subMeshIndex++;
        }
        
        finalMesh.CombineMeshes(finalCombinations.ToArray(), false);
        target.GetComponent<MeshFilter>().sharedMesh = finalMesh;
        target.GetComponent<MeshRenderer>().sharedMaterials = materials.ToArray();
        
        target.gameObject.SetActive(true);
        for (int i = target.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(target.GetChild(i).gameObject);
        }

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
