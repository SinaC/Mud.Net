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

[Spell(SpellName, AbilityEffects.Creation)]
[Syntax("cast [spell]")]
[Help(
@"This spell creates a Magic Mushroom, which you or anyone else can eat.")]
[OneLineHelp("produces a nourishing mushroom")]
public class CreateFood : ItemCreationSpellBase
{
    private const string SpellName = "Create Food";

    protected override ISpellGuard[] Guards => [new CannotBeInCombat()];

    private int MushroomBlueprintId { get; }

    public CreateFood(ILogger<CreateFood> logger, IRandomManager randomManager, IWiznet wiznet, IItemManager itemManager, IOptions<Rom24Options> options)
        : base(logger, randomManager, wiznet, itemManager)
    {
        MushroomBlueprintId = options.Value.SpellBlueprintIds.Mushroom;
    }

    protected override void Invoke()
    {
        var mushroom = ItemManager.AddItem<IItemFood>(Guid.NewGuid(), MushroomBlueprintId, $"SpellCreateFood[{Caster.DebugName}]", Caster.Room);
        if (mushroom == null)
        {
            Caster.Send("The spell fizzles and dies.");
            Wiznet.Log($"SpellCreateFood: cannot create item from blueprint {MushroomBlueprintId}.", new WiznetFlags("Bugs"), AdminLevels.Implementor);
            return;
        }
        mushroom.SetHours(Level / 2, Level);
        Caster.Act(ActOptions.ToAll, "{0} suddenly appears.", mushroom);
    }
}
