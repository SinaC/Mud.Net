using System;
using Mud.Server.Constants;

namespace Mud.Server
{
    [Flags]
    public enum AbilityFlags
    {
        Passive = 1,
        RequireMainHand = 2,
        NotShapeshifted = 4,
    }

    public interface IAbility
    {
        // Cost
        ResourceKinds ResourceKind { get; }
        AmountOperators CostType { get; }
        int CostAmount { get; }

        // CD/GCD
        int GlobalCooldown { get; }
        int Cooldown { get; }

        // School
        SchoolTypes School { get; }

        // Effects list
        // TODO

        // Flags list
        AbilityFlags Flags { get; }
    }
}
