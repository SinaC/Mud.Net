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
    public class ShockingGrasp : DamageTableSpellBase
    {
        public const string SpellName = "Shocking Grasp";

        public ShockingGrasp(IRandomManager randomManager)
            : base(randomManager)
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
