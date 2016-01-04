using System.Collections.Generic;
using Mud.Server.Blueprints;
using Mud.Server.Constants;

namespace Mud.Server
{
    public interface ICharacter : IEntity, IContainer
    {
        CharacterBlueprint Blueprint { get; }

        IRoom Room { get; }
        ICharacter Fighting { get; }

        IReadOnlyList<EquipmentSlot> Equipments { get; }

        // Attributes
        Sex Sex { get; }
        int HitPoints { get; }
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

        void ChangeRoom(IRoom destination);

        //
        bool MultiHit(ICharacter enemy, DamageTypes damageType);
        bool StartFighting(ICharacter enemy);
        bool StopFighting(bool both); // if both is true, every character fighting 'this' stop fighting
        bool ApplyDamage(int damage); // Simple remove damage from HitPoints, return true if damage killed the target
    }

    public class EquipmentSlot
    {
        public static readonly EquipmentSlot NullObject = new EquipmentSlot(WearLocations.None);

        public WearLocations WearLocation { get; private set; }
        public IItem Item { get; set; }

        public EquipmentSlot(WearLocations wearLocation)
        {
            WearLocation = wearLocation;
        }
    }
}
