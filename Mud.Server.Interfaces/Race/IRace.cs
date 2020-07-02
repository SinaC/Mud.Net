using System.Collections.Generic;
using Mud.Domain;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Interfaces.Race
{
    public interface IRace
    {
        string Name { get; }
        string DisplayName { get; }

        Sizes Size { get; }

        ICharacterFlags CharacterFlags { get; }

        IIRVFlags Immunities { get; }
        IIRVFlags Resistances { get; }
        IIRVFlags Vulnerabilities { get; }

        IEnumerable<EquipmentSlots> EquipmentSlots { get; }

        IBodyForms BodyForms { get; }
        IBodyParts BodyParts { get; }

        IActFlags ActFlags { get; }
        IOffensiveFlags OffensiveFlags { get; }
        IAssistFlags AssistFlags { get; }

        // TODO: specific behaviour such as 120% xp for human, infrared for dwarf, ...
    }
}
