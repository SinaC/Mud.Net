using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Damage)]
public class Fireball : DamageTableSpellBase
{
    private const string SpellName = "Fireball";

    public Fireball(ILogger<Fireball> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override int[] Table =>
    [
        0,
        0,   0,   0,   0,   0,      0,   0,   0,   0,   0,
        0,   0,   0,   0,  30,     35,  40,  45,  50,  55,
        60,  65,  70,  75,  80,     82,  84,  86,  88,  90,
        92,  94,  96,  98, 100,    102, 104, 106, 108, 110,
        112, 114, 116, 118, 120,    122, 124, 126, 128, 130
    ];

    protected override SchoolTypes DamageType => SchoolTypes.Fire;
    protected override string DamageNoun => "fireball";
}
