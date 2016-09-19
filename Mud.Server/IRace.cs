using System.Collections.Generic;
using Mud.Server.Abilities;
using Mud.Server.Constants;

namespace Mud.Server
{
    public interface IRace
    {
        string Name { get; }
        string DisplayName { get; }
        string ShortName { get; }

        IEnumerable<AbilityAndLevel> Abilities { get; }

        IEnumerable<EquipmentSlots> EquipmentSlots { get; }

        int GetPrimaryAttributeModifier(PrimaryAttributeTypes primaryAttribute);

        // TODO: specific behaviour such as 120% xp for human, infrared for dwarf, ...
        // TODO: xp/level, ...
    }
}
