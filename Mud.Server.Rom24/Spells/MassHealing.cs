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
    [Spell(SpellName, AbilityEffects.HealingArea, PulseWaitTime = 36)]
    public class MassHealing : NoTargetSpellBase
    {
        public const string SpellName = "Mass Healing";

        public MassHealing(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override void Invoke()
        {
            foreach (ICharacter victim in Caster.Room.People)
            {
                if ((Caster is IPlayableCharacter && victim is IPlayableCharacter)
                    || (Caster is INonPlayableCharacter && victim is INonPlayableCharacter))
                {
                    HealEffect healEffect = new HealEffect();
                    healEffect.Apply(victim, Caster, Heal.SpellName, Level, 0);
                    RefreshEffect refreshEffect = new RefreshEffect();
                    refreshEffect.Apply(victim, Caster, Refresh.SpellName, Level, 0);
                }
            }
        }
    }
}
