using System;
using System.Collections.Generic;
using System.Globalization;

namespace Mud.Server.Input
{
    // TODO:
    //public struct InputTrapState<TState>
    //{
    //    public bool PreserveInput { get; set; } // if true, input is not trimmed/lowered
    //    TState State { get; set; }
    //}

    //public abstract class InputTrapBase<TActor, TState> : StateMachineBase<TActor, string, InputTrapState<TState>>
    //{
    //    public override void ReceiveInput(TActor actor, string input)
    //    {
    //        if (!State.PreserveInput && input != null)
    //            input = input.Trim().ToLower(CultureInfo.InvariantCulture);
    //        Func<TActor, string, InputTrapState<TState>> func = StateMachine[State];
    //        InputTrapState<TState> newState = func(actor, input);
    //        State = newState;
    //    }
    //}

    public interface IInputTrap<in TActor>
    {
        bool IsFinalStateReached { get; }

        void ProcessInput(TActor actor, string input);
    }

    public abstract class InputTrapBase<TActor, TState> : IInputTrap<TActor>
    {
        protected Dictionary<TState, Func<TActor, string, TState>> StateMachine;

        public bool PreserveInput { get; protected set; }
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
            if (!PreserveInput && input != null)
                input = input.Trim().ToLower(CultureInfo.InvariantCulture);
            Func<TActor, string, TState> func = StateMachine[State];
            TState newState = func(actor, input);
            State = newState;
        }

        #endregion
    }
}
