using System;
using System.Collections.Generic;

namespace Mud.DataStructures
{
    public abstract class StateMachineBase<TActor, TData, TState>
    {
        protected Dictionary<TState, Func<TActor, TData, TState>> StateMachine;

        public TState State { get; protected set; }

        protected StateMachineBase()
        {
            StateMachine = new Dictionary<TState, Func<TActor, TData, TState>>();
        }

        public virtual void ProcessStage(TActor actor, TData data)
        {
            Func<TActor, TData, TState> func = StateMachine[State];
            TState newState = func(actor, data);
            State = newState;
        }

        protected virtual void AddStage(TState state, Func<TActor, TData, TState> func)
        {
            StateMachine[state] = func;
        }
    }
}
