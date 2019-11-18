using System.Collections.Generic;
using UnityEngine;

namespace Footsies {
	public class EndState : IState {
		private const float kEndStateTime = 3f;
		private const float kEndStateSkippableTime = 1.5f;
		
		public StateMachine StateMachine { get; set; }

		private readonly BattleCore battleCore;

		public EndState(BattleCore battleCore) {
			this.battleCore = battleCore;
		}

		public void OnEnter() {
			battleCore.Timer = kEndStateTime;

			List<Fighter> deadFighter = battleCore.Fighters.FindAll((f) => f.IsDead);
			if (deadFighter.Count == 1) {
				if (deadFighter[0] == battleCore.Fighter1) {
					battleCore.Fighter2RoundWon++;
					battleCore.Fighter2.RequestWinAction();
				} else if (deadFighter[0] == battleCore.Fighter2) {
					battleCore.Fighter1RoundWon++;
					battleCore.Fighter1.RequestWinAction();
				}
			}
		}

		public void Tick() {
			UpdateEndState();

			battleCore.Timer -= Time.deltaTime;
			if (battleCore.Timer <= 0f || battleCore.Timer <= kEndStateSkippableTime && IsKOSkipButtonPressed()) {
				StateMachine.SetState<StopState>();
			}
		}

		public void OnExit() {
			throw new System.NotImplementedException();
		}

		private static bool IsKOSkipButtonPressed() {
			return InputManager.Instance.GetButton(InputManager.Command.P1Attack)
			       || InputManager.Instance.GetButton(InputManager.Command.P2Attack);
		}

		private void UpdateEndState() {
			foreach (Fighter fighter in battleCore.Fighters) {
				fighter.IncrementActionFrame();
				fighter.UpdateActionRequest();
				fighter.UpdateMovement();
				fighter.UpdateBoxes();
			}

			battleCore.UpdatePushCharacterVsCharacter();
			battleCore.UpdatePushCharacterVsBackground();
		}
	}
}