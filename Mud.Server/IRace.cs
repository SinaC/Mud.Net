using System.Collections.Generic;
using Mud.Domain;
using Mud.Server.Abilities;

namespace Mud.Server
{
    public interface IRace
    {
        string Name { get; }
        string DisplayName { get; }
        string ShortName { get; }

        IEnumerable<AbilityUsage> Abilities { get; }

        IEnumerable<EquipmentSlots> EquipmentSlots { get; }

        int GetAttributeModifier(CharacterAttributes attribute);

        // TODO: specific behaviour such as 120% xp for human, infrared for dwarf, ...
        // TODO: xp/level, ...
    }
}
