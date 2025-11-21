using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;
using Mud.Server.Rom24.Effects;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Healing, PulseWaitTime = 18)]
[Syntax("cast [spell] <character>")]
[Help(
@"This spell refreshes the movement points of a character who is out of movement
points.")]
public class Refresh : DefensiveSpellBase
{
    private const string SpellName = "Refresh";

    public Refresh(ILogger<Refresh> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override void Invoke()
    {
        RefreshEffect effect = new ();
        effect.Apply(Victim, Caster, SpellName, Level, 0);
    }
}
