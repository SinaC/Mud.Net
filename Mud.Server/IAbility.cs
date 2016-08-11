using System;
using System.Collections.Generic;
using Mud.Server.Abilities;
using Mud.Server.Constants;

namespace Mud.Server
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
        AuraIsHidden = 256,
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
        TargetOrSelf, // target in room, if no target self
        Group, // everyone in group
        Room, // every enemy in room
        Distant, // target in any room (search in current, then anywhere)
    }

    public enum AbilityBehaviors
    {
        None, // passive ability
        Friendly, // heal
        Harmful, // damage ability
        Any // heal or damage ability
    }

    public enum AbilityKinds
    {
        Skill, // if harmful ability then use Yellow attack table
        Spell, // if harmful ability then use Spell attack table
    }

    public interface IAbility
    {
        // Unique Id
        int Id { get; }

        // Name
        string Name { get; }

        // Target
        AbilityTargets Target { get; }

        // Behaviours
        AbilityBehaviors Behavior { get; }

        // Kind
        AbilityKinds Kind { get; }

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
