using System.Collections.Generic;
using Mud.Server.Blueprints;
using Mud.Server.Constants;
using Mud.Server.Item;

namespace Mud.Server
{
    public interface ICharacter : IEntity, IContainer
    {
        CharacterBlueprint Blueprint { get; }

        IRoom Room { get; }
        ICharacter Fighting { get; }

        IReadOnlyCollection<EquipmentSlot> Equipments { get; }

        // Attributes
        Sex Sex { get; }
        int Level { get; }
        int HitPoints { get; }
        int MaxHitPoints { get; }

        // Periodic Effects
        IReadOnlyCollection<IPeriodicEffect> PeriodicEffects { get; }

        // TODO: race, classes, ...

        // Impersonation/Controller
        bool Impersonable { get; }
        IPlayer ImpersonatedBy { get; }

        ICharacter Slave { get; } // who is our slave (related to charm command/spell)
        ICharacter ControlledBy { get; } // who is our master (related to charm command/spell)

        bool ChangeImpersonation(IPlayer player); // if non-null, start impersonation, else, stop impersonation
        bool ChangeController(ICharacter master); // if non-null, start slavery, else, stop slavery

        // Equipments
        bool RemoveEquipment(IEquipable item);

        // Visibility
        bool CanSee(ICharacter character);
        bool CanSee(IItem obj);

        // Attributes
        int BaseAttribute(AttributeTypes attribute);
        int CurrentAttribute(AttributeTypes attribute);
        void ModifyAttribute(AttributeTypes attribute, int value);

        // Periodic Effects
        void AddPeriodicEffect(IPeriodicEffect effect);
        void RemovePeriodicEffect(IPeriodicEffect effect);

        // Move
        void ChangeRoom(IRoom destination);

        // Combat
        bool Heal(ICharacter source, string ability, int amount, bool visible);
        bool MultiHit(ICharacter enemy);
        bool StartFighting(ICharacter enemy);
        bool StopFighting(bool both); // if both is true, every character fighting 'this' stop fighting
        bool CombatDamage(ICharacter source, string ability, int damage, DamageTypes damageType, bool visible); // damage with known damager
        bool UnknownSourceDamage(string ability, int damage, DamageTypes damageType, bool visible); // damage with unknown damager or no damager
        bool KillingPayoff(ICharacter victim); // to be called only if victim is dead   TODO: don't make this accessible in interface
        IItemCorpse RawKill(ICharacter victim); // kill victim without any xp gain/loss + create corpse
    }

    public class EquipmentSlot
    {
        public static readonly EquipmentSlot NullObject = new EquipmentSlot(WearLocations.None);

        public WearLocations WearLocation { get; private set; }
        public IEquipable Item { get; set; }

        public EquipmentSlot(WearLocations wearLocation)
        {
            WearLocation = wearLocation;
        }
    }
}
