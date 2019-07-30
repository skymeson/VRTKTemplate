using UnityEngine;
using System.Collections;

namespace DoomtrinityFPSPrototype.Character {
// Attach this script to empty gameobject in the scene, then choose enemy prefab defined in enum.
// Level initializer script spawns enemy from this gameobject.
public class EnemySpawn : MonoBehaviour {

	// ========================================================================================================================
	// Instance variables

	public Enemy.EnemyType enemyType;
	public bool overrideSpawnArgs = false; // use spawn args defined here instead of default ones in 'Enemy'.
	public Enemy.SpawnArgs spawnArgs;
	public bool spawnOnStart = true;

	// ========================================================================================================================
}
}