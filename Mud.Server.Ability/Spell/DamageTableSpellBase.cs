﻿using Mud.Common;
using Mud.Server.Random;

namespace Mud.Server.Ability.Spell
{
    public abstract class DamageTableSpellBase : DamageSpellBase
    {
        protected DamageTableSpellBase(IRandomManager randomManager)
            : base(randomManager)
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