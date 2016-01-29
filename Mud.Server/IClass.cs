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

        IReadOnlyCollection<AbilityAndLevel> Abilities { get; }
        List<ResourceKinds> ResourceKinds { get; }

        // TODO: PrimaryAttributeTypes gain when levelling
    }
}
