using Mud.Domain;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Class;

namespace Mud.Server.Interfaces.Race;

public interface IPlayableRace : IRace
{
    string ShortName { get; }

    IEnumerable<IAbilityUsage> Abilities { get; }

    int GetStartAttribute(CharacterAttributes attribute);
    int GetMaxAttribute(CharacterAttributes attribute);

    int ClassExperiencePercentageMultiplier(IClass c);

    // TODO: specific behaviour such as 120% xp for human, infrared for dwarf, ...
}
