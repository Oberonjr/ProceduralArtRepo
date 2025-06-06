using System.Collections.Generic;
using Demo;
using UnityEngine;

public class GenerationManager : MonoBehaviour
{
    private static GenerationManager _instance;
    public static GenerationManager Instance => _instance;

    [SerializeField] private List<C17_SimpleBuilding> buildings;
    [SerializeField] private WarpMeshAlongSpline roadPrefab;
    [SerializeField] private int maxBuildHeight;
    
    private C17_SimpleBuilding buildingPrefab;
    AreaManager areaManager;
    GridManager gridManager;
    void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        GenerateEverything();
    }

    public void GenerateEverything()
    {
        if (areaManager == null || gridManager == null)
        {
#if UNITY_EDITOR
            areaManager = FindObjectOfType<AreaManager>();
            gridManager = FindObjectOfType<GridManager>();
#else
        areaManager = AreaManager.Instance;
        gridManager = GridManager.Instance;
#endif
        }
        gridManager.GenerateGrid();
        areaManager.GenerateNewAreas();
        gridManager.SnapAllToGrid();
        GenerateCity();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            GenerateEverything();
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            GenerateCity();
        }
    }
    
    public void GenerateCity()
    {
#if UNITY_EDITOR
        areaManager = FindObjectOfType<AreaManager>();
        gridManager = FindObjectOfType<GridManager>();
#else
        areaManager = AreaManager.Instance;
        gridManager = GridManager.Instance;
#endif
        DestroyCity();
        foreach (Location loc in areaManager.Locations)
        {
            GenerateArea(loc);
        }
    }

    public void GenerateArea(Location loc)
    {
        buildingPrefab = buildings[Random.Range(0, buildings.Count)];
        switch (loc.locationType)
        {
            case AreaType.Road:
                WarpMeshAlongSpline road = Instantiate(roadPrefab, areaManager.transform);
                road.transform.rotation = Quaternion.identity;
                road.transform.position = new Vector3(0, 0.25f, 0);
                road.MeshScale.x = gridManager.nodeSize / 10f;
                Curve roadCurve = road.GetComponent<Curve>();
                roadCurve.points = new List<Vector3>();
                Vector3 bottomLeftPosition = gridManager.GetNodeWorldPosition(loc.bottomLeft);
                Vector3 topRightPosition = gridManager.GetNodeWorldPosition(loc.topRight);
                Vector3 direction = (bottomLeftPosition - topRightPosition).normalized;
                if(direction.magnitude < 0.1f) direction = new Vector3(0, 0, 1);
                Vector3 offset = direction * (gridManager.nodeSize / 2f);
                roadCurve.points.Add(bottomLeftPosition + offset);
                roadCurve.points.Add(topRightPosition - offset);
                roadCurve.Apply();
                road.transform.name = loc.Name;
                break;
            case AreaType.Park:
                break;
            case AreaType.TallBlock:
                while (buildingPrefab is C17_RoundedCornerBuilding)
                {
                    buildingPrefab = buildings[Random.Range(0, buildings.Count)];
                }
                var tallBuilding = Instantiate(buildingPrefab, areaManager.transform);
                tallBuilding.transform.position = loc.position + new Vector3(0, 0.5f, 0);
                tallBuilding.Width = (int)loc.size.x;
                tallBuilding.Depth = (int)loc.size.z;
                tallBuilding.Height = Random.Range((int)(maxBuildHeight/2f), maxBuildHeight);
                tallBuilding.randomRoofType = false;
                tallBuilding.slanted = false;
                tallBuilding.Build();
                tallBuilding.transform.name = loc.Name;
                break;
            default:
                int cornerOffset = 0;
                if (buildingPrefab is C17_RoundedCornerBuilding)
                {
                    cornerOffset = 1;
                }
                var building = Instantiate(buildingPrefab, areaManager.transform);
                building.transform.position = loc.position + new Vector3(0, 0.5f, 0);
                building.Width = (int)loc.size.x - cornerOffset;
                building.Depth = (int)loc.size.z - cornerOffset;
                building.Height = Random.Range(4, (int)(maxBuildHeight/2f));
                building.Build();
                building.transform.name = loc.Name;
                break;
        }
    }
    
    public void DestroyCity()
    {
#if UNITY_EDITOR
        areaManager = FindObjectOfType<AreaManager>();
#endif
        for (int i = areaManager.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(areaManager.transform.GetChild(i).gameObject);
        }
    }
}
