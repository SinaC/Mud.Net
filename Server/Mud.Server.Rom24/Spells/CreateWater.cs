using Microsoft.Extensions.Logging;
using Mud.Random;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common.Attributes;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.SpellGuards;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Guards;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Creation)]
[Syntax("cast [spell] <drink-container>")]
[Help(
@"This spell replenishes a drink container with water.")]
[OneLineHelp("fills any available container with water")]
public class CreateWater : ItemInventorySpellBase<IItemDrinkContainer>
{
    private const string SpellName = "Create Water";

    protected override ISpellGuard[] Guards => [new CannotBeInCombat()];

    private ITimeManager TimeManager { get; }

    public CreateWater(ILogger<CreateWater> logger, IRandomManager randomManager, ITimeManager timeManager)
        : base(logger, randomManager)
    {
        TimeManager = timeManager;
    }

    protected override string InvalidItemTypeMsg => "It is unable to hold water.";

    public override string? Setup(ISpellActionInput spellActionInput)
    {
        var baseSetup = base.Setup(spellActionInput);
        if (baseSetup != null)
            return baseSetup;

        if (Item.LiquidName != "water" && !Item.IsEmpty)
            return "It contains some other liquid.";
        return null;
    }

    protected override void Invoke()
    {
        int multiplier = TimeManager.SkyState == SkyStates.Raining
            ? 4
            : 2;
        int water = Math.Min(Level * multiplier, Item.MaxLiquid - Item.LiquidLeft);
        if (water > 0)
        {
            Item.Fill("water", water);
            Caster.Act(ActOptions.ToCharacter, "{0:N} is filled.", Item);
        }
    }
}
