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
    [Spell(SpellName, AbilityEffects.Healing)]
    public class Heal : DefensiveSpellBase
    {
        public const string SpellName = "Heal";

        public Heal(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override void Invoke()
        {
            HealEffect effect = new HealEffect();
            effect.Apply(Victim, Caster, SpellName, Level, 0);
        }
    }
}
