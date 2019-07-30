using UnityEngine;
using System.Collections;
using DoomtrinityFPSPrototype.Item;
using DoomtrinityFPSPrototype.Utils;

namespace DoomtrinityFPSPrototype.Weapons {
	public class Ammo : Pickup {

	    public enum AmmoTypes
	    {
	        ammoPistol,
	        ammoMachinegun,
	        ammoSuperShotgun
	    }

		[SerializeField] private AmmoTypes ammoType;
		[SerializeField] private int ammoQuantity;

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
		// GetAmmoType

	    public int GetAmmoType() {
	        return (int)ammoType;
	    }

		// ========================================================================================================================
		// GetAmmoQuantity

	    public int GetAmmoQuantity() {
	        return ammoQuantity;
	    }

		// ========================================================================================================================
		// GetAmmoName

		public static string GetAmmoName(int ammo) {
			AmmoTypes ammoType = (AmmoTypes)ammo;
			string name = "";
			switch (ammoType) {

			case AmmoTypes.ammoPistol :
				name = "Pistol ammo";
				break;
			case AmmoTypes.ammoMachinegun :
				name = "MG ammo";
				break;
			case AmmoTypes.ammoSuperShotgun :
				name = "Shotgun ammo";
				break;
			default :
				name = "Ammo";
				break;
			}
			return name;
		}

		// ========================================================================================================================
	}
}