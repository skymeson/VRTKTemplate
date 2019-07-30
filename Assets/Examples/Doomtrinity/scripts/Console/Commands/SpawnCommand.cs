using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace DoomtrinityFPSPrototype.Utils {
	public class SpawnCommand : CommandParam {

		private static Dictionary<string,string> spawnableEntities = new Dictionary<string,string>(){
			{"monster_alien","Enemy/Alien"},
			{"monster_capsule","Enemy/CapsuleEnemy"},
			{"weapon_pistol","Weapon/Pickup/PistolPickup"},
			{"weapon_machinegun","Weapon/Pickup/MachinegunPickup"},
			{"weapon_shotgun","Weapon/Pickup/SuperShotgunPickup"},
			{"ammo_pistol","Weapon/Pickup/ammoClipPistol"},
			{"ammo_machinegun","Weapon/Pickup/ammoClipMachinegun"},
			{"ammo_shotgun","Weapon/Pickup/ammoShellShotgun"},
			{"item_healthpack","Items/HealthPack2"},
			{"primitive_cube","Cube"},
			{"primitive_capsule","Capsule"}
		};

		// ========================================================================================================================
		// Constructors

		public SpawnCommand (string name, ConsoleMethod method, string helpText):base(name, method, helpText){
			base.ParamList = spawnableEntities.Keys.ToArray ();
		}

		// ========================================================================================================================
		// getEntityToSpawn
		public static string getEntityToSpawn(string alias) {
			string entity = null;
			if(alias != null && spawnableEntities.ContainsKey(alias)){
				entity = spawnableEntities[alias];
			}
			return entity;
		}

		// ========================================================================================================================
	}
}