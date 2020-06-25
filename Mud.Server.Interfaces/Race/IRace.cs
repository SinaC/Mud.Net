using System.Collections.Generic;
using Mud.DataStructures.Flags;
using Mud.Domain;

namespace Mud.Server.Interfaces.Race
{
    public interface IRace
    {
        string Name { get; }
        string DisplayName { get; }

        Sizes Size { get; }

        Flags CharacterFlags { get; }

        IRVFlags Immunities { get; }
        IRVFlags Resistances { get; }
        IRVFlags Vulnerabilities { get; }

        IEnumerable<EquipmentSlots> EquipmentSlots { get; }

        BodyForms BodyForms { get; }
        BodyParts BodyParts { get; }

        ActFlags ActFlags { get; }
        OffensiveFlags OffensiveFlags { get; }
        AssistFlags AssistFlags { get; }

        // TODO: specific behaviour such as 120% xp for human, infrared for dwarf, ...
    }
}
