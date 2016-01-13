using System;
using System.Collections.Generic;
using Mud.Server.Abilities;
using Mud.Server.Constants;

namespace Mud.Server.World
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
        RequiresComboPoints = 64,
        CannotBeUsed = 128,
    }

    public enum AbilityMechanics
    {
        None,
        Bleeding,
        Silenced,
        Shielded,
    }

    public enum AbilityTargets
    {
        Self, // caster
        Target, // target in room
        TargetOrSelf, // target in room, if not target self
        Group, // everyone in group
        Room, // every enemy in room
        Distant, // target in any room (search in current, then anywhere)
    }

    public interface IAbility
    {
        // Unique Id
        int Id { get; }

        // Name
        string Name { get; }

        // Target
        AbilityTargets Target { get; }

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

        List<AbilityEffect> Effects { get; }
    }
}
