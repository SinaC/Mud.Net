using System.Collections.Generic;
using Mud.Server.Blueprints;

namespace Mud.Server
{
    public interface ICharacter : IEntity, IContainer
    {
        CharacterBlueprint Blueprint { get; }

        IRoom Room { get; }

        IReadOnlyList<EquipmentSlot> Equipments { get; }

        // Attributes
        Sex Sex { get; }
        // TODO: race, classes, ...

        //
        bool Impersonable { get; }
        IPlayer ImpersonatedBy { get; }

        ICharacter Slave { get; } // who is our slave (related to charm command/spell)
        ICharacter ControlledBy { get; } // who is our master (related to charm command/spell)

        bool ChangeImpersonation(IPlayer player); // if non-null, start impersonation, else, stop impersonation
        bool ChangeController(ICharacter master); // if non-null, start slavery, else, stop slavery

        bool CanSee(ICharacter character);
        bool CanSee(IItem obj);
    }

    public class EquipmentSlot
    {
        public WearLocations WearLocation { get; private set; }
        public IItem Item { get; set; }

        public EquipmentSlot(WearLocations wearLocation)
        {
            WearLocation = wearLocation;
        }
    }
}
