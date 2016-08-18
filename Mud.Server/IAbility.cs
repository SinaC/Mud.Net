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
        Passive = 0x0001,
        RequiresMainHand = 0x0002,
        CannotBeUsedWhileShapeshifted = 0x0004,
        CannotBeDodgedParriedBlocked = 0x0008,
        CannotMiss = 0x0010,
        CannotBeReflected = 0x0020,
        RequiresComboPoints = 0x0040,
        CannotBeUsed = 0x0080,
        AuraIsHidden = 0x0100,
        RequireBearForm = 0x0200,
        RequireCatForm = 0x0400,
        RequireMoonkinForm = 0x0800,
        RequireShadowForm = 0x1000
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
