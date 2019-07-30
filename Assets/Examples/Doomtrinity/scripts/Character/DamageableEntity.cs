using UnityEngine;
using System.Collections;
using System;
using DoomtrinityFPSPrototype.Utils;

namespace DoomtrinityFPSPrototype.Character {

	// Base class for damageable entities - player, enemy.
	public abstract class DamageableEntity : PersistantMono, IDamageable
	{
		// ========================================================================================================================
		// Instance variables

		public float spawnHealth;
	    protected bool dead;

		// ========================================================================================================================
		// Properties

		public virtual float Health {get; protected set;}

		// ========================================================================================================================
		// Save and Restore

		public abstract override DATA_TYPE Save (SaveLoadData.PersistantData savedata); // Implement this behaviour in derived classes!
		public abstract override void Restore(SaveLoadData.PersistantData savedata); // Implement this behaviour in derived classes!	
	    
		// ========================================================================================================================
		// Start

	    protected override void Begin () { // handle override in extended classes
	        if (Health == 0 ) { // if health is > 0 then we're loading from a save file
	            Health = spawnHealth;
	        }
	        
		}

		// ========================================================================================================================
		// Damage

	    public virtual void Damage(float damage, Vector3 hitPoint, Vector3 hitDir ) {
	        Damage(damage);
	    }

	    public virtual void Damage(float damage)
	    {
	        Health -= damage;

	        if (Health <= 0 && !dead)
	        {
	            Die();
	        }
	    }
	    
		// ========================================================================================================================
		// Die

	    protected virtual void Die() {
	        dead = true;
			GameObject.Destroy(gameObject);
	    }

		// ========================================================================================================================
	}
}