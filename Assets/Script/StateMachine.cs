using System;
using System.Collections.Generic;

namespace Footsies {
    public class StateMachine {
        // TODO is this needed?
        private static readonly List<StateChangedStackItem> onStateChangedStack = new List<StateChangedStackItem>();
        
        public event Action<State> OnStateChanged = delegate{};

        private State currentState;
        private State nextState;
        private bool containsVisualStates;
        private readonly Dictionary<Type, State> states = new Dictionary<Type, State>();
        
        public void Tick() {
            HandleStateTransition();
            currentState?.Tick();
        }

        public void AddState(State state) {
            Type stateType = state.GetType();
            state.StateMachine = this;
            
            if(states.ContainsKey(stateType)) {
                Globals.Logger.LogError($"IState of type {stateType} already exists.");
                return;
            }
            
            states.Add(stateType, state);
        }

        public T GetState<T>() where T : State {
            Type stateType = typeof(T);

            if (states.TryGetValue(stateType, out State existingState)) {
                return existingState as T;
            }

            Globals.Logger.LogError($"IState of type {stateType} does not exist.");
            return null;

        }

        public void SetState<T>(bool immediate = true) {
            Type stateType = typeof(T);
            if(!states.TryGetValue(stateType, out nextState)) {
                Globals.Logger.LogError($"Unable to transition to state {stateType}, state not found.");
            }

            if (immediate) {
                HandleStateTransition();
            }
        }

        public void ExitCurrentState() {
            nextState = null;
            
            if (currentState != null) {
                Globals.Logger.Log($"Exiting state {currentState}");
                currentState.OnExit();
                currentState = null;
                OnStateChanged(null);
            }
        }

        private void FlushOnStateChangedStack() {
            while(onStateChangedStack.Count > 0) {
                Action<State> onStateChanged = onStateChangedStack[0].OnStateChanged;
                State stackState = onStateChangedStack[0].State;
                onStateChangedStack.RemoveAt(0);
                onStateChanged(stackState);
            }
        }

        private void HandleStateTransition() {
            if (nextState == null) { return; }

            if(currentState != null) {
                Globals.Logger.Log($"Exiting state {currentState}");
                currentState.OnExit();
                currentState = null;
                OnStateChanged(null);
            }

            currentState = nextState;
            onStateChangedStack.Add(new StateChangedStackItem(OnStateChanged, nextState));
            nextState = null;

            if(currentState != null) {
                Globals.Logger.Log($"Transitioning to state {currentState}");
                currentState.OnEnter();
            }

            FlushOnStateChangedStack();
        }
        
        private class StateChangedStackItem {
            public readonly Action<State> OnStateChanged;
            public readonly State State;
            
            public StateChangedStackItem(Action<State> onStateChanged, State state) {
                OnStateChanged = onStateChanged;
                State = state;
            }
        }
    }
}