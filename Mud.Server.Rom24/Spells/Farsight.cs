using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.None)]
[Syntax(
    "cast [spell]",
    "cast [spell] <direction>")]
[Help(
@"The farsight spell expands the caster's consciousness, allowing him or her
to see far away beings like they were in the same room.  It takes intense
concentration, often leaving the caster helpless for several minutes.
The spell may be used for a general scan that reaches a short distance in
all directions, or with a directional component to see creatures much
farther away.")]
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
