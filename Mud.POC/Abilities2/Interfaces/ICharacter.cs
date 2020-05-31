using System.Collections.Generic;
using Mud.POC.Abilities2.Domain;

namespace Mud.POC.Abilities2.Interfaces
{
    public interface ICharacter : IEntity, IContainer
    {
        IRoom Room { get; }
        IEnumerable<IItem> Inventory { get; }
        IEnumerable<IItem> Equipments { get; }

        int Level { get; }
        CharacterFlags CharacterFlags { get; }
        Positions Position { get; }
        Sex Sex { get; }

        int this[ResourceKinds kind] { get; }
        int MaxResource(ResourceKinds kind);
        IEnumerable<ResourceKinds> CurrentResourceKinds { get; }
        int UpdateResource(ResourceKinds kind, int amount);

        int HitPoints { get; }
        void UpdateHitPoints(int amount);

        IRVFlags Immunities { get; }

        int Alignment { get; }
        bool IsEvil { get; }
        bool IsNeutral { get; }
        bool IsGood { get; }
        void UpdateAlignment(int amount);

        bool CanSee(IItem item);

        ICharacter Fighting { get; }
        bool IsSafe(ICharacter aggressor);
        bool IsSafeSpell(ICharacter caster, bool area);
        bool SavesSpell(int level, SchoolTypes damageType);
        DamageResults AbilityDamage(ICharacter source, IAbility ability, int damage, SchoolTypes damageType, string damageNoun, bool isVisible);
        bool MultiHit(ICharacter aggressor);
        bool StopFighting(bool both);

        IEnumerable<KnownAbility> KnownAbilities { get; }
        (int learned, KnownAbility ability) GetLearnInfo(IAbility ability);

        int CooldownPulseLeft(IAbility ability);
        void SetCooldown(IAbility ability);

        void AddPet(INonPlayableCharacter pet);

        void Send(string format, params object[] args);
        void Act(ActOptions option, string format, params object[] arguments);
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
}
