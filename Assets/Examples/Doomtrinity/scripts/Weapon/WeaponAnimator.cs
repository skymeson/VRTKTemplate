using UnityEngine;
using System.Collections;
using DoomtrinityFPSPrototype.Utils;

namespace DoomtrinityFPSPrototype.Weapons {

	public class WeaponReloadedMessage : BaseMessage {
		public WeaponReloadedMessage() {}
	}

	public class WeaponReloadStartMessage : BaseMessage {
		public WeaponReloadStartMessage() {}
	}

	[RequireComponent(typeof (Animator))]
	public class WeaponAnimator : UpdateableMonoBehaviour {

		// ========================================================================================================================
		// Instance variables

		private Animator animator;
		private CharacterController characterController;

		private bool isRunning = false;
		private bool previouslyRunning = false;

		// ========================================================================================================================
		// Start

		protected override void Begin() {
			animator = GetComponent<Animator> ();
			characterController = FindObjectOfType<CharacterController> ();
		}

		// ========================================================================================================================
		// Update

		public override void Think(float dt) {
			if (characterController != null) {
				isRunning = (characterController.velocity.sqrMagnitude > 0 && characterController.isGrounded);
				if(!previouslyRunning && isRunning) { // Avoid to set it every frame
					previouslyRunning = true;
					SetMoveBool (true);
				} else if(previouslyRunning && !isRunning) {
					previouslyRunning = false;
					SetMoveBool (false);
				}
			}
		}

		// ========================================================================================================================
		// OnDisable

		protected override void OnKill() {
			previouslyRunning = false;
		}

		// ========================================================================================================================
		// ResetFastFireBool

		private void ResetFastFireBool(){
			animator.SetBool ("FastFire", false);
		}

		// ========================================================================================================================
		// ResetFireBool

		private void ResetFireBool(){
			animator.SetBool ("isFiring", false);
		}

		// ========================================================================================================================
		// ResetReloadBool

		private void ResetReloadBool(){
			animator.SetBool ("isReloading", false);
			MessagingSystem.Instance.QueueMessage (new WeaponReloadedMessage ());
		}

		// ========================================================================================================================
		// SetMoveBool

		private void SetMoveBool(bool isMoving){
			animator.SetBool ("isMoving", isMoving);
		}

		// ========================================================================================================================
		// ReloadStart

		private void ReloadStart(){
			MessagingSystem.Instance.QueueMessage (new WeaponReloadStartMessage ());
		}

		// ========================================================================================================================
	}
}