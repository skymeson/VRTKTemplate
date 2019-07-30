using UnityEngine;

namespace DoomtrinityFPSPrototype.Utils {
	
	public static class LazySingletonInitializer {

		// ========================================================================================================================
		// InitializeLazySingleton

		// Add in this method any singleton component that uses lazy initialization, to avoid initialization in the middle of the game.
		[RuntimeInitializeOnLoadMethod]
		public static void InitializeLazySingleton () {
			Object obj = null; // Using for assignment only so we can wake up instances with Singleton.Instance.
			obj = UpdateMaster.Instance;
			obj = MessagingSystem.Instance;
			obj = SaveLoadData.Instance;
			obj = SaveLoadSettings.Instance;
			obj = null;
			// Debug.Log ("LazySingletonInitializer.InitializeLazySingleton");
		}

		// ========================================================================================================================
	}
}