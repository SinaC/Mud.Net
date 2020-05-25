using Mud.Server.Common;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Interfaces;

namespace Mud.POC.Abilities2
{
    public abstract class CharacterDamageSpellBase : OffensiveSpellBase
    {
        protected bool SavesSpellResult { get; private set; }

        protected CharacterDamageSpellBase(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        #region ISpell

        public override AbilityEffects Effects => AbilityEffects.Damage;

        #endregion

        #region OffensiveSpellBase

        public override void Action(ICharacter caster, int level, ICharacter victim)
        {
            int damage = DamageValue(level);
            SavesSpellResult = victim.SavesSpell(level, DamageType);
            if (SavesSpellResult)
                damage /= 2;
            bool killed = victim.AbilityDamage(caster, this, damage, DamageType, DamageNoun, true);
            if (!killed)
                PostDamage(caster, level, victim);
        }

        #endregion

        protected abstract SchoolTypes DamageType { get; }
        protected abstract int DamageValue(int level);
        protected abstract string DamageNoun { get; }

        protected virtual void PostDamage(ICharacter caster, int level, ICharacter victim)
        {
        }
    }
}
