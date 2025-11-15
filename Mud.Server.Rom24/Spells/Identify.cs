using Mud.Common;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Table;
using Mud.Server.Random;
using System.Text;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Detection)]
public class Identify : ItemInventorySpellBase
{
    private ITableValues TableValues { get; }

    private const string SpellName = "Identify";

    public Identify(IRandomManager randomManager, ITableValues tableValues)
        : base(randomManager)
    {
        TableValues = tableValues;
    }

    protected override void Invoke()
    {
        StringBuilder sb = new ();
        sb.AppendFormatLine("Object {0} is type {1}, extra flags {2}.", Item.DisplayName, Item.GetType().Name.Replace("IItem", string.Empty).Replace("Item", string.Empty), Item.ItemFlags.ToString() ?? "(none)");
        sb.AppendFormatLine("Weight is {0}, value is {1}, level is {2}.", Item.TotalWeight/10, Item.Cost, Item.Level);

        switch (Item)
        {
            case IItemCastSpellsNoCharge itemCastSpellsNoCharge:
                sb.AppendFormat("Level {0} spells of:", itemCastSpellsNoCharge.Level);
                AppendSpell(sb, itemCastSpellsNoCharge.FirstSpellName);
                AppendSpell(sb, itemCastSpellsNoCharge.SecondSpellName);
                AppendSpell(sb, itemCastSpellsNoCharge.ThirdSpellName);
                AppendSpell(sb, itemCastSpellsNoCharge.FourthSpellName);
                sb.AppendFormatLine(".");
                break;
            case IItemCastSpellsCharge itemCastSpellsCharge:
                sb.AppendFormat("Has {0} charges of level {1}", itemCastSpellsCharge.CurrentChargeCount, itemCastSpellsCharge.SpellLevel);
                AppendSpell(sb, itemCastSpellsCharge.SpellName);
                sb.AppendFormatLine(".");
                break;
            case IItemDrinkable itemDrinkable:
                var liquidInfo = TableValues.LiquidInfo(itemDrinkable.LiquidName);
                sb.AppendFormatLine("It holds {0}-colored {1}.", liquidInfo.color, liquidInfo.name);
                break;
            case IItemContainer itemContainer:
                sb.AppendFormatLine("Maximum weight {0}# Maximum weight per item: {1}# flags: {2} Weight multiplier: {3}%", itemContainer.MaxWeight, itemContainer.MaxWeightPerItem, itemContainer.ContainerFlags.ToString(), itemContainer.WeightMultiplier);
                break;
            case IItemWeapon itemWeapon:
                sb.AppendFormatLine("Weapon type: {0}", itemWeapon.Type.ToString());
                sb.AppendFormatLine("Damage is {0}d{1} (average {2})", itemWeapon.DiceCount, itemWeapon.DiceValue, 1 + (itemWeapon.DiceCount * itemWeapon.DiceValue / 2));
                sb.AppendFormatLine("Weapons flags: {0}", itemWeapon.WeaponFlags);
                break;
            case IItemArmor itemArmor:
                sb.AppendFormatLine("Armor class is {0} pierce, {1} bash, {2} slash, and {3} magic.", itemArmor.Pierce, itemArmor.Bash, itemArmor.Slash, itemArmor.Exotic);
                break;
        }

        if (Item.Auras != null)
        {
            foreach (IAura aura in Item.Auras)
                aura.Append(sb);
        }

        Caster.Send(sb);
    }

    private static StringBuilder AppendSpell(StringBuilder sb, string spell)
    {
        if (!string.IsNullOrWhiteSpace(spell))
            sb.AppendFormatLine(" '{0}'", spell);
        return sb;
    }
}
