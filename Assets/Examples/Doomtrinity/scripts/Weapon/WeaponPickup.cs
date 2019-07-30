using UnityEngine;
using System.Collections;
using DoomtrinityFPSPrototype.Item;
using DoomtrinityFPSPrototype.Utils;

namespace DoomtrinityFPSPrototype.Weapons {
	public class WeaponPickup : Pickup {

		// ========================================================================================================================
		// Instance variables

		[SerializeField] private Weapon weapon;

		// ========================================================================================================================
		// Save and Restore

		public override DATA_TYPE Save(SaveLoadData.PersistantData savedata)
		{
			savedata.pickups.Add (name);
			return DATA_TYPE.PICKUP;
		}

		public override void Restore(SaveLoadData.PersistantData savedata)
		{
		}

		// ========================================================================================================================
		// GetWeapon

	    public Weapon GetWeapon() {
	        return weapon;
	    }

		// ========================================================================================================================
	}
}