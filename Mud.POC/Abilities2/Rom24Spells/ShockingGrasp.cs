﻿using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Shocking Grasp", AbilityEffects.Damage)]
    public class ShockingGrasp : DamageTableSpellBase
    {
        public ShockingGrasp(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override int[] Table => new int[]
        {
             0,
             0,  0,  0,  0,  0,  0, 20, 25, 29, 33,
            36, 39, 39, 39, 40, 40, 41, 41, 42, 42,
            43, 43, 44, 44, 45, 45, 46, 46, 47, 47,
            48, 48, 49, 49, 50, 50, 51, 51, 52, 52,
            53, 53, 54, 54, 55, 55, 56, 56, 57, 57
        };
        protected override SchoolTypes DamageType => SchoolTypes.Lightning;
        protected override string DamageNoun => "shocking grasp";
    }
}
