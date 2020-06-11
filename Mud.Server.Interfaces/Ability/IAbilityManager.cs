using System;
using System.Collections.Generic;
using Mud.Server.Input;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;

namespace Mud.Server.Interfaces.Ability
{
    public interface IAbilityManager
    {
        IEnumerable<IAbility> Abilities { get; }
        IAbility this[string name] { get; }
        IAbility this[int id] { get; }
        IEnumerable<IAbility> Spells { get; }
        IEnumerable<IAbility> Skills { get; }
        IEnumerable<IAbility> Passives { get; }

        CastResults Cast(ICharacter caster, string rawParameters, params CommandParameter[] parameters);
        CastResults CastFromItem(IAbility ability, int level, ICharacter caster, IEntity target, string rawParameters, params CommandParameter[] parameters);
        UseResults Use(IAbility ability, ICharacter user, string rawParameters, params CommandParameter[] parameters);

        AbilityTargetResults GetAbilityTarget(IAbility ability, ICharacter caster, out IEntity target, string rawParameters, params CommandParameter[] parameters);
        AbilityTargetResults GetItemAbilityTarget(IAbility ability, ICharacter caster, ref IEntity target);

        IKnownAbility Search(IEnumerable<IKnownAbility> knownAbilities, int level, Func<IAbility, bool> abilityFilterFunc, CommandParameter parameter);

        // Effects
        void AcidEffect(IEntity target, IAbility ability, ICharacter source, int level, int damage);
        void ColdEffect(IEntity target, ICharacter source, int level, int damage);
        void FireEffect(IEntity target, ICharacter source, int level, int damage);
        void PoisonEffect(IEntity target, ICharacter source, int level, int damage);
        void ShockEffect(IEntity target, ICharacter source, int level, int damage);
    }

    public enum CastResults
    {
        Ok = 0,
        MissingParameter = 1,
        InvalidParameter = 2,
        InvalidTarget = 3,
        TargetNotFound = 4,
        CantUseRequiredResource = 5,
        NotEnoughResource = 6,
        InCooldown = 7,
        Failed = 8,
        Error = 9
    }

    public enum UseResults
    {
        Ok = 0,
        MissingParameter = 1,
        InvalidParameter = 2,
        InvalidTarget = 3,
        TargetNotFound = 4,
        CantUseRequiredResource = 5,
        NotEnoughResource = 6,
        Failed = 7,
        NotKnown = 8,
        InCooldown = 9,
        MustBeFighting = 10,
        Error = 11
    }

    public enum AbilityTargetResults
    {
        MissingParameter,
        InvalidTarget,
        TargetNotFound,
        Ok,
        Error
    }
}
