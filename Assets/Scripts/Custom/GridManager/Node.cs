using System;
using UnityEngine;

[Serializable]
public enum NodeType
{
    BUILDING,
    ROAD
}

public class Node 
{
    public Vector2 position;
    public int size;
    public NodeType type;
    public bool occupied;

    public Node(Vector2 pPosition, int pSize, NodeType pType = NodeType.BUILDING)
    {
        position = pPosition;
        size = pSize;
        type = pType;
    }
}
