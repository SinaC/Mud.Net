using System.Collections.Generic;
using Mud.Server.Abilities;
using Mud.Server.Constants;

namespace Mud.Server
{
    public interface IClass
    {
        string Name { get; }
        string DisplayName { get; }
        string ShortName { get; }

        IEnumerable<AbilityAndLevel> Abilities { get; }
        IEnumerable<ResourceKinds> ResourceKinds { get; } // TOOD: use

        // TODO: PrimaryAttributeTypes gain when levelling
    }
}
