namespace Footsies {
	public class StopState : State {
		private const uint kMaxRoundWon = 3;

		private readonly BattleCore battleCore;

		public StopState(BattleCore battleCore) {
			this.battleCore = battleCore;
		}
		
		public override void OnEnter() {
			if (battleCore.Fighter1RoundWon >= kMaxRoundWon
			    || battleCore.Fighter2RoundWon >= kMaxRoundWon) {
				GameManager.Instance.LoadTitleScene();
			}
			
			StateMachine.SetState<IntroState>();
		}
	}
}