using Adobe.Substance;
using Adobe.Substance.Input;
using Adobe.Substance.Runtime;
using UnityEngine;

public class TEST_SubstanceChanger : MonoBehaviour
{
    public SubstanceGraphSO SubstanceGraph;
    [Header("Window settings")]
    [SerializeField] private Vector2 outerFrameSize;
    [SerializeField, Range(1,10)] private int windowAmount;
    [SerializeField] private bool underWindow;
    [SerializeField, Range(0, 1)] private float underWindowHeight;

    [Header("Frame")] 
    [SerializeField, Range(0, 1)] private float grainIntensity;
    [SerializeField, Range(0, 1)] private float frameRoughness;
    [SerializeField, Range(0, 1)] private float frameMetallic;

    [Header("Glass")] 
    [SerializeField, Range(0, 1)] private float deteriorationEdge;
    [SerializeField, Range(0, 16)] private float deteriorationRefraction;
    [SerializeField, Range(0, 1)] private float colorHue;
    [SerializeField, Range(0, 1)] private float colorSaturation;
    [SerializeField, Range(0, 1)] private float colorLightness;
    [SerializeField, Range(0, 1)] private float glassRoughness;
    [SerializeField, Range(0, 1)] private float glassMetallic;
    
    [Header("Glass shatter")]
    [SerializeField, Range(0, 1)] private float brokenPanels;
    [SerializeField, Range(0, 1)] private float shatterDepth;
    [SerializeField, Range(0, 1)] private float shatterIntensity;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (var param in SubstanceGraph.Input)
        {
            //Debug.Log($"Label: {param.Description.Label}; Identifier: {param.Description.Identifier};  Type: {param.Description.Type}");
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var param in SubstanceGraph.Input)
        {
            switch (param.Description.Identifier)
            {
                case "Size_xy":
                    SubstanceInputFloat2 windowSize = param as SubstanceInputFloat2;
                    windowSize.Data = outerFrameSize;
                    break;
                case "x_amount":
                    SubstanceInputInt iWindowAmount = param as SubstanceInputInt;
                    iWindowAmount.Data = windowAmount;
                    break;
                case "switch":
                    SubstanceInputInt windowSwitch = param as SubstanceInputInt;
                    if (underWindow)
                    {
                        windowSwitch.Data = 1;
                    }
                    else
                    {
                        windowSwitch.Data = 0;
                    }
                    break;
                case "offset":
                    SubstanceInputFloat offsetUnderWindow = param as SubstanceInputFloat;
                    offsetUnderWindow.Data = underWindowHeight;
                    break;
                case "opacitymult":
                    SubstanceInputFloat IGrainIntensity = param as SubstanceInputFloat;
                    IGrainIntensity.Data = grainIntensity;
                    break;
                case "height_depth":
                    SubstanceInputFloat IDeteriorationEdge = param as SubstanceInputFloat;
                    IDeteriorationEdge.Data = deteriorationEdge;
                    break;
                case "Intensity":
                    SubstanceInputFloat IDeteriorationRefraction = param as SubstanceInputFloat;
                    IDeteriorationRefraction.Data = deteriorationRefraction;
                    break;
                case "hue_1":
                    SubstanceInputFloat IColorHue = param as SubstanceInputFloat;
                    IColorHue.Data = colorHue;
                    break;
                case "saturation_1":
                    SubstanceInputFloat IColorSaturation = param as SubstanceInputFloat;
                    IColorSaturation.Data = colorSaturation;
                    break;
                case "luminosity_1":
                    SubstanceInputFloat ILightness = param as SubstanceInputFloat;
                    ILightness.Data = colorLightness;
                    break;
                case "Position":
                    SubstanceInputFloat IBrokenPanels = param as SubstanceInputFloat;
                    IBrokenPanels.Data = brokenPanels;
                    break;
                case "hardness":
                    SubstanceInputFloat IShatterDepth = param as SubstanceInputFloat;
                    IShatterDepth.Data = shatterDepth;
                    break;
                case "Intensity_1":
                    SubstanceInputFloat IShatterIntensity = param as SubstanceInputFloat;
                    IShatterIntensity.Data = shatterIntensity;
                    break;
                case "opacitymult_1":
                    SubstanceInputFloat IFrameRoughness = param as SubstanceInputFloat;
                    IFrameRoughness.Data = frameRoughness;
                    break;
                case "opacitymult_2":
                    SubstanceInputFloat IGlassRoughness = param as SubstanceInputFloat;
                    IGlassRoughness.Data = glassRoughness;
                    break;
                case "opacitymult_3":
                    SubstanceInputFloat IGlassMetallic = param as SubstanceInputFloat;
                    IGlassMetallic.Data = glassMetallic;
                    break;
                case "opacitymult_4":
                    SubstanceInputFloat IFrameMetallic = param as SubstanceInputFloat;
                    IFrameMetallic.Data = frameMetallic;
                    break;
                default:
                    break;
            }
            
        }
    }
}
