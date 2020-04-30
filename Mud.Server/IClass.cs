using System.Collections.Generic;
using Mud.Domain;
using Mud.Server.Abilities;

namespace Mud.Server
{
    public interface IClass
    {
        string Name { get; }
        string DisplayName { get; }
        string ShortName { get; }

        // Kind of resource available for class
        IEnumerable<ResourceKinds> ResourceKinds { get; } // TOOD: use
        // Abilities available for this class
        IEnumerable<AbilityAndLevel> Abilities { get; }
        // Current available kind of resource depending on form (subset of ResourceKinds property, i.e.: druids in bear form only have rage but mana will still regenerated even if not in current)
        IEnumerable<ResourceKinds> CurrentResourceKinds(Forms form);

        int GetAttributeByLevel(CharacterAttributes attribute, int level);
    }
}
