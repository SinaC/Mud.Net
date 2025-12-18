using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Healing)]
[Syntax("cast [spell] <character>")]
[Help(
@"These spells cure damage on the target character.  The higher-level spells
heal more damage.")]
[OneLineHelp("closes all but the worst wounds")]
public class CureCritical : HealSpellBase
{
    private const string SpellName = "Cure Critical";

    public CureCritical(ILogger<CureCritical> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override string HealVictimPhrase => "You feel better!";
    protected override int HealValue => RandomManager.Dice(3, 8) + Level - 6;
}
