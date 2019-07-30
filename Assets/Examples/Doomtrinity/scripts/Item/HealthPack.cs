using UnityEngine;
using System.Collections;
using DoomtrinityFPSPrototype.Utils;

namespace DoomtrinityFPSPrototype.Item {
	public class HealthPack : Pickup {

		// ========================================================================================================================
		//Instance variables

		[SerializeField] private int healthRestore;

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
		// GetHealthRestore

	    public int GetHealthRestore() {
	        return healthRestore;
	    }

		// ========================================================================================================================
	}
}