using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Random;
using Mud.Server.Ability.Spell;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;

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
        0,   0,   0,   0,   0,       0,   0,   0,   0,   0,
        0,   0,   0,   0,  30,       35,  40,  45,  50,  55,
        60,  65,  70,  75,  80,      82,  84,  86,  88,  90,
        92,  94,  96,  98, 100,      102, 104, 106, 108, 110,
        112, 114, 116, 118, 120,     122, 124, 126, 128, 130,
        132, 134, 136, 138, 140,     142, 144, 146, 148, 150,
        152, 154, 156, 158, 160,     162, 164, 166, 168, 168,
        170, 170, 172, 172, 174,     174, 176, 176, 178, 178,
        180, 180, 182, 182, 184,     184, 186, 186, 188, 188,
        200, 200, 201, 201, 202,     202, 203, 203, 204, 204,
        205, 205, 206, 206, 207,     207, 208, 208, 209, 209
    ];

    protected override SchoolTypes DamageType => SchoolTypes.Fire;
    protected override string DamageNoun => "fireball";
}
