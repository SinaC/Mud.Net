using System.Collections.Generic;

namespace Mud.POC.Abilities
{
    public interface IClass
    {
        string Name { get; }
        string DisplayName { get; }
        string ShortName { get; }

        // Kind of resource available for class
        IEnumerable<ResourceKinds> ResourceKinds { get; }

        // Current available kind of resource depending on form (subset of ResourceKinds property, i.e.: druids in bear form only have rage but mana will still regenerated even if not in current)
        IEnumerable<ResourceKinds> CurrentResourceKinds(Forms form);

        // Abilities available for this class
        IEnumerable<AbilityUsage> Abilities { get; }

        int GetAttributeByLevel(CharacterAttributes attribute, int level);
    }
}
