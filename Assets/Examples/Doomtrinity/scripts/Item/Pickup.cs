using UnityEngine;
using System.Collections;
using DoomtrinityFPSPrototype.Utils;

namespace DoomtrinityFPSPrototype.Item {
public abstract class Pickup : PersistantMono {

	// ========================================================================================================================
	// Save and Restore

	public abstract override DATA_TYPE Save(SaveLoadData.PersistantData savedata); // Implement this behaviour in derived classes!
	public abstract override void Restore(SaveLoadData.PersistantData savedata); // Implement this behaviour in derived classes!	

	// ========================================================================================================================
}
}