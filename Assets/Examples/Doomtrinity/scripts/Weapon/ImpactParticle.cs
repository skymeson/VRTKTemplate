using UnityEngine;
using System.Collections;
using DoomtrinityFPSPrototype.PrefabPoolSystem;

namespace DoomtrinityFPSPrototype.Weapons {
	[RequireComponent(typeof(ParticleSystem))]
	public class ImpactParticle : MonoBehaviour {

		// ========================================================================================================================
		// Instance variables
		
		private ParticleSystem prtSystem;

		// ========================================================================================================================
		// Init

		public void Init () {
			// PrefabPoolLoader is not strictly needed in the scene, so we must do a null check.
			if (PrefabPoolLoader.PrjImpactParent != null) {
				transform.parent = PrefabPoolLoader.PrjImpactParent.transform;
			}
			prtSystem = GetComponent<ParticleSystem> ();
			StartCoroutine (Despawn ());
		}	

		// ========================================================================================================================
		// Despawn

		private IEnumerator Despawn() {
			yield return new WaitForSeconds (prtSystem.startLifetime);
			PrefabPoolingSystem.Despawn (gameObject);
		}

		// ========================================================================================================================
	}
}