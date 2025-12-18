using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Damage)]
[Syntax("cast [spell] <victim>")]
[Help(
@"This spell inflicts damage on the victim.")]
[OneLineHelp("sends a column of flame from the heavens")]
public class Flamestrike : DamageSpellBase
{
    private const string SpellName = "Flamestrike";

    public Flamestrike(ILogger<Flamestrike> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override SchoolTypes DamageType => SchoolTypes.Fire;
    protected override int DamageValue => RandomManager.Dice(6 + Level / 2, 8);
    protected override string DamageNoun => "flamestrike";
}
