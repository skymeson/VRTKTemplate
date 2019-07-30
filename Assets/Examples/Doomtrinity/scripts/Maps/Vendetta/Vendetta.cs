using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DoomtrinityFPSPrototype.Character;
using DoomtrinityFPSPrototype.Utils;

namespace DoomtrinityFPSPrototype.Maps {
	// Ad-hoc script for 'Vendetta' map.
	// The idea is to move enemies forward, and "activate" them when one of them reach a specific trigger.
	// Actually all enemies move with same velocity so a parent "mover" would be a more optimized system,
	// instead of check all of them, but that's not a big problem, plus it allows more flexible behaviours if needed.
	// If the 'EnemyActivator' trigger detects any enemy in 'OnTriggerEnter', this would enable that specific enemy path finder.
	// When that happens, break the check loop in 'TranslateEnemies', and enable the path finder for all enemies.
	public class Vendetta : MonoBehaviour {
		
		// ========================================================================================================================
		// Instance variables

		private bool pathFinderEnabled = false;
		[SerializeField] private float railMoveTime = 1.6f;

		// ========================================================================================================================
		// Start

		private void Start () {
			
			MessagingSystem.Instance.AttachListener (typeof(LevelInitializedMessage), this.ProcessLevelInitializedMessage);
		}

		// ========================================================================================================================
		// ProcessLevelInitializedMessage

		private bool ProcessLevelInitializedMessage(BaseMessage msg) {
			StartCoroutine (TranslateEnemies ());
			return false;
		}

		// ========================================================================================================================
		// TranslateEnemies

		private IEnumerator TranslateEnemies() {
			
			List<Enemy> enemies = LevelInitializer.Instance.SpawnedEnemies;
			// TODO cheat message
			// implement messaging system for cheats.
			// cheats could break game logic, so we can manage them gracefully by listening those messages.
			while (!pathFinderEnabled) {
				foreach(Enemy enemy in enemies) {
					if(enemy != null && enemy.isActiveAndEnabled) {
						if(!enemy.IsPathFinderEnabled()) {
							enemy.transform.Translate (Vector3.forward * Time.deltaTime * railMoveTime);
						} else {
							pathFinderEnabled = true;
							break;
						}

					}
				}
				yield return null;
			}
			foreach(Enemy enemy in enemies) {
				if(enemy != null && enemy.isActiveAndEnabled) {
					enemy.EnablePathFinder (true);
				}
			}


			yield return null;
		}

		// ========================================================================================================================
		// OnDestroy

		private void OnDestroy() {
			if (MessagingSystem.IsAlive) {
				MessagingSystem.Instance.DetachListener (typeof(LevelInitializedMessage), this.ProcessLevelInitializedMessage);
			}
		}

		// ========================================================================================================================
	}
}