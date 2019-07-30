using UnityEngine;
using System;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;
using DoomtrinityFPSPrototype.Utils;
using DoomtrinityFPSPrototype.PrefabPoolSystem;
using DoomtrinityFPSPrototype.dtGUI;
using DoomtrinityFPSPrototype.Audio;
using DoomtrinityFPSPrototype.Inventory;
using DoomtrinityFPSPrototype.Item;
using DoomtrinityFPSPrototype.Maps;
using DoomtrinityFPSPrototype.Weapons;
using DoomtrinityFPSPrototype.InputManagement;
using WeaponType = DoomtrinityFPSPrototype.Weapons.Weapon.WEAPONS;
using AmmoType = DoomtrinityFPSPrototype.Weapons.Ammo.AmmoTypes;

namespace DoomtrinityFPSPrototype.Character {

	// =====================================================
	// Messages

	// PlayerDeathMessage
	public class PlayerDeathMessage: BaseMessage {
		public PlayerDeathMessage() { }
	}

	// PlayerHealthChangeMessage
	public class PlayerHealthChangeMessage: BaseMessage {
		public readonly bool _damaged;
		public PlayerHealthChangeMessage(bool damaged) {
			_damaged = damaged;
		}
	}

	// PlayerPickupMessage
	public class PlayerPickupMessage: BaseMessage {
		public readonly string _itemName;
		public PlayerPickupMessage(string itemName) {
			_itemName = itemName;
		}
	}

	// =====================================================

	// The player script manages all the input that are not movement input,
	// this includes firing, reloading, change weapon etc.
	// Also, it handles all triggered events, e.g. when picking up ammo from the ground.
	// This script requires the first person controller script component.
	[RequireComponent(typeof(WeaponController))]
	[RequireComponent(typeof(FirstPersonController))]
	public class Player : DamageableEntity {

		// ========================================================================================================================
		// Instance variables
	       
	    public WeaponController weaponController { get; private set; }
		[SerializeField] private Camera deathCamera;

	    private bool isLevelEnd = false;
		private bool waitForSave = false;
		private WeaponType startWeapon = WeaponType.Pistol;
		private FirstPersonController fpController;
		private float health = 0;
		private bool godmode = false;

		// ========================================================================================================================
		// Properties

		public override float Health {
			get {
				return health;
			}
			protected set {
				float prev_health = health;
				health = value;
				MessagingSystem.Instance.QueueMessage (new PlayerHealthChangeMessage (prev_health > health));
			}
		}

		// ========================================================================================================================
		// Save and Restore

		public override DATA_TYPE Save(SaveLoadData.PersistantData savedata)
		{
			savedata.player_s.tr = Util.GetTransform (this);
			savedata.player_s.tr.Y += 0.1f; // prevent falling down through floor when restore.
	        savedata.player_s.health = Health;

			// Get current weapon.
			if (weaponController.currentWeapon != null) {
				savedata.player_s.currentWeapon = (int)weaponController.currentWeapon.GetWeaponType ();
			} else {
				savedata.player_s.currentWeapon = -1;
			}

			return DATA_TYPE.PLAYER;
		}

		public override void Restore(SaveLoadData.PersistantData savedata)
		{			
		}

		public void Restore(SaveStruct.Player_s player_s) {
			
			Vector3 playerPos = new Vector3 (player_s.tr.X, player_s.tr.Y, player_s.tr.Z);
			Quaternion playerRot = Quaternion.Euler(player_s.tr.RotX,player_s.tr.RotY,player_s.tr.RotZ);
			transform.position = playerPos;
			transform.rotation *= playerRot;

			Health = player_s.health;

			// The equiped weapon at the moment of save.
			if ( player_s.currentWeapon != -1) {
				startWeapon = (WeaponType)player_s.currentWeapon;
			}
	    }

		// ========================================================================================================================
		// Singleton stuff

		private static Player _instance;
		public static Player Instance {
			get {
				return _instance;
			}
		}

		private void Awake() {

			if (_instance != null && _instance != this) {
				Destroy(this.gameObject);            
			}
			else {
				_instance = this;

				// Disable the object so he won't start until the level is initialized.
				// Actually, this is need only to call 'Restore' before start ( 'Begin' ).
				gameObject.SetActive (false);
				// The best place to attach listener is start, but this is a special case.
				MessagingSystem.Instance.AttachListener (typeof(LevelInitializedMessage), this.ProcessLevelInitializedMessage);
			}
		}

		// ========================================================================================================================
		// ProcessLevelInitializedMessage

		private bool ProcessLevelInitializedMessage(BaseMessage msg) {
			gameObject.SetActive (true);
			DevConsole.AddCommand(new SpawnCommand("spawn",Spawn,string.Format("'{0}'\n{1}","Usage: spawn <entityname>","spawn the specified entity near the player")));
			DevConsole.AddCommand(new CommandParam("bind",BindButtonToCommand,string.Format("'{0}'\n{1}","Usage: bind <key> <command>","bind a button to a specific console command"),InputManager.GetAllowedKeys()));
			DevConsole.AddCommand(new Command("giveall",GiveAll,"Give all weapons, max ammo and max health"));
			DevConsole.AddCommand(new Command("killEmAll",KillEnemies,"Kill all enemies"));
			DevConsole.AddCvar (new Cvar<bool> ("god", GodMode, false, false, "Enable/Disable god mode"));
			return false;
		}

		// ========================================================================================================================
		// Start

		protected override void Begin() {
			weaponController = GetComponent<WeaponController>();
			fpController = GetComponent<FirstPersonController> ();

			base.Begin();

			weaponController.SelectWeapon (startWeapon);

			Time.timeScale = 1;
	        Cursor.lockState = CursorLockMode.Locked;
	        Cursor.visible = false;

			MessagingSystem.Instance.AttachListener (typeof(LevelEndMessage), this.ProcessLevelEndMessage);
	    }

		// ========================================================================================================================
		// Update
	    
		public override void Think (float dt) {

			EvaluateControls ();
	    }

		// ========================================================================================================================
		// EvaluateControls

		private void EvaluateControls() {

			if(!isLevelEnd) {
				if (!UpdateMaster.Instance.IsEscaping) {
					
					// Movement input is managed in first person controller script.
					// weapon input
					if (InputManager.GetKey (ActionCode.Attack)) {
						weaponController.OnTriggerHold ();

					} else if(InputManager.GetKeyUp (ActionCode.Attack)) {
						weaponController.OnTriggerRelease ();
					}

					if (InputManager.GetKeyDown (ActionCode.Reload)) {
						weaponController.Reload ();

					} else if (InputManager.GetKeyDown (ActionCode.SelectPistol)) {
						weaponController.SelectWeapon (Weapon.WEAPONS.Pistol);

					} else if (InputManager.GetKeyDown (ActionCode.SelectMachinegun)) {
						weaponController.SelectWeapon (Weapon.WEAPONS.Machinegun);

					} else if (InputManager.GetKeyDown (ActionCode.SelectShotgun)) {
						weaponController.SelectWeapon (Weapon.WEAPONS.SuperShotgun);

					}

					// InputManager.GetCommands() -- > foreach registered command check Input.GetKeyDown --> return string[] commands to be executed --> ExecuteCommand
					string[] commands = InputManager.GetCommands();
					if (commands != null && commands.Length > 0) {
						for (int i = 0; i < commands.Length; i++) {
							Debug.Log(commands[i]);
							DevConsole.ExecuteCommand (commands [i]);
						}
					}
					// Show Objective
					if (InputManager.GetKeyDown (ActionCode.ShowObjective)) {
						if (GameUI.Instance != null) {
							GameUI.Instance.OnObjective ();
						}
					}

					// Quick Save
					if (Input.GetKeyDown (KeyCode.F7) && !waitForSave) {
						waitForSave = true;
						StartCoroutine (WaitForNextSave ());
						if (GameController.Instance != null) {
							GameController.Instance.Save ();
						}
					}
				}
			}

		}

		// ========================================================================================================================
		// WaitForNextSave

		private IEnumerator WaitForNextSave() {
			yield return new WaitForSeconds(1);
			waitForSave = false;
		}

		// ========================================================================================================================
		// OnTriggerEnter

		private void OnTriggerEnter(Collider other) {
			ProcessPickup (other);
	    }

		// ========================================================================================================================
		// ProcessPickup

		// Manages what to do when pick up something from ground e.g. ammo.
		private void ProcessPickup(Collider other) {
			string pickupName = null;
			string other_tag = other.gameObject.tag;

			switch (other_tag) {
			case "ammo": 
				PickupAmmo (other, out pickupName);
				break;
			case "weapon": 
				PickupWeapon (other, out pickupName);
				break;
			case "health": 
				PickupHealth (other, out pickupName);
				break;
			}

			// Show pickup name on gui
			if (!String.IsNullOrEmpty (pickupName)) {
				MessagingSystem.Instance.QueueMessage (new PlayerPickupMessage (pickupName));
			}
		}

		// ========================================================================================================================
		// PickupAmmo

		private void PickupAmmo(Collider other, out string pickupName) {
			pickupName = null;
			Ammo ammo = other.gameObject.GetComponent<Ammo>();
			int ammoType = ammo.GetAmmoType();
			if (!PlayerInventory.Instance.HasMaxAmmo(ammoType)) {
				PlayerInventory.Instance.AddAmmo(ammoType, ammo.GetAmmoQuantity());
				pickupName = Ammo.GetAmmoName(ammoType);
				Destroy(other.gameObject);
			}
		}

		// ========================================================================================================================
		// PickupWeapon

		private void PickupWeapon(Collider other, out string pickupName) {
			pickupName = null;
			bool destroy = false;
			Weapon weapon = other.gameObject.GetComponent<WeaponPickup>().GetWeapon();

			int ammoType = weapon.GetAmmoType();
			if (!PlayerInventory.Instance.HasMaxAmmo(ammoType)) {
				PlayerInventory.Instance.AddAmmo(ammoType, weapon.GetAmmoGiveQty());
				pickupName = Ammo.GetAmmoName(ammoType);
				destroy = true;
			}

			Weapon.WEAPONS weaponType = weapon.GetWeaponType();
			if ( !PlayerInventory.Instance.IsWeaponEnabled(weaponType) ) {
				PlayerInventory.Instance.EnableWeapon(weaponType);
				pickupName = Weapon.GetWeaponName(weaponType);
				weaponController.SelectWeapon(weaponType);
				destroy = true;
			}

			if (destroy) { Destroy(other.gameObject); }
		}

		// ========================================================================================================================
		// PickupHealth

		private void PickupHealth(Collider other, out string pickupName) {
			pickupName = null;
			HealthPack healthPack = other.gameObject.GetComponent<HealthPack>();
			if (Health < spawnHealth) {
				GiveHealth(healthPack.GetHealthRestore());
				pickupName = "Health pack";
				Destroy(other.gameObject);
			}
		}

		// ========================================================================================================================
		// Damage

	    public override void Damage(float damage) {

			StartCoroutine (ViewCameraDamageShake ());
			if (!godmode) {
				base.Damage(damage);
			}
	    }

		// ========================================================================================================================
		// GodMode

		private void GodMode(bool _godmode){
			godmode = _godmode;
		}

		// ========================================================================================================================
		// Die

	    protected override void Die ()
		{
			MessagingSystem.Instance.QueueMessage (new PlayerDeathMessage ());
			AudioManager.Instance.PlaySound ("playerDeath", transform.position);
			Instantiate(deathCamera,Camera.main.transform.position,Camera.main.transform.rotation);
			base.Die ();
	        Cursor.lockState = CursorLockMode.None;
	        Cursor.visible = true;
	    }

		// ========================================================================================================================
		// ViewCameraDamageShake

		// See 'ViewCameraRecoilShake' in 'Weapon.cs' for more info on this coroutine.
		private IEnumerator ViewCameraDamageShake() {
			float hitCompletePercentage = 0;
			float hitSpeed = 5.0f;

			Vector3 startRotation;
			Vector3 endRotation;
			Vector3 rotVec = new Vector3 (16,0,0);
			Transform cameraTransform = Camera.main.transform;

			while(hitCompletePercentage <= 1) {
				if (fpController.rotateView) {
					startRotation = cameraTransform.localEulerAngles;
					endRotation = startRotation + rotVec;

					hitCompletePercentage += Time.deltaTime * hitSpeed;
					float animInterpolation = (-Mathf.Pow(hitCompletePercentage, 2) + hitCompletePercentage) * 4; // parabola --> y = 4(-x^2+x)

					cameraTransform.localEulerAngles = Vector3.Lerp(startRotation, endRotation, animInterpolation);
				}

				yield return null;	

			}
			yield return null;
		}

		// ========================================================================================================================
		// GiveHealth

	    public void GiveHealth(int _health) {
			float tmp_current_health = Health;
			tmp_current_health += _health;
			if (tmp_current_health > spawnHealth) {
				tmp_current_health = spawnHealth;
	        }
			Health = tmp_current_health; // Need to use 'tmp_current_health' to set 'Health' one time only - a message is fired in the property.
	    }

		// ========================================================================================================================
		// OnKill

		protected override void OnKill() {
			if (MessagingSystem.IsAlive) {
				MessagingSystem.Instance.DetachListener (typeof(LevelEndMessage), this.ProcessLevelEndMessage);
				MessagingSystem.Instance.DetachListener (typeof(LevelInitializedMessage), this.ProcessLevelInitializedMessage);
			}
		}

		// ========================================================================================================================
		// OnDestroy

		private void OnDestroy() {
			
			DevConsole.RemoveCommand("spawn");
			DevConsole.RemoveCommand("bind");
			DevConsole.RemoveCommand("giveall");
			DevConsole.RemoveCommand("killEmAll");
			DevConsole.RemoveCvar ("god");
		}

		// ========================================================================================================================
		// Spawn

		private void Spawn(string arg) {
			
			GameObject obj = null;
			string objName = SpawnCommand.getEntityToSpawn (arg);
			obj = Resources.Load(objName) as GameObject;
			Vector3 obj_offset = Vector3.zero;
			if (obj == null) {
				// parse if primitive
				foreach (PrimitiveType p in Enum.GetValues(typeof(PrimitiveType))) {
					if (p.ToString ().Equals (objName)) {
						obj = GameObject.CreatePrimitive (p);
						break;
					}
				}

			} else {
				obj = PrefabPoolingSystem.Spawn (obj);

			}
			if (obj != null) {
				Collider obj_collider = obj.GetComponent<Collider> ();
				if (obj_collider != null) {

					if (obj_collider is BoxCollider) {
						obj_offset.y += obj.GetComponent<BoxCollider> ().size.y / 2;

					} else if (obj_collider is CapsuleCollider) {
						obj_offset.y += obj.GetComponent<CapsuleCollider> ().height / 2;

					} else if (obj_collider is SphereCollider) {
						obj_offset.y += obj.GetComponent<SphereCollider> ().radius;
					}
				}
				obj_offset.y += 0.1f; // always give a small offset to detach obj from ground.
				// set pos and rot

				Vector3 ground_pos = gameObject.transform.position - new Vector3 (0.0f, GetComponent<CharacterController> ().height / 2, 0.0f);
				obj.transform.position = ground_pos + obj_offset;
				obj.transform.rotation = gameObject.transform.rotation;
				obj.transform.Translate (0.0f, 0.0f, 3.0f, gameObject.transform);

				if (obj.GetComponent<Enemy> () != null) { // enable path finder (due to game logic, enemies should have navmesh disabled by default)
					obj.GetComponent<Enemy> ().EnablePathFinder (true);
					// MessagingSystem.Instance.QueueMessage (new CheatMessage (Cheat.EnemySpawn)); // listen to this message type if you need to manage enemy spawn cheats gracefully
					// Debug.Log ("spawning enemy...");
				}

			} else {
				DevConsole.Log (String				.Format ("Object '{0}' not found", arg));
			}

		}

		// ========================================================================================================================
		// BindButtonToCommand

		private void BindButtonToCommand(string arg) {
			// words[0] --> button
			// words[1] --> command (e.g. spawn cube)
			if(!String.IsNullOrEmpty(arg)){
				string[] words = arg.Split(new char[] {' '}, 2);
				bool cmdRegistered = false;
				if(words.Length > 1){
					cmdRegistered = InputManager.RegisterCommand (words [0][0], words [1]);
				}
				if (!cmdRegistered) {
					DevConsole.LogInfo (String.Format("bind {0} key failed",words[0][0]));
				}
			}
			
		}

		// ========================================================================================================================
		// GiveAll

		public void GiveAll(){
			GiveHealth (999);
			GiveWeaponsAndAmmo ();
		}

		// ========================================================================================================================
		// GiveWeaponsAndAmmo

		public void GiveWeaponsAndAmmo(){
			foreach (WeaponType weaponType in Enum.GetValues(typeof(WeaponType))) {
				PlayerInventory.Instance.EnableWeapon (weaponType);
			}
			foreach (AmmoType ammoType in Enum.GetValues(typeof(AmmoType))) {
				PlayerInventory.Instance.AddAmmo ((int)ammoType,999);
			}

		}

		// ========================================================================================================================
		// KillEnemies

		private void KillEnemies() {
			EnemyBase[] enemies = FindObjectsOfType<EnemyBase> ();
			for (int i = 0; i < enemies.Length; i++) {
				Destroy(enemies[i].gameObject);
			}
		}

		// ========================================================================================================================
		// GetCurrentWeapon

		public int GetCurrentWeapon(){
			if (weaponController.currentWeapon != null) {
				return (int)(weaponController.currentWeapon.GetWeaponType());
			}
			return -1;
		}

		// ========================================================================================================================
		// ProcessLevelEndMessage

		private bool ProcessLevelEndMessage ( BaseMessage msg) {
			isLevelEnd = true;
			return false;
		}

		// ========================================================================================================================
	    
	}
}