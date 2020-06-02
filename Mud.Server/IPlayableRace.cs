using System.Collections.Generic;
using Mud.Domain;
using Mud.Server.Abilities;

namespace Mud.Server
{
    public interface IPlayableRace : IRace
    {
        string ShortName { get; }

        IEnumerable<AbilityUsage> Abilities { get; }

        int GetStartAttribute(CharacterAttributes attribute);
        int GetMaxAttribute(CharacterAttributes attribute);

        int ClassExperiencePercentageMultiplier(IClass c);

        // TODO: specific behaviour such as 120% xp for human, infrared for dwarf, ...
    }
}
