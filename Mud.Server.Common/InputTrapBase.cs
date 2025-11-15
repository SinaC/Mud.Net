using Mud.Server.Interfaces;
using System.Globalization;

namespace Mud.Server.Common;

public abstract class InputTrapBase<TActor, TState> : IInputTrap<TActor>
    where TState : notnull
{
    protected Dictionary<TState, Func<TActor, string, TState>> StateMachine { get; init; } = default!;

    public bool KeepInputAsIs { get; protected set; }
    public TState State { get; protected set; } = default!;

    protected InputTrapBase()
    {
        StateMachine = [];
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
            input = input.Trim().ToLower(CultureInfo.InvariantCulture);
        var func = StateMachine[State];
        var newState = func(actor, input);
        State = newState;
    }

    #endregion
}
