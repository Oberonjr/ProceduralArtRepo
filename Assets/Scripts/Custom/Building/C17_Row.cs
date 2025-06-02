using System.Collections.Generic;
using Demo;
using UnityEngine;

public class C17_Row : Shape
{
    public int Number;
    public Floor floor=null;
    public Vector3 direction;
    private bool hasUniqueDetail = false;
    
    public void Initialize(int Number, Floor pFloor, bool UniqueDetail = false, Vector3 dir=new Vector3()) {
        this.Number=Number;

        // All public reference types must be cloned, to avoid unexpected shared reference errors when 
        //  changing values in the inspector!:
        this.floor=pFloor;

        if (dir.magnitude!=0) {
            direction=dir;
        } else {
            direction=new Vector3(1, 0, 0);
        }
        hasUniqueDetail = UniqueDetail;
    }

    protected override void Execute() {
        if (Number <= 3)
        {
            Debug.LogWarning("Cannot generate rows with a width smaller than 3!");
            return;
        }

        var param = Root.GetComponent<C17_FacadeParameters>();
        List<int> pattern = null;
        if (param!=null) {
            pattern=param.wallPattern;
        }
        int doorLocation = Random.Range(1 , Number - 1);
        for (int i=0;i<Number;i++) {            // spawn the prefabs
            // Choose a prefab index, either...
            int index = 0;
            if (pattern!=null && pattern.Count>0) { // ...given by the pattern from FacadeParameters, or ...
                index = pattern[i % pattern.Count] % floor.WallStyle.Length;
            } // ...(pseudo-)randomly chosen.
            else if (floor.FloorType == FloorType.GroundFloor)
            {
                if (i == doorLocation && hasUniqueDetail)
                {
                    index = 0;
                }
                else
                {
                    index = Random.Range(1,floor.WallStyle.Length);
                }
            }
            else { 
                index = RandomInt(floor.WallStyle.Length);
            }
            
            // Spawn the prefab, using i and the direction vector to determine the position:
            SpawnPrefab(floor.WallStyle[index],
                direction * (i - (Number-1)/2f)
            );
        }
    }
}
