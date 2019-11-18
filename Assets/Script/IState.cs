namespace Footsies {
	public interface IState {
		StateMachine StateMachine { get; set;  }
		void OnEnter();
		void Tick();
		void OnExit();
	}
}