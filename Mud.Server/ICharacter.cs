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

        IReadOnlyCollection<EquipedItem> Equipments { get; }

        // Attributes
        Sex Sex { get; }
        int Level { get; }
        int HitPoints { get; }
        int MaxHitPoints { get; }

        // Effects
        IReadOnlyCollection<IPeriodicEffect> PeriodicEffects { get; }
        IReadOnlyCollection<IBuffDebuff> BuffDebuffs { get; }

        // TODO: race, classes, ...

        // Impersonation/Controller
        bool Impersonable { get; }
        IPlayer ImpersonatedBy { get; }

        ICharacter Slave { get; } // who is our slave (related to charm command/spell)
        ICharacter ControlledBy { get; } // who is our master (related to charm command/spell)

        bool ChangeImpersonation(IPlayer player); // if non-null, start impersonation, else, stop impersonation
        bool ChangeController(ICharacter master); // if non-null, start slavery, else, stop slavery

        // Equipments
        bool Unequip(IEquipable item);

        // Visibility
        bool CanSee(ICharacter character);
        bool CanSee(IItem obj);

        // Attributes
        int BaseAttribute(AttributeTypes attribute);
        int CurrentAttribute(AttributeTypes attribute);
        //void ModifyAttribute(AttributeTypes attribute, int value);

        // Effects
        void AddPeriodicEffect(IPeriodicEffect effect);
        void RemovePeriodicEffect(IPeriodicEffect effect);
        void AddBuffDebuff(IBuffDebuff buffDebuff);
        void RemoveBuffDebuff(IBuffDebuff buffDebuff);

        // Move
        void ChangeRoom(IRoom destination);

        // Combat
        bool Heal(ICharacter source, string ability, int amount, bool visible);
        bool MultiHit(ICharacter enemy);
        bool StartFighting(ICharacter enemy);
        bool StopFighting(bool both); // if both is true, every character fighting 'this' stop fighting
        bool CombatDamage(ICharacter source, string ability, int damage, SchoolTypes damageType, bool visible); // damage with known damager
        bool UnknownSourceDamage(string ability, int damage, SchoolTypes damageType, bool visible); // damage with unknown damager or no damager
        bool RawKill(ICharacter victim, bool killingPayoff); // kill victim + create corpse (if killingPayoff is true, xp gain/loss)
        void ResetAttributes();
    }

    public class EquipedItem
    {
        public static readonly EquipedItem NullObject = new EquipedItem(EquipmentSlots.None);

        public EquipmentSlots Slot { get; private set; }
        public IEquipable Item { get; set; }

        public EquipedItem(EquipmentSlots slot)
        {
            Slot = slot;
        }
    }
}
