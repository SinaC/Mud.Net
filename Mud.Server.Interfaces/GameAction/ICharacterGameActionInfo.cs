using Mud.Domain;

namespace Mud.Server.Interfaces.GameAction;

public interface ICharacterGameActionInfo : IGameActionInfo
{
    Positions MinPosition { get; }
    bool NotInCombat { get; }
}
