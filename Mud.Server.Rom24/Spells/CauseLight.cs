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
[OneLineHelp("inflicts minor wounds on an enemy")]
public class CauseLight : DamageSpellBase
{
    private const string SpellName = "Cause Light";

    public CauseLight(ILogger<CauseLight> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override SchoolTypes DamageType => SchoolTypes.Harm;
    protected override int DamageValue => RandomManager.Dice(1, 8) + Level / 3;
    protected override string DamageNoun => "spell";
}
