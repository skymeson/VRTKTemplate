using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using DoomtrinityFPSPrototype.Utils;
using DoomtrinityFPSPrototype.Weapons;

namespace DoomtrinityFPSPrototype.Inventory {

	public class AmmoTakeMessage : BaseMessage {
		public AmmoTakeMessage() { }
	}

	// ============================================================================================================================
	// class PlayerInventory

	// The inventory holds the player's weapons and ammo stuff.
	// Here we know the total ammo of each type, all the weapons in the game and the weapons we can use.
	// This class uses the Singleton design pattern, but the instance does not persist across scenes ( no DontDestroyOnLoad() ).
	// This means that, when the next level is loaded, the ammo and weapons data is retrieved from external xml file,
	// if game has been saved.
	// Otherwise, default values defined in Inspector will be loaded.
	public class PlayerInventory : PersistantMono {

		// ========================================================================================================================
	    // Instance variables

	    // =======================================================================
	    // weapons
	    
	    // Set in inspector all weapons of the game. See nested class definition for more info.
		[SerializeField] private PlayerWeapons[] playerWeapons;

	    // This dict holds all weapons. State is enabled or not enabled. A weapon could be enabled in the Inspector,
	    // or if it is grabbed somewhere in the level. Player can only use enabled weapons.
		public Dictionary<Weapon.WEAPONS, bool> weapons { get; private set; }

	    // =======================================================================
	    // ammo

	    // Set in the Inspector the ammo type and the start quantity for this ammo.
		[SerializeField] private StartAmmo[] startAmmo;

	    // This dict holds all ammo and relative quantity.
	    public Dictionary<int, int> ammoQuantityForTypes { get; private set; }

	    // This dict holds the max ammo quantity allowed for each ammo type.
	    public Dictionary<int, int> maxAmmoForTypes { get; private set; }

		// ========================================================================================================================
		// SaveInit and Restore

		public override DATA_TYPE Save(SaveLoadData.PersistantData savedata)
		{
			
			// Enabled weapons in the inventory. All weapons are loaded through 'PlayerInventory' class at game start,
			// but this doesn't mean that player can use all weapons.

			// Initialize the weapon's array if this is the first time we get here.
			if(savedata.inventory_s.weapons == null || savedata.inventory_s.weapons.Length == 0) 
			{
				int weaponsCount = weapons.Count;
				// Initialize the array of 'weapon index' and 'enabled?' with the number of weapons in player inventory.
				savedata.inventory_s.weapons = new int[weaponsCount];
				savedata.inventory_s.isWeaponEnabled = new bool[weaponsCount];
			}

			int i = 0;
			// Cycle through all weapons to get the name and enabled state, then store them in 'persistantData' variable.
			foreach (KeyValuePair<Weapon.WEAPONS,bool> weapon in weapons)
			{
				savedata.inventory_s.weapons[i] = (int)weapon.Key;
				savedata.inventory_s.isWeaponEnabled[i] = weapon.Value;
				i++;
			}

			// All ammo and respective quantity in the inventory.

			// Initialize the ammo array if this is the first time we get here.
			if(savedata.inventory_s.ammoType == null || savedata.inventory_s.ammoType.Length == 0) 
			{
				int ammoCount = ammoQuantityForTypes.Count;
				// Initialize the array of 'ammo type' and 'ammo qty' with the types of ammo in player inventory.
				savedata.inventory_s.ammoType = new int[ammoCount];
				savedata.inventory_s.ammoQty = new int[ammoCount];
			}

			i = 0;
			// Cycle through all ammo to get the type and quantity, then store them in 'persistantData' variable.
			foreach (KeyValuePair<int, int> ammoType in ammoQuantityForTypes) 
			{
				savedata.inventory_s.ammoType[i] = ammoType.Key;
				savedata.inventory_s.ammoQty[i] = ammoType.Value;
				i++;
			}

			return DATA_TYPE.INVENTORY;
		}

		public override void Restore(SaveLoadData.PersistantData savedata)
		{			
		}

		public void Restore(SaveStruct.Inventory_s inventory_s)
		{
			RestoreAmmo (inventory_s.ammoType, inventory_s.ammoQty);
			RestoreWeapons (inventory_s.weapons, inventory_s.isWeaponEnabled);
		}

	    // ========================================================================================================================
	    // Singleton stuff

		private static PlayerInventory _instance;
	    public static PlayerInventory Instance {
	        get {
	            return _instance;
	        }
	    }

		private void Awake() {

	        if (_instance != null && _instance != this) {
	            Destroy(this.gameObject);
	        } else {
	            _instance = this;
	            // Data is saved in external xml file, so we should not use 'DontDestroyOnLoad(this.gameObject)' here.
				// Also, the game is structured in the way that inventory is loaded from scratch at each new scene start, 
				// since each scene ( except 'Menu' ) must have its independent 'PlayerInventory'

				// Initialize weapon and ammo dict.
				weapons = new Dictionary<Weapon.WEAPONS,bool>();

				ammoQuantityForTypes = new Dictionary<int, int>();
				maxAmmoForTypes = new Dictionary<int, int>();

				// Add all ammo with relative quantity in the inventory.
				LoadAmmoTypesInInventory();

				// Add in inventory all weapons dragged in the Inspector.
				AddStartWeaponsInInventory();
	       }	        
	    }

	    // ========================================================================================================================
	    // AddStartWeaponsInInventory

		private void AddStartWeaponsInInventory() {
	        foreach (PlayerWeapons playerWeapon  in playerWeapons) {

	            weapons.Add (playerWeapon.weaponType, playerWeapon.enabled);
	        }
	    }

	    // ========================================================================================================================
		// EnableWeapon

		public void EnableWeapon(Weapon.WEAPONS weaponType) {
			
			if (weapons.ContainsKey(weaponType)) 
			{
				weapons[weaponType] = true;
			}

		}

		// ========================================================================================================================
		// DisableWeapon

		public void DisableWeapon(Weapon.WEAPONS weaponType) {

			if (weapons.ContainsKey(weaponType)) 
			{
				weapons[weaponType] = false;
			}

		}

		// ========================================================================================================================
	    // SelectWeapon

		public bool SelectWeapon(Weapon.WEAPONS weaponType) {
			if(weapons.ContainsKey(weaponType) ){
				return weapons [weaponType];
			}
			return false;
		}

	    // ========================================================================================================================
		// IsWeaponEnabled

		public bool IsWeaponEnabled(Weapon.WEAPONS weaponType) {
			if(weapons.ContainsKey(weaponType) ){
				return weapons[weaponType];
			}
			return false;
		}

	    // ========================================================================================================================
	    // LoadAmmoTypesInInventory

		private void LoadAmmoTypesInInventory() {
	        foreach (int ammoType in Enum.GetValues(typeof(Ammo.AmmoTypes))) {
	            ammoQuantityForTypes.Add(ammoType, 0);
	        }
	        AddStartAmmoInInventory();
	    }

	    // ========================================================================================================================
	    // AddStartAmmoInInventory

		private void AddStartAmmoInInventory() {
	        foreach (StartAmmo ammo in startAmmo) {
	            if (ammoQuantityForTypes.ContainsKey((int)ammo.ammoType))
	            { 
	                int ammoQty = ammo.startAmmo > ammo.maxAmmo ? ammo.maxAmmo : ammo.startAmmo; // Need to add some range and clamp stuff...
	                ammoQuantityForTypes[(int)ammo.ammoType] = ammoQty;
	                maxAmmoForTypes[(int)ammo.ammoType] = ammo.maxAmmo;
	            }
	        }
	    }

	    // ========================================================================================================================
	    // AddAmmo

	    // Call this when we pick up ammo from the ground.
	    public void AddAmmo(int ammoType, int ammoQuantity) {
	        if (ammoQuantityForTypes.ContainsKey(ammoType)) {
	            ammoQuantityForTypes[ammoType] += ammoQuantity;

	            if (ammoQuantityForTypes[ammoType] > maxAmmoForTypes[ammoType]) {
	                ammoQuantityForTypes[ammoType] = maxAmmoForTypes[ammoType];
	            }
	            
	            MessagingSystem.Instance.QueueMessage (new AmmoTakeMessage ());
	        }
	    }

	    // ========================================================================================================================
	    // GetAmmo

	    // Get the quantity for a specific ammo type.
	    public int GetAmmo(int ammoType) {
	        if (ammoQuantityForTypes.ContainsKey(ammoType)) {
	            return ammoQuantityForTypes[ammoType];
	        }
	        return 0;
	    }

	    // ========================================================================================================================
	    // UseAmmo

	    // Subtract 1 round from the total quantity of the specified ammo type.
	    public void UseAmmo(int ammoType) {
	        if (ammoQuantityForTypes.ContainsKey(ammoType)) {
	            ammoQuantityForTypes[ammoType] -= 1;
	        }
	    }

	    // ========================================================================================================================
	    // HasMaxAmmo

	    public bool HasMaxAmmo(int ammoType) {
	        if (ammoQuantityForTypes.ContainsKey(ammoType)) {
	            return ammoQuantityForTypes[ammoType] < maxAmmoForTypes[ammoType] ? false : true;
	        }
	        return false;
	    }

	    // ========================================================================================================================
		// RestoreAmmo

	    // Call this when loading from external xml file, to retrieve the ammo data.
	    public void RestoreAmmo(int[] type, int[] qty) {
			if (ammoQuantityForTypes.Count == type.Length) {
				for (int i = 0; i < type.Length; i++)
				{
					ammoQuantityForTypes[type[i]] = qty[i];
				}
			}

		}

	    // ========================================================================================================================
		// RestoreWeapons

	    // Call this when loading from external xml file, to retrieve the weapon state ( enabled or not ebabled).
	    public void RestoreWeapons( int[] weaponType, bool[] enabled) {
			if(weapons.Count == weaponType.Length) {
				for(int i=0; i < weaponType.Length; i++){
					weapons[(Weapon.WEAPONS)weaponType[i]] = enabled[i];
				}
			}

		}

	    // ========================================================================================================================
	    // nested class PlayerWeapons

		// This class let us to set up all the weapons from the Inspector. Set flag to 'enabled' if this weapon must be in the
	    // inventory by default.
	    [System.Serializable]
	    public class PlayerWeapons 
		{
			public bool enabled;
			public Weapon.WEAPONS weaponType;
	        
	    }

	    // ========================================================================================================================
	    // nested class StartAmmo

	    // This class let us to set up all the ammo from the Inspector. 
	    // startAmmo is the quantity that will be stored in inventory by default.
	    // Ammo Types are defined in 'Ammo' script.
	    // Max ammo is the max quantity allowed for this ammo.
	    [System.Serializable]
	    public class StartAmmo {
	        public Ammo.AmmoTypes ammoType;
	        public int startAmmo;
	        public int maxAmmo;
	    }
		// ========================================================================================================================

	}
}
