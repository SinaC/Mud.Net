using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;

namespace Mud.Server.Interfaces;

public interface ITargetedAction
{
    IEnumerable<IEntity> ValidTargets(ICharacter actor);
}
