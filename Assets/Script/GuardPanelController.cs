using UnityEngine;

#pragma warning disable 649

namespace Footsies {
	/// <summary>
	/// Set guard sprite numbers with guard health from each player
	/// </summary>
	public class GuardPanelController : MonoBehaviour {
		[SerializeField] private GameObject _battleCoreGameObject;

		[SerializeField] private bool isPlayerOne;

		[SerializeField] private GameObject[] guardImageObjects;

		#region private field

		private BattleCore battleCore;

		private int currentGuardHealth;

		#endregion

		private void Awake() {
			if (_battleCoreGameObject != null) {
				battleCore = _battleCoreGameObject.GetComponent<BattleCore>();
			}

			currentGuardHealth = 0;
			UpdateGuardHealthImages();
		}

		// Update is called once per frame
		private void Update() {
			if (currentGuardHealth != GetGuardHealth()) {
				currentGuardHealth = GetGuardHealth();
				UpdateGuardHealthImages();
			}
		}

		private int GetGuardHealth() {
			if (battleCore == null) {
				return 0;
			}

			if (isPlayerOne) {
				return battleCore.Fighter1.GuardHealth;
			} else {
				return battleCore.Fighter2.GuardHealth;
			}
		}

		private void UpdateGuardHealthImages() {
			for (int i = 0; i < guardImageObjects.Length; i++) {
				if (i <= (int) currentGuardHealth - 1) {
					guardImageObjects[i].SetActive(true);
				} else {
					guardImageObjects[i].SetActive(false);
				}
			}
		}
	}
}