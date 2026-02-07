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
[OneLineHelp("sends a powerful jolt into a foe")]
public class ShockingGrasp : DamageTableSpellBase
{
    private const string SpellName = "Shocking Grasp";

    public ShockingGrasp(ILogger<ShockingGrasp> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override int[] Table =>
    [
         0,
         0,  0,  0,  0,  0,      0, 20, 25, 29, 33,
        36, 39, 39, 39, 40,     40, 41, 41, 42, 42,
        43, 43, 44, 44, 45,     45, 46, 46, 47, 47,
        48, 48, 49, 49, 50,     50, 51, 51, 52, 52,
        53, 53, 54, 54, 55,     55, 56, 56, 57, 57,
        58, 58, 59, 59, 60,     60, 61, 61, 62, 62,
        63, 63, 64, 64, 65,     65, 66, 66, 67, 67,
        68, 68, 69, 69, 70,     70, 71, 71, 72, 72,
        73, 73, 74, 74, 75,     75, 76, 76, 77, 77,
        78, 78, 79, 79, 80,     80, 81, 81, 82, 82,
        83, 83, 84, 84, 85,     85, 86, 86, 87, 87
    ];
    protected override SchoolTypes DamageType => SchoolTypes.Lightning;
    protected override string DamageNoun => "shocking grasp";
}
