using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
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

        protected override string SetTargets(SpellActionInput spellActionInput)
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
