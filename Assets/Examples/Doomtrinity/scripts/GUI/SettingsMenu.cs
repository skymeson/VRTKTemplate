using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DoomtrinityFPSPrototype.Audio;
using DoomtrinityFPSPrototype.Utils;

namespace DoomtrinityFPSPrototype.dtGUI {
	// Main menu in 'Menu' scene.
	public class SettingsMenu : MonoBehaviour {

		// ========================================================================================================================
		// Instance variables

		[SerializeField] private GameObject callerMenu;
		[SerializeField] private GameObject inputMenuHolder;
		[SerializeField] private GameObject videoMenuHolder;
		[SerializeField] private GameObject audioMenuHolder;
		[SerializeField] private GameObject mouseMenuHolder;
		[SerializeField] private GameObject buttonsMenuHolder;

		[SerializeField] private Slider[] volumeSliders;
		[SerializeField] private Toggle fullscreenToggle;
		[SerializeField] private Dropdown resolutionsDropdown;
		[SerializeField] private Slider mouseSensitivitySlider;
		[SerializeField] private Dropdown qualityPresetsDropdown;
		[SerializeField] private Toggle vSyncToggle;

		// variables to save/load
		public float mouseSensitivity { get; private set; }

		// ========================================================================================================================
		// Start

		private void Start() {
			// =============================================
			// Restore mouse UI
			mouseSensitivity = SaveLoadSettings.Instance.gameSettings.inputCfg.mouseSensitivity;
			mouseSensitivitySlider.value = mouseSensitivity;

			// =============================================
			// Restore audio sliders
			volumeSliders [0].value = SaveLoadSettings.Instance.gameSettings.audioCfg.masterVol;
			volumeSliders [1].value = SaveLoadSettings.Instance.gameSettings.audioCfg.sfxVol;
			volumeSliders [2].value = SaveLoadSettings.Instance.gameSettings.audioCfg.musicVol;

			// =============================================
			// Restore video settings UI
			fullscreenToggle.isOn = Screen.fullScreen;
			// Add the list of available resolutions to the dropdown object.
			List<string> labelRes = new List<string> ();
			int res_index = 0; // using for the selected item on the dropdown.
			bool skip_res_check = false;
			foreach (Resolution res in Screen.resolutions) {
				labelRes.Add (res.ToString());
				// Increment the res index (used for selected dropdown value) until we find the matching value.
				if (res.width == Screen.currentResolution.width) {
					skip_res_check = true;
				}
				if(!skip_res_check) res_index++;
			}
			resolutionsDropdown.AddOptions (labelRes);
			resolutionsDropdown.value = res_index;

			vSyncToggle.isOn = QualitySettings.vSyncCount == 1 ? true : false;
			// Add the list of available quality presets to the dropdown object.
			List<string> labelQuality = new List<string> (QualitySettings.names);

			qualityPresetsDropdown.AddOptions (labelQuality);
			qualityPresetsDropdown.value = QualitySettings.GetQualityLevel();
		}

		// ========================================================================================================================
		// InputMenu

		public void InputMenu()
		{
			inputMenuHolder.SetActive(true);
			audioMenuHolder.SetActive(false);
			videoMenuHolder.SetActive(false);
		}

		// ========================================================================================================================
		// MouseMenu

		public void MouseMenu()
		{
			mouseMenuHolder.SetActive(true);
			buttonsMenuHolder.SetActive(false);
		}

		// ========================================================================================================================
		// ButtonsMenu

		public void ButtonsMenu()
		{
			mouseMenuHolder.SetActive(false);
			buttonsMenuHolder.SetActive(true);
		}

		// ========================================================================================================================
		// VideoMenu

		public void VideoMenu()
		{
			inputMenuHolder.SetActive(false);
			audioMenuHolder.SetActive(false);
			videoMenuHolder.SetActive(true);
		}

		// ========================================================================================================================
		// AudioMenu

		public void AudioMenu()
		{
			inputMenuHolder.SetActive(false);
			audioMenuHolder.SetActive(true);
			videoMenuHolder.SetActive(false);
		}

		// ========================================================================================================================
		// MainMenu

		public void MainMenu() {
			SaveLoadSettings.Instance.SaveCfg();
			callerMenu.SetActive (true); // return to caller menu.
			this.gameObject.SetActive (false);
			CubeSpinner.OnMenuChange ();
		}

		// ========================================================================================================================
		// SetMouseSensitivity

		public void SetMouseSensitivity(float sensitivity) {
			mouseSensitivity = sensitivity;
		}

		// ========================================================================================================================
		// SetScreenRes

		public void SetScreenRes(int i) {
			Resolution res = Screen.resolutions[i]; // The index of the dropdown list matches the index of resolutions array,
			// since dropdown has been initialized with all available resolutions.
			Screen.SetResolution (res.width, res.height, true);
		}

		// ========================================================================================================================
		// SetQualityPreset

		public void SetQualityPreset(int i) {
			QualitySettings.SetQualityLevel (i,true);
		}

		// ========================================================================================================================
		// SetFullscreen

		public void SetFullscreen(bool isFullscreen) {
			Screen.fullScreen = isFullscreen;
		}

		// ========================================================================================================================
		// SetVSync

		public void SetVSync(bool enableVSync) {
			QualitySettings.vSyncCount = enableVSync? 1 : 0; // Should take into account vSyncCount '2' too...
		}

		// ========================================================================================================================
		// setMasterVolume

		public void setMasterVolume(float volume) {
			AudioManager.Instance.SetVolume (volume, AudioManager.AudioChannel.Master);
		}

		// ========================================================================================================================
		// setMusicVolume

		public void setMusicVolume(float volume) {
			AudioManager.Instance.SetVolume (volume, AudioManager.AudioChannel.Music);
		}

		// ========================================================================================================================
		// setSfxVolume

		public void setSfxVolume(float volume) {
			AudioManager.Instance.SetVolume (volume, AudioManager.AudioChannel.Sfx);
		}

		// ========================================================================================================================
	}
}
