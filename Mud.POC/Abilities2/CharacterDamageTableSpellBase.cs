using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;

namespace Mud.POC.Abilities2
{
    public abstract class CharacterDamageTableSpellBase : CharacterDamageSpellBase
    {
        protected CharacterDamageTableSpellBase(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        #region CharacterDamageSpellBase

        protected override int DamageValue(int level)
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
