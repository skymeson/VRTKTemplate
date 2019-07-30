using UnityEngine;
using System.Collections;
using DoomtrinityFPSPrototype.PrefabPoolSystem;

namespace DoomtrinityFPSPrototype.Weapons {
	public class Shell : MonoBehaviour {

		// ========================================================================================================================
		// Instance variables

		[SerializeField] private Rigidbody rigidBody;
		[SerializeField] private float forceMin;
		[SerializeField] private float forceMax;

		private float lifeTime = 4; // 4 secs
	    private float fadeTime = 2;

		// ========================================================================================================================
		// Init

		public void Init () {
			
			// Shells use the object pooling system, so if a shell  is deactivated while moving, 
			// ( under gravity force for example, if falling for a long time ),
			// when respawned it will restore its old velocity, so this velocity must be reset.
			rigidBody.velocity = Vector3.zero;

			// PrefabPoolLoader is not strictly needed in the scene, so we must do a null check.
			if (PrefabPoolLoader.ShellsParent != null) {
				transform.parent = PrefabPoolLoader.ShellsParent.transform;
			}
	        float force = Random.Range(forceMin, forceMax);
	        rigidBody.AddForce(transform.right * force);
	        rigidBody.AddTorque(Random.insideUnitSphere * force);

	        StartCoroutine(Fade());
		}

		// ========================================================================================================================
		// Fade
		
		private IEnumerator Fade () {
	        yield return new WaitForSeconds(lifeTime);

	        float percent = 0;
	        float fadeSpeed = 1 / fadeTime;
	        Material mat = GetComponent<Renderer>().material;
	        Color initialColor = mat.color;

	        while (percent < 1) {
	            percent += Time.deltaTime * fadeSpeed;
	            mat.color = Color.Lerp(initialColor, Color.clear, percent);
	            yield return null;
	        }

	        PrefabPoolingSystem.Despawn(gameObject);
			mat.color = initialColor;
		}

		// ========================================================================================================================
	}
}