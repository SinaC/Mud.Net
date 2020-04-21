using System;
using System.Collections.Generic;
using Mud.Domain;
using Mud.Server.Abilities;
using Mud.Server.Blueprints.Character;
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
        CharacterBlueprintBase Blueprint { get; }

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
        long Experience { get; }
        int Level { get; }
        int HitPoints { get; }
        int this[ResourceKinds resource] { get; }
        int this[PrimaryAttributeTypes attribute] { get; }
        int this[SecondaryAttributeTypes attribute] { get; }
        IEnumerable<ResourceKinds> CurrentResourceKinds { get; }
        long ExperienceToLevel { get; }

        // Form
        Forms Form { get; }

        // Abilities
        IEnumerable<AbilityAndLevel> KnownAbilities { get; }

        // Auras
        IEnumerable<IPeriodicAura> PeriodicAuras { get; }
        IEnumerable<IAura> Auras { get; }

        // Group/Follower
        ICharacter Leader { get; }
        IEnumerable<ICharacter> GroupMembers { get; } //!! leader is not be member of its own group and only leader stores GroupMembers
        bool IsSameGroup(ICharacter character); // check is 'this' and 'character' are in the same group

        // Impersonation/Controller
        bool Impersonable { get; }
        IPlayer ImpersonatedBy { get; }

        ICharacter Slave { get; } // who is our slave (related to charm command/spell)
        ICharacter ControlledBy { get; } // who is our master (related to charm command/spell)

        // Quest
        IEnumerable<IQuest> Quests { get; }
        void AddQuest(IQuest quest);
        void RemoveQuest(IQuest quest);
        bool IsQuestObjective(ICharacter questingCharacter);

        // Group/Follower
        bool ChangeLeader(ICharacter leader);
        bool AddGroupMember(ICharacter member, bool silent);
        bool RemoveGroupMember(ICharacter member, bool silent);
        bool AddFollower(ICharacter follower);
        bool StopFollower(ICharacter follower);

        // Impersonation/Controller
        bool ChangeImpersonation(IPlayer player); // if non-null, start impersonation, else, stop impersonation
        bool ChangeController(ICharacter master); // if non-null, start slavery, else, stop slavery

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
        bool RawKilled(ICharacter killer, bool killingPayoff); // kill 'this' + create corpse (if killingPayoff is true, xp gain/loss)
        void KillingPayoff(ICharacter victim);
        void GainExperience(long experience); // add/substract experience

        // Ability
        IDictionary<IAbility, DateTime> AbilitiesInCooldown { get; }
        bool HasAbilitiesInCooldown { get; }
        int CooldownSecondsLeft(IAbility ability); // Return cooldown seconds left for an ability (Int.MinValue if was not in CD)
        void SetCooldown(IAbility ability);
        void ResetCooldown(IAbility ability, bool verbose);

        // Look
        void AutoLook();

        // Equipment
        EquipedItem SearchEquipmentSlot(IEquipable item, bool replace);

        // CharacterData
        void FillCharacterData(CharacterData characterData);
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
