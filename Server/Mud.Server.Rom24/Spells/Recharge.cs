using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Enchantment | AbilityEffects.Creation, PulseWaitTime = 24), NotInCombat(Message = StringHelpers.YouLostYourConcentration)]
[Syntax("cast [spell] <item>")]
[Help(
@"The recharge spell is used to restore energy to depleted wands and staves.
Fully exhausted items cannot be recharged, and the difficulty of the spell
is proportional to the number of charges used.  Magic items can only be
recharged one time successfully.")]
[OneLineHelp("restores power to a depleted wand or staff")]
public class Recharge : ItemInventorySpellBase<IItemCastSpellsCharge>
{
    private const string SpellName = "Recharge";

    private IItemManager ItemManager { get; }

    public Recharge(ILogger<Recharge> logger, IRandomManager randomManager, IItemManager itemManager)
        : base(logger, randomManager)
    {
        ItemManager = itemManager;
    }

    protected override string InvalidItemTypeMsg => "That item does not carry charges.";

    protected override void Invoke()
    {
        if (Item.SpellLevel >= (3 * Level) / 2)
        {
            Caster.Send("Your skills are not great enough for that");
            return;
        }

        if (Item.AlreadyRecharged)
        {
            Caster.Send("That item has already been recharged once.");
            return;
        }

        int chance = 40 + 2 * Level;
        chance -= Item.SpellLevel;
        chance -= (Item.MaxChargeCount - Item.CurrentChargeCount) * (Item.MaxChargeCount - Item.CurrentChargeCount);
        chance = Math.Max(Level / 2, chance);
        int percent = RandomManager.Range(1, 100);

        if (percent < chance / 2)
        {
            Caster.Act(ActOptions.ToAll, "{0:N} glows softly.", Item);
            int current = Math.Max(Item.CurrentChargeCount, Item.MaxChargeCount);
            Item.Recharge(current, Item.MaxChargeCount);
            return;
        }

        if (percent <= chance)
        {
            Caster.Act(ActOptions.ToAll, "{0:N} glows softly.", Item);
            int chargeMax = Item.MaxChargeCount - Item.CurrentChargeCount;
            int chargeBack = chargeMax > 0
                ? Math.Max(1, (chargeMax * percent) / 100)
                : 0;
            Item.Recharge(Item.CurrentChargeCount + chargeBack, Item.MaxChargeCount);
            return;
        }

        if (percent <= Math.Min(95, (3 * chance) / 2))
        {
            Caster.Send("Nothing seems to happen.");
            if (Item.MaxChargeCount > 1)
                Item.Recharge(Item.CurrentChargeCount, Item.MaxChargeCount - 1);
            return;
        }

        Caster.Act(ActOptions.ToAll, "{0:N} glows brightly and explodes!", Item);
        ItemManager.RemoveItem(Item);
    }
}
