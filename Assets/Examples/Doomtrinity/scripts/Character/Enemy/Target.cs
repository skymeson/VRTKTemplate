using UnityEngine;
using System.Collections;
using DoomtrinityFPSPrototype.Audio;
using DoomtrinityFPSPrototype.Utils;

namespace DoomtrinityFPSPrototype.Character {
	// Target class holds all target defs and behaviours.
	[RequireComponent(typeof(Animator))]
	public class Target : EnemyBase {

		public enum TargetDir
		{
			left,
			right,
			both
		}
	    
	    // ========================================================================================================================
		// Instance variables

		[SerializeField] private ParticleSystem deathEffect;
		[SerializeField] private TargetDir direction;
		[SerializeField] private float moveSpeed = 1;
		[SerializeField] private bool rotate = false;
		[SerializeField] private int distance = 1; // meters

		private Animator animator;

		// ========================================================================================================================
		// Save and Restore

		// Save is disabled in firing range map! These methods won't be used.

		public override DATA_TYPE Save(SaveLoadData.PersistantData savedata)
		{
			return DATA_TYPE.ENEMY;
		}

		public override void Restore(SaveLoadData.PersistantData savedata)
		{
		}

		// ========================================================================================================================
		// Awake

		private void Awake() {
			animator = GetComponent<Animator> ();

			enemiesCount++;
	    }

		// ========================================================================================================================
		// Start

		protected override void Begin () { // override
	        base.Begin();
			if (rotate) {
			    StartCoroutine (Rotate ());
			} else {
				StartCoroutine (Move ());
			}
		}

		// ========================================================================================================================
		// Move

		private IEnumerator Move() {

			float interpolant = 0;
            Vector3 dir = transform.right * distance;

            bool returnToStart = false;
            //Vector3 startPos = transform.localPosition;
            Vector3 startPos = transform.position;

            Vector3 endPos = startPos;

			switch (direction) {

			case TargetDir.left:
				endPos += dir * -1;
				break;
			case TargetDir.right:
				endPos += dir;
				break;
			}

			while (true) {
				
				if(interpolant >= 1) {
					interpolant = 1;
					returnToStart = true;
				} else if(interpolant <= 0) {
					interpolant = 0;
					returnToStart = false;
					if(direction == TargetDir.both) {
						dir *= -1;
						endPos = startPos + dir;
					}
				}

				if (returnToStart) {
					interpolant -= Time.deltaTime * moveSpeed;
				} else {
					interpolant += Time.deltaTime * moveSpeed;
				}


                transform.position = Vector3.Lerp (startPos, endPos, interpolant);

				yield return null;
			}

		}

		// ========================================================================================================================
		// Rotate

		private IEnumerator Rotate() {

			float interpolant = 0;
			Vector3 rotVec = new Vector3 (0,0,-10);
			bool returnToStart = false;
			Vector3 startRot = transform.localEulerAngles;
			Vector3 endRot = startRot;

			switch (direction) {

			case TargetDir.left:
				endRot += rotVec * -1;
				break;
			case TargetDir.right:
				endRot += rotVec;
				break;
			}

			while (true) {

				if(interpolant >= 1) {
					interpolant = 1;
					returnToStart = true;
				} else if(interpolant <= 0) {
					interpolant = 0;
					returnToStart = false;
					if(direction == TargetDir.both) {
						rotVec *= -1;
						endRot = startRot + rotVec;
					}
				}

				if (returnToStart) {
					interpolant -= Time.deltaTime * moveSpeed;
				} else {
					interpolant += Time.deltaTime * moveSpeed;
				}
				transform.localEulerAngles = Vector3.Lerp (startRot, endRot, interpolant);

				yield return null;
			}

		}

		// ========================================================================================================================
		// Damage

	    public override void Damage(float damage, Vector3 hitPoint, Vector3 hitDir)
	    {
			AudioManager.Instance.PlaySound ("impact", transform.position);
			if ((damage >= Health) && !dead) { // !dead is required to call this only once. With buckshot, this method may be called more times.

				EnemyDeath ();

				AudioManager.Instance.PlaySound ("enemyDeath", transform.position);
				Destroy (Instantiate (deathEffect.gameObject, hitPoint, Quaternion.FromToRotation (Vector3.forward, hitDir)) as GameObject, deathEffect.startLifetime);
			} else { // play pain anim only if not critical hit
				animator.SetBool ("isHit", true);
			}
	        base.Damage(damage, hitPoint, hitDir);
	    }

		// ========================================================================================================================
		// ResetHitBool

		private void ResetHitBool() {
			animator.SetBool ("isHit", false);
		}

		// ========================================================================================================================
	}
}