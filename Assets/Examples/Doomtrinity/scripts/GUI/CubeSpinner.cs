using UnityEngine;
using System.Collections;

namespace DoomtrinityFPSPrototype.dtGUI {
[RequireComponent(typeof (MeshRenderer))]
public class CubeSpinner : MonoBehaviour {

	// ========================================================================================================================
	// Instance variables

	[SerializeField] private float defaultSpinVel = .05f;
	[SerializeField] private float fastSpinVel = 16.0f;
	private float spinDiffVel = 0;
	private float spinVel = .05f;
	[SerializeField] private float spinDecelTime = 0.5f;
	private static bool changeSpin = false;

	[SerializeField] private Color defaultColor = Color.cyan;
	[SerializeField] private Color changeColor = Color.red;
	private Material mat;

	// ========================================================================================================================
	// Start

	private void Start() {
		mat = GetComponent<MeshRenderer> ().material;
		mat.color = defaultColor;
		spinVel = defaultSpinVel;
		spinDiffVel = fastSpinVel - defaultSpinVel;
		Time.timeScale = 1; // if came back from pause menu, timescale is 0, so timed stuff won't run --> cube won't rotate
		StartCoroutine (Rotate ());
	}

	// ========================================================================================================================
	// Rotate

	private IEnumerator Rotate () {
		bool newSpin = changeSpin;
		while (true) {

			if(newSpin != changeSpin) { // fall here if 'options' or 'back' button is pressed.
				newSpin = changeSpin;
				mat.color = newSpin ? changeColor : defaultColor; // 'changeColor' in options menu, 'defaultColor' in main menu

				spinVel = fastSpinVel;
				spinDiffVel = fastSpinVel - defaultSpinVel;
			}

			spinVel = Mathf.Clamp (spinVel, defaultSpinVel, fastSpinVel);
			transform.Rotate(new Vector3(45,5,45)*Time.deltaTime*spinVel);

			if(spinVel > defaultSpinVel){
				if (spinDecelTime <= 0) { // avoid divide by zero
					spinDecelTime = 0.1f;
				}
				spinVel -= (spinDiffVel / spinDecelTime) * Time.deltaTime;
			}

			yield return null; // wait a frame.
		}

	}

	// ========================================================================================================================
	// OnMenuChange

	// Called when switching between main menu - options menu.
	public static void OnMenuChange() {
		changeSpin = !changeSpin;
	}

	// ========================================================================================================================
}
}