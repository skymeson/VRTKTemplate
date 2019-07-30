using UnityEngine;
using System.Collections;

// All structs used to save data fer each specific type. Settings structs are defined in 'SaveLoadSettings' script.
namespace DoomtrinityFPSPrototype.Utils {
	public class SaveStruct {

		// ========================================================================================================================
		// nested struct Enemy_s

		// Enemy - transform, health, type, spawn args
		[System.Serializable]
		public struct Enemy_s
		{
			public Util.PosRot_s tr;
			public float health;
			public int enemyType;
			public float attackDistanceThreshold;
			public float attackRate;
			public float damageToTarget;
			public float moveSpeed;
			public bool pathFinderEnabled;
		}

		// ========================================================================================================================
		// nested struct Player_s

		// Player - transform, health, current weapon
		[System.Serializable]
		public struct Player_s
		{
			public Util.PosRot_s tr;
			public float health;
			public int currentWeapon;
		}

		// ========================================================================================================================
		// nested struct Inventory_s

		// Player Inventory - ammo, weapons. See 'PlayerInventory' script for more info.
		[System.Serializable]
		public struct Inventory_s
		{
			public int[] weapons; // all weapons  must be set in playerInventory at game start.
			public bool[] isWeaponEnabled;
			public int[] ammoType;
			public int[] ammoQty;
		}

		// ========================================================================================================================
		// nested struct Stats_s

		// Stats - number of all enemies at level start, killed enemies
		[System.Serializable]
		public struct Stats_s
		{
			public int startEnemiesCount;
			public int killedEnemiesCount;
		}

		// ========================================================================================================================
	}
}