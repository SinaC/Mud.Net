using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Detection)]
[Syntax("cast [spell] <object>")]
[Help(
@"This spell detects the presence of poison in food or drink.")]
[OneLineHelp("determines if food is safe to eat")]
public class DetectPoison : ItemInventorySpellBase<IItemPoisonable>
{
    private const string SpellName = "Detect Poison";

    public DetectPoison(ILogger<DetectPoison> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override string InvalidItemTypeMsg => "It doesn't look poisoned.";

    protected override void Invoke()
    {
        if (Item.IsPoisoned)
            Caster.Send("You smell poisonous fumes.");
        else
            Caster.Send("It looks delicious.");
    }
}
