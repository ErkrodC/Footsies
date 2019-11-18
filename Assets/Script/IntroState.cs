using System.Collections.Generic;
using UnityEngine;

namespace Footsies {
	public class IntroState : IState {
		private const float kIntroStateTime = 3f;
		private static readonly int roundStart = Animator.StringToHash("RoundStart");
		
		public StateMachine StateMachine { get; set; }

		private readonly BattleCore battleCore;
		
		public IntroState(BattleCore battleCore) {
			this.battleCore = battleCore;
		}
		
		public void OnEnter() {
			battleCore.Fighter1.SetupBattleStart(battleCore.FighterDataList[0], new Vector2(-2f, 0f), true);
			battleCore.Fighter2.SetupBattleStart(battleCore.FighterDataList[0], new Vector2(2f, 0f), false);

			battleCore.Timer = kIntroStateTime;

			battleCore.RoundUIAnimator.SetTrigger(roundStart);
		}

		public void Tick() {
			InputData p1Input = battleCore.GetP1InputData();
			InputData p2Input = battleCore.GetP2InputData();
			battleCore.RecordInput(p1Input, p2Input);
			battleCore.Fighter1.UpdateInput(p1Input);
			battleCore.Fighter2.UpdateInput(p2Input);

			foreach (Fighter fighter in battleCore.Fighters) {
				fighter.IncrementActionFrame();
				fighter.UpdateIntroAction();
				fighter.UpdateMovement();
				fighter.UpdateBoxes();
			}

			battleCore.UpdatePushCharacterVsCharacter();
			battleCore.UpdatePushCharacterVsBackground();

			battleCore.Timer -= Time.deltaTime;
			if (battleCore.Timer <= 0f) {
				StateMachine.SetState<FightState>();
			}

			if (battleCore.debugPlayLastRoundInput
			    && !battleCore.IsReplayingLastRoundInput) {
				StartPlayLastRoundInput();
			}
		}

		public void OnExit() {
			throw new System.NotImplementedException();
		}
		
		private void StartPlayLastRoundInput() {
			battleCore.IsReplayingLastRoundInput = true;
			battleCore.CurrentReplayingInputIndex = 0;
		}
	}
}