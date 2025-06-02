using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPainter : MonoBehaviour
{
    [SerializeField] private GameObject buildingPrefab;
	public List<GameObject> createdBuildings = new List<GameObject>();

	public GameObject CreateBuilding(Vector3 position)
    {
        GameObject building = Instantiate(buildingPrefab);
        building.transform.position = position;
		createdBuildings.Add(building);
        return building;
    }
}
