using UnityEngine;
using System.Collections;

namespace DoomtrinityFPSPrototype.Utils {

	public class SingletonAsComponent<T> : MonoBehaviour where T : SingletonAsComponent<T> {

		// Derive from this class if the gameobject is basically an empty "singleton" object that must be in the scene,
		// but it does not require any custom stuff in its inspector ( in other words, if the gameobject only needs the derived
		// class script component attached, and it must be a singleton ).
		// This allows us not to care about creating ad-hoc gameobjects in the hierarchy including only the singleton script component,
		// since these gameobjects are created automatically at first call to 'Instance' ( lazy initialization ).
		// To avoid lazy initialization in critical game actions, get the 'Instance' at game start ( see 'LevelInitializer' ).

		// ========================================================================================================================
		// Instance variables

		private static T __instance;
		private bool _alive = true;

		// ========================================================================================================================
		// Singleton stuff

		protected static SingletonAsComponent<T> _Instance {
			get {
				// DO NOT place persistant objects ( DontDestroyOnLoad ) directly in the Hierarchy!
				// If you have this object in both current and next scene, there will be 2 objects of this type in the scene!
				// That happens cause this check will be skipped. You could theoretically enhance this check, but if you pay
				// attention to this rule, you can leave it as is.
				if(!__instance) {
					T [] managers = GameObject.FindObjectsOfType(typeof(T)) as T[];
					if (managers != null) {
						if(managers.Length == 1) {
							__instance = managers[0];
							return __instance;
						} else if (managers.Length > 1) {
							Debug.LogWarning("You have more than one " + typeof(T).Name + " in the scene. You only need 1, it's a singleton! Destroying duplicates...");
							for(int i = 0; i < managers.Length; ++i) {
								T manager = managers[i];		
								Destroy(manager.gameObject);
							}
						}					
					}

					GameObject go = new GameObject(typeof(T).Name, typeof(T));
					__instance = go.GetComponent<T>();
					// DontDestroyOnLoad(__Instance.gameObject); // Eventually done in 'Awake' method of the derived class.
				}
				return __instance;
			} 
			set {
				__instance = value as T;
			}
		}

		// ========================================================================================================================
		// IsAlive

		// A call to Instance in other object's destruction, e.g. something like --> if( UpdateMaster.Instance != null ) DoStuff();
		// will cause a new instantiation of the singleton.
		// That is wrong, and this message will be thrown by Unity:
		// Some objects were not cleaned up when closing the scene. (Did you spawn new GameObjects from OnDestroy?)
		// To avoid this, we must check if the object is alive first, by calling its 'IsAlive' property. 
		public static bool IsAlive {
			get { 
				if (__instance == null)
					return false;
				return __instance._alive; 
			}
		}

		// ========================================================================================================================
		// OnDestroy

		void OnDestroy() {
			_alive = false;
		}

		// ========================================================================================================================
		// OnApplicationQuit

		void OnApplicationQuit() {
			_alive = false;
		}

		// ========================================================================================================================
	}
}