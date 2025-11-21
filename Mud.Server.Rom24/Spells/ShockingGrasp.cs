using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Damage)]
[Syntax("cast [spell] <victim>")]
[Help(
@"These spells inflict damage on the victim.  The higher-level spells do
more damage.")]
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
         0,  0,  0,  0,  0,  0, 20, 25, 29, 33,
        36, 39, 39, 39, 40, 40, 41, 41, 42, 42,
        43, 43, 44, 44, 45, 45, 46, 46, 47, 47,
        48, 48, 49, 49, 50, 50, 51, 51, 52, 52,
        53, 53, 54, 54, 55, 55, 56, 56, 57, 57
    ];
    protected override SchoolTypes DamageType => SchoolTypes.Lightning;
    protected override string DamageNoun => "shocking grasp";
}
