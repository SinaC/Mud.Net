using System.Collections.Generic;
using Mud.Domain;
using Mud.Server.Interfaces.Abilities;

namespace Mud.Server.Interfaces
{
    public interface IPlayableRace : IRace
    {
        string ShortName { get; }

        IEnumerable<IAbilityUsage> Abilities { get; }

        int GetStartAttribute(CharacterAttributes attribute);
        int GetMaxAttribute(CharacterAttributes attribute);

        int ClassExperiencePercentageMultiplier(IClass c);

        // TODO: specific behaviour such as 120% xp for human, infrared for dwarf, ...
    }
}
