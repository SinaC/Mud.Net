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
    public class DispelEvil : DamageSpellBase
    {
        public const string SpellName = "Dispel Evil";

        public DispelEvil(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override SchoolTypes DamageType => SchoolTypes.Holy;
        protected override string DamageNoun => "dispel evil";
        protected override int DamageValue
            => Victim.HitPoints >= Caster.Level * 4
                ? RandomManager.Dice(Level, 4)
                : Math.Max(Victim.HitPoints, RandomManager.Dice(Level, 4));

        protected override string SetTargets(ISpellActionInput spellActionInput)
        {
            string baseSetTargets = base.SetTargets(spellActionInput);
            if (baseSetTargets != null)
                return baseSetTargets;

            // Check alignment
            if (Caster is IPlayableCharacter && Caster.IsEvil)
                Victim = Caster;

            if (Victim.IsGood)
            {
                Caster.Act(ActOptions.ToAll, "Mota protects {0:N}.", Victim);
                return string.Empty; // TODO: should return above message
            }
            if (Victim.IsNeutral)
            {
                Caster.Act(ActOptions.ToCharacter, "{0:N} does not seem to be affected.", Victim);
                return string.Empty; // TODO: should return above message
            }

            return null;
        }
    }
}
