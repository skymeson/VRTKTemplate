using UnityEngine;
using System.Collections;
using DoomtrinityFPSPrototype.Utils;

namespace DoomtrinityFPSPrototype.Character {

	public class EnemyDeathMessage : BaseMessage {
		public EnemyDeathMessage() { }
	}

	public abstract class EnemyBase : DamageableEntity {
		
		// ========================================================================================================================
		// static variables

		public static int enemiesCount { get; protected set;}
		public static int enemiesKilled { get; protected set;}

		// ========================================================================================================================
		// EnemyDeath

		protected void EnemyDeath() {
			MessagingSystem.Instance.QueueMessage (new EnemyDeathMessage ());
			enemiesKilled++;
		}

		// ========================================================================================================================
		// RestoreEnemiesCount

		public static void RestoreEnemiesCount() {
			enemiesKilled = SaveLoadData.Instance.GetPersistantData ().stats_s.killedEnemiesCount;
			enemiesCount = SaveLoadData.Instance.GetPersistantData().stats_s.startEnemiesCount;
		}

		// ========================================================================================================================
		// ResetEnemiesCount

		public static void ResetEnemiesCount() {
			enemiesCount = 0;
			enemiesKilled = 0;
		}

		// ========================================================================================================================
	}
}