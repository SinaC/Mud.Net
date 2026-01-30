using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Healing, PulseWaitTime = 18), NotInCombat(Message = StringHelpers.YouCantConcentrateEnough)]
[Syntax("cast [spell] <character>")]
[Help(
@"This spell refreshes the movement points of a character who is out of movement
points.")]
[OneLineHelp("restores energy to tired adventurers")]
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
