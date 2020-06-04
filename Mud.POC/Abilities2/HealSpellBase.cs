using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2
{
    public abstract class HealSpellBase : DefensiveSpellBase
    {
        protected HealSpellBase(IRandomManager randomManager, IWiznet wiznet) 
            : base(randomManager, wiznet)
        {
        }

        protected override void Invoke()
        {
            int value = HealValue;
            Victim.UpdateHitPoints(value);
            Victim.Send(HealVictimPhrase);
            if (Caster != Victim)
                Caster.Send(HealCasterPhrase);
        }

        protected abstract int HealValue { get; }
        protected abstract string HealVictimPhrase { get; }
        protected virtual string HealCasterPhrase => "Ok.";
    }
}
