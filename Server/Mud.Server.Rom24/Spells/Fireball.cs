using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common.Attributes;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Damage)]
[Syntax("cast [spell] <victim>")]
[Help(
@"These spells inflict damage on the victim.  The higher-level spells do
more damage.")]
[OneLineHelp("a powerful spell, great for burning your enemy to ashes")]
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
