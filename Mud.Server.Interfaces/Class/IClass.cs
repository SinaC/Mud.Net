using Mud.Domain;
using Mud.Server.Interfaces.Ability;

namespace Mud.Server.Interfaces.Class;

public interface IClass
{
    string Name { get; }
    string DisplayName { get; }
    string ShortName { get; }

    // Kind of resource available for class
    IEnumerable<ResourceKinds> ResourceKinds { get; } // TOOD: use
    // Current available kind of resource depending on form (subset of ResourceKinds property, i.e.: druids in bear form only have rage but mana will still regenerated even if not in current)
    IEnumerable<ResourceKinds> CurrentResourceKinds(Forms form);

    // Will give a +2 to this attribute
    BasicAttributes PrimeAttribute { get; }
    // Abilities available for this class
    IEnumerable<IAbilityUsage> Abilities { get; }
    // Max practice percentage (learned in KnownAbility)
    int MaxPracticePercentage { get; }

    (int thac0_00, int thac0_32) Thac0 { get; }

    int MinHitPointGainPerLevel { get; }
    int MaxHitPointGainPerLevel { get; }
}
