using System;
using Mud.Server.Input;

namespace Mud.POC.NewMud2
{
    public interface IAbility
    {
        string Name { get; }
        AbilityTypes Type { get; }

        AbilityActionResults PreAction(ICharacter user, string rawParameters, params CommandParameter[] parameters);
        AbilityActionResults Action(ICharacter user, string rawParameters, params CommandParameter[] parameters);
        AbilityActionResults PostAction(ICharacter user, string rawParameters, params CommandParameter[] parameters);
    }

    public interface IDamageAbility
    {
        string DamageNoun { get; }
    }

    public interface IHealAbility
    {
        string HealNoun { get; }
    }

    public enum AbilityActionResults
    {
        Error = 0,
        Failed = 1,
        SavesCheckSuccess = 2,
        TargetNotFound = 3,
        InvalidTarget = 4,
        Ok = 5,
    }

    [Flags]
    public enum AbilityTypes
    {
        Damage = 1 << 1,
        DamageArea = 1 << 2,
        Healing = 1 << 3,
        HealingArea = 1 << 4,
        Buff = 1 << 5,
        Debuff = 1 << 6,
        Cure = 1 << 7,
        Dispel = 1 << 8,
        Transportation = 1 << 9,
        Disrupt = 1 << 10,
        Trap = 1 << 11
    }
}
