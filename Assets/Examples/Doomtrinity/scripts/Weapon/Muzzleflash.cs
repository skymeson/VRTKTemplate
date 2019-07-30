using UnityEngine;
using System.Collections;

namespace DoomtrinityFPSPrototype.Weapons {
	public class Muzzleflash : MonoBehaviour {

		// ========================================================================================================================
		// Instance variables

		[SerializeField] private GameObject flashHolder;
		[SerializeField] private float flashTime;
		[SerializeField] private Sprite[] flashSprites;
		[SerializeField] private Sprite[] flashSpritesFront;
		[SerializeField] private SpriteRenderer spriteRenderer;
		[SerializeField] private SpriteRenderer spriteRendererFront;

		// ========================================================================================================================
		// Start

		private void Start() {
	        
			flashHolder.SetActive(false);
	    }

		// ========================================================================================================================
		// Activate

		public void Activate() {

			StartCoroutine (ActivateCoroutine ());
		}

		// ========================================================================================================================
		// ActivateCoroutine

		private IEnumerator ActivateCoroutine() {
	        flashHolder.SetActive(true);

	        int flashSpriteIndex = Random.Range(0, flashSprites.Length);

	        spriteRenderer.sprite = flashSprites[flashSpriteIndex];
			spriteRendererFront.sprite = flashSpritesFront[flashSpriteIndex];        

			yield return new WaitForSeconds (flashTime);
			flashHolder.SetActive(false);

	    }

		// ========================================================================================================================
	}
}