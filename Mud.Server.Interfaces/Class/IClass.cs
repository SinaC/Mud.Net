using Mud.Domain;
using Mud.Server.Domain;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Interfaces.Class;

public interface IClass
{
    string Name { get; }
    string DisplayName { get; }
    string ShortName { get; }
    string? Help { get; }

    // Kind of resource available for class
    IEnumerable<ResourceKinds> ResourceKinds { get; }
    // Current available kind of resource depending on shape (subset of ResourceKinds property, i.e.: druids in bear form only have rage but mana will still regenerated even if not in current)
    IEnumerable<ResourceKinds> CurrentResourceKinds(Shapes shape);

    // Will give a +2 to this attribute
    BasicAttributes PrimeAttribute { get; }

    // Abilities available for this class
    IEnumerable<IAbilityUsage> AvailableAbilities { get; }
    // Ability groups available for this class
    IEnumerable<IAbilityGroupUsage> AvailableAbilityGroups { get; }
    // Ability groups given for free during creation
    IEnumerable<IAbilityGroupUsage> BasicAbilityGroups { get; }
    // Ability groups given when no customization is done
    IEnumerable<IAbilityGroupUsage> DefaultAbilityGroups { get; }

    // Max practice percentage (learned in KnownAbility)
    int MaxPracticePercentage { get; }

    (int thac0_00, int thac0_32) Thac0 { get; }

    int MinHitPointGainPerLevel { get; }
    int MaxHitPointGainPerLevel { get; }
}
