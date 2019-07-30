using UnityEngine;
using System.Collections;
using DoomtrinityFPSPrototype.Utils;
using DoomtrinityFPSPrototype.Audio;
using DoomtrinityFPSPrototype.PrefabPoolSystem;

namespace DoomtrinityFPSPrototype.Character {
	// Enemy class holds all enemy defs and behaviours.
	// [RequireComponent(typeof(Animator))]
	[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
	public class Enemy : EnemyBase {

		[System.Serializable]
		public struct SpawnArgs {
			public float attackDistanceThreshold;
			public float attackRate;
			public float damageToTarget;
			public float moveSpeed;
			public float spawnHealth;
			public bool pathFinderEnabled;
		}

		// EnemyType enum holds all enemy identifiers.
		// Add at bottom of the list any new enemy.
		// IMPORTANT! Enemy prefab name must match this name, in 'Resources/Enemy/'!
		// Other scripts like 'SaveLoadData' and 'LevelInitializer' rely on this name.
		// In other words, this is a list of enemy prefab names.
	    public enum EnemyType
	    {
	        Alien,
			RedAlien,
			CapsuleEnemy
	    }
	    
	    public enum State { Idle, Chasing, Attacking};

		// ========================================================================================================================
		// Instance variables

		[SerializeField] private EnemyType enemyType; // make sure to give the same name to the prefab in 'Resources/Enemy/'
		[SerializeField] private ParticleSystem deathEffect;
		// spawn args
		[SerializeField] private float attackDistanceThreshold = 1.0f; // distance from player
		[SerializeField] private float attackRate = 2; // 2 secs
		[SerializeField] private float damageToTarget = 1;
		[SerializeField] private float moveSpeed = 7;

		private State currentState;
		private UnityEngine.AI.NavMeshAgent pathFinder;
		private Transform target;
		private DamageableEntity damageableTarget;    

		private float nextAttack = 0;
		private float enemyCollisionRadius = 0.1f;
		private float targetCollisionRadius;
		private bool hasTarget;
		private Animator animator;

		// ========================================================================================================================
		// Save and Restore

		public override DATA_TYPE Save(SaveLoadData.PersistantData savedata)
		{
			SaveStruct.Enemy_s enemy_s = new SaveStruct.Enemy_s ();
			enemy_s.tr = Util.GetTransform (this);

			enemy_s.health = Health;
			enemy_s.enemyType = (int)enemyType;

			enemy_s.attackDistanceThreshold = attackDistanceThreshold;
			enemy_s.attackRate = attackRate;
			enemy_s.damageToTarget = damageToTarget;
			enemy_s.moveSpeed = moveSpeed;
			enemy_s.pathFinderEnabled = pathFinder.enabled;

			savedata.enemies_s.Add (enemy_s);
			return DATA_TYPE.ENEMY;
		}

		public override void Restore(SaveLoadData.PersistantData savedata)
		{			
		}

		public void Restore(SaveStruct.Enemy_s enemy_s)
		{
			Vector3 position = new Vector3 (enemy_s.tr.X,enemy_s.tr.Y,enemy_s.tr.Z);
			Quaternion rotation = Quaternion.Euler (enemy_s.tr.RotX, enemy_s.tr.RotY, enemy_s.tr.RotZ);

			transform.position = position;
			transform.rotation = rotation;
			Health = enemy_s.health;

			// Reset spawn args
			attackDistanceThreshold = enemy_s.attackDistanceThreshold;
			attackRate = enemy_s.attackRate;
			damageToTarget = enemy_s.damageToTarget;
			moveSpeed = enemy_s.moveSpeed;
			pathFinder.enabled = enemy_s.pathFinderEnabled;
		}

		// ========================================================================================================================
		// Awake

		private void Awake() {
			animator = GetComponent<Animator> ();
	        pathFinder = GetComponent<UnityEngine.AI.NavMeshAgent>();

			// Disable the object so he won't start until the level is initialized.
			// Actually, this is need only to call 'Restore' before start ( 'Begin' ).
			gameObject.SetActive (false);
			// The best place to attach listener is start, but this is a special case.
			MessagingSystem.Instance.AttachListener (typeof(LevelInitializedMessage), this.ProcessLevelInitializedMessage);
	    }

		// ========================================================================================================================
		// ProcessLevelInitializedMessage

		private bool ProcessLevelInitializedMessage(BaseMessage msg) {
			gameObject.SetActive (true);
			return false;
		}

		// ========================================================================================================================
		// SetSpawnArgs

		// Set the enemy definitions. This could be done by level initializer, if 'overrideSpawnArgs' is flagged in EnemySpawn.
		public void SetSpawnArgs( SpawnArgs spawnArgs ) 
		{
			attackDistanceThreshold = spawnArgs.attackDistanceThreshold;
			attackRate = spawnArgs.attackRate;
			damageToTarget = spawnArgs.damageToTarget;
			moveSpeed = spawnArgs.moveSpeed;
			pathFinder.enabled = spawnArgs.pathFinderEnabled;
			spawnHealth = spawnArgs.spawnHealth;
		}

		// ========================================================================================================================
		// Start

		protected override void Begin () { // override

			if (!SaveLoadData.Instance.isLoadingFromSave) {
				enemiesCount++;
			}
			// PrefabPoolLoader is not strictly needed in the scene, so we must do a null check.
			if (PrefabPoolLoader.EnemiesParent != null) {
				// Keep the hierarchy tidy, place the enemy under gameobject 'Enemies'
				transform.parent = PrefabPoolLoader.EnemiesParent.transform;
			}

			base.Begin();

			pathFinder.speed = moveSpeed;
			if (deathEffect != null) {
				deathEffect.startColor = Color.green;
			}

			// Search for target
	        if (Player.Instance != null)
	        {
	            hasTarget = true;
				target = Player.Instance.transform;
	            damageableTarget = target.GetComponent<DamageableEntity>();
				targetCollisionRadius = target.GetComponent<CharacterController>().radius;
	        }

	        if (hasTarget) 
			{
	            currentState = State.Chasing;

				MessagingSystem.Instance.AttachListener(typeof(PlayerDeathMessage),this.ProcessPlayerDeathMessage);
				StartCoroutine(UpdatePath());
	        }
		}

		// ========================================================================================================================
		// Update

		public override void Think (float dt) {

			if (hasTarget) {
				if (Time.time > nextAttack && target!= null)
				{
					float sqrDistanceToTarget = (target.position - transform.position).sqrMagnitude; // sqr for better performance
					if (sqrDistanceToTarget < Mathf.Pow(attackDistanceThreshold + enemyCollisionRadius + targetCollisionRadius, 2))
					{
						nextAttack = Time.time + attackRate;
						StartCoroutine(Attack());
					}
				}
			}
		}

		// ========================================================================================================================
		// ProcessPlayerDeathMessage

		private bool ProcessPlayerDeathMessage(BaseMessage msg) {
			hasTarget = false;
			currentState = State.Idle;
			return false;
		}

		// ========================================================================================================================
		// Damage

	    public override void Damage(float damage, Vector3 hitPoint, Vector3 hitDir)
	    {
			AudioManager.Instance.PlaySound ("impact", transform.position);
			if ((damage >= Health) && !dead) { // !dead is required to call this only once. With buckshot, this method may be called more times.

				EnemyDeath ();

				AudioManager.Instance.PlaySound ("enemyDeath", transform.position);
				if (deathEffect != null) {
					// Spawn enemy death particle
					Destroy (Instantiate (deathEffect.gameObject, hitPoint, Quaternion.FromToRotation (Vector3.forward, hitDir)) as GameObject, deathEffect.startLifetime);
				}
			} else { // play pain anim only if not critical hit
				if(animator != null){
					animator.SetBool ("isHit", true);
				}

			}
	        base.Damage(damage, hitPoint, hitDir);
	    }

		// ========================================================================================================================
		// SetHealth

		// This should only be used for enemy restore!
	    public void SetHealth(float _health) {
	        Health = _health;
	    }

		// ========================================================================================================================
		// Attack
		private IEnumerator Attack() {    

	        currentState = State.Attacking;
	        pathFinder.enabled = false;

			if (animator != null) {
				animator.SetBool ("isAttacking", true); // Attack anim state, DoDamage will be called
			} else { // attack anim by code, thanks to Sebastian Lague
				Vector3 startPos = transform.position;
				Vector3 dirToTarget = (target.position - transform.position).normalized;
				// Vector3 endPos = target.position - dirToTarget * (enemyCollisionRadius); // overlap a bit to resemble contact
				Vector3 endPos = target.position - dirToTarget;
				float attackSpeed = 3;
				float attackCompletedPercentage = 0;
				bool damageTarget = false;

				while (attackCompletedPercentage <= 1) {

					if ((attackCompletedPercentage >= .5f) && !damageTarget) {
						damageTarget = true;
						DoDamage ();
					}

					attackCompletedPercentage += Time.deltaTime * attackSpeed;
					float animInterpolation = (-Mathf.Pow(attackCompletedPercentage, 2) + attackCompletedPercentage) * 4; // parabola --> y = 4(-x^2+x)

					transform.position = Vector3.Lerp(startPos, endPos, animInterpolation);

					yield return null; // skip one frame
				}

			}


			while (!pathFinder.enabled) {
				transform.LookAt (target);
				yield return null; // skip one frame
			}

	    }

		// ========================================================================================================================
		// DoDamage

		private void DoDamage() {
			if(target != null) {
				float sqrDistanceToTarget = (target.position - transform.position).sqrMagnitude;
				if (sqrDistanceToTarget < Mathf.Pow(attackDistanceThreshold + enemyCollisionRadius + targetCollisionRadius, 2)) {

					damageableTarget.Damage(damageToTarget);
					AudioManager.Instance.PlaySound ("enemyAttack", transform.position);
				}
			}
			if (animator != null) {
				animator.SetBool ("isAttacking", false);
			}

			pathFinder.enabled = true;
			currentState = State.Chasing;
		}

		// ========================================================================================================================
		// ResetHitBool

		private void ResetHitBool() {
			if (animator != null) {
				animator.SetBool ("isHit", false);
			}

		}

		// ========================================================================================================================
		// UpdatePath

		private IEnumerator UpdatePath() {
	        float refreshRate = .13f; // 130 ms

			while (hasTarget ) {
				if( (currentState == State.Chasing ) && pathFinder.enabled && !dead) {
					Vector3 dirToTarget = (target.position - transform.position).normalized;
					Vector3 targetPosition = target.position - dirToTarget * (enemyCollisionRadius + targetCollisionRadius + attackDistanceThreshold/2);
					pathFinder.SetDestination(targetPosition);
	            }
	        
	            yield return new WaitForSeconds(refreshRate);
	        }
		}

		// ========================================================================================================================
		// OnKill

		protected override void OnKill() {
			if (MessagingSystem.IsAlive) {
				MessagingSystem.Instance.DetachListener (typeof(PlayerDeathMessage), this.ProcessPlayerDeathMessage);
				MessagingSystem.Instance.DetachListener (typeof(LevelInitializedMessage), this.ProcessLevelInitializedMessage);
			}
		}

		// ========================================================================================================================
		// EnablePathFinder

		public void EnablePathFinder(bool enable) {
			pathFinder.enabled = enable;
		}

		// ========================================================================================================================
		// IsPathFinderEnabled

		public bool IsPathFinderEnabled() {
			return pathFinder.enabled;
		}

		// ========================================================================================================================
		// GetEnemyType

	    public int GetEnemyType() {
	        return (int)enemyType;
	    }
	    
		// ========================================================================================================================
		// GetSpawnArgs

		// Get the enemy definitions.
		public SpawnArgs GetSpawnArgs() 
		{
			SpawnArgs args = new SpawnArgs();
			args.attackDistanceThreshold = attackDistanceThreshold;
			args.attackRate = attackRate;
			args.damageToTarget = damageToTarget;
			args.moveSpeed = moveSpeed;
			args.pathFinderEnabled = pathFinder.enabled;

			return args;
		}

		// ========================================================================================================================
	}
}