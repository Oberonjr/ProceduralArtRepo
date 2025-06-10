using System;
using UnityEngine;
using TMPro;
public class FPSTracker : MonoBehaviour
{
   [SerializeField] TMP_Text fpsText;

   private int lastFrameIndex;
   private float[] frameDeltaTimeArray;

   private void Awake()
   {
      frameDeltaTimeArray = new float[50];
   }

   private void Update()
   {
      frameDeltaTimeArray[lastFrameIndex] = Time.unscaledDeltaTime;
      lastFrameIndex = (lastFrameIndex + 1) % frameDeltaTimeArray.Length;

      
   }

   private void FixedUpdate()
   {
      fpsText.text = "FPS: " + Mathf.RoundToInt(CalculateFPS());
   }

   private float CalculateFPS()
   {
      float total = 0f;
      foreach (float deltaTime in frameDeltaTimeArray)
      {
         total += deltaTime;
      }
      return frameDeltaTimeArray.Length / total;
   }
}
