using Mud.Common;
using Mud.Server.Blueprints.Item;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using System.Text;

namespace Mud.Server.Admin.Information;

[AdminCommand("iinfo", "Information")]
[Alias("oinfo")]
[Syntax("[cmd] <id>")]
public class Iinfo : AdminGameAction
{
    private IItemManager ItemManager { get; }

    public Iinfo(IItemManager itemManager)
    {
        ItemManager = itemManager;
    }

    protected ItemBlueprintBase Blueprint { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            return BuildCommandSyntax();

        if (!actionInput.Parameters[0].IsNumber)
            return BuildCommandSyntax();

        int id = actionInput.Parameters[0].AsNumber;
        Blueprint = ItemManager.GetItemBlueprint(id)!;
        if (Blueprint == null)
            return "Not found.";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        StringBuilder sb = new();
        sb.AppendFormatLine("Id: {0} Type: {1}", Blueprint.Id, Blueprint.GetType());
        sb.AppendFormatLine("Name: {0}", Blueprint.Name);
        sb.AppendFormatLine("ShortDescription: {0}", Blueprint.ShortDescription);
        sb.AppendFormatLine("Description: {0}", Blueprint.Description);
        sb.AppendFormatLine("Level: {0} Weight: {1}", Blueprint.Level, Blueprint.Weight);
        sb.AppendFormatLine("Cost: {0} NoTake: {1}", Blueprint.Cost, Blueprint.NoTake);
        sb.AppendFormatLine("Flags: {0} WearLocation: {1}", Blueprint.ItemFlags, Blueprint.WearLocation);
        if (Blueprint.ExtraDescriptions != null)
        {
            foreach (var lookup in Blueprint.ExtraDescriptions)
                foreach (string extraDescr in lookup)
                {
                    sb.AppendFormatLine("ExtraDescription: {0}", lookup.Key);
                    sb.AppendLine(extraDescr);
                }
        }
        switch (Blueprint)
        {
            case ItemCastSpellsNoChargeBlueprintBase noChargeBlueprint: // pill, potion, scroll
                sb.AppendFormatLine("Level: {0} Spell1: {1} Spell2: {2} Spell3: {3} Spell4: {4}", noChargeBlueprint.SpellLevel, noChargeBlueprint.Spell1, noChargeBlueprint.Spell2, noChargeBlueprint.Spell3, noChargeBlueprint.Spell4);
                break;
            case ItemCastSpellsChargeBlueprintBase chargeBlueprint: // wand, staff
                sb.AppendFormatLine("Level: {0} #MaxCharge: {1} #CurrentCharge: {2} Spell: {3} AlreadyRecharged: {4}", chargeBlueprint.SpellLevel, chargeBlueprint.MaxChargeCount, chargeBlueprint.CurrentChargeCount, chargeBlueprint.Spell, chargeBlueprint.AlreadyRecharged);
                break;

            case ItemArmorBlueprint armor:
                sb.AppendFormatLine("Bash: {0} Pierce: {1} Slash: {2} Exotic: {3}", armor.Bash, armor.Pierce, armor.Slash, armor.Exotic);
                break;
            case ItemBoatBlueprint _:
                break;
            case ItemContainerBlueprint container:
                sb.AppendFormatLine("MaxWeight: {0} Key: {1} MaxWeightPerItem: {2} WeightMultiplier: {3} Flags: {4}", container.MaxWeight, container.Key, container.MaxWeightPerItem, container.WeightMultiplier, container.ContainerFlags);
                break;
            case ItemCorpseBlueprint _:
                break;
            case ItemDrinkContainerBlueprint drinkContainer:
                sb.AppendFormatLine("MaxLiquid: {0} CurrentLight: {1} LiquidType: {2} IsPoisoned: {3}", drinkContainer.MaxLiquidAmount, drinkContainer.CurrentLiquidAmount, drinkContainer.LiquidType, drinkContainer.IsPoisoned);
                break;
            case ItemFoodBlueprint foodBlueprint:
                sb.AppendFormatLine("FullHours: {0} HungerHours: {1} IsPoisoned: {2}", foodBlueprint.FullHours, foodBlueprint.HungerHours, foodBlueprint.IsPoisoned);
                break;
            case ItemFountainBlueprint fountainBlueprint:
                sb.AppendFormatLine("LiquidType: {0}", fountainBlueprint.LiquidType);
                break;
            case ItemFurnitureBlueprint furnitureBlueprint:
                sb.AppendFormatLine("MaxPeople: {0} MaxWeight: {1} Action: {2} Place: {3} HealBonus: {4} ResourceBonus: {5}", furnitureBlueprint.MaxPeople, furnitureBlueprint.MaxWeight, furnitureBlueprint.FurnitureActions, furnitureBlueprint.FurniturePlacePreposition, furnitureBlueprint.HealBonus, furnitureBlueprint.ResourceBonus);
                break;
            case ItemJewelryBlueprint _:
                break;
            case ItemKeyBlueprint _:
                break;
            case ItemLightBlueprint lightBlueprint:
                sb.AppendFormatLine("DurationHours: {0}", lightBlueprint.DurationHours);
                break;
            case ItemMoneyBlueprint moneyBlueprint:
                sb.AppendFormatLine("Silver: {0} Gold: {0}", moneyBlueprint.SilverCoins, moneyBlueprint.GoldCoins);
                break;
            case ItemPortalBlueprint portalBlueprint:
                sb.AppendFormatLine("Destination: {0} Key: {1} Flags: {2} #MaxCharge: {3} #CurrentCharge: {4}", portalBlueprint.Destination, portalBlueprint.Key, portalBlueprint.PortalFlags, portalBlueprint.MaxChargeCount, portalBlueprint.CurrentChargeCount);
                break;
            case ItemQuestBlueprint _:
                break;
            case ItemShieldBlueprint shieldBlueprint:
                sb.AppendFormatLine("Armor: {0}", shieldBlueprint.Armor);
                break;
            case ItemTreasureBlueprint _:
                break;
            case ItemWarpStoneBlueprint _:
                break;
            case ItemWeaponBlueprint weaponBlueprint:
                sb.AppendFormatLine("WeaponType: {0} damage: {1}d{2} DamageType: {3} Flags: {4} DamageNoun: {5}", weaponBlueprint.Type, weaponBlueprint.DiceCount, weaponBlueprint.DiceValue, weaponBlueprint.DamageType, weaponBlueprint.Flags, weaponBlueprint.DamageNoun);
                break;
        }
        Actor.Send(sb);
    }
}
