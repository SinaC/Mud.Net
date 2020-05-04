using System.Collections.Generic;
using Mud.Domain;
using Mud.POC.Affects;

namespace Mud.POC.Abilities
{
    public interface ICharacter : IEntity
    {
        int Level { get; }
        int HitPoints { get; }

        Positions Position { get; }

        IClass Class { get; }

        IRace Race { get; }

        IRoom Room { get; }

        ICharacter Fighting { get; }

        IEnumerable<IItem> Inventory { get; }

        IEnumerable<IItem> Equipments { get; }

        IEnumerable<KnownAbility> KnownAbilities { get; } // race abilities + class abilities + other if any (if an ability is found in multiple scope, the lowest level, cost, ... will be choosen)
        int LearnedAbility(string name); // return ability practice %
        int LearnedAbility(IAbility ability);

        int this[ResourceKinds resource] { get; }
        IEnumerable<ResourceKinds> CurrentResourceKinds { get; }

        CharacterFlags BaseCharacterFlags { get; }
        CharacterFlags CurrentCharacterFlags { get; }
        int BaseAttributes(CharacterAttributes attribute);
        int CurrentAttributes(CharacterAttributes attribute);

        int GetMaxResource(ResourceKinds resource);
        void UpdateResource(ResourceKinds resource, int amount);

        IAura GetAura(int abilityId);
        IAura GetAura(string abilityName);
        IAura GetAura(IAbility ability);

        bool MultiHit(ICharacter enemy);
        bool WeaponDamage(ICharacter source, IItemWeapon weapon, int damage, SchoolTypes damageType, bool visible); // damage from weapon(or bare hands) of known source
        bool AbilityDamage(IEntity source, IAbility ability, int damage, SchoolTypes damageType, bool visible); // damage from ability of known source

        void Send(string msg, params object[] args);
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
        /// Everyone in the group (leader is not in group)
        /// </summary>
        ToGroup
    }
}
