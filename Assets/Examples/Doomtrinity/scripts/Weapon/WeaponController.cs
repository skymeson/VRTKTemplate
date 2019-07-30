using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DoomtrinityFPSPrototype.Inventory;
using DoomtrinityFPSPrototype.PrefabPoolSystem;

namespace DoomtrinityFPSPrototype.Weapons {
	// This class basically acts as 'intermediary' between player and weapon.
	// Listen the player, then tell to weapon what to do.
	public class WeaponController : MonoBehaviour {

		[System.Serializable]
		public struct WeaponAttacher
		{
			public Weapon.WEAPONS attachID;
			public Transform attachTransform;
		}

		// ========================================================================================================================
		// Instance variables

	    public Weapon currentWeapon { get; private set; }
		private Transform weaponPos;
		[SerializeField] private WeaponAttacher[] weaponAttacher;

		private Dictionary<Weapon.WEAPONS,Transform> attacherDict = new Dictionary<Weapon.WEAPONS, Transform> ();

		// ========================================================================================================================
		// Awake

		private void Awake() {
			foreach(WeaponAttacher wpnAttacher in weaponAttacher) {
				attacherDict.Add (wpnAttacher.attachID, wpnAttacher.attachTransform);
			}
		}

	    
		// ========================================================================================================================
		// SelectWeapon

	    public void SelectWeapon(Weapon _weapon) {
	        if (currentWeapon != null) {            
	            PrefabPoolingSystem.Despawn(currentWeapon.gameObject);
	        }        
	        currentWeapon = PrefabPoolingSystem.Spawn(_weapon.gameObject, weaponPos.position, weaponPos.rotation).GetComponent<Weapon>();
	        currentWeapon.Init();
	        
	        // set weapon origin
	        currentWeapon.transform.parent = weaponPos;
	    }

		public void SelectWeapon(Weapon.WEAPONS weaponType) {
			
			string wpnName = weaponType.ToString ();
			Weapon wpnObj = Resources.Load("Weapon/" + wpnName,typeof(Weapon)) as Weapon;

			if (wpnObj == null) {
				Debug.LogError (string.Format ("Weapon {0} not found in Resources/Weapon/ !", wpnName));
				return;
			}
			if (PlayerInventory.Instance.SelectWeapon(weaponType)) { // if weapon is enabled
				if((currentWeapon != null && currentWeapon.GetWeaponType() != weaponType) || currentWeapon == null ) 
				{
					weaponPos = attacherDict[weaponType];
					SelectWeapon(wpnObj);
				}
			}
		}

		// ========================================================================================================================
		// OnTriggerHold

	    public void OnTriggerHold() {
	        if (currentWeapon != null) {
	            currentWeapon.OnTriggerHold();
	        }
	    }

		// ========================================================================================================================
		// OnTriggerRelease

	    public void OnTriggerRelease() {
	        if (currentWeapon != null) {
	            currentWeapon.OnTriggerRelease();
	        }
	    }

		// ========================================================================================================================
		// Reload

		public void Reload(){
			if (currentWeapon != null) {
				currentWeapon.Reload();
			}
		}

		// ========================================================================================================================
	}
}