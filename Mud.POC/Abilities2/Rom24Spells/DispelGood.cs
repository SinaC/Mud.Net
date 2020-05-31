using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;
using Mud.Server.Input;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class DispelGood : DamageSpellBase
    {
        public override int Id => 37;

        public override string Name => "Dispel Good";

        protected override SchoolTypes DamageType => SchoolTypes.Negative;

        protected override string DamageNoun => "dispel good";

        protected override int DamageValue(ICharacter caster, int level, ICharacter victim)
            => victim.HitPoints >= caster.Level * 4
                ? RandomManager.Dice(level, 4)
                : Math.Max(victim.HitPoints, RandomManager.Dice(level, 4));

        public DispelGood(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override AbilityTargetResults SetTargets(ICharacter caster, string rawParameters, params CommandParameter[] parameters)
        {
            AbilityTargetResults result = base.SetTargets(caster, rawParameters, parameters);
            if (result != AbilityTargetResults.Ok)
                return result;
            // Check alignment
            if (caster is IPlayableCharacter && caster.IsGood)
                Victim = caster;
            return AbilityTargetResults.Ok;
        }

        protected override bool PreDamage(ICharacter caster, int level, ICharacter victim)
        {
            if (victim.IsEvil)
            {
                caster.Act(ActOptions.ToAll, "Mota protects {0}.", victim);
                return false;
            }
            if (victim.IsNeutral)
            {
                caster.Act(ActOptions.ToCharacter, "{0:N} does not seem to be affected.", victim);
                return false;
            }
            return true;
        }
    }
}
