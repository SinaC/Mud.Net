using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Random;

namespace Mud.Server.Ability.Spell;

public abstract class DamageTableSpellBase : DamageSpellBase
{
    protected DamageTableSpellBase(ILogger<DamageTableSpellBase> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override int DamageValue
    {
        get
        {
            int baseDamage = Table.Get(1+Level); // starts at 0
            int minDamage = baseDamage / 2;
            int maxDamage = baseDamage * 2;
            return RandomManager.Range(minDamage, maxDamage);
        }
    }

    protected abstract int[] Table { get; }
}
