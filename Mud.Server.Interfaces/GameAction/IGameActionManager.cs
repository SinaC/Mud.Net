using Mud.Server.Interfaces.Actor;
using System.Collections.Generic;

namespace Mud.Server.Interfaces.GameAction
{
    public interface IGameActionManager
    {
        IEnumerable<IGameActionInfo> GameActions { get; }
        IGameActionInfo this[string name] { get; }

        IGameAction CreateInstance(string name);

        IActionInput CreateActionInput(IGameActionInfo commandInfo, IActor actor, string commandLine, string command, string rawParameters, params ICommandParameter[] parameters);
    }
}
