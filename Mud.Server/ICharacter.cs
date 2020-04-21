using System;
using System.Collections.Generic;
using Mud.Domain;
using Mud.Server.Abilities;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Item;

namespace Mud.Server
{
    public enum ActOptions
    {
        /// <summary>
        /// Everyone in the room except Character
        /// </summary>
        ToRoom,
        /// <summary>
        /// Only to Character
        /// </summary>
        ToCharacter,
        /// <summary>
        /// Everyone in the room
        /// </summary>
        ToAll,
        /// <summary>
        /// Everyone in the group (leader is not in group)
        /// </summary>
        ToGroup
    }

    public interface ICharacter : IEntity, IContainer
    {
        IRoom Room { get; }
        ICharacter Fighting { get; }

        IEnumerable<EquipedItem> Equipments { get; }

        // Furniture (sleep/sit/stand)
        IItemFurniture Furniture { get; }

        // Position
        Positions Position { get; }

        // Class/Race
        IClass Class { get; }
        IRace Race { get; }

        // Attributes
        Sex Sex { get; }
        int Level { get; }
        int HitPoints { get; }
        int this[ResourceKinds resource] { get; }
        int this[PrimaryAttributeTypes attribute] { get; }
        int this[SecondaryAttributeTypes attribute] { get; }
        IEnumerable<ResourceKinds> CurrentResourceKinds { get; }

        // Form
        Forms Form { get; }

        // Abilities
        IEnumerable<AbilityAndLevel> KnownAbilities { get; }

        // Auras
        IEnumerable<IPeriodicAura> PeriodicAuras { get; }
        IEnumerable<IAura> Auras { get; }

        ICharacter Slave { get; } // who is our slave (related to charm command/spell)
        ICharacter ControlledBy { get; } // who is our master (related to charm command/spell)

        // Act
        void Act(ActOptions option, string format, params object[] arguments);
        void ActToNotVictim(ICharacter victim, string format, params object[] arguments); // to everyone except this and victim
        void Act(IEnumerable<ICharacter> characters, string format, params object[] arguments); // to every character in provided list

        // Equipments
        bool Unequip(IEquipable item);

        // Furniture
        bool ChangeFurniture(IItemFurniture furniture);

        // Position
        bool ChangePosition(Positions position);

        // Visibility
        bool CanSee(ICharacter target);
        bool CanSee(IItem target);
        bool CanSee(IExit exit);

        // Attributes
        int GetBasePrimaryAttribute(PrimaryAttributeTypes attribute);
        int GetMaxResource(ResourceKinds resource);
        void ChangeResource(ResourceKinds resource, int amount);
        void UpdateResources();

        // Form
        bool ChangeForm(Forms form);

        // Auras
        void AddPeriodicAura(IPeriodicAura aura);
        void RemovePeriodicAura(IPeriodicAura aura);
        void AddAura(IAura aura, bool recompute);
        void RemoveAura(IAura aura, bool recompute);

        // Controller
        bool ChangeController(ICharacter master); // if non-null, start slavery, else, stop slavery 

        // Recompute
        void Reset(); // Reset attributes, remove auras, periodic auras
        void ResetAttributes(bool resetHitPoints);
        void RecomputeAttributes();

        // Move
        bool Move(ExitDirections direction, bool checkFighting, bool follow = false);
        bool Enter(IItemPortal portal, bool follow = false);
        void ChangeRoom(IRoom destination);

        // Combat
        bool Heal(ICharacter source, IAbility ability, int amount, bool visible);
        bool MultiHit(ICharacter enemy);
        bool StartFighting(ICharacter enemy);
        bool StopFighting(bool both); // if both is true, every character fighting 'this' stop fighting
        bool WeaponDamage(ICharacter source, IItemWeapon weapon, int damage, SchoolTypes damageType, bool visible); // damage from weapon(or bare hands) of known source
        bool AbilityDamage(ICharacter source, IAbility ability, int damage, SchoolTypes damageType, bool visible); // damage from ability of known source
        bool UnknownSourceDamage(IAbility ability, int damage, SchoolTypes damageType, bool visible); // damage with unknown source or no source
        void Slay(IPlayableCharacter killer);
        void KillingPayoff(ICharacter victim);

        // Ability
        IDictionary<IAbility, DateTime> AbilitiesInCooldown { get; }
        bool HasAbilitiesInCooldown { get; }
        int CooldownSecondsLeft(IAbility ability); // Return cooldown seconds left for an ability (Int.MinValue if was not in CD)
        void SetCooldown(IAbility ability);
        void ResetCooldown(IAbility ability, bool verbose);

        // Equipment
        EquipedItem SearchEquipmentSlot(IEquipable item, bool replace);
    }

    public class EquipedItem
    {
        public EquipmentSlots Slot { get; }
        public IEquipable Item { get; set; }

        public EquipedItem(EquipmentSlots slot)
        {
            Slot = slot;
        }
    }
}
