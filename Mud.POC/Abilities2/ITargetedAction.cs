using Mud.POC.Abilities2.ExistingCode;
using System.Collections.Generic;

namespace Mud.POC.Abilities2
{
    public interface ITargetedAction
    {
        IEnumerable<IEntity> AvailableTargets(ICharacter actor);
    }
}
