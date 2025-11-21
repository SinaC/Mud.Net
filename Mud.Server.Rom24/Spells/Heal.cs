using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;
using Mud.Server.Rom24.Effects;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Healing)]
[Syntax("cast [spell] <character>")]
[Help(
@"These spells cure damage on the target character.  The higher-level spells
heal more damage.")]
public class Heal : DefensiveSpellBase
{
    private const string SpellName = "Heal";

    public Heal(ILogger<Heal> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override void Invoke()
    {
        HealEffect effect = new ();
        effect.Apply(Victim, Caster, SpellName, Level, 0);
    }
}
