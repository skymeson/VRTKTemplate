using UnityEngine;
using System.Collections;
using DoomtrinityFPSPrototype.Character;
using DoomtrinityFPSPrototype.Audio;
using DoomtrinityFPSPrototype.PrefabPoolSystem;
using DoomtrinityFPSPrototype.Utils;

namespace DoomtrinityFPSPrototype.Weapons {
	// Class Projectile holds all projectile specs and behaviours.
	[RequireComponent(typeof(TrailRenderer))]
	public class Projectile : UpdateableMonoBehaviour {

		// ========================================================================================================================
		// Instance variables

		private float speed = 10; // 'muzzleVelocity' in 'Weapon' script overrides this, if SetSpeed method is called. 
		private Vector3 dir;
		[SerializeField] private float damage = 1;
		[SerializeField] private LayerMask collisionMask;
		[SerializeField] private Color trailColor;
		[SerializeField] private ParticleSystem impactEffect;
		[SerializeField] private float fuse = 4;
	    
		private float skinWidth = .1f; // compensate the enemy movements. This should prevent from passing through enemy. Increase for very fast enemies.
		private bool initialized = false;
		private TrailRenderer trailRenderer;

		// ========================================================================================================================
		// Awake

		private void Awake() {
			trailRenderer = GetComponent<TrailRenderer> ();
			trailRenderer.material.SetColor("_TintColor", trailColor);
		}

		// ========================================================================================================================
		// Init

	    public void Init() {

			// PrefabPoolLoader is not strictly needed in the scene, so we must do a null check.
			if (PrefabPoolLoader.ProjectilesParent != null) {
				transform.parent = PrefabPoolLoader.ProjectilesParent.transform;
			}

	        Collider[] initialCollisions = Physics.OverlapSphere(transform.position, .1f, collisionMask); // damage enemy if overlaps while shooting
	        if (initialCollisions.Length > 0) {
	            OnHitObject(initialCollisions[0], transform.position);
	        }

			// trailRenderer.enabled = false;

			if(gameObject.activeSelf) {
				// StartCoroutine (EnableTrailRenderer());
				StartCoroutine(DespawnProjectile(fuse));
			}

			initialized = true;
	    }

		// ========================================================================================================================
		// EnableTrailRenderer

		private IEnumerator EnableTrailRenderer(){
			yield return new WaitForSeconds (0.1f);
			trailRenderer.enabled = true;
		}

		// ========================================================================================================================
		// SetSpeed

	    public void SetSpeed(float newSpeed, Vector3 randomDirection) {
	        speed = newSpeed;
	        dir = randomDirection;
	    }

		// ========================================================================================================================
		// Update

		public override void Think (float dt) {
			if(initialized) {
				float moveDist = speed * Time.deltaTime;
				CheckCollision(moveDist);
				transform.Translate( (Vector3.forward + dir) * moveDist);
			}        
		}

		// ========================================================================================================================
		// CheckCollision

		private void CheckCollision(float moveDistance) {
	        Ray ray = new Ray(transform.position, transform.forward);
	        RaycastHit hit;

	        if (Physics.Raycast(ray, out hit, moveDistance + skinWidth, collisionMask, QueryTriggerInteraction.Collide)) {
	            OnHitObject(hit.collider, hit.point);
	        }
	    }

		// ========================================================================================================================
		// OnHitObject

		private void OnHitObject(Collider c, Vector3 hitPoint)
	    {
	        // IDamageable damageableObj = c.GetComponent<IDamageable>();
			IDamageable damageableObj = c.GetComponentInParent<IDamageable>(); // colliders in enemies are child.

	        if (damageableObj != null)
	        {
	            damageableObj.Damage(damage, hitPoint, transform.forward);
	        } else {
	            AudioManager.Instance.PlaySound("impact", transform.position);
	            
				ImpactParticle impactPrt = PrefabPoolingSystem.Spawn(impactEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.back, transform.forward)).GetComponent<ImpactParticle>();
				impactPrt.Init ();
			}
			// Despawn projectile
	        PrefabPoolingSystem.Despawn(gameObject);
	    }

		// ========================================================================================================================
		// DespawnProjectile

		// This should be called just after the projectile is launched.
		public IEnumerator DespawnProjectile(float fuse)
	    {
	        yield return new WaitForSeconds(fuse);
	        PrefabPoolingSystem.Despawn(gameObject);
	    }

		// ========================================================================================================================
	}
}