namespace Footsies {
	public class StopState : IState {
		private const uint kMaxRoundWon = 3;
		
		public StateMachine StateMachine { get; set; }

		private readonly BattleCore battleCore;

		public StopState(BattleCore battleCore) {
			this.battleCore = battleCore;
		}
		
		public void OnEnter() {
			if (battleCore.Fighter1RoundWon >= kMaxRoundWon
			    || battleCore.Fighter2RoundWon >= kMaxRoundWon) {
				GameManager.Instance.LoadTitleScene();
			}
		}

		public void Tick() { 
			StateMachine.SetState<IntroState>();
		}

		public void OnExit() {
			throw new System.NotImplementedException();
		}
	}
}