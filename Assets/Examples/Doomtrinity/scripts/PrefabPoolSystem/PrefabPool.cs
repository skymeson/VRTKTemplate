using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DoomtrinityFPSPrototype.PrefabPoolSystem {
// This struct is used to cache a reference to the GameObject, and the precached list of its
// IPoolableComponents. This interface should be implemented by at least one of the Components that 
// is attached to the GameObject to pool.
// Anyway, IPoolableComponent system is actually unused.
public struct PoolablePrefabData {
	public GameObject gameObj;
	public IPoolableComponent[] poolableComponents;
}

// This class keeps track  a list of active (spawned) GameObjects, 
// and a list of inactive (despawned) objects that were instantiated from it.
public class PrefabPool {

    Dictionary<GameObject,PoolablePrefabData> _activeList = new Dictionary<GameObject,PoolablePrefabData>();

    // Queue data structure, just need to get an object, 
    // no matter which in particular.
    Queue<PoolablePrefabData> _inactiveList = new Queue<PoolablePrefabData>();

    // ========================================================================================================================
    // Spawn

    // Check if we have any inactive instances of the Prefab. If true, pop the next available one from 
	// the Queue and respawn it. If false, instantiate a new GameObject from the Prefab.
    // Add the object to the active list and return it.
    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation) {
		PoolablePrefabData data;
		if (_inactiveList.Count > 0) {
			 data = _inactiveList.Dequeue();
		} else {
			// instantiate a new object
			GameObject newGameObj = GameObject.Instantiate(prefab, position, rotation) as GameObject;
			data = new PoolablePrefabData();
			data.gameObj = newGameObj;
			data.poolableComponents = newGameObj.GetComponents<IPoolableComponent>();
		}

		data.gameObj.SetActive (true);
		data.gameObj.transform.position = position;
		data.gameObj.transform.rotation = rotation;
		for(int i = 0; i < data.poolableComponents.Length; ++i) {
			data.poolableComponents[i].Spawned ();
		}

		_activeList.Add (data.gameObj, data);

		return data.gameObj;
	}

    // ========================================================================================================================
    // Despawn

    // Remove the object from the active list, and push it into the inactive queue.
    public bool Despawn(GameObject objToDespawn) {
		if (!_activeList.ContainsKey(objToDespawn)) {
			Debug.LogError ("This Object is not managed by this object pool!");
			return false;
		}

		PoolablePrefabData data = _activeList[objToDespawn];

		for(int i = 0; i < data.poolableComponents.Length; ++i) {
			data.poolableComponents[i].Despawned ();
		}
		data.gameObj.SetActive (false);

		_activeList.Remove (objToDespawn);
		_inactiveList.Enqueue(data);
		return true;
	}

	// ========================================================================================================================
	// RollCall

    /*
	public void RollCall() {
		Debug.Log ("\tInactiveList");
		foreach(PoolablePrefabData data in _inactiveList) {
			Debug.Log ("\t\t" + data.gameObj);
		}
		Debug.Log ("\tActiveList");
		foreach(PoolablePrefabData data in _activeList.Values) {
			Debug.Log ("\t\t" + data.gameObj);
		}
	} */
	// ========================================================================================================================
}
}