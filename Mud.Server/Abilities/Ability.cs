using System.Collections.Generic;
using System.Linq;
using Mud.Server.Constants;

namespace Mud.Server.Abilities
{
    public class Ability : IAbility
    {
        #region IAbility

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

        // Effect list
        public List<AbilityEffect> Effects { get; private set; } // IReadOnlyCollection

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
            Effects = effects == null ? null : effects.ToList();
        }
    }
}
