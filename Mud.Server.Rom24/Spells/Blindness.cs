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

[Spell(SpellName, AbilityEffects.Debuff)]
[AbilityCharacterWearOffMessage("You can see again.")]
[AbilityDispellable("{0:N} is no longer blinded.")]
[Syntax("cast [spell] <victim>")]
[Help(
@"This spell renders the target character blind.")]
[OneLineHelp("strikes the target blind")]
public class Blindness : OffensiveSpellBase
{
    private const string SpellName = "Blindness";

    private IEffectManager EffectManager { get; }

    public Blindness(ILogger<Blindness> logger, IRandomManager randomManager, IEffectManager effectManager)
        : base(logger, randomManager)
    {
        EffectManager = effectManager;
    }

    protected override void Invoke()
    {
        var effect = EffectManager.CreateInstance<ICharacter>("Blindness");
        effect?.Apply(Victim, Caster, SpellName, Level, 0);
    }
}
