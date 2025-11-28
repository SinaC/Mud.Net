using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Damage | AbilityEffects.Debuff)]
[Syntax("cast [spell] <victim>")]
[Help(
@"These spells inflict damage on the victim.  The higher-level spells do
more damage.")]
public class ColourSpray : DamageTableSpellBase
{
    private const string SpellName = "Colour Spray";

    private IEffectManager EffectManager { get; }

    public ColourSpray(ILogger<ColourSpray> logger, IRandomManager randomManager, IEffectManager effectManager)
        : base(logger, randomManager)
    {
        EffectManager = effectManager;
    }

    protected override SchoolTypes DamageType => SchoolTypes.Light;
    protected override string DamageNoun => "colour spray";
    protected override int[] Table =>
    [
         0,
         0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
        30, 35, 40, 45, 50, 55, 55, 55, 56, 57,
        58, 58, 59, 60, 61, 61, 62, 63, 64, 64,
        65, 66, 67, 67, 68, 69, 70, 70, 71, 72,
        73, 73, 74, 75, 76, 76, 77, 78, 79, 79
    ];

    protected override void Invoke()
    {
        base.Invoke();
        if (SavesSpellResult || DamageResult != DamageResults.Done)
            return;
        var blindnessEffect = EffectManager.CreateInstance<ICharacter>("Blindness");
        blindnessEffect?.Apply(Victim, Caster, "Blindness", Level, 0);
    }
}
