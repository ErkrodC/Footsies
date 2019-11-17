using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Footsies {
	public class UIEventAction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler {
		public enum Action {
			LoadVsCPU,
			LoadVsPlayer,
			ExitGame,
			BGMToggle,
			SEToggle,
		}

		public Action action;

		private void Awake() {
			if (action == Action.BGMToggle) {
				Toggle toggle = gameObject.GetComponent<Toggle>();
				if (toggle != null) {
					toggle.isOn = SoundManager.Instance.IsBGMOn;
				}
			}
		}

		public void InvokeAction() {
			switch (action) {
				case Action.LoadVsCPU:
					LoadVsCPU();
					break;
				case Action.LoadVsPlayer:
					LoadVsPlayer();
					break;
				case Action.ExitGame:
					ExitGame();
					break;
				case Action.BGMToggle:
					ToggleBGM();
					break;
				case Action.SEToggle:
					break;
			}
		}

		public void LoadVsCPU() {
			GameManager.Instance.LoadVsCPUScene();
		}

		public void LoadVsPlayer() {
			GameManager.Instance.LoadVsPlayerScene();
		}

		public void ExitGame() {
			Application.Quit();
		}

		public void ToggleBGM() {
			bool isOn = SoundManager.Instance.ToggleBGM();
			Toggle toggle = gameObject.GetComponent<Toggle>();
			if (toggle != null) {
				toggle.isOn = isOn;
			}
		}

		public void OnPointerEnter(PointerEventData eventData) {
			EventSystem.current.SetSelectedGameObject(gameObject);
		}

		public void OnPointerDown(PointerEventData eventData) {
			EventSystem.current.SetSelectedGameObject(gameObject);
		}

		public void OnPointerExit(PointerEventData eventData) { }
	}
}