using UnityEngine;
using UnityEngine.SceneManagement;

namespace Footsies {
	public class GameManager : Singleton<GameManager> {
		public enum SceneIndex {
			Title = 1,
			Battle = 2,
		}

		public AudioClip menuSelectAudioClip;

		public SceneIndex CurrentScene { get; private set; }
		public bool IsVsCPU { get; private set; }

		private void Awake() {
			DontDestroyOnLoad(gameObject);

			Application.targetFrameRate = 60;
		}

		private void Start() {
			LoadTitleScene();
		}

		private void Update() {
			if (CurrentScene == SceneIndex.Battle) {
				if (Input.GetButtonDown("Cancel")) {
					LoadTitleScene();
				}
			}
		}

		public void LoadTitleScene() {
			SceneManager.LoadScene((int) SceneIndex.Title);
			CurrentScene = SceneIndex.Title;
		}

		public void LoadVsPlayerScene() {
			IsVsCPU = false;
			LoadBattleScene();
		}

		public void LoadVsCPUScene() {
			IsVsCPU = true;
			LoadBattleScene();
		}

		private void LoadBattleScene() {
			SceneManager.LoadScene((int) SceneIndex.Battle);
			CurrentScene = SceneIndex.Battle;

			if (menuSelectAudioClip != null) {
				SoundManager.Instance.PlaySE(menuSelectAudioClip);
			}
		}
	}
}