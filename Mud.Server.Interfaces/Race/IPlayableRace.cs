using Mud.Domain;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Class;

namespace Mud.Server.Interfaces.Race;

public interface IPlayableRace : IRace
{
    string ShortName { get; }
    string? Help { get; }

    int CreationPointsStartValue { get; }

    bool EnhancedPrimeAttribute { get; }

    IEnumerable<IAbilityUsage> Abilities { get; }

    int GetStartAttribute(BasicAttributes attribute);
    int GetMaxAttribute(BasicAttributes attribute);

    int ClassExperiencePercentageMultiplier(IClass c);

    // TODO: specific behaviour such as 120% xp for human, infrared for dwarf, ...
}
