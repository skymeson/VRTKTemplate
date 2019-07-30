using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using DoomtrinityFPSPrototype.Character;
using DoomtrinityFPSPrototype.Inventory;
using DoomtrinityFPSPrototype.Item;
using DoomtrinityFPSPrototype.Maps;
using DoomtrinityFPSPrototype.PrefabPoolSystem;
using ENEMY_TYPE = DoomtrinityFPSPrototype.Character.Enemy.EnemyType; 

namespace DoomtrinityFPSPrototype.Utils {

	// =====================================================
	// Messages

	// LevelInitializedMessage
	public class LevelInitializedMessage: BaseMessage {
		public LevelInitializedMessage() { }
	}
	// =====================================================

	// This class manages the things to do when a scene is loaded.
	// A scene could be loaded from scratch ( when we end the current level and go to next level, or 'new game' in main menu),
	// or from a save file.
	// This instance does not persist across scenes ( there's no DontDestroyOnLoad method ), 
	// so it is initialized ( Awake then Start ) at every scene start.
	public class LevelInitializer : MonoBehaviour {

	    // ========================================================================================================================
	    // Instance variables

		private GameController gameController;

		private SaveLoadData.PersistantData persistantData;
		private List<Enemy> spawnedEnemies = new List<Enemy> (); // The list of enemies that will be spawned.
		public List<Enemy> SpawnedEnemies { get { return spawnedEnemies; } }

		// ========================================================================================================================
	    // Singleton stuff

		private static LevelInitializer _instance;
	    public static LevelInitializer Instance {
	        get {
	            return _instance;
	        }
	    }

		private void Awake() {

	        if (_instance != null && _instance != this) {
	            Destroy(this.gameObject);            
	        }
	        else {
	            _instance = this;
	        }
	    }

	    // ========================================================================================================================
	    // Start

		private void Start() {
			
			if (Player.Instance != null) {
				
				// =======================================================================
				// Load data from save file, or do a new inizialization of the scene from scratch.

				if (SaveLoadData.Instance.isLoadingFromSave) {
					LoadLevel ();
				} else {
					InitializeLevel ();
				}

				// =======================================================================
				// Initialize game controller.
				gameController = FindObjectOfType<GameController> ();

				if (gameController != null) {
					gameController.Init ();
				}
				// =======================================================================

				MessagingSystem.Instance.QueueMessage(new LevelInitializedMessage());

			} else {
				Debug.LogError ("Player (FPSController2 prefab) missing in the scene!");
			}
	        
	    }

		// ========================================================================================================================
	    // InitializeLevel

	    // Do all the required things as it's starting a new scene from scratch.
		private void InitializeLevel()
	    {
			// Firing range ( first scene ) is a special scene, it doesn't need specific initialization.
			if (SceneManager.GetActiveScene ().name != "FiringRange") {
				
				SpawnEnemies();
			}
	    }

	    // ========================================================================================================================
	    // SpawnEnemies

		private void SpawnEnemies()
	    {
	        EnemySpawn[] enemiesSpawn = FindObjectsOfType<EnemySpawn>() as EnemySpawn[];
	        foreach (EnemySpawn enemySpawn in enemiesSpawn)
	        {
				if (enemySpawn.spawnOnStart) { // Actually the game is structured to spawn all enemies on start
					Enemy enemy;
					string enemyPrefabName = enemySpawn.enemyType.ToString ();
					// Spawn the enemy. This is done for each enemySpawn gameobject found in the scene.
					// Enemies should never be placed in the scene directly, but they should always rely on enemySpawn gameobject.

					GameObject enemyObj = Resources.Load("Enemy/" + enemyPrefabName ) as GameObject;
					if(enemyObj == null) {
						Debug.LogError (string.Format ("Prefab {0} not found in Resources/Enemy/ !", enemyPrefabName));
						continue;
					}
					enemy = PrefabPoolingSystem.Spawn(enemyObj, enemySpawn.transform.position, enemySpawn.transform.rotation).GetComponent<Enemy>();

					if (enemySpawn.overrideSpawnArgs) {
						enemy.SetSpawnArgs (enemySpawn.spawnArgs);
					}

					spawnedEnemies.Add (enemy);
				}
	        }

	    }

		// ========================================================================================================================
		// LoadLevel

		// Load the game from a save file. Actually load the following things,
		// Player - transform, health, current weapon, inventory
		// Enemies - transform, health, spawn args
		// Stats - enemies killed
		private void LoadLevel() {

			// This only load data from xml file to 'persistantData' variable in 'SaveLoadData' script,
			// but things are not restored yet. Restore is done later.
	        
			if (SaveLoadData.Instance.LoadGame ()) {
				RestoreData ();
			} else {
				Debug.LogError ("LevelInitializer.LoadLevel(): Error while loading from file, starting new level.");
				InitializeLevel(); // initialize level as a new game if can't load.
			}

	    }

	    // ========================================================================================================================
	    // RestoreData

		private void RestoreData() {
	        
			persistantData = SaveLoadData.Instance.GetPersistantData();

			RestoreInventory();
	        RestoreEnemies();
			RestorePickups ();
			RestorePlayer();
	    }

		// ========================================================================================================================
		// RestoreInventory

		private void RestoreInventory() {
			if (PlayerInventory.Instance != null) {
				PlayerInventory.Instance.Restore (persistantData.inventory_s);
			}
		}

		// ========================================================================================================================
		// RestorePlayer

		private void RestorePlayer() {
			if (Player.Instance != null) {
				Player.Instance.Restore (persistantData.player_s);
			}        

		}

		// ========================================================================================================================
		// RestoreEnemies

		private void RestoreEnemies() {

			foreach(SaveStruct.Enemy_s enemy_s in persistantData.enemies_s)
			{
				ENEMY_TYPE enemy_type = (ENEMY_TYPE)enemy_s.enemyType;
				// Pay attention here! Prefab must be in 'Resources/Enemy' and match the name in ENEMY_TYPE enum!
				string enemyPrefabName = enemy_type.ToString();

				GameObject enemyObj = Resources.Load("Enemy/" + enemyPrefabName ) as GameObject;
				if(enemyObj == null) {
					Debug.LogError (string.Format ("Prefab {0} not found in Resources/Enemy/ !", enemyPrefabName));
					continue;
				}

				Enemy enemy = PrefabPoolingSystem.Spawn(enemyObj).GetComponent<Enemy>();
				enemy.Restore (enemy_s);
				SpawnedEnemies.Add (enemy);
			}
			// Restore the counter of all enemies in the scene at first launch of the level.
			EnemyBase.RestoreEnemiesCount ();

		}

		// ========================================================================================================================
		// RestorePickups

		private void RestorePickups() {
			Pickup[] pickups = FindObjectsOfType<Pickup> ();
			foreach (Pickup pickup in pickups) 
			{	// Remove any pickup object from the scene if not present in save file.
				// This relies on objects names, so you must ensure that every pickup object in the scene
				// has a unique name. This works for small scenes, and should not be used in complex scenes.
				if(!persistantData.pickups.Contains(pickup.name)) 
				{
					Destroy(pickup.gameObject); 
				}
			}
		}

		// ========================================================================================================================
	}
}