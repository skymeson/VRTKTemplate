using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using UnityEngine.SceneManagement;
using DoomtrinityFPSPrototype.dtGUI;
using DoomtrinityFPSPrototype.Audio;
using DoomtrinityFPSPrototype.InputManagement;

namespace DoomtrinityFPSPrototype.Utils {

	public class SettingsSavedMessage : BaseMessage {
		public SettingsSavedMessage() { }
	}

	public class SaveLoadSettings : SingletonAsComponent<SaveLoadSettings> {

	    // ========================================================================================================================
	    // instance variables

	    // This variable holds all data to save/load.
	    public SaveSettings gameSettings { get; private set; }

		// ========================================================================================================================
	    // Singleton stuff

		public static SaveLoadSettings Instance {
			get { return ((SaveLoadSettings)_Instance); }
			set { _Instance = value; }
		}		

		private void Awake() {
		
			DontDestroyOnLoad(this.gameObject); // Persist across scenes.                         

			gameSettings = new SaveSettings();
			if (LoadXML (Application.persistentDataPath + "/GameCfg.xml")) {
				RestoreSettings ();
			} else {
				// No xml, load default value.
				LoadDefaultSettings ();
			}
		}

		// ========================================================================================================================
		// RestoreSettings

		private void RestoreSettings() {
			LoadKeyBinds ();
		}

		// ========================================================================================================================
		// LoadKeyBinds

		// Make sure to call this in Awake ( or in a method called in Awake ). [RuntimeInitializeOnLoadMethod] is used in 'Init'
		// method of 'InputManager' to load default stuff. That attribute lets the method to be called automatically, 
		// after all 'Awake' methods are fired. If we first restore key binds from here, a bool that makes the 'InputManager'
		// skip the load of default binds will be set.
		private void LoadKeyBinds() {

			int[] actions = gameSettings.buttonsCfg.actions;
			int[] keys = gameSettings.buttonsCfg.keys;

			InputManager.LoadBinds (actions, keys);
		}

	    // ========================================================================================================================
	    // LoadDefaultSettings

		private void LoadDefaultSettings() {
	        // input
			// Keybinds default are loaded through 'InputManager'
	        gameSettings.inputCfg.mouseSensitivity = 2;

			// Video stuff is saved by Unity and we can use Screen class to restore UI.

	        // audio
	        gameSettings.audioCfg.masterVol = 1;
	        gameSettings.audioCfg.musicVol = 1;
	        gameSettings.audioCfg.sfxVol = 1;
	    }

	    // ========================================================================================================================
	    // GetSettingsToSave

		private void GetSettingsToSave() {

			SettingsMenu settingsMenu = FindObjectOfType<SettingsMenu>();

	        // =======================================================================
	        // input

			if (settingsMenu != null) {
				gameSettings.inputCfg.mouseSensitivity = settingsMenu.mouseSensitivity;
	        }
			GetKeyBinds ();

	        // =======================================================================
	        // video

	        // Video stuff is saved by Unity and we can use Screen class to restore UI.

	        // =======================================================================
	        // audio
			if (AudioManager.Instance != null) {
				gameSettings.audioCfg.masterVol = AudioManager.Instance.masterVolumePercent;
				gameSettings.audioCfg.musicVol = AudioManager.Instance.musicVolumePercent;
				gameSettings.audioCfg.sfxVol = AudioManager.Instance.sfxVolumePercent;
			}
	    }

		// ========================================================================================================================
		// GetKeyBinds

		private void GetKeyBinds() {
			
			int actionsCount = InputManager.KeyBinds.Count;
			gameSettings.buttonsCfg.actions = new int[actionsCount];
			gameSettings.buttonsCfg.keys = new int[actionsCount];

			int i = 0;
			// Cycle through all binds to get the action and its relative physical key, then store them in 'gameSettings'.
			foreach (KeyValuePair<ActionCode,KeyCode> bind in InputManager.KeyBinds) {
				gameSettings.buttonsCfg.actions [i] = (int)bind.Key;
				gameSettings.buttonsCfg.keys [i] = (int)bind.Value;
				i++;
			}
		}

		// ========================================================================================================================
	    // SaveXML

	    // Saves game data to XML file.
	    // Call this function to save settings to an XML file.
		private void SaveXML(string FileName = "GameConfig.xml")
	    {
	        //Get settings to save.
	        GetSettingsToSave();

	        // Save game settings in the external xml file.
	        XmlSerializer Serializer = new XmlSerializer(typeof(SaveSettings));
			using (FileStream Stream = new FileStream (FileName, FileMode.Create)) {
				Serializer.Serialize (Stream, gameSettings);
			}

			MessagingSystem.Instance.QueueMessage (new SettingsSavedMessage ());
	    }

	    // ========================================================================================================================
	    // LoadXML

	    // Load game data from XML file.
	    // Call this function to load settings from an XML file.
	    private bool LoadXML( string FileName = "GameConfig.xml")
	    {
	        // If file doesn't exist, then exit.
	        if (!File.Exists(FileName)) return false;

	        // Get data from xml file.
	        XmlSerializer Serializer = new XmlSerializer(typeof(SaveSettings));
			using (FileStream Stream = new FileStream (FileName, FileMode.Open)) {
				gameSettings = Serializer.Deserialize (Stream) as SaveSettings;
				return true;
			}
	        return false;
	    }

		// ========================================================================================================================
		// SaveCfg

	    public void SaveCfg() {
			SaveXML(Application.persistentDataPath + "/GameCfg.xml");
			Debug.Log("Saved to: " + Application.persistentDataPath + "/GameCfg.xml");
	    }

		// ========================================================================================================================
	}

	// ========================================================================================================================
	// class SaveSettings

	//Data to save to file XML or Binary
	[System.Serializable]
	[XmlRoot("GameConfig")]
	public class SaveSettings {

		// Settings structures to save/load to and from file.
		// This represents a conversion of object and values into simpler ones, like floats,
		// which can be serialized and saved in external xml file.

		[System.Serializable]
		public struct InputCfg_s
		{
			public float mouseSensitivity;
		}

		[System.Serializable]
		public struct AudioCfg_s
		{
			public float masterVol;
			public float musicVol;
			public float sfxVol;
		}

		[System.Serializable]
		public struct ButtonsCfg_s
		{
			public int[] actions;
			public int[] keys;
		}


		// objects to save
		public InputCfg_s inputCfg = new InputCfg_s();
		public AudioCfg_s audioCfg = new AudioCfg_s();
		public ButtonsCfg_s buttonsCfg = new ButtonsCfg_s();

	}
}