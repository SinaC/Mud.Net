using Mud.Server.Interfaces.Actor;
using System.Collections.Generic;

namespace Mud.Server.Interfaces.GameAction
{
    public interface IGameActionManager
    {
        IEnumerable<IGameActionInfo> GameActions { get; }
        IGameActionInfo this[string name] { get; }

        IGameAction CreateInstance(IGameActionInfo gameActionInfo);

        IActionInput CreateActionInput<TActor>(IGameActionInfo gameActionInfo, TActor actor, string commandLine, string command, string rawParameters, params ICommandParameter[] parameters)
            where TActor : IActor;
    }
}
