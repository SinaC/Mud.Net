using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common.Attributes;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Damage)]
[Syntax("cast [spell] <victim>")]
[Help(
@"These spells inflict damage on the victim.  The higher-level spells do
more damage.")]
[OneLineHelp("causes major damage to the target")]
public class CauseCritical : DamageSpellBase
{
    private const string SpellName = "Cause Critical";

    public CauseCritical(ILogger<CauseCritical> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override SchoolTypes DamageType => SchoolTypes.Harm;
    protected override int DamageValue => RandomManager.Dice(3, 8) + Level - 6;
    protected override string DamageNoun => "spell";
}
