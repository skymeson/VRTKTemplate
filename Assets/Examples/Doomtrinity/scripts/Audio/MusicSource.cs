using UnityEngine;
using System.Collections;
using DoomtrinityFPSPrototype.Utils;

namespace DoomtrinityFPSPrototype.Audio {

	// Simple script to play a specific theme in the current scene.
	[RequireComponent(typeof (AudioSource))]
	public class MusicSource : MonoBehaviour {

		// ========================================================================================================================
		// Instance variables
		private AudioSource musicSource;

		// ========================================================================================================================
		// Start

		private void Start () {
			if (AudioManager.Instance != null) {
				musicSource = GetComponent<AudioSource> ();

				MessagingSystem.Instance.AttachListener (typeof(AudioVolMessage), this.ProcessAudioVolMessage);
				AudioManager.Instance.PlayMusic (musicSource);
			} else {
				Debug.LogError ("MusicSource.Start: AudioManager is missing!");
			}
		}

		// ========================================================================================================================
		// ProcessAudioVolMessage

		private bool ProcessAudioVolMessage(BaseMessage msg) {
			if (AudioManager.Instance != null) {
				musicSource.volume = AudioManager.Instance.masterVolumePercent * AudioManager.Instance.musicVolumePercent;
			}
			return false;
		}

		// ========================================================================================================================
		// OnDestroy

		// Important! Remember to usubscribe methods since AudioManager is a singleton and the game would throw
		// an exception trying to call a method of a destroyed object, when changing scene.
		private void OnDestroy() {
			if (MessagingSystem.IsAlive) {
				MessagingSystem.Instance.DetachListener (typeof(AudioVolMessage), this.ProcessAudioVolMessage);
			}
		}

		// ========================================================================================================================
	}
}