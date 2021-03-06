﻿using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;

namespace Mud.Server.Ability.Spell
{
    public abstract class NoTargetSpellBase : SpellBase
    {
        protected NoTargetSpellBase(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override string SetTargets(ISpellActionInput spellActionInput) => null;
    }
}
