using Mud.POC.Abilities2.Interfaces;
using Mud.POC.Abilities2.Rom24Spells;
using Mud.Server.Common;

namespace Mud.POC.Abilities2
{
    public abstract class HealSpellBase : DefensiveSpellBase
    {
        public HealSpellBase(IRandomManager randomManager, IWiznet wiznet) 
            : base(randomManager, wiznet)
        {
        }

        #region IAbility

        public override AbilityEffects Effects => AbilityEffects.Healing;

        #endregion

        #region DefensiveSpellBase

        public override void Action(ICharacter caster, int level, ICharacter victim)
        {
            int value = HealValue(level);
            victim.UpdateHitPoints(value);
            victim.Send(HealVictimPhrase);
            if (caster != victim)
                caster.Send(HealCasterPhrase);
        }

        #endregion

        protected abstract int HealValue(int level);
        protected abstract string HealVictimPhrase { get; }
        protected virtual string HealCasterPhrase => "Ok.";
    }
}
