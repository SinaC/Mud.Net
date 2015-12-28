using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Mud.DataStructures;

namespace Mud.Server.Input
{
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

    public abstract class InputTrapBase<TActor, TState> : StateMachineBase<TActor, string, TState>
    {
        public bool PreserveInput { get; protected set; }

        public void ProcessInput(TActor actor, string input)
        {
            // Lower and trim if needed
            if (!PreserveInput && input != null)
                input = input.Trim().ToLower(CultureInfo.InvariantCulture);
            ProcessStage(actor, input);
            //Func<TActor, string, TState> func = StateMachine[State];
            //TState newState = func(actor, input);
            //State = newState;
        }
    }
}
