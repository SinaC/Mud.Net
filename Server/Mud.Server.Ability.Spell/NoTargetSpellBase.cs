using Microsoft.Extensions.Logging;
using Mud.Random;
using Mud.Server.Ability.Spell.Interfaces;

namespace Mud.Server.Ability.Spell;

public abstract class NoTargetSpellBase : SpellBase
{
    protected NoTargetSpellBase(ILogger<NoTargetSpellBase> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override string? SetTargets(ISpellActionInput spellActionInput) => default;
}
