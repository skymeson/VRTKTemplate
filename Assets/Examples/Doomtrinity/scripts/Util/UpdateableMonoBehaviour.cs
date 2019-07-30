using UnityEngine;
using System.Collections;

namespace DoomtrinityFPSPrototype.Utils {

	// Derive from this class just like you would do with 'MonoBehaviour'.
	// This class has some advantages over classic MonoBehaviour, take a look at description in 'UpdateMaster' script.
	// IMPORTANT! If your class derives from this, you must use the custom methods below, DO NOT use the "matching" Unity's callback!
	// - 'Think' 		--> Unity's 'Update'
	// - 'Begin'  		--> Unity's 'Start'
	// - 'OnActivate' 	--> Unity's 'OnEnable'
	// - 'OnKill' 		--> Unity's 'OnDisable'
	public class UpdateableMonoBehaviour : MonoBehaviour, IUpdateable {

		// =================================================================================
		// Instance variables

		// The master update object will check this flag to choose between 'OnEnable' or 'Start'.
		// Theoretically, we could just call 'OnEnable' only, but most scripts do critical initialization in
		// 'Start', so we should prevent to call the update ( 'Think' ) function on these scripts
		// until start ( 'Begin' ) is finished. We need 'OnEnable' cause it's called when the gameobject
		// returns active.
		// Note that 'Start' is called only once, while 'OnEnable' is called each time the object is activated.
		// This is needed for prefab pool system.
		private bool started = false;

		// =================================================================================
		// Start

		// Called by Unity
		private void Start () {
			if (UpdateMaster.Instance != null) {
				Begin();
				UpdateMaster.Instance.RegisterUpdateableObject(this);
				started = true;
			}
		}

		// =================================================================================
		// OnEnable

		// Called by Unity
		private void OnEnable () {
			if (UpdateMaster.Instance != null && started ) {
				UpdateMaster.Instance.RegisterUpdateableObject(this);
				OnActivate ();
			}
		}

		// =================================================================================
		// OnDestroy

		// Called by Unity
		private void OnDisable() {
			if (UpdateMaster.IsAlive) {
				UpdateMaster.Instance.DeregisterUpdateableObject(this);
			}
			OnKill ();
		}

		// =================================================================================
		// Begin

		protected virtual void Begin() {
			// derived classes should override this method for initialization code
			// to avoid replacing the Start() function accidentally
		}

		// =================================================================================
		// OnActivate

		protected virtual void OnActivate() {
			// derived classes should override this method for enable code
			// to avoid replacing the OnEnable() function accidentally
		}

		// =================================================================================
		// Think

		public virtual void Think (float dt) {
			// derived classes should override this method for update code, 
			// replacing the method Update() called by Unity on MonoBehaviour.
		}

		// =================================================================================
		// OnKill

		protected virtual void OnKill() { 
			// derived classes should override this method for cleanup code
			// to avoid replacing the OnDisable() function accidentally
		}

		// =================================================================================
	}
}
