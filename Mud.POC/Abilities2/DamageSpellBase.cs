using Mud.Server.Common;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Interfaces;

namespace Mud.POC.Abilities2
{
    public abstract class DamageSpellBase : OffensiveSpellBase
    {
        protected DamageSpellBase(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        #region ISpell

        public override AbilityEffects Effects => AbilityEffects.Damage;

        #endregion

        #region OffensiveSpellBase

        public override void Action(ICharacter caster, int level, ICharacter victim)
        {
            //
            bool preDamage = PreDamage(caster, level, victim);
            if (!preDamage)
                return;
            //
            int damage = DamageValue(caster, level, victim);
            bool savesSpellResult = victim.SavesSpell(level, DamageType);
            if (savesSpellResult)
                damage /= 2;
            DamageResults damageResult = victim.AbilityDamage(caster, this, damage, DamageType, DamageNoun, true);
            //
            PostDamage(caster, level, victim, savesSpellResult, damageResult);
        }

        #endregion

        protected abstract SchoolTypes DamageType { get; }
        protected abstract int DamageValue(ICharacter caster, int level, ICharacter victim);
        protected abstract string DamageNoun { get; }

        protected virtual bool PreDamage(ICharacter caster, int level, ICharacter victim)
        {
            // NOP
            return true;
        }

        protected virtual void PostDamage(ICharacter caster, int level, ICharacter victim, bool savesSpellResult, DamageResults damageResult)
        {
            // NOP
        }
    }
}
