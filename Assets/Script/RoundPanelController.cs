﻿using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 649

namespace Footsies {
	/// <summary>
	/// Set round sprite with number of win from each player
	/// </summary>
	public class RoundPanelController : MonoBehaviour {
		[SerializeField] private GameObject _battleCoreGameObject;

		[SerializeField] private Sprite spriteEmpty;

		[SerializeField] private Sprite spriteWon;

		[SerializeField] private bool isPlayerOne;

		[SerializeField] private GameObject[] roundWonImageObjects;

		#region private field

		private BattleCore battleCore;
		private Image[] roundWonImages;

		private uint currentRoundWon;

		#endregion

		private void Awake() {
			if (_battleCoreGameObject != null) {
				battleCore = _battleCoreGameObject.GetComponent<BattleCore>();
			}

			roundWonImages = new Image[roundWonImageObjects.Length];
			for (int i = 0; i < roundWonImageObjects.Length; i++) {
				roundWonImages[i] = roundWonImageObjects[i].GetComponent<Image>();
			}

			currentRoundWon = 0;
			UpdateRoundWonImages();
		}

		// Update is called once per frame
		private void Update() {
			if (currentRoundWon != GetRoundWon()) {
				currentRoundWon = GetRoundWon();
				UpdateRoundWonImages();
			}
		}

		private uint GetRoundWon() {
			if (battleCore == null) {
				return 0;
			}

			if (isPlayerOne) {
				return battleCore.Fighter1RoundWon;
			} else {
				return battleCore.Fighter2RoundWon;
			}
		}

		private void UpdateRoundWonImages() {
			for (int i = 0; i < roundWonImages.Length; i++) {
				if (i <= (int) currentRoundWon - 1) {
					roundWonImages[i].sprite = spriteWon;
				} else {
					roundWonImages[i].sprite = spriteEmpty;
				}
			}
		}
	}
}