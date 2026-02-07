using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Domain;
using Mud.Flags;
using Mud.Random;
using Mud.Server.Ability.Spell;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.SpellGuards;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Creation, PulseWaitTime = 24)]
[Syntax("cast [spell]")]
[Help(
@"This useful spell creates a floating field of force which follows the caser
around, allowing him or her to pile treasure high with no fear of weight
penalties.  It lasts no more than twice the casters level in hours, and
usually less.  It can hold 10 pounds per level of the caster, with a 
maximum of five pounds per item.  The spell requires an open float location
on the character, and the only way to remove the disc is to die or allow it
to run out of energy.")]
[OneLineHelp("creates a floating disc of force perfect for hauling treasure")]
public class FloatingDisc : ItemCreationSpellBase
{
    private const string SpellName = "Floating Disc";

    protected override ISpellGuard[] Guards => [new CannotBeInCombat()];

    private int FloatingDiscBlueprintId { get; }

    public FloatingDisc(ILogger<FloatingDisc> logger, IRandomManager randomManager, IWiznet wiznet, IItemManager itemManager, IOptions<Rom24Options> options)
        : base(logger, randomManager, wiznet, itemManager)
    {
        FloatingDiscBlueprintId = options.Value.SpellBlueprintIds.FloatingDisc;
    }

    protected override void Invoke()
    {
        // TODO: using data is kindy hacky to perform a custom level item
        var floatingDisc = ItemManager.AddItem<IItemContainer>(Guid.NewGuid(), FloatingDiscBlueprintId, $"SpellFloatingDisc[{Caster.DebugName}]", Caster);
        if (floatingDisc == null)
        {
            Caster.Send("The spell fizzles and dies.");
            Wiznet.Log($"SpellFloatingDisc: cannot create item from blueprint {FloatingDiscBlueprintId}.", new WiznetFlags("Bugs"), AdminLevels.Implementor);
            return;
        }
        int maxWeight = Level * 10;
        int maxWeightPerItem = Level * 5;
        int duration = Level * 2 - RandomManager.Range(0, Level / 2);
        floatingDisc.SetTimer(TimeSpan.FromMinutes(duration));
        floatingDisc.SetCustomValues(Level, maxWeight, maxWeightPerItem);

        Caster.Act(ActOptions.ToRoom, "{0:N} {0:h} created a floating black disc.", Caster);
        Caster.Send("You create a floating disc.");
        // TODO: Try to equip it ?
    }
}
