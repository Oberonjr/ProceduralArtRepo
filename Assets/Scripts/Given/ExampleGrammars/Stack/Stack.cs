using UnityEngine;

namespace Demo {
	public class Stack : Shape {
		public GameObject prefab;
		public int HeightRemaining;
		[SerializeField] private Vector3 rotationFactor;
		[SerializeField] private Vector3 scalingFactor = new Vector3(1, 1, 1);

		public void Initialize(GameObject pPrefab, int pHeightRemaining) {
			prefab=pPrefab;
			HeightRemaining=pHeightRemaining;
		}

		protected override void Execute() {
			// Spawn the (box) prefab as child of this game object:
			// (Optional parameters: localPosition, localRotation, alternative parent)
			GameObject box = SpawnPrefab(prefab);

			// Example: fat box:
			//box.transform.localScale=new Vector3(7, 1, 1);

			if (HeightRemaining>0) {
				Stack newStack = null;

				/**
				// Simple stack:
				// Spawn a smaller stack on top of this:
				newStack = CreateSymbol<Stack>("stack", new Vector3(0, 1, 0));
				newStack.Initialize(prefab, HeightRemaining-1);
				// Generate it with a short delay (when in play mode):
				newStack.Generate(buildDelay);
				**/
				
				/**
				// Scaling:
				// Every new stack gets a bit smaller:
				newStack = CreateSymbol<Stack>("stack", new Vector3(0, 1, 0));
				newStack.Initialize(prefab, HeightRemaining-1);
				newStack.transform.localScale= scalingFactor;
				newStack.Generate(buildDelay); 
				**/
				 
				/**
				// Rotation:
				// Every new stack rotates by 30 degrees around the y-axis:
				newStack = CreateSymbol<Stack>("stack", new Vector3(0, 1, 0));
				newStack.Initialize(prefab, HeightRemaining-1);
				newStack.transform.localRotation = Quaternion.Euler(rotationFactor);
				newStack.Generate(buildDelay); 
				**/


				/**
				// Rotation & scaling:
				// Every new stack rotates by 45 degrees around the z-axis, and becomes a bit smaller:
				newStack = CreateSymbol<Stack>("stack", Vector3.Scale(new Vector3(-0.25f, 1.25f, 0), scalingFactor));
				newStack.Initialize(prefab, HeightRemaining-1);
				newStack.scalingFactor = this.scalingFactor;
				newStack.rotationFactor = this.rotationFactor;
				newStack.transform.localRotation = Quaternion.Euler(rotationFactor);
				newStack.transform.localScale = scalingFactor;
				newStack.Generate(buildDelay); 				
				**/
			
				
				// Two smaller child stacks, spawned with an offset:
				// **** WARNING: don't do this with HeighRemaining values larger than about 8! ****
				//      (exponential growth breaks computers!)
				if (HeightRemaining>8) {
					HeightRemaining=8;
				}
				for (int i = 0; i<2; i++) {
					newStack = CreateSymbol<Stack>("stack", Vector3.Scale(new Vector3(i-0.5f, 1, 0), scalingFactor));
					newStack.Initialize(prefab, HeightRemaining-1);
					newStack.scalingFactor = scalingFactor;
					newStack.transform.localScale=scalingFactor;
					newStack.rotationFactor = this.rotationFactor;
					newStack.transform.localRotation = Quaternion.Euler(rotationFactor);
					newStack.Generate(buildDelay);
				}
				
			}
		}
	}
}