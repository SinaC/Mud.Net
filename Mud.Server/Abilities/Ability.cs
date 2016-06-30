using System.Collections.Generic;
using System.Linq;
using Mud.Server.Constants;

namespace Mud.Server.Abilities
{
    public class Ability : IAbility
    {
        #region IAbility

        // Unique Id
        public int Id { get; }

        // Name
        public string Name { get; }

        // Target
        public AbilityTargets Target { get; }

        // Cost
        public ResourceKinds ResourceKind { get; }
        public AmountOperators CostType { get; }
        public int CostAmount { get; }

        // GCD/CD/Duration
        public int GlobalCooldown { get; }
        public int Cooldown { get; }
        public int Duration { get; }

        // School/Mechanic/DispelType
        public SchoolTypes School { get; }
        public AbilityMechanics Mechanic { get; }
        public DispelTypes DispelType { get; }

        // Flags
        public AbilityFlags Flags { get; }

        // Effect list
        public List<AbilityEffect> Effects { get; } // IReadOnlyCollection

        #endregion

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
            Effects = effects?.ToList();
        }
    }
}
