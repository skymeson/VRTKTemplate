using UnityEngine;
using System.Collections;
using DoomtrinityFPSPrototype.Utils;

namespace DoomtrinityFPSPrototype.Audio {

	public class AudioVolMessage : BaseMessage {
		
		public AudioVolMessage() {}
	}

	// Audio stuff.
	// Needs SoundLibrary script component.
	// The background music is played through a specific 'MusicSource' gameobject in the scene.
	[RequireComponent(typeof (SoundLibrary))]
	public class AudioManager : MonoBehaviour {

		public enum AudioChannel {Master,Sfx,Music};

		// ========================================================================================================================
		// Instance variables

		private SoundLibrary sndLib;

		public float masterVolumePercent { get; private set;}
		public float sfxVolumePercent { get; private set;}
		public float musicVolumePercent { get; private set;}
			    
	    // ========================================================================================================================
	    // Singleton stuff

		private static AudioManager _instance;
	    public static AudioManager Instance {
	        get {
	            return _instance;
	        }
	    }

		private void Awake() {

	        if (_instance != null && _instance != this) {
	            Destroy(this.gameObject);            
	        }
	        else {
	            _instance = this;
				DontDestroyOnLoad(this.gameObject); // Persist across scenes.
	        }
	    }

		// ========================================================================================================================
		// Start

		private void Start() {

			sndLib = GetComponent<SoundLibrary>();

	        // set value retrieved in saveloadsettings instance
			SaveSettings.AudioCfg_s audioCfg = SaveLoadSettings.Instance.gameSettings.audioCfg;
			masterVolumePercent = audioCfg.masterVol;
			musicVolumePercent = audioCfg.musicVol;
			sfxVolumePercent = audioCfg.sfxVol;
	    }

		// ========================================================================================================================
		// PlayMusic
		    
		public void PlayMusic(AudioSource musicSource) 
		{	
			musicSource.volume = musicVolumePercent * masterVolumePercent;
			musicSource.Play ();
		}

		// ========================================================================================================================
		// SetVolume

		public void SetVolume(float volumePercent, AudioChannel AudioChannel) {
			switch (AudioChannel) {
			case AudioChannel.Master:
				masterVolumePercent = volumePercent;
				break;
			case AudioChannel.Sfx:
				sfxVolumePercent = volumePercent;
				break;
			case AudioChannel.Music:
				musicVolumePercent = volumePercent;
				break;
			}

			// Fire the event so audio sources can update the volume.
			MessagingSystem.Instance.QueueMessage (new AudioVolMessage ());
		}

		// ========================================================================================================================
		// PlaySound

		public void PlaySound(AudioClip audioClip, Vector3 pos) {
			if (audioClip != null) {
				AudioSource.PlayClipAtPoint (audioClip, pos, sfxVolumePercent * masterVolumePercent);
			}
		}

		// Use this to pick a random sound from the array with a specified id, in the sound library.
		public void PlaySound(string sndName, Vector3 pos) {
			PlaySound (sndLib.GetAudioClipFromName (sndName), pos);
		}

		// ========================================================================================================================
		// GetSfxVolume

		// Actually used to get volume for first person controller script audio clips.
		public float GetSfxVolume() {
			return sfxVolumePercent * masterVolumePercent;
		}

		// ========================================================================================================================
	}
}