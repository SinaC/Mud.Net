using Mud.Server.Random;

namespace Mud.Server.Rom24
{
    public abstract class HealSpellBase : DefensiveSpellBase
    {
        protected HealSpellBase(IRandomManager randomManager) 
            : base(randomManager)
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
