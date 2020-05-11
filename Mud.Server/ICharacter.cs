using System;
using System.Collections.Generic;
using Mud.Domain;
using Mud.Server.Abilities;
using Mud.Server.Aura;
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
        IEnumerable<IItem> Inventory { get; } // same as IContainer.Content

        // Furniture (sleep/sit/stand)
        IItemFurniture Furniture { get; }

        // Position
        Positions Position { get; }

        // Class/Race
        IClass Class { get; }
        IRace Race { get; }

        // Attributes
        int Level { get; }
        int HitPoints { get; }
        int MovePoints { get; }

        CharacterFlags BaseCharacterFlags { get; }
        CharacterFlags CharacterFlags { get; }

        IRVFlags BaseImmunities { get; }
        IRVFlags Immunities { get; }

        IRVFlags BaseResistances { get; }
        IRVFlags Resistances { get; }

        IRVFlags BaseVulnerabilities { get; }
        IRVFlags Vulnerabilities { get; }

        Sex BaseSex { get; }
        Sex Sex { get; }

        int Alignment { get; }
        bool IsEvil { get; }
        bool IsGood { get; }
        bool IsNeutral { get; }

        int this[CharacterAttributes attribute] { get; }
        int this[BasicAttributes attribute] { get; }
        int this[Armors armor] { get; }
        int HitRoll { get; }
        int DamRoll { get; }

        int this[ResourceKinds resource] { get; }
        IEnumerable<ResourceKinds> CurrentResourceKinds { get; }

        // Form
        Forms Form { get; }

        // Abilities
        IEnumerable<KnownAbility> KnownAbilities { get; }

        // Slave
        INonPlayableCharacter Slave { get; } // who is our slave (related to charm command/spell)
        ICharacter ControlledBy { get; } // who is our master (related to charm command/spell)

        // Act
        void Act(ActOptions option, string format, params object[] arguments);
        void ActToNotVictim(ICharacter victim, string format, params object[] arguments); // to everyone except this and victim
        void Act(IEnumerable<ICharacter> characters, string format, params object[] arguments); // to every character in provided list

        // Equipments
        bool Unequip(IEquipableItem item);

        // Furniture
        bool ChangeFurniture(IItemFurniture furniture);

        // Position
        bool ChangePosition(Positions position);

        // Visibility
        bool CanSee(ICharacter victim);
        bool CanSee(IItem item);
        bool CanSee(IExit exit);
        bool CanSee(IRoom room);

        // Attributes
        int BaseAttribute(CharacterAttributes attribute);
        int MaxResource(ResourceKinds resource);
        void UpdateResource(ResourceKinds resource, int amount);
        void UpdateHitPoints(int amount);
        void UpdateMovePoints(int amount);
        void UpdateAlignment(int amount);
        void Regen();
        void AddBaseCharacterFlags(CharacterFlags characterFlags);
        void RemoveBaseCharacterFlags(CharacterFlags characterFlags);

        // Form
        bool ChangeForm(Forms form);

        // Controller
        bool ChangeSlave(INonPlayableCharacter slave); // if non-null, start slavery, else, stop slavery 
        bool ChangeController(ICharacter master); // if non-null, start slavery, else, stop slavery 

        // Move
        bool Move(ExitDirections direction, bool checkFighting, bool follow = false);
        bool Enter(IItemPortal portal, bool follow = false);
        void ChangeRoom(IRoom destination);
        void AutoLook();

        // Combat
        ResistanceLevels CheckResistance(SchoolTypes damageType);
        bool Heal(IEntity source, IAbility ability, int amount, bool visible);
        bool UnknownSourceHeal(IAbility ability, int amount, bool visible);
        bool MultiHit(ICharacter enemy);
        bool StartFighting(ICharacter enemy);
        bool StopFighting(bool both); // if both is true, every character fighting 'this' stop fighting
        bool WeaponDamage(ICharacter source, IItemWeapon weapon, int damage, SchoolTypes damageType, bool visible); // damage from weapon(or bare hands) of known source
        bool AbilityDamage(IEntity source, IAbility ability, int damage, SchoolTypes damageType, bool visible); // damage from ability of known source
        bool UnknownSourceDamage(IAbility ability, int damage, SchoolTypes damageType, bool visible); // damage with unknown source or no source
        void Slay(IPlayableCharacter killer);
        void KillingPayoff(ICharacter victim);
        bool SavesSpell(int level, SchoolTypes damageType);
        bool IsSafeSpell(ICharacter caster, bool area);
        bool IsSafe(ICharacter character);

        // Abilities
        (int learned, KnownAbility knownAbility) GetLearnInfo(IAbility ability);
        (int learned, KnownAbility knownAbility) GetLearnInfo(string abilityName);
        IDictionary<IAbility, DateTime> AbilitiesInCooldown { get; }
        bool HasAbilitiesInCooldown { get; }
        int CooldownSecondsLeft(IAbility ability); // Return cooldown seconds left for an ability (Int.MinValue if was not in CD)
        void SetCooldown(IAbility ability);
        void ResetCooldown(IAbility ability, bool verbose);

        // Equipment
        IItem GetEquipment(EquipmentSlots slot); // return item found in first non-empty specified slot
        EquipedItem SearchEquipmentSlot(IEquipableItem item, bool replace);

        // Affects
        void ApplyAffect(CharacterFlagsAffect affect);
        void ApplyAffect(CharacterIRVAffect affect);
        void ApplyAffect(CharacterAttributeAffect affect);
        void ApplyAffect(CharacterSexAffect affect);
    }

    public class EquipedItem
    {
        public EquipmentSlots Slot { get; }
        public IEquipableItem Item { get; set; }

        public EquipedItem(EquipmentSlots slot)
        {
            Slot = slot;
        }

        public EquipedItemData MapEquipedData()
        {
            return new EquipedItemData
            {
                Slot = Slot,
                Item = Item.MapItemData()
            };
        }
    }
}
