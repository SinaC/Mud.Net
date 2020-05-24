using Mud.Server.Common;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Interfaces;

namespace Mud.POC.Abilities2
{
    public abstract class CharacterDamageSpellBase : OffensiveSpellBase
    {
        public CharacterDamageSpellBase(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        #region ISpell

        public override AbilityEffects Effects => AbilityEffects.Damage;

        #endregion

        #region OffensiveSpellBase

        protected override void Action(ICharacter caster, int level, ICharacter victim)
        {
            int damage = DamageValue(level);
            if (victim.SavesSpell(level, SchoolTypes.Acid))
                damage /= 2;
            victim.AbilityDamage(caster, this, damage, DamageType, DamageNoun, true);
        }

        #endregion

        protected abstract SchoolTypes DamageType { get; }
        protected abstract int DamageValue(int level);
        protected abstract string DamageNoun { get; }
    }
}
