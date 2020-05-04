using Mud.Domain;
using System.Collections.Generic;

namespace Mud.POC.Abilities
{
    public interface IRace
    {
        string Name { get; }
        string DisplayName { get; }
        string ShortName { get; }

        // Abilities available for this class
        IEnumerable<AbilityUsage> Abilities { get; }

        IEnumerable<EquipmentSlots> EquipmentSlots { get; }

        int GetAttributeModifier(CharacterAttributes attribute);

        RaceFlags RaceFlags { get; }

        int ExperiencePerLevel { get; }

        IEnumerable<IClass> AllowedClasses { get; }

        IEnumerable<Alignments> AllowedAlignments { get; set; }

        // TODO: specific behaviour such as 120% xp for human, infrared for dwarf, ...
    }

    public enum RaceFlags
    {
        None                = 0x00000000,
        CreationAvailable   = 0x00000001,
        RebirthOnly         = 0x00000002,
        RemortOnly          = 0x00000004,
        Cursed              = 0x00000008
    }

    public enum Alignments
    {
        Evil = -1,
        Neutral = 0,
        Good = 1
    }
}
