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
[OneLineHelp("inflicts wounds on a foe")]
public class CauseSerious : DamageSpellBase
{
    private const string SpellName = "Cause Serious";

    public CauseSerious(ILogger<CauseSerious> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override SchoolTypes DamageType => SchoolTypes.Harm;
    protected override int DamageValue => RandomManager.Dice(2, 8) + Level / 2;
    protected override string DamageNoun => "spell";
}
