using System;
using System.Collections.Generic;
using System.Globalization;

namespace Mud.Server.Common
{
    public interface IInputTrap<in TActor>
    {
        bool IsFinalStateReached { get; }

        void ProcessInput(TActor actor, string input);
    }

    public abstract class InputTrapBase<TActor, TState> : IInputTrap<TActor>
    {
        protected Dictionary<TState, Func<TActor, string, TState>> StateMachine;

        public bool KeepInputAsIs { get; protected set; }
        public TState State { get; protected set; }

        protected InputTrapBase()
        {
            StateMachine = new Dictionary<TState, Func<TActor, string, TState>>();
        }

        protected virtual void AddStage(TState state, Func<TActor, string, TState> func)
        {
            StateMachine[state] = func;
        }

        #region IInputTrap

        public abstract bool IsFinalStateReached { get; }

        public virtual void ProcessInput(TActor actor, string input)
        {
            // Lower and trim if needed
            if (!KeepInputAsIs)
                input = input?.Trim().ToLower(CultureInfo.InvariantCulture);
            Func<TActor, string, TState> func = StateMachine[State];
            TState newState = func(actor, input);
            State = newState;
        }

        #endregion
    }
}
