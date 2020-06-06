using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Known Alignment", AbilityEffects.Detection)]
    public class KnowAlignment : DefensiveSpellBase
    {
        public KnowAlignment(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override void Invoke()
        {
            int ap = Victim.Alignment;
            string msg;
            if (ap > 700) msg = "{0:N} has a pure and good aura.";
            else if (ap > 350) msg = "{0:N} is of excellent moral character.";
            else if (ap > 100) msg = "{0:N} is often kind and thoughtful.";
            else if (ap > -100) msg = "{0:N} doesn't have a firm moral commitment.";
            else if (ap > -350) msg = "{0:N} lies to {0:s} friends.";
            else if (ap > -700) msg = "{0:N} is a black-hearted murderer.";
            else msg = "{0:N} is the embodiment of pure evil!.";
            Caster.Act(ActOptions.ToCharacter, msg, Victim);
        }
    }
}
