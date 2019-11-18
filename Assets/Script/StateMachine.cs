using System;
using System.Collections.Generic;

namespace Footsies {
    public class StateMachine {
        // TODO is this needed?
        private static readonly List<StateChangedStackItem> onStateChangedStack = new List<StateChangedStackItem>();
        
        public event Action<IState> OnStateChanged = delegate{};

        private IState currentState;
        private IState nextState;
        private bool containsVisualStates;
        private readonly Dictionary<Type, IState> states = new Dictionary<Type, IState>();
        
        public void Tick() {
            HandleStateTransition();
            currentState?.Tick();
        }

        public void AddState(IState state) {
            Type stateType = state.GetType();
            
            if(states.ContainsKey(stateType)) {
                Globals.Logger.LogError($"IState of type {stateType} already exists.");
                return;
            }
            
            states.Add(stateType, state);
        }

        public T GetState<T>() where T : class, IState {
            Type stateType = typeof(T);

            if (states.TryGetValue(stateType, out IState existingState)) {
                return existingState as T;
            }

            Globals.Logger.LogError($"IState of type {stateType} does not exist.");
            return null;

        }

        public void SetState<T>() {
            Type stateType = typeof(T);
            if(!states.TryGetValue(stateType, out nextState)) {
                Globals.Logger.LogError($"Unable to transition to state {stateType}, state not found.");
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
                Action<IState> onStateChanged = onStateChangedStack[0].OnStateChanged;
                IState stackState = onStateChangedStack[0].State;
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
            public readonly Action<IState> OnStateChanged;
            public readonly IState State;
            
            public StateChangedStackItem(Action<IState> onStateChanged, IState state) {
                OnStateChanged = onStateChanged;
                State = state;
            }
        }
    }
}