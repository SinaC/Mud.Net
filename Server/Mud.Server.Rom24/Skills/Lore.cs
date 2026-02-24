using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Domain;
using Mud.Flags;
using Mud.Random;
using Mud.Server.Ability.Skill;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Guards.CharacterGuards;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Table;
using System.Text;

namespace Mud.Server.Rom24.Skills;

[CharacterCommand("lore", "Ability", "Skill", "Information")]
[Syntax("[cmd] <item>")]
[Skill(SkillName, AbilityEffects.None, PulseWaitTime = 36)]
[Help(
@"Lore is a general skill, consisting of knowledge of myths and legends. Use
of the lore skill gives a chance of obtaining information on an object,
concerning its power and uses.  It also may occasionally increase the value
of an object, because more will be known about its worth.  All classes may
learn lore, although thieves are best at it, and warriors find it very hard
to use.")]
[OneLineHelp("the lore of magical items")]
public class Lore : ItemInventorySkillBase
{
    private const string SkillName = "Lore";

    protected override IGuard<ICharacter>[] Guards => [new RequiresAtLeastOneArgument { Message = "Check the lore on what ?" }];

    private ITableValues TableValues { get; }
    private IWiznet Wiznet { get; }

    public Lore(ILogger<ItemInventorySkillBase> logger, IRandomManager randomManager, ITableValues tableValues, IWiznet wiznet)
        : base(logger, randomManager)
    {
        TableValues = tableValues;
        Wiznet = wiznet;
    }

    protected override bool Invoke()
    {
        if (!RandomManager.Chance(Learned))
        {
            Actor.Act(ActOptions.ToCharacter, "You see nothing special about {0}.", Item);
            return false;
        }

        StringBuilder sb = new();
        sb.AppendFormatLine("Object {0} is type {1}, extra flags {2}.", Item.DisplayName, Item.ItemType, Item.ItemFlags.ToString() ?? "(none)");
        sb.AppendFormatLine("Weight is {0}, value is {1}, level is {2}.", Item.TotalWeight / 10, Item.Cost, Item.Level);

        switch (Item)
        {
            case IItemCastSpellsNoCharge itemCastSpellsNoCharge:
                sb.AppendFormat("Level {0} spells of:", itemCastSpellsNoCharge.Level);
                AppendSpell(sb, itemCastSpellsNoCharge.FirstSpellName!);
                AppendSpell(sb, itemCastSpellsNoCharge.SecondSpellName!);
                AppendSpell(sb, itemCastSpellsNoCharge.ThirdSpellName!);
                AppendSpell(sb, itemCastSpellsNoCharge.FourthSpellName!);
                sb.AppendFormatLine(".");
                break;
            case IItemCastSpellsCharge itemCastSpellsCharge:
                sb.AppendFormat("Has {0} charges of level {1}", itemCastSpellsCharge.CurrentChargeCount, itemCastSpellsCharge.SpellLevel);
                AppendSpell(sb, itemCastSpellsCharge.SpellName!);
                sb.AppendFormatLine(".");
                break;
            case IItemDrinkable itemDrinkable:
                if (itemDrinkable.LiquidName == null)
                {
                    Wiznet.Log($"Invalid liquid name {itemDrinkable.LiquidName} item {itemDrinkable.DebugName}", new WiznetFlags("Bugs"), AdminLevels.Implementor);
                    sb.AppendLine("It holds a mysterious liquid");
                }
                else
                {
                    var (name, color, _, _, _, _, _) = TableValues.LiquidInfo(itemDrinkable.LiquidName);
                    sb.AppendFormatLine("It holds {0}-colored {1}.", color, name);
                }
                break;
            case IItemContainer itemContainer:
                sb.AppendFormatLine("Maximum weight {0}# Maximum weight per item: {1}# flags: {2} Weight multiplier: {3}%", itemContainer.MaxWeight, itemContainer.MaxWeightPerItem, itemContainer.ContainerFlags, itemContainer.WeightMultiplier);
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

        Actor.Send(sb);

        return true;
    }

    private static StringBuilder AppendSpell(StringBuilder sb, string spell)
    {
        if (!string.IsNullOrWhiteSpace(spell))
            sb.AppendFormat(" '{0}'", spell);
        return sb;
    }
}
