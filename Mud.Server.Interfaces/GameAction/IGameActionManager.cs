using Mud.Server.Interfaces.Actor;
using System.Collections.Generic;

namespace Mud.Server.Interfaces.GameAction
{
    public interface IGameActionManager
    {
        IEnumerable<IGameActionInfo> GameActions { get; }

        string Execute<TActor>(IGameActionInfo gameActionInfo, TActor actor, string command, string rawParameters, params ICommandParameter[] parameters)
            where TActor: IActor;
        string Execute<TGameAction, TActor>(TActor actor, string command, string rawParameters, params ICommandParameter[] parameters)
            where TActor : IActor;
    }
}
