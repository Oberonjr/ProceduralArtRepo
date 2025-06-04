using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GridManager : MonoBehaviour
{
    #region GridParameters

    [Header("Grid Parameters")] 
    public Vector2Int gridSize = new Vector2Int(10, 10);
    public int nodeSize = 1;
    [HideInInspector] public Node[,] grid;
    [HideInInspector] public bool[,] occupied;
    #endregion

    #region GridVisualizing

    [Header("Grid Visualizing")] 
    public bool drawOutline;
    public bool drawGrid;
    private GUIStyle labelStyle = new GUIStyle();

    private void OnDrawGizmos()
    {
        if (grid == null) return;
        if (!drawGrid) return;
        foreach (Node n in grid)
        {
            Gizmos.color = GetColorForType(n);
            Gizmos.DrawCube(new Vector3(n.position.x, 0.2f, n.position.y), new Vector3(0.9f, 0.1f, 0.9f) * nodeSize);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (grid == null) return;
        if (drawOutline)
        {
            Vector3 center = transform.position;
            Vector3 size = new Vector3(gridSize.x, 0, gridSize.y) * nodeSize;
            Gizmos.DrawWireCube(center + Vector3.up * 0.2f, size + new Vector3(0.2f, 0, 0.2f));
        }
    }

    private Color GetColorForType(Node node)
    {
        Color color;
        NodeType type = node.type;
        switch (type)
        {
            case NodeType.BUILDING:
                color = Color.blue;
                break;
            case NodeType.ROAD:
                color = Color.yellow;
                break;
            default:
                color = Color.gray;
                break;
        }

        color.a = 0.5f;
        if (node.occupied)
        {
            color.a = 0.15f;
        }
        return color;
    }

    #endregion

    private AreaManager _areaManager;
    private GenerationManager _generationManager;
    private Vector3 worldCenter;
    private Vector2 gridWorldSize;
    private Vector3 bottomLeft;
    #region Singleton

    private static GridManager _instance;
    public static GridManager Instance => _instance;

    #endregion

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Debug.LogWarning("More than one GridManager in scene. Destroying object: " + gameObject.name);
            Destroy(gameObject);
        }

    }

    void Start()
    {
        _areaManager = AreaManager.Instance;
        _generationManager = GenerationManager.Instance;
        GenerateGrid();
        SnapAllToGrid();
        GenerationManager.Instance.GenerateCity();
    }

    //Node Grid generation
    public void GenerateGrid()
    {
#if UNITY_EDITOR
        _areaManager = FindObjectOfType<AreaManager>();
        _generationManager = FindObjectOfType<GenerationManager>();
#endif
        LiberateGrid();
        _areaManager.ClearAllGeneratedAreas();
        _generationManager.DestroyCity();
        grid = new Node[gridSize.x, gridSize.y];
        occupied = new bool[gridSize.x, gridSize.y];
        worldCenter = new Vector3(transform.position.x, 0, transform.position.z);
        gridWorldSize = new Vector2(gridSize.x * nodeSize, gridSize.y * nodeSize);
        bottomLeft = worldCenter - new Vector3(gridWorldSize.x, 0, gridWorldSize.y) * 0.5f;

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 nodeWorldPos = bottomLeft + new Vector3((x + 0.5f) * nodeSize, 0, (y + 0.5f) * nodeSize);
                NodeType type = NodeType.BUILDING;
                Node newNode = new Node(new Vector2(nodeWorldPos.x, nodeWorldPos.z), nodeSize, type);
                grid[x, y] = newNode;
            }
        }
    }
    
    
    //Node utility
    public Node GetClosestNode(Vector3 worldPos)
    {
        Node targetNode = null;
        float closestDistance = Mathf.Infinity;
        foreach (Node n in grid)
        {
            if (Vector3.Distance(worldPos, GetNodeWorldPosition(n)) < closestDistance)
            {
                closestDistance = Vector3.Distance(worldPos, GetNodeWorldPosition(n));
                targetNode = n;
            }
        }
        return targetNode;
    }

    public Vector2Int GetNodeIndex(Node node)
    {
        Vector3 nodeWorldPos = GetNodeWorldPosition(node);
        int x = (int)((nodeWorldPos.x - bottomLeft.x) / nodeSize);
        int y = (int)((nodeWorldPos.z - bottomLeft.z) / nodeSize);
        
        return new Vector2Int(x, y);
    }

    public Vector3 GetNodeWorldPosition(Node node, float yOffset = 0)
    {
        return new Vector3(node.position.x, yOffset, node.position.y);
    }

    public bool IsOnGridEdge(List<Node> nodes)
    {
        foreach (Node n in nodes)
        {
            Vector2Int index = GetNodeIndex(n);
            if (index.x == 0 || index.y == 0 || index.x == gridSize.x - 1 || index.y == gridSize.y - 1)
            {
                return true;
            }
        }
        return false;
    }
    
    
    
    //Area tooling
    public void SnapLocToGrid(Location loc)
    {
        Dictionary<Vector2Int, Node> nodesUnderLoc = GetNodesUnderLoc(loc);
        List<Node> touchedNodes = new List<Node>();
        foreach (KeyValuePair<Vector2Int, Node> n in nodesUnderLoc)
        {
            if(!n.Value.occupied)
                touchedNodes.Add(n.Value);
        }
        if (touchedNodes.Count == 0) return;
        if (loc.locationType == AreaType.Road && IsOnGridEdge(touchedNodes)) return;

        float minX = float.MaxValue; 
        float maxX = float.MinValue;
        float minZ = float.MaxValue; 
        float maxZ = float.MinValue;

        foreach (Node n in touchedNodes)
        {
            minX = Mathf.Min(minX, n.position.x);
            maxX = Mathf.Max(maxX, n.position.x);
            minZ = Mathf.Min(minZ, n.position.y);
            maxZ = Mathf.Max(maxZ, n.position.y);
        }

        minX -= nodeSize * 0.5f;
        maxX += nodeSize * 0.5f;
        minZ -= nodeSize * 0.5f;
        maxZ += nodeSize * 0.5f;

        float width = Mathf.Clamp(maxX - minX, 0, 30);
        float depth = Mathf.Clamp(maxZ - minZ, 0, 30);

        float centerX = (minX + maxX) * 0.5f;
        float centerZ = (minZ + maxZ) * 0.5f;

        loc.position = new Vector3(centerX, loc.position.y, centerZ);
        loc.size = new Vector3(width * 0.99f, loc.size.y, depth * 0.99f);
        SetNodesUnderLoc(loc, GetNodesUnderLoc(loc));
    }


    public void SnapAllToGrid()
    {
        foreach (Location l in _areaManager.Locations)
        {
            SnapLocToGrid(l);
        }
    }

    public Dictionary<Vector2Int, Node> GetNodesUnderLoc(Location loc)
    {
        if(grid == null) return null;
        Bounds locBounds = new Bounds(loc.position, loc.size);
        
        Node leftBottom = GetClosestNode(loc.position - (loc.size / 2));
        Vector2Int bLIndex = GetNodeIndex(leftBottom);
        
        Dictionary<Vector2Int, Node> localNodeMap = new Dictionary<Vector2Int, Node>();

        foreach (Node n in grid)
        {
            if (n.type == NodeType.ROAD && loc.locationType != AreaType.Road) continue;
            Vector3 nodeWorldPos = GetNodeWorldPosition(n, loc.position.y);
            Vector3 nodeSizeVec = new Vector3(nodeSize, 1, nodeSize);
            Bounds nodeBounds = new Bounds(nodeWorldPos, nodeSizeVec);
            if (!nodeBounds.Intersects(locBounds)) continue;

            Vector2Int nodeIndex = GetNodeIndex(n);
            Vector2Int localIndex = new Vector2Int(
                nodeIndex.x - bLIndex.x,
                nodeIndex.y - bLIndex.y
            );
            localNodeMap[localIndex] = n;
        }
        return localNodeMap;
    }
    
    public void SetNodesUnderLoc(Location loc, Dictionary<Vector2Int, Node> localNodeMap = null)
    {
        if(grid == null) return;
        if(localNodeMap == null) localNodeMap = GetNodesUnderLoc(loc);
        int width = localNodeMap.Keys.Max(k => k.x) + 1;
        int depth = localNodeMap.Keys.Max(k => k.y) + 1;

        loc.coveredNodes = new Node[width, depth];
        foreach (KeyValuePair<Vector2Int, Node> kvp in localNodeMap)
        {
            loc.coveredNodes[kvp.Key.x, kvp.Key.y] = kvp.Value;
            
            Vector2Int nodeIndex = GetNodeIndex(kvp.Value);
            if (nodeIndex.x >= 0 && nodeIndex.x < occupied.GetLength(0) &&
                nodeIndex.y >= 0 && nodeIndex.y < occupied.GetLength(1))
            {
                occupied[nodeIndex.x, nodeIndex.y] = true;
                grid[nodeIndex.x, nodeIndex.y].occupied = true;
            }
        }
    }

    public void LiberateGrid()
    {
        occupied = new bool[gridSize.x, gridSize.y];
    }

    public void ResetGrid()
    {
        foreach (Node n in grid)
        {
            n.type = NodeType.BUILDING;
        }
    }
    
    public void LiberateGridUnderLoc(Location loc)
    {
        if (loc.coveredNodes == null || loc.coveredNodes.Length == 0)
        {
            SetNodesUnderLoc(loc);
        }
        foreach (Node n in loc.coveredNodes)
        {
            Vector2Int nodeIndex = GetNodeIndex(n);
            occupied[nodeIndex.x, nodeIndex.y] = false;
            grid[nodeIndex.x, nodeIndex.y].occupied = false;
        }
    }

    public void LiberateGridOverLocations(List<Location> locations)
    {
        foreach (Location loc in locations)
        {
            LiberateGridUnderLoc(loc);
        }
    }
}