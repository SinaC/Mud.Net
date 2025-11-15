using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Creation)]
public class CreateWater : ItemInventorySpellBase<IItemDrinkContainer>
{
    private const string SpellName = "Create Water";

    private ITimeManager TimeManager { get; }

    public CreateWater(IRandomManager randomManager, ITimeManager timeManager)
        : base(randomManager)
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
