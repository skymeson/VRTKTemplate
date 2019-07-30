using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DoomtrinityFPSPrototype.Audio;
using DoomtrinityFPSPrototype.Utils;

namespace DoomtrinityFPSPrototype.dtGUI {
	// Main menu in 'Menu' scene.
	public class Menu : MonoBehaviour {

		// ========================================================================================================================
		// Instance variables

		[SerializeField] private SettingsMenu settingsMenu;
		[SerializeField] private int firstScene = 1;
		[SerializeField] private Text infoMsg;

	    // ========================================================================================================================
		// Play

		public void Play() {
			SaveLoadData.Instance.LoadNewScene(firstScene);
	    }

		// ========================================================================================================================
		// Continue

	    public void Continue()
	    {
			if (SaveLoadData.Instance.CanLoadScene ()) {
				SaveLoadData.Instance.LoadSceneFromMenu ();
			} else {
				string msg = "Load file not found! Press NEW GAME to start.";
				StopCoroutine ("InfoMessageShow");
				StartCoroutine("InfoMessageShow", msg);
			}
	                
	    }

		// ========================================================================================================================
		// InfoMessageShow

		private IEnumerator InfoMessageShow(string msg) {
			infoMsg.text = msg;
			infoMsg.gameObject.SetActive (true);
			yield return new WaitForSeconds (5);
			infoMsg.gameObject.SetActive (false);
		}

		// ========================================================================================================================
		// Quit

	    public void Quit() {
			Application.Quit ();
		}

		// ========================================================================================================================
		// OptionsMenu

		public void OptionsMenu() {
			this.gameObject.SetActive(false);
			settingsMenu.gameObject.SetActive (true);
			CubeSpinner.OnMenuChange ();
		}

		// ========================================================================================================================
	}
}