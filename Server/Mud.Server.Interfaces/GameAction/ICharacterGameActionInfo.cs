using Mud.Server.Interfaces.Guards;

namespace Mud.Server.Interfaces.GameAction;

public interface ICharacterGameActionInfo : IActorGameActionInfo
{
    ICharacterGuard[] CharacterGuards { get; }
}
