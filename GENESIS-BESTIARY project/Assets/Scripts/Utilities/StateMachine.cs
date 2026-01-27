using System.Collections.Generic;

namespace GenesisBestiary.Utilities
{
    public interface IState
    {
        void Enter();
        void Tick();
        void Exit();
    }

    public abstract class StateMachine<TState>
    {
        protected readonly Dictionary<TState, IState> States = new Dictionary<TState, IState>();

        public IState CurrentState { get; private set; }
        public TState CurrentKey { get; private set; }

        public void Tick()
        {
            CurrentState?.Tick();
        }

        protected void ChangeState(TState key)
        {
            if (!States.TryGetValue(key, out var nextState))
            {
                return;
            }

            if (ReferenceEquals(CurrentState, nextState))
            {
                return;
            }

            CurrentState?.Exit();
            CurrentState = nextState;
            CurrentKey = key;
            CurrentState.Enter();
        }
    }
}
