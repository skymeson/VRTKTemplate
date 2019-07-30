using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DoomtrinityFPSPrototype.Utils {
	// DATA_TYPE is actually unused. It was used before as returned value from Save() of specific scripts, to get the proper save struct.
	public enum DATA_TYPE
	{
	    PLAYER,
	    INVENTORY,
	    ENEMY,
		PICKUP
	}

	public interface IPersistant
	{
		DATA_TYPE Save (SaveLoadData.PersistantData savedata);
		void Restore (SaveLoadData.PersistantData savedata);
	}
}