using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;
using DoomtrinityFPSPrototype.Character;
using DoomtrinityFPSPrototype.Audio;
using DoomtrinityFPSPrototype.Inventory;
using DoomtrinityFPSPrototype.PrefabPoolSystem;
using DoomtrinityFPSPrototype.Utils;

namespace DoomtrinityFPSPrototype.Weapons {

	public class AmmoQtyChangeMessage : BaseMessage {
		public readonly bool _hasInfiniteAmmo;
		public readonly int _ammoInClip;
		public readonly int _ammoInInventory;
		public readonly int _weaponType;
		public AmmoQtyChangeMessage(bool hasInfiniteAmmo, int ammoInClip, int ammoInInventory, int weaponType) {
			_hasInfiniteAmmo 	= hasInfiniteAmmo;
			_ammoInClip 		= ammoInClip;
			_ammoInInventory 	= ammoInInventory;
			_weaponType 		= weaponType;
		}
	}

	// Class Weapon holds all weapons specs and behaviours.
	[RequireComponent(typeof (Muzzleflash))]
	[RequireComponent(typeof (AudioSource))]
	public class Weapon : MonoBehaviour {
	    
		// WEAPONS enum holds all weapons identifier.
		// Add at bottom of the list any new weapon.
		// IMPORTANT! Weapon name must match the prefab name of the weapon in 'Resources/Weapon'!
		// Other scripts rely on this name for things like weapon to take from the ground,
		// or inventory management.
	    public enum WEAPONS
	    {
	        Pistol,
	        Machinegun,
	        SuperShotgun
	    }

		// ========================================================================================================================
		// Instance variables

		[SerializeField] WEAPONS weaponType;

		// fire stuff
		private enum FireMode { Single, Auto };
		private float nextFire;
		private bool triggerReleased = true;
		private bool rotateView = true;
		[Header("Fire")]
		[SerializeField] private FireMode fireMode;
		[SerializeField] private float fireRate;
		[SerializeField] private float spread;
		[SerializeField] private bool launchFromBarrel = false;
		private Transform player_camera_transform;

	    // projectile stuff. Damage of projectile defined in projectile prefab.
		[Header("Projectile")]
		[SerializeField] private Transform barrel;
		[SerializeField] private Projectile projectile;
		[SerializeField] private float prjFuse;
		[SerializeField] private float muzzleVelocity;
		[SerializeField] private int buckshot;

	    // ammo and reload stuff
		[Header("Ammo")]
		[SerializeField] private Ammo.AmmoTypes ammoType;
		[SerializeField] private int ammoGiveQty;
	    [SerializeField] private bool infiniteAmmo;
		[SerializeField] private int clipSize;
		[SerializeField] private float reloadTime;
		private bool isReloading;
		private int ammoAvailable;
		public int ammoInInventory {get; private set;}
		public int ammoInClip {get; private set;}

	    [Header("Effects")]
		[SerializeField] private Transform shell;
		[SerializeField] private Transform shellEjectOrigin;
		[SerializeField] private bool randomRecoilAngle = true;
		[SerializeField] [Range(-20f, 20f)] private float recoilAngle = 1f;
		[SerializeField] [Range(1f, 10f)] private float recoilSpeed = 10f;
		[SerializeField] private ParticleSystem muzzlesmoke;
		private Muzzleflash muzzleflash;
		private Animator animator;

		[Header("Sounds")]
		[SerializeField] private AudioClip sndFire;
		[SerializeField] private AudioClip sndReload;
		private AudioSource audioSource;	    

		// ========================================================================================================================
		// Start

		private void Start() { 
			player_camera_transform = Camera.main.transform; // Cache camera transform component.
	        muzzleflash = GetComponent<Muzzleflash>();
			// animator = transform.GetChild(0).GetComponent<Animator> (); // hazardous code...
			animator = transform.GetComponentInChildren<Animator>();

			MessagingSystem.Instance.AttachListener (typeof(WeaponReloadedMessage), this.ProcessWeaponReloadedMessage);
			MessagingSystem.Instance.AttachListener (typeof(WeaponReloadStartMessage), this.ProcessWeaponReloadStartMessage);
			// Need to stop rotation of camera recoil shake if press esc.
			MessagingSystem.Instance.AttachListener (typeof(EscapeMessage), this.ProcessEscapeMessage);

			MessagingSystem.Instance.AttachListener (typeof(AmmoTakeMessage), this.ProcessAmmoTakeMessage);
			ammoInClip = 0;
	        ammoAvailable += PlayerInventory.Instance.GetAmmo((int)ammoType);
			ammoInInventory = ammoAvailable;
			if (ammoAvailable > 0) {
				AddToClip();
			}

			audioSource = GetComponent<AudioSource>();
			audioSource.volume = AudioManager.Instance.GetSfxVolume ();

			MessagingSystem.Instance.AttachListener (typeof(SettingsSavedMessage), this.ProcessSettingsSavedMessage);

	    }

		// ========================================================================================================================
		// Init

	    public void Init() {
	        
			MessagingSystem.Instance.QueueMessage (new AmmoQtyChangeMessage (infiniteAmmo,ammoInClip,ammoInInventory,(int)weaponType));
	        isReloading = false;
	    }

		// ========================================================================================================================
		// Fire

		private void Fire() {
	        float currentTime = Time.time;
			if ( !isReloading && ( currentTime > nextFire ) && ammoInClip > 0 ) {

	            if (fireMode == FireMode.Single) {
	                if (!triggerReleased) {
	                    return;
	                }
	            }
	            				
				if(!infiniteAmmo) {
					UseAmmo();
				}
	            
	            nextFire = currentTime + fireRate;
	            if (buckshot == 0) {
	                buckshot = 1;
	            }
	            for (int i=0; i<buckshot; i++) {
					Vector3 prjOrigin = launchFromBarrel ? barrel.position : player_camera_transform.position;
					Quaternion prjRotation = launchFromBarrel ? barrel.rotation : player_camera_transform.rotation;
					GameObject newPrjObj = PrefabPoolingSystem.Spawn(projectile.gameObject, prjOrigin, prjRotation);
	                Projectile newProjectile = newPrjObj.GetComponent<Projectile>();
					newProjectile.Init();
	                
					Vector3 prjDir = Vector3.zero + Random.insideUnitSphere * spread;
	                newProjectile.SetSpeed(muzzleVelocity, prjDir);
	            }

				if (animator != null) {
					if (!animator.GetBool ("isFiring")) {
						animator.SetBool ("isFiring", true);
					} else {
						animator.SetBool ("FastFire", true);
					}
				}

				Shell brass = PrefabPoolingSystem.Spawn(shell.gameObject,shellEjectOrigin.position, shellEjectOrigin.rotation).GetComponent<Shell>();
				brass.Init ();

				muzzleflash.Activate ();

				muzzlesmoke.Play ();

				StartCoroutine(ViewCameraRecoilShake());
				audioSource.PlayOneShot(sndFire);

			} else if (!infiniteAmmo && !isReloading && ammoInClip == 0 && ammoAvailable > 0) {
				Reload ();
			}
	    }

		// ========================================================================================================================
		// ViewCameraRecoilShake

		// NOTE 
		// This coroutine is tricky, the final result is good but it relies on the work of mouse look script ('LookRotation').
		// Normally, when interpolating between 'start' and 'end' values in a coroutine, the initialization of these vaules 
		// should be done outside the while loop. But in this specific case we also need to take into account the rotation added
		// by the mouse input axis ( to avoid "stuttering" if move vertical mouse axis while shooting ).
		// So, the key rule in this behaviour is that we update the 'start' and 'end' pos every frame, but in this specific 
		// case, this would make an undesired behaviour if we only rely on "internal" set of these vaules, 
		// since they would increase over time while they should stay unchanged over the whole interpolation.
		// The reason why this not happens here lies on continuous update of camera transform by the mouse look script
		// ( through the first person controller ). In fact, this is what actually happens, 
		// we set the localEulerAngles of the camera then we wait for the next frame with 'yield retun null', 
		// the camera rotation will be updated but in the next frame its value will be reset by the mouse look script,
		// so when we set again the 'start' rotation, this will be the same initial rotation plus any possible change
		// due to mouse input ( if we keep the mouse still this wuold be excactly the same as in previous frame ).
		// The trick is that the localEulerAngles of the camera is set right away - this happens in the same frame 
		// so you won't actually see any strange camera reset, and the final result is the expected smooth rotation.
		private IEnumerator ViewCameraRecoilShake() {
			float recoilCompletePercentage = 0;
			float recoilSpeed = this.recoilSpeed;

			Vector3 startRotation;
			Vector3 endRotation;
			Vector3 rotVec;

			// Fixed recoil angle is intended for powerful weapons like shotgun.
			// Only with random recoil angle, the transition is smooth in both directions, 
			// ( startrot to endrot and vice-versa ).

			if (randomRecoilAngle) {
				rotVec = Random.insideUnitSphere * recoilAngle;
			} else {
				rotVec = new Vector3 (recoilAngle,0,0);
			}

			while(recoilCompletePercentage <= 1) {
				if (rotateView) {
					startRotation = player_camera_transform.localEulerAngles;
					endRotation = startRotation + rotVec;

					recoilCompletePercentage += Time.deltaTime * recoilSpeed;

					float animInterpolation;
					if (randomRecoilAngle) {
						animInterpolation = (-Mathf.Pow(recoilCompletePercentage, 2) + recoilCompletePercentage) * 4; // parabola --> y = 4(-x^2+x)
					} else { 
						// How fast is transition from endRot to startRot, 
						// ( startRot to endRot has no transition, it must be very fast to simulate powerful recoil ).
						animInterpolation = 1 - recoilCompletePercentage;
					}
					player_camera_transform.localEulerAngles = Vector3.Lerp(startRotation, endRotation, animInterpolation);
				}

				yield return null;	

			}
			yield return null;
		}

		// ========================================================================================================================
		// ProcessEscapeMessage

		private bool ProcessEscapeMessage(BaseMessage msg) {
			OnRotateViewToggle ();
			return false;
		}

		// ========================================================================================================================
		// OnRotateViewToggle

		private void OnRotateViewToggle() {
			rotateView = !rotateView;
		}

		// ========================================================================================================================
		// Reload

		public void Reload(){
			if (!isReloading && (ammoInClip < clipSize) && (ammoAvailable > ammoInClip) ) {
				
				isReloading = true;

				if (animator != null) {
					animator.SetBool ("isReloading", true);
				}
			}
		}

		// ========================================================================================================================
		// ProcessWeaponReloadedMessage
	    
		private bool ProcessWeaponReloadedMessage(BaseMessage msg){ // Called when the animation event of reload anim is fired.
			if (this.isActiveAndEnabled) {
				AddToClip ();
				isReloading = false;
			}
			return false;
		}

	// ****************************************************************************************************************************
	//  Ammo stuff

	    // ========================================================================================================================
	    // AddToClip

		private void AddToClip() {
			int ammoToAdd = clipSize - ammoInClip;

			if (ammoAvailable > clipSize) {
				ammoInClip = clipSize;
				ammoInInventory -= ammoToAdd;
			} else {
				ammoInClip = ammoAvailable;
				ammoInInventory = 0;
			}

			MessagingSystem.Instance.QueueMessage (new AmmoQtyChangeMessage (infiniteAmmo,ammoInClip,ammoInInventory,(int)weaponType));
		}

	    // ========================================================================================================================
	    // UseAmmo

		private void UseAmmo() {
			ammoAvailable--;
			ammoInClip--;

	        PlayerInventory.Instance.UseAmmo((int)ammoType);

			MessagingSystem.Instance.QueueMessage (new AmmoQtyChangeMessage (infiniteAmmo,ammoInClip,ammoInInventory,(int)weaponType));
		}

	    // ========================================================================================================================
	    // HasInfiniteAmmo

	    public bool HasInfiniteAmmo() {
			return infiniteAmmo;
		}

	    // ========================================================================================================================
	    // GiveAmmo

	    public void GiveAmmo(int ammoAmount) {
	        ammoAvailable += ammoAmount;
	        ammoInInventory += ammoAmount;
	    }

	    // ========================================================================================================================
	    // GetAmmoGiveQty

	    public int GetAmmoGiveQty() {
	        return ammoGiveQty;
	    }

		// ========================================================================================================================
		// GetAmmoType

	    public int GetAmmoType() {
	        return (int)ammoType;
	    }

		// ========================================================================================================================
		// ProcessAmmoTakeMessage

		private bool ProcessAmmoTakeMessage(BaseMessage msg) {
			SyncAmmoWithInventory ();
			return false;
		}

		// ========================================================================================================================
		// SyncAmmoWithInventory

		private void SyncAmmoWithInventory() {
	        ammoAvailable = PlayerInventory.Instance.GetAmmo((int)ammoType);
	        ammoInInventory = ammoAvailable - ammoInClip;
			MessagingSystem.Instance.QueueMessage (new AmmoQtyChangeMessage (infiniteAmmo,ammoInClip,ammoInInventory,(int)weaponType));
	    }

	// END Ammo stuff
	// ****************************************************************************************************************************


		// ========================================================================================================================
		// OnTriggerHold

	    public void OnTriggerHold() {
			// if(ammoInClip > 0) {}
			Fire ();
	        triggerReleased = false;
	    }

		// ========================================================================================================================
		// OnTriggerRelease

	    public void OnTriggerRelease() {
	        triggerReleased = true;
	    }

		// ========================================================================================================================
		// GetWeaponType

	    public WEAPONS GetWeaponType(){
			return weaponType;
		}

		// ========================================================================================================================
		// GetWeaponName

		public static string GetWeaponName(WEAPONS wpnType) {
			
			string name = "";
			switch (wpnType) {

			case WEAPONS.Pistol :
				name = "Blaster";
				break;
			case WEAPONS.Machinegun :
				name = "Machinegun";
				break;
			case WEAPONS.SuperShotgun :
				name = "Shotgun";
				break;
			default :
				name = "New weapon";
				break;
			}
			return name;
		}

		// ========================================================================================================================
		// ProcessSettingsSavedMessage

		private bool ProcessSettingsSavedMessage(BaseMessage msg) {
			audioSource.volume = AudioManager.Instance.GetSfxVolume ();
			return false;
		}

		// ========================================================================================================================
		// ProcessWeaponReloadStartMessage
		private bool ProcessWeaponReloadStartMessage(BaseMessage msg) {
			if (this.isActiveAndEnabled) {
				audioSource.PlayOneShot(sndReload);
			}
			return false;
		}

		// ========================================================================================================================
		// OnDestroy

		private void OnDestroy() {
			if (MessagingSystem.IsAlive) {
				MessagingSystem.Instance.DetachListener (typeof(EscapeMessage), this.ProcessEscapeMessage);
				MessagingSystem.Instance.DetachListener (typeof(AmmoTakeMessage), this.ProcessAmmoTakeMessage);
				MessagingSystem.Instance.DetachListener (typeof(SettingsSavedMessage), this.ProcessSettingsSavedMessage);
				MessagingSystem.Instance.DetachListener (typeof(WeaponReloadedMessage), this.ProcessWeaponReloadedMessage);
				MessagingSystem.Instance.DetachListener (typeof(WeaponReloadStartMessage), this.ProcessWeaponReloadStartMessage);
			}
		}

		// ========================================================================================================================
	}
}