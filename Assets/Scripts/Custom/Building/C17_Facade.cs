using System;
using System.Collections.Generic;
using Demo;
using UnityEngine;
using Random = UnityEngine.Random;

public class C17_Facade : Shape
{
    public int HeightRemaining;
    public int Width;
    public bool DoorHaver;
    [HideInInspector] public bool isFirst = true;
    [HideInInspector] public List<int> localPattern = null;
    [HideInInspector] public bool slanted = false;
    private bool generateWithPattern = false;
    private int localPatternIndex = 0;
    public Action<List<int>> OnFacadeFinished;
    public void Initialize(int pHeightRemaining, int pWidth, bool pIsFirst = true, bool pGenerateWithPattern = false, List<int> pLocalPattern = null, int pLocalPatternIndex = 0) {
        HeightRemaining=pHeightRemaining;
        Width=pWidth;
        isFirst=pIsFirst;
        generateWithPattern=pGenerateWithPattern;
        localPattern=pLocalPattern;
        localPatternIndex=pLocalPatternIndex;
    }

    public void Build()
    {
        GetComponent<C17_FacadeParameters>().CheckContents();
        Execute();
    }
    
    protected override void Execute() {
        
        if (HeightRemaining <= 0)
        {
            Root.GetComponent<C17_Facade>().BroadcastPattern(localPattern);
            return;
        }
        C17_FacadeParameters param = Root.GetComponent<C17_FacadeParameters>();
        slanted = param.slantedRoof;
        C17_Row wall = CreateSymbol<C17_Row>("wall");
        if (isFirst && param.hasFloor)
        {
            
            wall.Initialize(Width, param.groundFloor, DoorHaver);
        }
        else if(HeightRemaining == 1 && param.hasRoof && slanted)
        {
            //wall.Initialize(Width, param.roofFloor);
            C17_Roof roof = CreateSymbol<C17_Roof>("roof");
            roof.Initialize(Width, param.roofFloor.WallStyle[0]);
            roof.Build();
        }
        else if (generateWithPattern)
        {
            wall.Initialize(Width, param.wallStyle[localPattern[localPatternIndex]]);
            localPatternIndex++;
        }
        else
        {
            if(localPattern == null)
                localPattern = new List<int>();
            int r = Random.Range(0, param.wallStyle.Length);
            while (param.wallStyle[r].FloorType == FloorType.GroundFloor || param.wallStyle[r].FloorType == FloorType.Roof)
            {
                r = Random.Range(0, param.wallStyle.Length);
            }
            wall.Initialize(Width, param.wallStyle[r]);
            localPattern.Add(r);
            localPatternIndex++;
        }
        wall.Generate(buildDelay);

        C17_Facade nextLayer = CreateSymbol<C17_Facade>("facade" + localPatternIndex, new Vector3(0, 1, 0));
        nextLayer.Initialize(HeightRemaining-1, Width, false, generateWithPattern, localPattern, localPatternIndex);
        nextLayer.Generate(buildDelay);
    }

    protected void BroadcastPattern(List<int> pLocalPattern)
    {
        OnFacadeFinished?.Invoke(pLocalPattern);
    }
}
