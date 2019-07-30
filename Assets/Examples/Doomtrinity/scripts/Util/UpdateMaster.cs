using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DoomtrinityFPSPrototype.InputManagement;
using DoomtrinityFPSPrototype.dtGUI;

namespace DoomtrinityFPSPrototype.Utils {

	public class EscapeMessage : BaseMessage {
		public EscapeMessage() {  }
	}

	public interface IUpdateable {
		void Think(float dt);
	}

	// This class manages the update behaviour for all classes that implement the 'IUpdateable' interface ( see 'UpdateableMonoBehaviour' ).
	// This has some advantages over classic MonoBehaviour update behaviour:
	// - it allows you to handle the update frequency - or disable the update! This is useful
	//   in many circumstances, for example for stopping an object that performs its move stuff inside 'Update';
	// - helps to keep the update behaviour inside managed code, avoiding the native-managed bridge
	//   that would take place with classic 'Update' callbacks. This allows better performance if you
	//   have hundreds ( or thousands ) of objects that must run update.
	public class UpdateMaster : SingletonAsComponent<UpdateMaster> {

		// ========================================================================================================================
		// Instance variables

		private bool resume = false;
		private bool isEscaping = false;

		private float _updateFrequency = 0; // 0 --> Update every frame.
		private float _timer;
		private List<IUpdateable> _updateableObjects = new List<IUpdateable>();

		// ========================================================================================================================
		// Properties

		public bool IsEscaping { get {return isEscaping; } }

		// ========================================================================================================================
		// Singleton stuff

		public static UpdateMaster Instance {
			get { return ((UpdateMaster)_Instance); }
			set { _Instance = value; }
		}

		// ========================================================================================================================
		// RegisterUpdateableObject

		public void RegisterUpdateableObject(IUpdateable obj) {
			if (!_updateableObjects.Contains(obj)) {
				_updateableObjects.Add(obj);
			}
		}

		// ========================================================================================================================
		// DeregisterUpdateableObject

		public void DeregisterUpdateableObject(IUpdateable obj) {
			if (_updateableObjects.Contains(obj)) {
				_updateableObjects.Remove(obj);
			}
		}

		// ========================================================================================================================
		// Update

		private void Update() {
			float dt = Time.deltaTime;

			_timer += dt;
			if (_timer > _updateFrequency) {
				_timer -= _updateFrequency;
				for(int i = 0; i < _updateableObjects.Count; ++i) {
					_updateableObjects[i].Think(dt);
				}
			}

			// Call after all Think have been excecuted.
			if ( InputManager.GetKeyDown(ActionCode.Escape) || resume ) {

				resume = false;
				bool isEscaping_tmp = GameUI.Instance.OnEscape();

				if ( isEscaping != isEscaping_tmp ) {
					isEscaping = isEscaping_tmp;
					Time.timeScale = isEscaping ? 0 : 1;

					_timer = isEscaping ? -1 : 0;

					MessagingSystem.Instance.QueueMessage (new EscapeMessage ());
				}

			}

			// Handle resume if done by button
			if (!resume && isEscaping && Time.timeScale == 1) { // Hack...Use timescale to detect if resumed by game ui.
				resume = true;
			}
		}

		// ========================================================================================================================	
	}
}