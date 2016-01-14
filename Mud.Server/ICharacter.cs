using System;
using System.Collections.Generic;
using Mud.Server.Blueprints;
using Mud.Server.Constants;
using Mud.Server.Item;
using Mud.Server.Server;
using Mud.Server.World;

namespace Mud.Server
{
    public enum ActOptions
    {
        ToRoom, // everyone in the room except Character
        ToCharacter, // only to Character
        ToAll // everyone in the room
    }

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
        int this[ResourceKinds resource] { get; }
        int this[PrimaryAttributeTypes attribute] { get; }
        int this[ComputedAttributeTypes attribute] { get; }

        // Auras
        IReadOnlyCollection<IPeriodicAura> PeriodicAuras { get; }
        IReadOnlyCollection<IAura> Auras { get; }

        // TODO: race, classes, ...

        // Group/Follower
        ICharacter Leader { get; }
        IReadOnlyCollection<ICharacter> GroupMembers { get; }

        // Impersonation/Controller
        bool Impersonable { get; }
        IPlayer ImpersonatedBy { get; }

        ICharacter Slave { get; } // who is our slave (related to charm command/spell)
        ICharacter ControlledBy { get; } // who is our master (related to charm command/spell)

        // Group/Follower
        bool ChangeLeader(ICharacter leader);
        bool AddGroupMember(ICharacter member);
        bool RemoveGroupMember(ICharacter member);
        bool AddFollower(ICharacter follower);
        bool StopFollower(ICharacter follower);

        // Impersonation/Controller
        bool ChangeImpersonation(IPlayer player); // if non-null, start impersonation, else, stop impersonation
        bool ChangeController(ICharacter master); // if non-null, start slavery, else, stop slavery

        // Act
        void Act(ActOptions options, string format, params object[] arguments);
        void ActToNotVictim(ICharacter victim, string format, params object[] arguments); // to everyone except this and victim

        // Equipments
        bool Unequip(IEquipable item);

        // Visibility
        bool CanSee(ICharacter character);
        bool CanSee(IItem obj);

        // Attributes
        int GetBasePrimaryAttribute(PrimaryAttributeTypes attribute);
        int GetMaxResource(ResourceKinds resource);

        // Auras
        void AddPeriodicAura(IPeriodicAura aura);
        void RemovePeriodicAura(IPeriodicAura aura);
        void AddAura(IAura aura, bool recompute);
        void RemoveAura(IAura aura, bool recompute);

        // Recompute
        void ResetAttributes();
        void RecomputeAttributes();

        // Move
        bool Move(ServerOptions.ExitDirections direction, bool follow = false);
        void ChangeRoom(IRoom destination);

        // Combat
        bool Heal(ICharacter source, IAbility ability, int amount, bool visible);
        bool MultiHit(ICharacter enemy);
        bool StartFighting(ICharacter enemy);
        bool StopFighting(bool both); // if both is true, every character fighting 'this' stop fighting
        bool WeaponDamage(ICharacter source, IItemWeapon weapon, int damage, SchoolTypes damageType, bool visible); // damage from weapon(or bare hands) of known source
        bool Damage(ICharacter source, IAbility ability, int damage, SchoolTypes damageType, bool visible); // damage from known source
        bool UnknownSourceDamage(IAbility ability, int damage, SchoolTypes damageType, bool visible); // damage from unknown source
        bool RawKill(ICharacter victim, bool killingPayoff); // kill victim + create corpse (if killingPayoff is true, xp gain/loss)

        // Ability
        IReadOnlyCollection<KeyValuePair<IAbility, DateTime>> AbilitiesInCooldown { get; }
        int CooldownSecondsLeft(IAbility ability); // Return cooldown seconds left for an ability (Int.MinValue if was not in CD)
        void SetCooldown(IAbility ability);
        void ResetCooldown(IAbility ability);
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
