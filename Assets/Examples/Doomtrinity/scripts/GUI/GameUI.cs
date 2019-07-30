using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using DoomtrinityFPSPrototype.Character;
using DoomtrinityFPSPrototype.Maps;
using DoomtrinityFPSPrototype.Utils;
using DoomtrinityFPSPrototype.Weapons;

namespace DoomtrinityFPSPrototype.dtGUI {

	// Actually used to choose what to do when escaping.
	public enum MenuType {
		None,
		Pause,
		SaveProgress,
		GameOver,
		NextLevel,
		Settings
	}

	// Manage in-game menu ( pause, next level, game over ) and hud.
	public class GameUI : MonoBehaviour {

		// ========================================================================================================================
		// Instance variables

		[SerializeField] private Image fadePlane;
		[SerializeField] private GameObject gameOverUI;
		[SerializeField] private GameObject saveProgressUI;
		[SerializeField] private Text txtScoreUI;
		[SerializeField] private RectTransform healthBar;
		[SerializeField] private RectTransform ammoCounter;
		[SerializeField] private Text txtAmmoInClip;
		[SerializeField] private Text txtAmmoInInventory;
		[SerializeField] private RectTransform hud;
		[SerializeField] private GameObject pauseUI;
		[SerializeField] private GameObject nextLevelUI;
		[SerializeField] private GameObject settingsUI;
		[SerializeField] private Text txtObjective;
		[SerializeField] private Text txtMapName;
		[SerializeField] private Image damageFx;
		[SerializeField] private Text txtPickup;
		[SerializeField] private Text txtLevelComplete;
		[SerializeField] private Text txtSave;
		[SerializeField] private Text infoMsg;
		[SerializeField] private RectTransform crosshair;

		private int nextSceneIndex;
		private bool isGameOver = false;
		private MenuType currentMenu = MenuType.None;
		private Coroutine objectiveMessage;
		private Coroutine pickupMessage;
		private Coroutine saveMessage;


		// ========================================================================================================================
		// Singleton stuff

		private static GameUI _instance;
		public static GameUI Instance {
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
			}
		}

		// ========================================================================================================================
		// Start
	    
		private void Start () {
	        
			MessagingSystem.Instance.AttachListener (typeof(PlayerDeathMessage), this.ProcessPlayerDeathMessage);
			MessagingSystem.Instance.AttachListener (typeof(PlayerHealthChangeMessage), this.ProcessPlayerHealthChangeMessage);
			MessagingSystem.Instance.AttachListener (typeof(EnemyDeathMessage), this.ProcessEnemyDeathMessage);
			MessagingSystem.Instance.AttachListener (typeof(AmmoQtyChangeMessage), this.ProcessAmmoQtyChangeMessage);
			MessagingSystem.Instance.AttachListener (typeof(LevelEndMessage), this.ProcessLevelEndMessage);
			MessagingSystem.Instance.AttachListener (typeof(GameSavedMessage), this.ProcessGameSavedMessage);
			MessagingSystem.Instance.AttachListener (typeof(PlayerPickupMessage), this.ProcessPlayerPickupMessage);

			if (GameController.Instance != null) {
				
				MapDefs mapDefs = GameController.Instance.gameObject.GetComponent<MapDefs> ();
				nextSceneIndex = mapDefs.NextMapIndex;
				txtObjective.text = GameController.Instance.ObjectiveName ();
				txtMapName.text = mapDefs.MapName;
				StartCoroutine (FadeTextMapName ());
			}

	    }

// ********************************************************************************************************************************
//  hud -->

		// ========================================================================================================================
		// ProcessEnemyDeathMessage

		private bool ProcessEnemyDeathMessage(BaseMessage msg) {

			OnEnemyKilled ();
			return false;
		}

		// ========================================================================================================================
		// OnEnemyKilled
	    
		private void OnEnemyKilled() {
	                
			txtScoreUI.text = EnemyBase.enemiesKilled.ToString() + " / " + EnemyBase.enemiesCount;
	    }

		// ========================================================================================================================
		// ProcessAmmoQtyChangeMessage

		private bool ProcessAmmoQtyChangeMessage(BaseMessage msg) {
			
			AmmoQtyChangeMessage ammo_msg = msg as AmmoQtyChangeMessage;

			if(Player.Instance != null && (Player.Instance.GetCurrentWeapon() == ammo_msg._weaponType)){
				if (ammo_msg._hasInfiniteAmmo ) {
					txtAmmoInClip.text = "";
					txtAmmoInInventory.text = "--";
				} else {
					txtAmmoInClip.text = ammo_msg._ammoInClip.ToString();
					txtAmmoInInventory.text = ammo_msg._ammoInInventory.ToString();
				}
			}

			return false;
		}

		// ========================================================================================================================
		// OnHealthChanged

		private void OnHealthChanged(bool damaged) {
	        float healthPercent = 0; // Set default to 0, player may die before ui gets updated so you could still see the red bar.
			if (Player.Instance != null)
	        {
				healthPercent = Player.Instance.Health / Player.Instance.spawnHealth;
	        }
	        healthBar.localScale = new Vector3(healthPercent, 1, 1);
			if (damaged) {
				float speed = 5;
				OnDamage (speed);
			}
	    }

		// ========================================================================================================================
		// OnObjective

		public void OnObjective() {
			txtObjective.gameObject.SetActive (true);
			if(objectiveMessage != null) {
				StopCoroutine(objectiveMessage);
			}
			txtObjective.color = Color.white;
			objectiveMessage = StartCoroutine(FadeText(txtObjective));
		}

		// ========================================================================================================================
		// OnPickup

		private bool ProcessPlayerPickupMessage(BaseMessage msg) {
			PlayerPickupMessage pickup_msg = msg as PlayerPickupMessage;

			txtPickup.gameObject.SetActive (true);
			if(pickupMessage != null) {
				StopCoroutine(pickupMessage);
			}
			txtPickup.color = Color.green;
			txtPickup.text = pickup_msg._itemName;
			pickupMessage = StartCoroutine(FadeText(txtPickup));
			return false;
		}

		// ========================================================================================================================
		// OnSave

		private bool ProcessGameSavedMessage(BaseMessage msg) {

			GameSavedMessage save_msg = msg as GameSavedMessage;

			txtSave.gameObject.SetActive (true);
			if(saveMessage != null) {
				StopCoroutine(saveMessage);
			}
			txtSave.color = Color.green;
			txtSave.text = ( save_msg._saved ? "Game saved." : "Can't save in this map!" );
			saveMessage = StartCoroutine(FadeText(txtSave));
			return false;
		}

		// ========================================================================================================================
		// OnDamage

		private void OnDamage(float speed) {
			StopCoroutine ("OnDamageCoroutine");
			StartCoroutine ("OnDamageCoroutine",speed);
		}

		// ========================================================================================================================
		// OnDamageCoroutine

		private IEnumerator OnDamageCoroutine(float speed) {
			damageFx.gameObject.SetActive (true);
			float percent = 1;

			while(percent > 0) {
				percent -= Time.deltaTime * speed;
				damageFx.canvasRenderer.SetAlpha(percent);
				yield return null;
			}
			damageFx.gameObject.SetActive (false);
			yield return null;
		}

//  hud <--
// ********************************************************************************************************************************
// menu ui -->

		// ========================================================================================================================
		// ProcessPlayerDeathMessage

		private bool ProcessPlayerDeathMessage (BaseMessage msg) {

			StartCoroutine(Fade(Color.clear, new Color(0,0,0,.85f), 1));
			hud.gameObject.SetActive (false);
			crosshair.gameObject.SetActive (false);
			gameOverUI.SetActive(true);
			isGameOver = true;
			currentMenu = MenuType.GameOver;

			return false;
		}

		// ========================================================================================================================
		// ProcessPlayerHealthChangeMessage

		private bool ProcessPlayerHealthChangeMessage (BaseMessage msg) {
			PlayerHealthChangeMessage health_msg = msg as PlayerHealthChangeMessage;
			OnHealthChanged (health_msg._damaged);
			return false;
		}

		// ========================================================================================================================
		// OnEscape

		public bool OnEscape() {

			bool escape = false;

			switch (currentMenu) {

			// Show pause UI
			case MenuType.None:
				fadePlane.color = new Color (0, 0, 0, .85f);
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				pauseUI.SetActive (true);
				currentMenu = MenuType.Pause;
				escape = true;
				break;

			// Return to game.
			case MenuType.Pause:
				fadePlane.color = Color.clear;
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
				pauseUI.SetActive(false);
				currentMenu = MenuType.None;
				escape = false;
				break;
			
			// Return to main menu.
			case MenuType.SaveProgress:
				// do nothing
				escape = true;
				break;

			// Return to pause menu.
			case MenuType.Settings:
				settingsUI.SetActive(false);
				pauseUI.SetActive(true);
				currentMenu = MenuType.Pause;
				escape = true;
				break;
			}
			return escape;

		}

		// ========================================================================================================================
		// ProcessLevelEndMessage

		private bool ProcessLevelEndMessage (BaseMessage msg) {
			txtLevelComplete.gameObject.SetActive (true);
			StartCoroutine (ShowEndLevelMenu ());
			return false;
		}

		// ========================================================================================================================
		// ShowNextLevelUI

		private IEnumerator ShowEndLevelMenu() {
			yield return new WaitForSeconds (2);
			StartCoroutine(Fade(Color.clear, new Color(0,0,0,.85f), 1));
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			nextLevelUI.SetActive (true);
			currentMenu = MenuType.NextLevel;
		}

		// ========================================================================================================================
		// ConfirmSave

		// UI input - called after clicking on 'MENU' button.
		public void ConfirmSave() {
			if (!isGameOver && SceneManager.GetActiveScene().name !="FiringRange" ) {
				gameOverUI.SetActive(false);
				nextLevelUI.SetActive(false);
				pauseUI.SetActive(false);
				saveProgressUI.SetActive(true);
				currentMenu = MenuType.SaveProgress;
			} else {
				ReturnToMenu();
			}

		}

//  menu ui <--
// ********************************************************************************************************************************
//  UI input -->

		// ========================================================================================================================
		// StartNewGame

	    public void StartNewGame() {
	        SaveLoadData.Instance.LoadNewScene(SceneManager.GetActiveScene().buildIndex);
	    }

		// ========================================================================================================================
		// NextLevel

		public void NextLevel() {
			SaveLoadData.Instance.LoadNewScene(nextSceneIndex);
		}

		// ========================================================================================================================
		// Resume

		public void Resume() {
			OnEscape ();
			Time.timeScale = 1;
		}

		// ========================================================================================================================
		// SaveProgress

	    public void SaveProgress() {
			SaveLoadData.Instance.SaveGame();
	        ReturnToMenu();
	    }

		// ========================================================================================================================
		// ReturnToMenu

		public void ReturnToMenu() {
			SaveLoadData.Instance.LoadScene("Menu");
		}

		// ========================================================================================================================
		// LoadLastSavedGame

	    public void LoadLastSavedGame() {
			if (SaveLoadData.Instance.CanLoadScene ()) {
				SaveLoadData.Instance.LoadSceneFromMenu ();
			} else {
				string msg = "Load file not found! Save game first.";
				StopCoroutine ("InfoMessageShow");
				StartCoroutine("InfoMessageShow", msg);
			}
	    }

		// ========================================================================================================================
		// OpenSettings

		public void OpenSettings() {
			pauseUI.SetActive (false);
			settingsUI.SetActive (true);
			currentMenu = MenuType.Settings;
		}

//  UI input <--
// ********************************************************************************************************************************
// util -->

		// ========================================================================================================================
		// InfoMessageShow

		private IEnumerator InfoMessageShow(string msg) {
			infoMsg.text = msg;
			infoMsg.gameObject.SetActive (true);
			yield return new WaitForSeconds (5);
			infoMsg.gameObject.SetActive (false);
		}

		// ========================================================================================================================
		// Fade

		private IEnumerator Fade(Color from, Color to, float time) {
			float speed = 1 / time;
			float percent = 0;

			while (percent < 1) {
				percent += Time.deltaTime * speed;
				fadePlane.color = Color.Lerp(from, to, percent);
				yield return null;
			}        
		}

		// ========================================================================================================================
		// FadeText

		private IEnumerator FadeText(Text text) {
			float speed = 2;
			float percent = 0;

			Color color = text.color;

			yield return new WaitForSeconds (5);

			while (percent < 1) {
				percent += Time.deltaTime * speed;
				text.color = Color.Lerp(color, Color.clear, percent);
				yield return null;
			}
			text.gameObject.SetActive (false);
		}

		// ========================================================================================================================
		// FadeTextMapName

		private IEnumerator FadeTextMapName() {
			float speed = 1;
			float percent = 0;

			txtMapName.color = Color.clear;
			txtMapName.gameObject.SetActive (true);
			yield return new WaitForSeconds (1);

			while (percent < 1) {
				percent += Time.deltaTime * speed;
				txtMapName.color = Color.Lerp(Color.clear, Color.green, percent);
				yield return null;
			}
			percent = 0;
			yield return new WaitForSeconds (3);

			while (percent < 1) {
				percent += Time.deltaTime * speed;
				txtMapName.color = Color.Lerp(Color.green, Color.clear, percent);
				yield return null;
			}
			txtMapName.gameObject.SetActive (false);

			// Show objective automatically at map start.
			OnObjective ();
		}

		// ========================================================================================================================
		// OnDestroy

		private void OnDestroy() {
			if (MessagingSystem.IsAlive) {
				MessagingSystem.Instance.DetachListener (typeof(PlayerDeathMessage), this.ProcessPlayerDeathMessage);
				MessagingSystem.Instance.DetachListener (typeof(PlayerHealthChangeMessage), this.ProcessPlayerHealthChangeMessage);
				MessagingSystem.Instance.DetachListener (typeof(EnemyDeathMessage), this.ProcessEnemyDeathMessage);
				MessagingSystem.Instance.DetachListener (typeof(LevelEndMessage), this.ProcessLevelEndMessage);
				MessagingSystem.Instance.DetachListener (typeof(AmmoQtyChangeMessage), this.ProcessAmmoQtyChangeMessage);
				MessagingSystem.Instance.DetachListener (typeof(GameSavedMessage), this.ProcessGameSavedMessage);
				MessagingSystem.Instance.DetachListener (typeof(PlayerPickupMessage), this.ProcessPlayerPickupMessage);
			}
		}

		// ========================================================================================================================

// util <--
// ********************************************************************************************************************************
	}
}