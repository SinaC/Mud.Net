using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Recharge", AbilityEffects.Enchantment | AbilityEffects.Creation, PulseWaitTime = 24)]
    public class Recharge : ItemInventorySpellBase<IItemCastSpellsCharge>
    {
        private IItemManager ItemManager { get; }

        public Recharge(IRandomManager randomManager, IWiznet wiznet, IItemManager itemManager)
            : base(randomManager, wiznet)
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
}
