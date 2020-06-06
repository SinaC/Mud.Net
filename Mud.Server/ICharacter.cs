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
        /// Everyone in the group
        /// </summary>
        ToGroup
    }

    public interface ICharacter : IEntity, IContainer
    {
        IRoom Room { get; }
        ICharacter Fighting { get; }

        IEnumerable<EquippedItem> Equipments { get; }
        IEnumerable<IItem> Inventory { get; } // same as IContainer.Content
        int MaxCarryWeight { get; }
        int MaxCarryNumber { get; }
        int CarryWeight { get; }
        int CarryNumber { get; }

        // Money
        long SilverCoins { get; }
        long GoldCoins { get; }
        (long silver, long gold) DeductCost(long cost);

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
        int MaxHitPoints { get; }
        int MovePoints { get; }
        int MaxMovePoints { get; }

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

        Sizes BaseSize { get; }
        Sizes Size { get; }

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

        // Followers
        ICharacter Leader { get; } // character we are following, different from group leader
        void AddFollower(ICharacter character);
        void RemoveFollower(ICharacter character);
        void ChangeLeader(ICharacter character);

        // Group
        bool IsSameGroupOrPet(ICharacter character);

        // Act
        void Act(ActOptions option, string format, params object[] arguments);
        void ActToNotVictim(ICharacter victim, string format, params object[] arguments); // to everyone except this and victim

        // Equipments
        bool Unequip(IItem item);
        bool Equip(IItem item);

        // Money
        void UpdateMoney(long silverCoins, long goldCoins);

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

        // Move
        bool Move(ExitDirections direction, bool follow);
        bool Enter(IItemPortal portal, bool follow);
        void ChangeRoom(IRoom destination);
        void AutoLook();

        // Combat
        SchoolTypes NoWeaponDamageType { get; }
        int NoWeaponBaseDamage { get; }
        string NoWeaponDamageNoun { get; }
        void UpdatePosition();
        bool StartFighting(ICharacter victim);
        bool StopFighting(bool both); // if both is true, every character fighting 'this' stop fighting
        void MultiHit(ICharacter victim); // 'this' starts a combat with 'victim'
        void MultiHit(ICharacter victim, IMultiHitModifier multiHitModifier); // 'this' starts a combat with 'victim' and has been initiated by an ability
        DamageResults AbilityDamage(ICharacter source, IAbility ability, int damage, SchoolTypes damageType, bool display); // 'this' is dealt damage by 'source' using an ability
        DamageResults HitDamage(ICharacter source, IItemWeapon wield, int damage, SchoolTypes damageType, bool display); // 'this' is dealt damage by 'source' using a weapon
        DamageResults Damage(ICharacter source, int damage, SchoolTypes damageType, string damageNoun, bool display); // 'this' is dealt damage by 'source' using 'damageNoun'
        ResistanceLevels CheckResistance(SchoolTypes damageType);
        void Slay(IPlayableCharacter killer);
        void KillingPayoff(ICharacter victim, IItemCorpse corpse);
        void DeathPayoff(ICharacter killer);
        bool SavesSpell(int level, SchoolTypes damageType);
        bool IsSafeSpell(ICharacter caster, bool area);
        bool IsSafe(ICharacter aggressor);

        // Abilities
        (int learned, KnownAbility knownAbility) GetWeaponLearnInfo(IItemWeapon weapon);
        (int learned, KnownAbility knownAbility) GetLearnInfo(IAbility ability);
        (int learned, KnownAbility knownAbility) GetLearnInfo(string abilityName);
        IDictionary<IAbility, int> AbilitiesInCooldown { get; }
        bool HasAbilitiesInCooldown { get; }
        int CooldownPulseLeft(IAbility ability); // Return cooldown seconds left for an ability (Int.MinValue if was not in CD)
        void SetCooldown(IAbility ability);
        bool DecreaseCooldown(IAbility ability, int pulseCount); // return true if timed out
        void ResetCooldown(IAbility ability, bool verbose);

        // Equipment
        IItem GetEquipment(EquipmentSlots slot); // return item found in first non-empty specified slot
        T GetEquipment<T>(EquipmentSlots slot) // return specific item found in first non-empty specified slot
            where T : IItem;
        EquippedItem SearchEquipmentSlot(IItem item, bool replace);

        // Affects
        void ApplyAffect(CharacterFlagsAffect affect);
        void ApplyAffect(CharacterIRVAffect affect);
        void ApplyAffect(CharacterAttributeAffect affect);
        void ApplyAffect(CharacterSexAffect affect);
        void ApplyAffect(CharacterSizeAffect affect);
    }

    public class EquippedItem
    {
        public EquipmentSlots Slot { get; }
        public IItem Item { get; set; }

        public EquippedItem(EquipmentSlots slot)
        {
            Slot = slot;
        }

        public EquippedItemData MapEquippedData()
        {
            return new EquippedItemData
            {
                Slot = Slot,
                Item = Item.MapItemData()
            };
        }
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

    public enum DamageResults
    {
        Dead, // target was already dead
        Safe, // target is safe
        NoDamage, // damage has been reduced to 0
        Killed, // target has been killed by damage
        Damaged, // normal damage
    }
}
