using Mud.Server.Ability.Interfaces;
using Mud.Server.Class.Interfaces;
using Mud.Server.Domain;

namespace Mud.Server.Race.Interfaces;

public interface IPlayableRace : IRace
{
    string ShortName { get; }
    string? Help { get; }

    bool SelectableDuringCreation { get; }
    int CreationPointsStartValue { get; }

    bool EnhancedPrimeAttribute { get; }

    IEnumerable<IAbilityUsage> Abilities { get; }

    int GetStartAttribute(BasicAttributes attribute);
    int GetMaxAttribute(BasicAttributes attribute);

    int ClassExperiencePercentageMultiplier(IClass c);

    // TODO: specific behaviour such as 120% xp for human, infrared for dwarf, ...
}
