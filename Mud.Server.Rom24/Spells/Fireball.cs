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
    public class Fireball : DamageTableSpellBase
    {
        public const string SpellName = "Fireball";

        public Fireball(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override int[] Table =>
            new []
            {
                0,
                0,   0,   0,   0,   0,      0,   0,   0,   0,   0,
                0,   0,   0,   0,  30,     35,  40,  45,  50,  55,
                60,  65,  70,  75,  80,     82,  84,  86,  88,  90,
                92,  94,  96,  98, 100,    102, 104, 106, 108, 110,
                112, 114, 116, 118, 120,    122, 124, 126, 128, 130
            };

        protected override SchoolTypes DamageType => SchoolTypes.Fire;
        protected override string DamageNoun => "fireball";
    }
}
