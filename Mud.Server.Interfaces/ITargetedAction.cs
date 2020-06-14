using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using System.Collections.Generic;

namespace Mud.Server.Interfaces
{
    public interface ITargetedAction
    {
        IEnumerable<IEntity> ValidTargets(ICharacter actor);
    }
}
