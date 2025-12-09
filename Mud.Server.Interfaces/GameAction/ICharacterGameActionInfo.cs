using Mud.Domain;

namespace Mud.Server.Interfaces.GameAction;

public interface ICharacterGameActionInfo : IActorGameActionInfo
{
    Positions MinPosition { get; }
    bool NotInCombat { get; }
}
