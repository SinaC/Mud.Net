using System.Collections.Generic;

namespace Mud.Server.Interfaces.GameAction
{
    public interface IGameActionManager
    {
        IEnumerable<IGameActionInfo> GameActions { get; }
        IGameActionInfo this[string name] { get; }

        IGameAction CreateInstance(string name);
    }
}
