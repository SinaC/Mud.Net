using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.None)]
public class Farsight : NoTargetSpellBase
{
    private const string SpellName = "Farsight";

    public Farsight(ILogger<Farsight> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override void Invoke()
    {
        Caster.Send("Not Yet Implemented."); // TODO: affect giving longer range when using scan command
    }
}
