using System.Collections.Generic;
using System.Text;
using Mud.POC.Abilities2.Domain;

namespace Mud.POC.Abilities2.ExistingCode
{
    public interface ICharacter : IEntity, IContainer
    {
        IRoom Room { get; }
        IEnumerable<IItem> Inventory { get; }
        IEnumerable<EquippedItem> Equipments { get; }
        IItem GetEquipment(EquipmentSlots equipmentSlots);
        TItem GetEquipment<TItem>(EquipmentSlots equipmentSlots)
            where TItem : IItem;

        int Level { get; }
        CharacterFlags CharacterFlags { get; }
        Positions Position { get; }
        Sex Sex { get; }

        void AddBaseCharacterFlags(CharacterFlags flags);
        void RemoveBaseCharacterFlags(CharacterFlags flags);
        void ChangePosition(Positions position);

        int this[ResourceKinds kind] { get; }
        int MaxResource(ResourceKinds kind);
        IEnumerable<ResourceKinds> CurrentResourceKinds { get; }
        int UpdateResource(ResourceKinds kind, int amount);

        int this[CharacterAttributes attribute] { get; }

        int HitPoints { get; }
        int MaxHitPoints { get; }
        int MovePoints { get; }
        int MaxMovePoints { get; }
        void UpdateHitPoints(int amount);
        void UpdateMovePoints(int amount);

        int CarryWeight { get; }

        Sizes Size { get; }

        IRVFlags Immunities { get; }

        void GainExperience(long amount);

        void ChangeRoom(IRoom destination);

        void AutoLook();

        IEnumerable<INonPlayableCharacter> Pets { get; }

        int Alignment { get; }
        bool IsEvil { get; }
        bool IsNeutral { get; }
        bool IsGood { get; }
        void UpdateAlignment(int amount);

        bool CanSee(IItem item);
        bool CanSee(ICharacter character);
        bool CanSee(IRoom room);

        ICharacter Fighting { get; }
        bool IsSafe(ICharacter aggressor);
        bool IsSafeSpell(ICharacter caster, bool area);
        bool SavesSpell(int level, SchoolTypes damageType);
        DamageResults AbilityDamage(ICharacter source, int damage, SchoolTypes damageType, string damageNoun, bool isVisible);
        bool MultiHit(ICharacter aggressor);
        void MultiHit(ICharacter victim, IMultiHitModifier multiHitModifier); // 'this' starts a combat with 'victim' and has been initiated by an ability
        bool StartFighting(ICharacter victim);
        bool StopFighting(bool both);

        bool IsSameGroupOrPet(ICharacter character);

        IEnumerable<IAbilityLearned> LearnedAbilities { get; }
        (int percentage, IAbilityLearned abilityLearned) GetAbilityLearned(string abilityName); // percentage is dynamically computed
        (int percentage, IAbilityLearned abilityLearned) GetWeaponLearned(IItemWeapon weapon); // percentage is dynamically computed

        int CooldownPulseLeft(string abilityName);
        void SetCooldown(string abilityName, int seconds);

        void AddPet(INonPlayableCharacter pet);

        void ActToNotVictim(ICharacter victim, string format, params object[] arguments);
        string ActPhrase(string format, params object[] arguments);
        void Page(StringBuilder sb);
    }

    public class EquippedItem
    {
        public EquipmentSlots Slot { get; }
        public IItem Item { get; set; }

        public EquippedItem(EquipmentSlots slot)
        {
            Slot = slot;
        }

        //public EquippedItemData MapEquippedData()
        //{
        //    return new EquippedItemData
        //    {
        //        Slot = Slot,
        //        Item = Item.MapItemData()
        //    };
        //}
    }

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
        /// Everyone in the group
        /// </summary>
        ToGroup
    }

    public enum DamageResults
    {
        Killed,
        Absorbed,
        NoDamage,
        Done
    }

    public interface IMultiHitModifier : IHitModifier
    {
        int MaxAttackCount { get; }
    }

    public interface IHitModifier
    {
        IAbility Ability { get; }
        int Learned { get; }
        int Thac0Modifier(int baseThac0);
        int DamageModifier(IItemWeapon weapon, int level, int baseDamage);
    }
}
