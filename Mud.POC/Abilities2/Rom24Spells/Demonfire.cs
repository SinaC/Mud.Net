using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Demonfire", AbilityEffects.Damage)]
    public class Demonfire : DamageSpellBase
    {
        protected override SchoolTypes DamageType => SchoolTypes.Negative;
        protected override string DamageNoun => "torments";
        protected override int DamageValue => RandomManager.Dice(Level, 10);

        public Demonfire(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override string SetTargets(AbilityActionInput abilityActionInput)
        {
            string baseSetTargets = base.SetTargets(abilityActionInput);
            if (baseSetTargets != null)
                return baseSetTargets;
            // Check alignment
            if (Caster is IPlayableCharacter && !Caster.IsEvil)
            {
                Victim = Caster;
                Caster.Send("The demons turn upon you!");
            }
            return null;
        }

        protected override void Invoke()
        {
            if (Victim != Caster)
            {
                Caster.Act(ActOptions.ToRoom, "{0} calls forth the demons of Hell upon {1}!", Caster, Victim);
                Victim.Act(ActOptions.ToCharacter, "{0} has assailed you with the demons of Hell!", Caster);
                Caster.Send("You conjure forth the demons of hell!");
            }

            base.Invoke();

            Caster.UpdateAlignment(-50);
        }
    }
}
