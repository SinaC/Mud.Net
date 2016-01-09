using Mud.Server.Constants;

namespace Mud.Server.Abilities
{
    // TODO: delete whole class
    //public interface IAbility
    //{
    //    // Cost
    //    ResourceKinds ResourceKind { get; }
    //    AmountOperators CostType { get; }
    //    int CostAmount { get; }

    //    // CD/GCD
    //    int GlobalCooldown { get; }
    //    int Cooldown { get; }

    //    // School
    //    SchoolTypes School { get; }

    //    // Effects list
    //    // TODO

    //    // Flags list
    //    AbilityFlags Flags { get; }
    //}
    public struct ToRemoveAbility
    {
        // Name
        public string Name { get; set; }

        // Cost
        public ResourceKinds ResourceKind { get; set; }
        public AmountOperators CostType { get; set; }
        public int CostAmount { get; set; }

        // CD/GCD/Duration
        public int GlobalCooldown { get; set; }
        public int Cooldown { get; set; }
        public int Duration { get; set; }

        // School
        public SchoolTypes School { get; set; }

        // Mechanic
        public AbilityMechanics Mechanic { get; set; }

        // Dispel Type
        public DispelTypes DispelType { get; set; }

        // Flags
        public AbilityFlags Flags { get; set; }
    }
}
