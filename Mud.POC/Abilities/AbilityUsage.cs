﻿using Mud.Domain;

namespace Mud.POC.Abilities
{
    public class AbilityUsage
    {
        public IAbility Ability { get; set; }

        public int Level { get; set; }

        public ResourceKinds ResourceKinds { get; set; }

        public int CostAmount { get; set; }

        public CostAmountOperators CostAmountOperator { get; set; }

        public int ImproveDifficulityMultiplier { get; set; }

        public AbilityUsage()
        {
            ResourceKinds = ResourceKinds.None;
            CostAmount = 0;
            CostAmountOperator = CostAmountOperators.None;
            ImproveDifficulityMultiplier = 1;
        }
    }
}
