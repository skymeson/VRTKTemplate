using UnityEngine;
using System.Collections;
using DoomtrinityFPSPrototype.Character;

namespace DoomtrinityFPSPrototype.Maps {
public class EnemyActivator : MonoBehaviour {
	
	// ========================================================================================================================
	// OnTriggerEnter

	private IEnumerator OnTriggerEnter (Collider other) {
		Enemy enemy = other.GetComponentInParent<Enemy> (); // colliders of enemies are child objects.
		if(enemy != null) {
			enemy.EnablePathFinder (true);
		}
		yield return null;
	}

	// ========================================================================================================================
}
}