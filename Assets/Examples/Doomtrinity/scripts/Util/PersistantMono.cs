using UnityEngine;
using System.Collections;

namespace DoomtrinityFPSPrototype.Utils {
	// Derive from this class for any object that shoud be saved in external xml file.
	// See how enemies use this system.
	public abstract class PersistantMono : UpdateableMonoBehaviour, IPersistant {

		public abstract DATA_TYPE Save (SaveLoadData.PersistantData savedata);
		public abstract void Restore(SaveLoadData.PersistantData savedata);    
	}
}