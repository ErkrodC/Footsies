using UnityEngine;

namespace Footsies {
	public class KOState : State {
		private const float kKOStateTime = 2f;
		private static readonly int roundEnd = Animator.StringToHash("RoundEnd");

		private readonly BattleCore battleCore;

		public KOState(BattleCore battleCore) {
			this.battleCore = battleCore;
		}
		
		public override void OnEnter() {
			battleCore.Timer = kKOStateTime;

			CopyLastRoundInput();

			battleCore.Fighter1.ClearInput();
			battleCore.Fighter2.ClearInput();

			battleCore.BattleAI = null;

			battleCore.RoundUIAnimator.SetTrigger(roundEnd);
		}

		public override void Tick() {
			battleCore.Timer -= Time.deltaTime;
			if (battleCore.Timer <= 0f) {
				StateMachine.SetState<EndState>();
			}
		}
		
		private void CopyLastRoundInput() {
			for (int i = 0; i < battleCore.CurrentRecordingInputIndex; i++) {
				battleCore.LastRoundP1Input[i] = battleCore.RecordingP1Input[i].ShallowCopy();
				battleCore.LastRoundP2Input[i] = battleCore.RecordingP2Input[i].ShallowCopy();
			}

			battleCore.LastRoundMaxRecordingInput = battleCore.CurrentRecordingInputIndex;

			battleCore.IsReplayingLastRoundInput = false;
			battleCore.CurrentReplayingInputIndex = 0;
		}
	}
}