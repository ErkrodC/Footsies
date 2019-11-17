using UnityEngine;

namespace Footsies {
	public class SoundManager : Singleton<SoundManager> {
		public GameObject seSourceObject1;
		public GameObject seSourceObject2;
		public GameObject bgmSourceObject;

		[Range(0.0f, 1.0f)] public float masterVolume = 1f;

		private AudioSource seSource1;
		private AudioSource seSource2;
		private AudioSource bgmSource;

		private float defaultBGMVolume;
		public bool IsBGMOn { get; private set; }

		private void Awake() {
			DontDestroyOnLoad(this);

			seSource1 = seSourceObject1.GetComponent<AudioSource>();
			seSource2 = seSourceObject2.GetComponent<AudioSource>();
			bgmSource = bgmSourceObject.GetComponent<AudioSource>();
			defaultBGMVolume = bgmSource.volume;
			IsBGMOn = true;
		}

		public bool ToggleBGM() {
			if (IsBGMOn) {
				bgmSource.volume = 0;
				IsBGMOn = false;
			} else {
				bgmSource.volume = defaultBGMVolume;
				IsBGMOn = true;
			}

			return IsBGMOn;
		}


		public void PlaySE(AudioClip clip) {
			seSource1.clip = clip;
			seSource1.panStereo = 0;
			seSource1.Play();
		}

		public void PlayFighterSE(AudioClip clip, bool isPlayerOne, float posX) {
			AudioSource audioSource = seSource1;
			if (!isPlayerOne) {
				audioSource = seSource2;
			}

			audioSource.clip = clip;
			audioSource.panStereo = posX / 5;
			audioSource.Play();
		}
	}
}