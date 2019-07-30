using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using DoomtrinityFPSPrototype.Character;
using DoomtrinityFPSPrototype.PrefabPoolSystem;
using DoomtrinityFPSPrototype.InputManagement;

namespace DoomtrinityFPSPrototype.Utils {
	// This class manages the game save and load stuff. 
	// It uses Singleton pattern. The instance persists across scenes ('DontDestroyOnLoad' method called in Awake).
	public class SaveLoadData : SingletonAsComponent<SaveLoadData>
	{
		// ========================================================================================================================
		// nested class PersistantData

		// This class exposes structs used to store the 'simplified' version of complex data ( like transform ),
		// which can be serialized and saved to external xml file.
	    [System.Serializable]
	    [XmlRoot("GameData")]
	    public class PersistantData
	    {
			// objects to save
			public string sceneName;
			public List<SaveStruct.Enemy_s> enemies_s = new List<SaveStruct.Enemy_s>();
			public SaveStruct.Player_s player_s = new SaveStruct.Player_s();
			public SaveStruct.Inventory_s inventory_s = new SaveStruct.Inventory_s();
			public SaveStruct.Stats_s stats_s = new SaveStruct.Stats_s ();
			public List<string> pickups = new List<string> ();
	    }

	    // ========================================================================================================================
	    // instance variables

	    // This variable holds all data to save/load.
		private PersistantData persistantData = new PersistantData();

		// This bool is used in 'LevelInitializer' script to know if it's a new game or a loaded game.
	    public bool isLoadingFromSave { get; private set; }

	    // ========================================================================================================================
	    // Singleton stuff

		public static SaveLoadData Instance {
			get { return ((SaveLoadData)_Instance); }
			set { _Instance = value; }
		}

		private void Awake()
		{
			DontDestroyOnLoad(this.gameObject); // Persist across scenes.
		}

		// ========================================================================================================================
	    // ClearPersistantData

	    private void ClearPersistantData()
	    {
	        persistantData.pickups.Clear();
			persistantData.enemies_s.Clear ();
	    }

	    // ========================================================================================================================
		// SaveData

		// Populate 'persistantData' variable.
		// This is the data to be saved to the file.
		private void SaveData() {
			ClearPersistantData();

			// Find all instances in the scene derived from 'PersistantMono'. More info in its script.
			PersistantMono[] persistantObjects = FindObjectsOfType<PersistantMono>();
			foreach (PersistantMono persistantObj in persistantObjects) {
				persistantObj.Save (persistantData);
			}

			// Get scene name.
			persistantData.sceneName = SceneManager.GetActiveScene().name;
			// Get stats.
			persistantData.stats_s.startEnemiesCount = EnemyBase.enemiesCount;
			persistantData.stats_s.killedEnemiesCount = EnemyBase.enemiesKilled;
		}

		// ========================================================================================================================
	    // SaveXML

	    // Saves game data to XML file.
	    // Call as SaveXML(Application.persistentDataPath + "/PersistantData.xml");
		private bool SaveXML(string FileName = "PersistantData.xml")
	    {
	        //Get player,enemies and inventory data to save.
	        SaveData();

	        // Save game data in the external xml file.
	        XmlSerializer Serializer = new XmlSerializer(typeof(PersistantData));
			using (FileStream Stream = new FileStream (FileName, FileMode.Create)) {
				Serializer.Serialize (Stream, persistantData);
				return true;
			}
			return false;
	    }

	    
	    // ========================================================================================================================
	    // LoadXML

	    // Load game data from XML file.
		// Call as LoadXML(Application.persistentDataPath + "/PersistantData.xml");
		private bool LoadXML( string FileName = "PersistantData.xml")
	    {
	        // If file doesn't exist, then exit.
	        if (!File.Exists(FileName)) return false;

	        // Get data from xml file.
	        XmlSerializer Serializer = new XmlSerializer(typeof(PersistantData));
			using (FileStream Stream = new FileStream (FileName, FileMode.Open)) {
				persistantData = Serializer.Deserialize (Stream) as PersistantData;
				return true;
			}
			return false;
	    }

		// Following methods are called by other scripts ( mainly 'LevelInitializer', 'GameUI', and 'Menu' ).

	    // ========================================================================================================================
	    // SaveGame

	    public bool SaveGame()
	    {
			bool saved = false;
			saved = SaveXML(Application.persistentDataPath + "/PersistantData.xml");
			if(saved) Debug.Log("Saved to: " + Application.persistentDataPath + "/PersistantData.xml");
			return saved;
	    }

	    // ========================================================================================================================
	    // LoadGame

	    public bool LoadGame()
	    {
			// Only populate 'persistantData' variable.
			// Restore is done in 'LevelInitializer' script.
			if( LoadXML(Application.persistentDataPath + "/PersistantData.xml") ){
				Debug.Log("Loaded from: " + Application.persistentDataPath + "/PersistantData.xml");
				return true;
			}
			return false;
	    }

	    // ========================================================================================================================
	    // LoadScene

		// Important! Use this to load a scene, don't call 'SceneManager.LoadScene' directly, 
		// prefab pool and other stuff must be reset first!
		public void LoadScene(string name)
	    {
			ResetStaticClasses ();
	        SceneManager.LoadScene(name);
	    }
		private void LoadScene(int index)
		{
			ResetStaticClasses ();
			SceneManager.LoadScene (index);
		}

		// ========================================================================================================================
		// ResetStaticClasses

		private void ResetStaticClasses() {
			// InputManager.Reset (); // Reset not needed ATM.
			PrefabPoolingSystem.Reset();
			// Reset 'enemiesKilled' and 'enemiesCount'. This is necessary since these are static variables,
			// and won't be reset automatically at scene restart while the game is running.
			EnemyBase.ResetEnemiesCount ();
		}

		// ========================================================================================================================
		// LoadSceneFromMenu

	    public void LoadSceneFromMenu()
	    {
			if (LoadXML (Application.persistentDataPath + "/PersistantData.xml")) {
				isLoadingFromSave = true;
				string sceneName = persistantData.sceneName;
				LoadScene(sceneName);
			}
	    }

		// ========================================================================================================================
		// CanLoadScene

		public bool CanLoadScene()
		{
			if (File.Exists(Application.persistentDataPath + "/PersistantData.xml")) { // should expose filename in field...
				return true;
			} 
			return false;
		}

	    // ========================================================================================================================
	    // LoadNewScene

	    public void LoadNewScene(int index)
	    {
	        isLoadingFromSave = false;

			if (index > 0 && index < SceneManager.sceneCountInBuildSettings) {
				LoadScene (index);
			} else {
				Debug.LogError (string.Format ("Scene {0} not found! Returning to menu.", index.ToString()));
				LoadScene ("Menu");
			}
	        
	    }

	    // ========================================================================================================================
	    // GetPersistantData

	    public PersistantData GetPersistantData() {
	        return persistantData;
	    }

	    // ========================================================================================================================

	}
}