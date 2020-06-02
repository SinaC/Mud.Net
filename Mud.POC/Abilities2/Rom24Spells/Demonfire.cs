using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;
using Mud.Server.Input;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class Demonfire : DamageSpellBase
    {
        public override int Id => 30;
        public override string Name => "Demonfire";
        protected override SchoolTypes DamageType => SchoolTypes.Negative;
        protected override string DamageNoun => "torments";

        public Demonfire(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override int DamageValue(ICharacter caster, int level, ICharacter victim) => RandomManager.Dice(level, 10);

        protected override bool PreDamage(ICharacter caster, int level, ICharacter victim)
        {
            if (victim != caster)
            {
                caster.Act(ActOptions.ToRoom, "{0} calls forth the demons of Hell upon {1}!", caster, victim);
                victim.Act(ActOptions.ToCharacter, "{0} has assailed you with the demons of Hell!", caster);
                caster.Send("You conjure forth the demons of hell!");
            }
            return true;
        }

        protected override void PostDamage(ICharacter caster, int level, ICharacter victim, bool savesSpellResult, DamageResults damageResult)
        {
            caster.UpdateAlignment(-50);
        }

        protected override AbilityTargetResults GetTarget(ICharacter caster, out IEntity target, string rawParameters, params CommandParameter[] parameters)
        {
            AbilityTargetResults result = base.GetTarget(caster, out target, rawParameters, parameters);
            if (result != AbilityTargetResults.Ok)
                return result;
            // Check alignment
            if (caster is IPlayableCharacter && !caster.IsEvil)
            {
                target = caster;
                caster.Send("The demons turn upon you!");
            }
            return AbilityTargetResults.Ok;
        }
    }
}
