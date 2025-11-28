using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Healing, PulseWaitTime = 18)]
[Syntax("cast [spell] <character>")]
[Help(
@"This spell refreshes the movement points of a character who is out of movement
points.")]
public class Refresh : DefensiveSpellBase
{
    private const string SpellName = "Refresh";

    private IEffectManager EffectManager { get; }

    public Refresh(ILogger<Refresh> logger, IRandomManager randomManager, IEffectManager effectManager)
        : base(logger, randomManager)
    {
        EffectManager = effectManager;
    }

    protected override void Invoke()
    {
        var effect = EffectManager.CreateInstance<ICharacter>("Refresh");
        effect?.Apply(Victim, Caster, SpellName, Level, 0);
    }
}
