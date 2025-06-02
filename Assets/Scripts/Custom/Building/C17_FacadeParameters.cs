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

}
