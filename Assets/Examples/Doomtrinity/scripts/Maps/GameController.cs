using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using DoomtrinityFPSPrototype.Character;
using DoomtrinityFPSPrototype.Utils;

namespace DoomtrinityFPSPrototype.Maps {

	// =====================================================
	// Messages

	public class LevelEndMessage : BaseMessage {
		public LevelEndMessage() { }
	}

	public class GameSavedMessage : BaseMessage {
		public readonly bool _saved;
		public GameSavedMessage(bool saved) {
			_saved = saved;
		}
	}

	// =====================================================

	// This class controls what happens in game and calls specific events,
	// for example: if we kill all enemies, then level should end.
	// Objective system is quite limited ATM. I'm considering to enhance it in future updates.
	[RequireComponent(typeof (MapDefs))]
	public class GameController : MonoBehaviour {

		// ========================================================================================================================
		// Instance variables

		[SerializeField] private bool levelEndIfNoObjective = true;

		private ObjectiveType currentObjective = ObjectiveType.None;

		// ========================================================================================================================
		// Singleton stuff

		private static GameController _instance;
		public static GameController Instance {
			get {
				return _instance;
			}
		}

		private void Awake() {

			if (_instance != null && _instance != this) {
				Destroy(this.gameObject);            
			}
			else {
				_instance = this;

				LoadMapObjective ();
			}
		}

		// ========================================================================================================================
		// LoadMapObjective

		public void LoadMapObjective () {
			MapDefs mapDefs = GetComponent<MapDefs> ();
			currentObjective = mapDefs.Objective;
		}

		// ========================================================================================================================
		// Init

		public void Init () {
			
			MessagingSystem.Instance.AttachListener (typeof(EnemyDeathMessage), this.ProcessEnemyDeathMessage);
			StartCoroutine(CheckEnemiesCount());
		}

		// ========================================================================================================================
		// CheckEnemiesCount

		private IEnumerator CheckEnemiesCount() {
			// Wait some time to let UI initialize, then check if all enemies have been destroyed. 
			// This is needed if load a game that was saved when level is complete.
			yield return new WaitForSeconds (3);
			OnEnemyKilled ();
		}

		// ========================================================================================================================
		// TerminateLevel

		private void TerminateLevel() {
			if (currentObjective == ObjectiveType.None && levelEndIfNoObjective) {
				MessagingSystem.Instance.QueueMessage (new LevelEndMessage ()); // Level ends if no objective.
			}
		}

		// ========================================================================================================================
		// ProcessEnemyDeathMessage

		private bool ProcessEnemyDeathMessage(BaseMessage msg) {
			OnEnemyKilled ();
			return false;
		}

		// ========================================================================================================================
		// OnEnemyKilled

		private void OnEnemyKilled() {
			if (( EnemyBase.enemiesKilled == EnemyBase.enemiesCount) && (currentObjective == ObjectiveType.KillEmAll) ) {
				currentObjective = ObjectiveType.None;
				TerminateLevel ();
			}
		}

		// ========================================================================================================================
		// Save

		public bool Save() {
			if(SceneManager.GetActiveScene ().name != "FiringRange" ) {
				bool saved = false;

				saved = SaveLoadData.Instance.SaveGame();
				MessagingSystem.Instance.QueueMessage (new GameSavedMessage (saved));

				return saved;
			}
			return false;
		}

		// ========================================================================================================================
		// OnDestroy

		private void OnDestroy() {
			if (MessagingSystem.IsAlive) {
				MessagingSystem.Instance.DetachListener (typeof(EnemyDeathMessage), this.ProcessEnemyDeathMessage);
			}
		}

		// ========================================================================================================================
		// CurrentObjectiveName

		public string ObjectiveName() {
			string objective_tmp = "";

			switch (currentObjective) {

			case ObjectiveType.None:
				objective_tmp = "No objective";
				break;
			case ObjectiveType.KillEmAll:
				objective_tmp = "Destroy all enemies";
				break;			
			}

			return objective_tmp;
		}

		// ========================================================================================================================
	}

	public enum ObjectiveType {
		None,
		KillEmAll
	}
}