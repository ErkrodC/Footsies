using UnityEngine;
using UnityEngine.SceneManagement;

namespace Footsies {
	public class GameManager : Singleton<GameManager> {
		#region constants

		private const int kTitleSceneIndex = 1;
		private const int kBattleSceneIndex = 2;

		#endregion

		public AudioClip menuSelectAudioClip;

		public bool IsVsCPU { get; private set; }
		private int currentScene;

		private void Awake() {
			DontDestroyOnLoad(gameObject);

			Application.targetFrameRate = 60;
		}

		private void Start() {
			LoadTitleScene();
		}

		private void Update() {
			if (currentScene == kBattleSceneIndex) {
				if (Input.GetButtonDown("Cancel")) {
					LoadTitleScene();
				}
			}
		}

		public void LoadTitleScene() {
			SceneManager.LoadScene(kTitleSceneIndex);
			currentScene = kTitleSceneIndex;
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
			SceneManager.LoadScene(kBattleSceneIndex);
			currentScene = kBattleSceneIndex;

			if (menuSelectAudioClip != null) {
				SoundManager.Instance.PlaySE(menuSelectAudioClip);
			}
		}
	}
}