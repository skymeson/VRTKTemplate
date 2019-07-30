using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DoomtrinityFPSPrototype.Audio {
// A library of sounds. This  script must be attached to the audio manager gameobject.
// Drag here any sound you'd like to use.
// Actually, some sounds are managed by weapon and first person controller scripts.
public class SoundLibrary : MonoBehaviour {

	// ========================================================================================================================
	// Instance variables

	[SerializeField] private SoundPrefix[] sndPrefixes;

	private Dictionary<string,AudioClip[]> groupDict = new Dictionary<string,AudioClip[]> ();

	// ========================================================================================================================
	// Awake

	private void Awake() {
		foreach (SoundPrefix sndPrefix in sndPrefixes) {
			groupDict.Add (sndPrefix.prefixID, sndPrefix.group);
		}
	}

	// ========================================================================================================================
	// GetAudioClipFromName

	// Pick up a random clip with a specified id.
	// For example, "impact" will return a random impact sound from the array placed under this prefix.
	public AudioClip GetAudioClipFromName(string name){
		if ( groupDict.ContainsKey (name) ) {
			AudioClip[] sounds = groupDict [name];
			return sounds [Random.Range (0, sounds.Length)];
		}
		return null;
	}

	// ========================================================================================================================
	// nested class SoundPrefix

	[System.Serializable]
	public class SoundPrefix {
		public string prefixID;
		public AudioClip[] group;
	}

	// ========================================================================================================================
}
}