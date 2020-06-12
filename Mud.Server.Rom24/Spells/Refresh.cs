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
    [Spell(SpellName, AbilityEffects.Healing, PulseWaitTime = 18)]
    public class Refresh : DefensiveSpellBase
    {
        public const string SpellName = "Refresh";

        public Refresh(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override void Invoke()
        {
            RefreshEffect effect = new RefreshEffect();
            effect.Apply(Victim, Caster, SpellName, Level, 0);
        }
    }
}
