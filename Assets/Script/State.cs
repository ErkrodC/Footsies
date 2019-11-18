namespace Footsies {
	public abstract class State {
		public StateMachine StateMachine { get; set; }
		public virtual void OnEnter() { }
		public virtual void Tick() { }
		public virtual void OnExit() { }
	}
}