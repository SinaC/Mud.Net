using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2
{
    public abstract class DamageTableSpellBase : DamageSpellBase
    {
        protected DamageTableSpellBase(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override int DamageValue
        {
            get
            {
                int baseDamage = Table.Get(Level);
                int minDamage = baseDamage / 2;
                int maxDamage = baseDamage * 2;
                return RandomManager.Range(minDamage, maxDamage);
            }
        }

        protected abstract int[] Table { get; }
    }
}
