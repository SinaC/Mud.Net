using System;
using Mud.Server.Constants;
using Mud.Server.Input;

namespace Mud.Server.Abilities
{
    [Flags]
    public enum AbilityFlags
    {
        None = 0,
        Passive = 1,
        RequiresMainHand = 2,
        CannotBeUsedWhileShapeshifted = 4,
        CannotBeDodgedParriedBlocked = 8,
        CannotMiss = 16,
        CannotBeReflected = 32,
        RequiresComboPoints = 64
    }

    public enum AbilityMechanics
    {
        None,
        Bleeding,
        Silenced,
    }

    public interface IAbility
    {
        // Name
        string Name { get; }

        // Cost
        ResourceKinds ResourceKind { get; }
        AmountOperators CostType { get; }
        int CostAmount { get; }

        // GCD/CD/Duration
        int GlobalCooldown { get; }
        int Cooldown { get; }
        int Duration { get; }

        // School/Mechanic/DispelType
        SchoolTypes School { get; }
        AbilityMechanics Mechanic { get; }
        DispelTypes DispelType { get; }

        // Flags
        AbilityFlags Flags { get; }

        // Process ability (cost/cd/flags have already been checked)
        bool Process(ICharacter source, params CommandParameter[] parameters);
    }
}
