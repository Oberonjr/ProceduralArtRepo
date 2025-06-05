using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using Random = UnityEngine.Random;

public class C17_SimpleBuilding : C17_Building
{
    [SerializeField] private List<C17_Facade> facades;
    [SerializeField, Range(3,30)] private int width;
    [SerializeField, Range(3,30)] private int depth;
    [SerializeField, Range(3,30)] private int height;

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
    
    private List<int> buildingPattern;
    private int facadeIndex;
    private bool slanted;
    
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
    public void Build()
    {
        facadeIndex = Random.Range(0, facades.Count);
        //EventBus<FinishBuildingFacade>.OnEvent += PopulateBuildingPattern;
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
        };
        
        bool isDoorSide = (0 == cachedDoorFacade);
        int facadeWidth = cachedWidth;
        
        cachedParam = facadeInstance.GetComponent<C17_FacadeParameters>();
        slanted = cachedParam.slantedRoof;
        facadeInstance.Initialize(cachedHeight, facadeWidth);
        facadeInstance.DoorHaver = isDoorSide;
        facadeInstance.transform.localPosition = new Vector3(0, 0, cachedDepth / 2f);
        facadeInstance.transform.localEulerAngles = new Vector3(0, 180, 0);
        facadeInstance.transform.name = "BuildingFacade0";
        facadeInstance.Build();
    }

    void BuildRemainingFacades()
    {
        for (int i = 1; i < 4; i++)
        {
            C17_Facade original = facades[cachedFacadeIndex];
            C17_Facade facadeInstance = Instantiate(original, transform);
            
            bool isDoorSide = (i == cachedDoorFacade);
            int facadeWidth = (i % 2 == 0) ? cachedWidth : cachedDepth;
            
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
        for (int x = 0; x < cachedWidth - coverSize; x++)
        {
            for (int z = 0; z < cachedDepth - coverSize; z++)
            {
                C17_Roof ceilingTile = CreateSymbol<C17_Roof>("CeilingTile " + index, Vector3.zero, Quaternion.identity, gameObject.transform);
                ceilingTile.Initialize(1, roofObj, false, 1); // 1x1 tile
                ceilingTile.transform.localPosition = new Vector3(x + coverSize/2, cachedHeight, z + coverSize/2) + cOffset;
                ceilingTile.Build();
                index++;
            }
        }
        
    }
}
