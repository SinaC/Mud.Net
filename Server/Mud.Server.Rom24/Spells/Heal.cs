using Microsoft.Extensions.Logging;
using Mud.Random;
using Mud.Server.Ability.Spell;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Effects.Interfaces;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Healing)]
[Syntax("cast [spell] <character>")]
[Help(
@"These spells cure damage on the target character.  The higher-level spells
heal more damage.")]
[OneLineHelp("the most powerful healing spell")]
public class Heal : DefensiveSpellBase
{
    private const string SpellName = "Heal";

    private IEffectManager EffectManager { get; }

    public Heal(ILogger<Heal> logger, IRandomManager randomManager, IEffectManager effectManager)
        : base(logger, randomManager)
    {
        EffectManager = effectManager;
    }

    protected override void Invoke()
    {
        var effect = EffectManager.CreateInstance<ICharacter>("Heal");
        effect?.Apply(Victim, Caster, SpellName, Level, 0);
    }
}
