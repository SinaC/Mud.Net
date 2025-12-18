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
[OneLineHelp("send a weak energy bolt into a foe")]
public class MagicMissile : DamageTableSpellBase
{
    private const string SpellName = "Magic Missile";

    public MagicMissile(ILogger<MagicMissile> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override int[] Table =>
    [
         0,
         3,  3,  4,  4,  5,  6,  6,  6,  6,  6,
         7,  7,  7,  7,  7,  8,  8,  8,  8,  8,
         9,  9,  9,  9,  9, 10, 10, 10, 10, 10,
        11, 11, 11, 11, 11, 12, 12, 12, 12, 12,
        13, 13, 13, 13, 13, 14, 14, 14, 14, 14
    ];
    protected override SchoolTypes DamageType => SchoolTypes.Energy;
    protected override string DamageNoun => "magic missile";
}
