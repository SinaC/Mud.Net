using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;

namespace Mud.POC.Abilities2
{
    public abstract class DamageTableSpellBase : DamageSpellBase
    {
        protected DamageTableSpellBase(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        #region CharacterDamageSpellBase

        protected override int DamageValue(ICharacter caster, int level, ICharacter victim)
        {
            int baseDamage = Table.Get(level);
            int minDamage = baseDamage / 2;
            int maxDamage = baseDamage * 2;
            return RandomManager.Range(minDamage, maxDamage);
        }

        #endregion

        protected abstract int[] Table { get; }
    }
}
