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
[OneLineHelp("sends forth a single bolt of lightning into an enemy")]
public class LightningBolt : DamageTableSpellBase
{
    private const string SpellName = "Lightning Bolt";

    public LightningBolt(ILogger<LightningBolt> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override int[] Table =>
    [
         0,
         0,  0,  0,  0,  0,      0,  0,  0, 25, 28,
        31, 34, 37, 40, 40,     41, 42, 42, 43, 44,
        44, 45, 46, 46, 47,     48, 48, 49, 50, 50,
        51, 52, 52, 53, 54,     54, 55, 56, 56, 57,
        58, 58, 59, 60, 60,     61, 62, 62, 63, 64,
        64, 65, 65, 66, 66,     67, 68, 68, 69, 69,
        70, 71, 71, 72, 72,     73, 73, 74, 75, 75,
        76, 76, 77, 77, 78,     78, 79, 79, 80, 80,
        81, 81, 82, 82, 83,     83, 84, 84, 85, 85,
        86, 86, 87, 87, 88,     88, 89, 89, 90, 90,
        91, 91, 92, 92, 93,     93, 94, 94, 95, 95
    ];
    protected override SchoolTypes DamageType => SchoolTypes.Lightning;
    protected override string DamageNoun => "lightning bolt";
}
