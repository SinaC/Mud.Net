using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects;
using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;
using Mud.Server.Rom24.Effects;
using Mud.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mud.Server.Rom24.Spells
{
    [Spell(SpellName, AbilityEffects.Damage)]
    public class LightningBolt : DamageTableSpellBase
    {
        public const string SpellName = "Lightning Bolt";

        public LightningBolt(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override int[] Table => new int[]
        {
             0,
             0,  0,  0,  0,  0,  0,  0,  0, 25, 28,
            31, 34, 37, 40, 40, 41, 42, 42, 43, 44,
            44, 45, 46, 46, 47, 48, 48, 49, 50, 50,
            51, 52, 52, 53, 54, 54, 55, 56, 56, 57,
            58, 58, 59, 60, 60, 61, 62, 62, 63, 64
        };
        protected override SchoolTypes DamageType => SchoolTypes.Lightning;
        protected override string DamageNoun => "lightning bolt";
    }
}
