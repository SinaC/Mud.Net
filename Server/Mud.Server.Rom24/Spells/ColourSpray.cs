using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Random;
using Mud.Server.Ability.Spell;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Effects.Interfaces;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Damage | AbilityEffects.Debuff)]
[Syntax("cast [spell] <victim>")]
[Help(
@"These spells inflict damage on the victim.  The higher-level spells do
more damage.")]
[OneLineHelp("blasts your opponent with a rainbow spray, which may blind him")]
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
        0,   0,   0,   0,   0,       0,   0,   0,   0,   0,
        30,  35,  40,  45,  50,      55,  55,  55,  56,  57,
        58,  58,  59,  60,  61,      61,  62,  63,  64,  64,
        65,  66,  67,  67,  68,      69,  70,  70,  71,  72,
        73,  73,  74,  75,  76,      76,  77,  78,  79,  79,
        80,  80,  81,  82,  82,      83,  83,  84,  85,  85,
        86,  86,  87,  88,  88,      89,  89,  90,  91,  91,
        92,  92,  93,  93,  94,      94,  95,  95,  96,  96,
        97,  97,  98,  98,  99,      99,  100, 100, 101, 101,
        102, 102, 103, 103, 104,     104, 105, 105, 106, 106,
        107, 107, 108, 108, 109,     109, 110, 110, 111, 111
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
