using UnityEngine;
using System;
using System.Collections;

namespace DoomtrinityFPSPrototype.Maps {
	// This class holds map definitions, like map name and objective.
	// It must me attached to the GameController prefab.
	public class MapDefs : MonoBehaviour {

		// ========================================================================================================================
		// Instance variables

		[SerializeField] private string mapName = "";
		[SerializeField] private ObjectiveType objective = ObjectiveType.KillEmAll;
		[SerializeField] private int nextMapIndex = 0;

		// ========================================================================================================================
		// Properties

		public string MapName { 
			get { 
				if(!String.IsNullOrEmpty(mapName)) {
					return "\"" + mapName + "\"";
				}
				return "";
			} 
		}
		public ObjectiveType Objective { get { return objective; } }
		public int NextMapIndex { get { return nextMapIndex; } }

		// ========================================================================================================================
	}
}