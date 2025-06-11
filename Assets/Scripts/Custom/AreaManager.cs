using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;
using Random = UnityEngine.Random;

public enum AreaType{
    StandardBlock,
    TallBlock,
    Park,
    Citadel,
    Road
}

[System.Serializable]
public class Location
{
    public string Name;
    public AreaType locationType;
    public Vector3 position;
    public Vector3 size = Vector3.one;
    
    
    #region NodeCoverage
    public Node[,] coveredNodes;
    public int Width
    {
        get
        {
            return coveredNodes.GetLength(0);
        }
    }
    public int Depth
    {
        get
        {
            return coveredNodes.GetLength(1);
        }
    }
    public Node bottomLeft
    {
        get
        {
            return coveredNodes[0, 0];
        }   
    }
    public Node topRight
    {
        get
        {
            return coveredNodes[Width - 1, Depth - 1];
        }   
    }
    #endregion
    public void Initialize(AreaType pLocationType, Vector3 pPosition, Vector3 pSize)
    {
        locationType = pLocationType;
        position = pPosition;
        size = pSize;
    }

    
}

public class AreaManager : MonoBehaviour
{
    #region Singleton
    private static AreaManager _instance;
    public static AreaManager Instance => _instance;
    #endregion
    
    #region Location Lists
    public List<Location> preplacedLocations;

    [SerializeField]private List<Location> generatedLocations = new List<Location>();
    public List<Location> GeneratedLocations => generatedLocations;
    
    [SerializeField]private List<Location> roadLocations = new List<Location>();
    public List<Location> RoadLocations => roadLocations;
    public List<Location> Locations
    {
        get
        {
            List<Location> allLocations = new List<Location>();
            allLocations.AddRange(preplacedLocations);
            allLocations.AddRange(generatedLocations);
            allLocations.AddRange(roadLocations);
            return allLocations;
        }
    }
    #endregion
    
    #region Gizmos and Handles
    
    [SerializeField]private bool drawAreaOutlines;
    [SerializeField]private bool drawHandles;
    [SerializeField] private bool displayNames;
    public bool DrawAreaOutlines => drawAreaOutlines;
    public bool DrawHandles => drawHandles;
    private GUIStyle labelStyle = new GUIStyle();
    // This code is always executed when this object is in the Scene
    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0, 1, 0.4f);
        for (int i = 0; i < Locations.Count; i++)
        {
            Gizmos.DrawCube(Locations[i].position, Vector3.one);
        }
    }

    // This code is executed when we have the object selected
    private void OnDrawGizmosSelected()
    {
        if (displayNames)
        {
            labelStyle.fontSize = 24;
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.normal.textColor = Color.white;
            labelStyle.alignment = TextAnchor.MiddleCenter;

            for (int i = 0; i < Locations.Count; i++)
            {
                Locations[i].Name = Locations[i].locationType.ToString() + i;
                Handles.Label(Locations[i].position + Vector3.up * 5f, Locations[i].Name, labelStyle);
            }
        }
        
        
    }

    private void DrawDebugLines()
    {
        for (int i = 1; i < Locations.Count; i++)
        {
            Debug.DrawLine(Locations[i - 1].position, Locations [i].position, Color.red);
        }
    }
    #endif
    #endregion

    #region Area Generation Parameters
    [Header("Area Generation Parameters")]
    [SerializeField] private Vector2Int minAreaSize = new Vector2Int(4, 4);
    [SerializeField] private Vector2Int maxAreaSize = new Vector2Int(10, 10);
    #endregion
    
    private GridManager _gridManager;
    private bool[,] visited = null;
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if(_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        _gridManager = GridManager.Instance;
    }
    
    public void GenerateNewAreas()
    {
#if UNITY_EDITOR
        _gridManager = FindObjectOfType<GridManager>();
#endif
        ClearAllGeneratedAreas();
        _gridManager.ResetGrid();
        Vector2Int gridSize = _gridManager.gridSize;
        int nodeSize = _gridManager.nodeSize;
        if (visited == null)
        {
        }
        visited = new bool[gridSize.x, gridSize.y];
        float gridRadius = Mathf.Abs((_gridManager.grid[0,0].position - _gridManager.grid[(int)(gridSize.x / 2f),(int)(gridSize.y / 2f)].position).magnitude);
        HandlePreplacedLocations();
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                if(_gridManager.occupied[x, y] || visited[x, y]) continue;
                Vector2Int areaSize = FindValidArea(new Vector2Int(x, y), visited);
                Vector3 areaRawSize = new Vector3(areaSize.x * nodeSize, 0, areaSize.y * nodeSize);
                Vector3 minRawArea = new Vector3(4, 0, 4);
                if (areaRawSize.x <= minRawArea.x || areaRawSize.z <= minRawArea.z)
                {
                    //If anything's raw area is too small, just don't generate anything at all
                    continue;
                }
                if(areaSize == Vector2Int.zero)
                {
                    //If anything should be out in areas that are too small, determine that here
                    continue;
                }

                for (int i = 0; i < areaSize.x; i++)
                {
                    for (int j = 0; j < areaSize.y; j++)
                    {
                        int gx = x + i;
                        int gy = y + j;
                        _gridManager.occupied[gx, gy] = true;
                        visited[gx, gy] = true;
                    }
                }
                Vector3 center = _gridManager.GetNodeWorldPosition(_gridManager.grid[x,y]) +
                                 new Vector3(areaSize.x * nodeSize, 0, areaSize.y * nodeSize) * 0.5f;
                float centerToGridCenterDistance = Mathf.Abs((center -_gridManager.GetNodeWorldPosition( _gridManager.grid[(int)(gridSize.x / 2f),(int)(gridSize.y / 2f)])).magnitude);
                Location loc = new Location();
                AreaType locType;
                if (centerToGridCenterDistance >= gridRadius / 2f)
                {
                    locType = AreaType.TallBlock;
                }
                else
                {
                    locType = AreaType.StandardBlock;
                }
                loc.Initialize(locType, 
                    center, 
                    new Vector3(areaSize.x * nodeSize, 0, areaSize.y * nodeSize));
                generatedLocations.Add(loc);
                GenerateRoadBelt(new Vector2Int(x, y), areaSize, nodeSize);
            }
        }
        
    }

    public void HandlePreplacedLocations()
    {
#if UNITY_EDITOR
        _gridManager = FindObjectOfType<GridManager>();
#endif
        int nodeSize = _gridManager.nodeSize;
        Vector2Int gridSize = _gridManager.gridSize;
        if (visited == null)
        {
            visited = new bool[gridSize.x, gridSize.y];
        }
        foreach (Location loc in preplacedLocations)
        {
            _gridManager.SnapLocToGrid(loc);
            if (loc.coveredNodes == null)
            {
                Debug.LogWarning($"Location {loc.Name} has no covered nodes");
                continue;
            }
            foreach (Node n in loc.coveredNodes)
            {
                Vector2Int globalIndex = _gridManager.GetNodeIndex(n);
                visited[globalIndex.x, globalIndex.y] = true;
                n.occupied = true;
            }
            Vector2Int bottomLeft = _gridManager.GetNodeIndex(loc.bottomLeft);
            Vector2Int dimensions = new Vector2Int(loc.Width, loc.Depth);
            GenerateRoadBelt(bottomLeft, dimensions, nodeSize);
        }
    }
    
    Vector2Int FindValidArea(Vector2Int startCoords, bool[,] visited)
    {
        GridManager gm = _gridManager;
        int maxW = Mathf.Min(maxAreaSize.x, gm.gridSize.x - startCoords.x);
        int maxH = Mathf.Min(maxAreaSize.y, gm.gridSize.y - startCoords.y);

        maxW = Mathf.Min(maxW, 30);
        maxH = Mathf.Min(maxH, 30);

        List<Vector2Int> validSizes = new List<Vector2Int>();
        
        for (int w = maxW; w >= minAreaSize.x; w--)
        {
            for (int h = maxH; h >= minAreaSize.y; h--)
            {
                bool canPlace = true;
                for (int i = 0; i < w && canPlace; i++)
                {
                    for (int j = 0; j < h && canPlace; j++)
                    {
                        int gx = startCoords.x + i;
                        int gy = startCoords.y + j;
                        if(gm.occupied[gx, gy] || visited[gx, gy]) canPlace = false;
                    }
                }
                if(canPlace)
                    validSizes.Add(new Vector2Int(w, h)); 
            }
        }

        if (validSizes.Count > 0)
        {
            return validSizes[Random.Range(0, validSizes.Count)];
        }
        
        return Vector2Int.zero;
    }

    void GenerateRoadBelt(Vector2Int startCoords, Vector2Int dimensions, int nodeSize)
    {
        GridManager gm = _gridManager;
        
        List<Vector2Int> beltCoords = new List<Vector2Int>();

        //Horizontal
        for (int i = -1; i <= dimensions.x; i++)
        {
            AddRoadTile(new Vector2Int(startCoords.x + i, startCoords.y - 1),beltCoords);
            AddRoadTile(new Vector2Int(startCoords.x + i, startCoords.y + dimensions.y),beltCoords);
        }
        //Vertical
        for (int j = 0; j <= dimensions.y; j++)
        {
            AddRoadTile(new Vector2Int(startCoords.x - 1, startCoords.y + j),beltCoords);
            AddRoadTile(new Vector2Int(startCoords.x + dimensions.x, startCoords.y + j),beltCoords);
        }

        
        List<List<Vector2Int>> roadStrips = GroupRoadTilesIntoLines(beltCoords);
        
        foreach (List<Vector2Int> line in roadStrips)
        {
            if (line.Count == 0) continue;
    
            
            Vector3 start = gm.GetNodeWorldPosition(gm.grid[line[0].x, line[0].y]);
            Vector3 end = gm.GetNodeWorldPosition(gm.grid[line[line.Count - 1].x, line[line.Count - 1].y]);

            Vector3 center = (start + end) * 0.5f;
            float width;
            float depth;

            if (Mathf.Abs(end.x - start.x) > Mathf.Abs(end.z - start.z))
            {
                width = end.x - start.x;
                depth = nodeSize * 0.9f;
            }
            else
            {
                width = nodeSize * 0.9f;
                depth = end.z - start.z;
            }

            Vector3 size = new Vector3(width, 0.1f, depth);

            Location roadLoc = new Location();
            roadLoc.Initialize(AreaType.Road, center, size);
            _gridManager.SetNodesUnderLoc(roadLoc);
            roadLocations.Add(roadLoc);
        }

    }

    void AddRoadTile(Vector2Int startCoords, List<Vector2Int> roadList)
    {
        GridManager gm = _gridManager;
        if (startCoords.x < 0 || startCoords.x >= gm.gridSize.x || startCoords.y < 0 ||
            startCoords.y >= gm.gridSize.y) return;
        if(gm.occupied[startCoords.x, startCoords.y]) return;
        gm.occupied[startCoords.x, startCoords.y] = true;
        gm.grid[startCoords.x, startCoords.y].type = NodeType.ROAD;
        roadList.Add(new Vector2Int(startCoords.x, startCoords.y));
    }

    List<List<Vector2Int>> GroupRoadTilesIntoLines(List<Vector2Int> coords)
    {
        List<List<Vector2Int>> result = new List<List<Vector2Int>>();

        HashSet<Vector2Int> visited = new HashSet<Vector2Int>(coords);

        while (visited.Count > 0)
        {
            Vector2Int start = visited.First();
            visited.Remove(start);

            List<Vector2Int> line = new List<Vector2Int> { start };

            // Going X
            Vector2Int current = start;
            while (visited.Contains(new Vector2Int(current.x + 1, current.y)))
            {
                current = new Vector2Int(current.x + 1, current.y);
                line.Add(current);
                visited.Remove(current);
            }

            if (line.Count == 1)
            {
                // Going Y
                current = start;
                while (visited.Contains(new Vector2Int(current.x, current.y + 1)))
                {
                    current = new Vector2Int(current.x, current.y + 1);
                    line.Add(current);
                    visited.Remove(current);
                }
            }

            result.Add(line);
        }

        return result;
    }

    public void ClearAllAreas()
    {
#if UNITY_EDITOR
        _gridManager = FindObjectOfType<GridManager>();
#endif
        _gridManager.LiberateGrid();
        preplacedLocations.Clear();
        generatedLocations.Clear();
        roadLocations.Clear();
        if (visited != null)
        {
            for (int i = 0; i < visited.GetLength(0); i++)
            {
                for (int j = 0; j < visited.GetLength(1); j++)
                {
                    visited[i, j] = false;
                }
            }
        }
        
    }
    
    public void ClearAllGeneratedAreas()
    {
        #if UNITY_EDITOR
        _gridManager = FindObjectOfType<GridManager>();
        #endif
        if (_gridManager.grid != null)
        {
            _gridManager.LiberateGridOverLocations(generatedLocations);
            _gridManager.LiberateGridOverLocations(roadLocations);
        }
        List<Node> nodesToClear = new List<Node>();
        foreach (Location l in generatedLocations)
        {
            if (l.coveredNodes != null)
            {
                foreach (Node n in l.coveredNodes)
                {
                    nodesToClear.Add(n);
                }
            }
            
        }
        foreach (Location l in roadLocations)
        {
            if (l.coveredNodes != null)
            {
                foreach (Node n in l.coveredNodes)
                {
                    nodesToClear.Add(n);
                }
            }
        }
        generatedLocations.Clear();
        roadLocations.Clear();
        foreach (Node n in nodesToClear)
        {
            Vector2Int nodeIndex = _gridManager.GetNodeIndex(n);
            visited[nodeIndex.x, nodeIndex.y] = false;
        }
    }
}

public class EpicGizmoDrawer
{
    #if UNITY_EDITOR
    // Another way to draw Gizmos. With the attribute we control under which circumstances these Gizmos should be drawn
    [DrawGizmo(GizmoType.Selected)]
    private static void IAlsoWantToDrawGizmosFrownyFace(AreaManager script, GizmoType gizmoType)
    {
        if (script.DrawAreaOutlines)
        {
            for (int i = 0; i < script.Locations.Count; i++)
            {
                Gizmos.DrawWireCube(script.Locations[i].position, script.Locations[i].size);
            }
        }
    }
    #endif
}