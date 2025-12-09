using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Healing)]
[Syntax("cast [spell] <character>")]
[Help(
@"These spells cure damage on the target character.  The higher-level spells
heal more damage.")]
[OneLineHelp("heals wounds")]
public class CureSerious : HealSpellBase
{
    private const string SpellName = "Cure Serious";

    public CureSerious(ILogger<CureSerious> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override string HealVictimPhrase => "You feel better!";
    protected override int HealValue => RandomManager.Dice(2, 8) + Level / 2;
}
