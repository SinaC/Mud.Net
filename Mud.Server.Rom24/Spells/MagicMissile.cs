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
using Mud.Server.Interfaces.Room;
using Mud.Server.Random;
using Mud.Server.Rom24.Effects;
using Mud.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Mud.Server.Rom24.Spells
{
    [Spell(SpellName, AbilityEffects.Damage)]
    public class MagicMissile : DamageTableSpellBase
    {
        public const string SpellName = "Magic Missile";

        public MagicMissile(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override int[] Table => new int []
        {
             0,
             3,  3,  4,  4,  5,  6,  6,  6,  6,  6,
             7,  7,  7,  7,  7,  8,  8,  8,  8,  8,
             9,  9,  9,  9,  9, 10, 10, 10, 10, 10,
            11, 11, 11, 11, 11, 12, 12, 12, 12, 12,
            13, 13, 13, 13, 13, 14, 14, 14, 14, 14
        };
        protected override SchoolTypes DamageType => SchoolTypes.Energy;
        protected override string DamageNoun => "magic missile";
    }
}
