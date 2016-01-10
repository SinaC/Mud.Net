using System;
using System.Collections.Generic;
using System.Linq;
using Mud.Server.Constants;

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

    public class Ability
    {
        // Unique Id
        public int Id { get; private set; }

        // Name
        public string Name { get; private set; }

        // Target
        public AbilityTargets Target { get; private set; }

        // Cost
        public ResourceKinds ResourceKind { get; private set; }
        public AmountOperators CostType { get; private set; }
        public int CostAmount { get; private set; }

        // GCD/CD/Duration
        public int GlobalCooldown { get; private set; }
        public int Cooldown { get; private set; }
        public int Duration { get; private set; }

        // School/Mechanic/DispelType
        public SchoolTypes School { get; private set; }
        public AbilityMechanics Mechanic { get; private set; }
        public DispelTypes DispelType { get; private set; }

        // Flags
        public AbilityFlags Flags { get; private set; }

        public List<AbilityEffect> Effects { get; private set; }

        public Ability(int id, string name, AbilityTargets target, ResourceKinds resourceKind, AmountOperators costType, int costAmount, int globalCooldown, int cooldown, int duration, SchoolTypes school, AbilityMechanics mechanic, DispelTypes dispelType, AbilityFlags flags, params AbilityEffect[] effects)
        {
            Id = id;
            Name = name;
            Target = target;
            ResourceKind = resourceKind;
            CostType = costType;
            CostAmount = costAmount;
            GlobalCooldown = globalCooldown;
            Cooldown = cooldown;
            Duration = duration;
            School = school;
            Mechanic = mechanic;
            DispelType = dispelType;
            Flags = flags;
            Effects = effects == null ? null : effects.ToList();
        }
    }
}
