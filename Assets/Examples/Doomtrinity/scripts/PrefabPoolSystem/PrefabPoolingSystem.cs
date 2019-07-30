using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DoomtrinityFPSPrototype.PrefabPoolSystem {
// Main entry point static class, global access.
// This prefab pool system can be used to handle multiple prefab pools automatically.
public static class PrefabPoolingSystem {

	// Use these dictionaries to acquire the corresponding PrefabPool objects. 
	// Spawning requires a Prefab. _prefabToPoolMap map Prefabs to the PrefabPool that manages them.
	// Despawning requires a GameObject. _gameObjToPoolMap maps spawned GameObjects to the PrefabPool that spawned them.
	static Dictionary<GameObject,PrefabPool> _prefabToPoolMap = new Dictionary<GameObject,PrefabPool>();
	static Dictionary<GameObject,PrefabPool> _gameObjToPoolMap = new Dictionary<GameObject,PrefabPool>();

	// ========================================================================================================================
	// Spawn

	public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation) {
		if (!_prefabToPoolMap.ContainsKey (prefab)) { // Figure out which PrefabPool the Prefab belongs to, if any
			_prefabToPoolMap.Add (prefab, new PrefabPool()); // If not, create one
		}
		// If there is a despawned object, spawn it or instantiate a new one 
		// (if no inactive instances left).
		PrefabPool pool = _prefabToPoolMap[prefab];
		GameObject gameObj = pool.Spawn(prefab, position, rotation); // Spawn a new object
		_gameObjToPoolMap.Add (gameObj, pool);
		return gameObj;
	}

	// Overload which places the object at 0 0 0, 
	// useful for invisible GameObjects that just need to exist, no matter where.
	public static GameObject Spawn(GameObject prefab)
	{
		return Spawn(prefab, Vector3.zero, Quaternion.identity);
	}

	// ========================================================================================================================
	// Despawn

	// Given a GameObject, figure out which PrefabPool is managing it. 
	// Returns a bool that can be used to verify whether or not the object was successfully despawned.
	public static bool Despawn(GameObject obj) {
		if (!_gameObjToPoolMap.ContainsKey(obj)) {
			Debug.LogError (string.Format ("Object {0} not managed by pool system!", obj.name));
			return false;
		}

		PrefabPool pool = _gameObjToPoolMap[obj];
		if (pool.Despawn (obj)) {
			_gameObjToPoolMap.Remove (obj);
			return true;
		}
		return false;
	}

	// ========================================================================================================================
	// Prespawn

	// Prespawn a given number of objects from a Prefab.
	// Use this method during Scene initialization, 
	// to prespawn a collection of objects to use in the level.
	public static List<GameObject> Prespawn(GameObject prefab, int numOfIstancesToSpawn) {

		List<GameObject> spawnedObjects = new List<GameObject>();

		for(int i = 0; i < numOfIstancesToSpawn; i++) {
			spawnedObjects.Add (Spawn (prefab));
		}

		for(int i = 0; i < numOfIstancesToSpawn; i++) {
			Despawn(spawnedObjects[i]);
		}

		// spawnedObjects.Clear ();
		return spawnedObjects;
	}

	// ========================================================================================================================
	// Reset

	// This class will persist at scene change since it is a static class.
	// Dictionaries will be full of null references in next scene since Unity destroys objects in previous scene, 
	// unless we use DontDestroyOnLoad.
	// We need a method that resets the pooling system for this event. 
	// This method should be called before a new Scene is loaded.
	public static void Reset() {
		_prefabToPoolMap.Clear ();
		_gameObjToPoolMap.Clear ();
	}

	// ========================================================================================================================
	// RollCall

	/* 
	public static void RollCall() {
		foreach(KeyValuePair<GameObject, PrefabPool> kvp in _prefabToPoolMap) {
			Debug.Log ("" + kvp.Key + ", " + kvp.Value);
			kvp.Value.RollCall();
		}
	} */

	// ========================================================================================================================
}
}