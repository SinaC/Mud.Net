﻿using Mud.Server.Common;
using Mud.POC.Abilities2.Domain;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell(SpellName, AbilityEffects.Damage)]
    public class AcidBlast : DamageSpellBase
    {
        public const string SpellName = "Acid Blast";

        public AcidBlast(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override SchoolTypes DamageType => SchoolTypes.Acid;
        protected override int DamageValue => RandomManager.Dice(Level, 12);
        protected override string DamageNoun => "acid blast";
    }
}
