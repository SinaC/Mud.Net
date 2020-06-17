using Mud.Server.Interfaces.GameAction;
using System.Collections.Generic;

namespace Mud.POC.Abilities
{
    public interface IAbilityManager
    {
        IEnumerable<IAbility> Abilities { get; }
        IAbility this[string name] { get; }
        IEnumerable<IAbility> Spells { get; }
        IEnumerable<IAbility> Skills { get; }
        IEnumerable<IAbility> Passives { get; }

        CastResults Cast(ICharacter caster, string rawParameters, params ICommandParameter[] parameters);
        CastResults CastFromItem(IAbility ability, ICharacter caster, IEntity target, string rawParameters, params ICommandParameter[] parameters);
        UseResults Use(IAbility ability, ICharacter caster, string rawParameters, params ICommandParameter[] parameters);

        AbilityTargetResults GetAbilityTarget(IAbility ability, ICharacter caster, out IEntity target, string rawParameters, params ICommandParameter[] parameters);
        AbilityTargetResults GetItemAbilityTarget(IAbility ability, ICharacter caster, ref IEntity target);
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
        Failed = 7,
        Error = 8
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
        MustBeFighting = 9,
        Error = 10
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
