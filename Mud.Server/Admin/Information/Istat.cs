using Mud.Common;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mud.Server.Admin.Information
{
    [AdminCommand("istat", "Information")]
    [Alias("ostat")]
    [Syntax("[cmd] <item>")]
    public class Istat : AdminGameAction
    {
        private IItemManager ItemManager { get; }
        private ITableValues TableValues { get; }

        public IItem What { get; protected set; }

        public Istat(IItemManager itemManager, ITableValues tableValues)
        {
            ItemManager = itemManager;
            TableValues = tableValues;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
                return BuildCommandSyntax();

            What = Impersonating == null
                ? FindHelpers.FindByName(ItemManager.Items, actionInput.Parameters[0])
                : FindHelpers.FindItemHere(Impersonating, actionInput.Parameters[0]);
            if (What == null)
                return StringHelpers.NotFound;

            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            StringBuilder sb = new StringBuilder();
            if (What.Blueprint != null)
                sb.AppendFormatLine("Blueprint: {0}", What.Blueprint.Id);
            // TODO: display blueprint
            else
                sb.AppendLine("No blueprint");
            sb.AppendFormatLine("Type: {0}", What.GetType().ToString().AfterLast('.'));
            sb.AppendFormatLine("Name: {0} Keywords: {1}", What.Name, string.Join(",", What.Keywords));
            sb.AppendFormatLine("DisplayName: {0}", What.DisplayName);
            sb.AppendFormatLine("Description: {0}", What.Description);
            if (What.ExtraDescriptions != null)
            {
                foreach (var lookup in What.ExtraDescriptions)
                    foreach (string extraDescr in lookup)
                        sb.AppendFormatLine("ExtraDescription: {0} " + Environment.NewLine + "{1}", lookup.Key, extraDescr);
            }
            sb.AppendFormatLine("Type: {0}", What.GetType().Name);
            if (What.IncarnatedBy != null)
                sb.AppendFormatLine("Incarnated by {0}", What.IncarnatedBy.DisplayName);
            else
                sb.AppendFormatLine("Incarnatable: {0}", What.Incarnatable);
            if (What.ContainedInto != null)
                sb.AppendFormatLine("Contained in {0}", What.ContainedInto.DebugName);
            sb.AppendFormatLine("Equipped by {0} on {1}", What.EquippedBy?.DebugName ?? "(none)", What.WearLocation);
            if (What.NoTake)
                sb.Append("Cannot be taken");
            sb.AppendFormatLine("Level: {0}", What.Level);
            sb.AppendFormatLine("Cost: {0} Weight: {1}", What.Cost, What.Weight);
            sb.AppendFormatLine("CarryCount: {0} TotalWeight: {1}", What.CarryCount, What.TotalWeight);
            if (What.DecayPulseLeft > 0)
                sb.AppendFormatLine("Decay in {0}", (What.DecayPulseLeft / Pulse.PulsePerSeconds).FormatDelay());
            sb.AppendFormatLine("Flags: {0} (base: {1})", What.ItemFlags, What.BaseItemFlags);
            //
            if (What is IItemArmor armor)
                sb.AppendFormatLine("Bash: {0} Pierce: {1} Slash: {2} Exotic: {3}", armor.Bash, armor.Pierce, armor.Slash, armor.Exotic);
            //
            if (What is IItemCastSpellsCharge castSpellsCharge)
                sb.AppendFormatLine("Level: {0} Current: {1} Max: {2} Spell: {3} Already recharged: {4}", castSpellsCharge.SpellLevel, castSpellsCharge.CurrentChargeCount, castSpellsCharge.MaxChargeCount, castSpellsCharge.SpellName ?? "-", castSpellsCharge.AlreadyRecharged);
            //
            if (What is IItemCastSpellsNoCharge castSpellsNoCharge)
                sb.AppendFormatLine("Level: {0} Spell1: {1} Spell2: {2} Spell3: {3} Spell4: {4}", castSpellsNoCharge.SpellLevel, castSpellsNoCharge.FirstSpellName ?? "-", castSpellsNoCharge.SecondSpellName ?? "-", castSpellsNoCharge.ThirdSpellName ?? "-", castSpellsNoCharge.FourthSpellName ?? "-");
            //
            if (What is IItemContainer container)
                sb.AppendFormatLine("Item count: {0} Weight: {1} MaxWeight: {2} Flags: {3} Key: {4} MaxWeightPerItem: {5} Weight multiplier: {6}",
                    container.Content.Count(), container.Weight, container.MaxWeight, container.ContainerFlags, container.KeyId, container.MaxWeightPerItem, container.WeightMultiplier);
            //
            if (What is IItemCorpse)
                sb.AppendLine("No additional informations");
            //
            if (What is IItemDrinkContainer drinkContainer)
            {
                sb.AppendFormatLine("Max: {0} Current: {1} Poisoned: {2}", drinkContainer.MaxLiquid, drinkContainer.LiquidLeft, drinkContainer.IsPoisoned);
                var liquidInfo = TableValues.LiquidInfo(drinkContainer.LiquidName);
                if (liquidInfo != default)
                    sb.AppendFormatLine("Liquid type: {0} color: {1} proof: {2} full: {3} thirst: {4} food: {5} size: {6}", drinkContainer.LiquidName, liquidInfo.color, liquidInfo.proof, liquidInfo.full, liquidInfo.thirst, liquidInfo.food, liquidInfo.servingsize);
                else
                    sb.AppendFormatLine("Liquid type: {0} (not found in liquid table)", drinkContainer.LiquidName);
            }
            //
            if (What is IItemFood food)
                sb.AppendFormatLine(" Full: {0} hours Hungry: {1} hours Poisonned: {2}", food.FullHours, food.HungerHours, food.IsPoisoned);
            //
            if (What is IItemFountain fountain)
            {
                var liquidInfo = TableValues.LiquidInfo(fountain.LiquidName);
                if (liquidInfo != default)
                    sb.AppendFormatLine("Liquid type: {0} color: {1} proof: {2} full: {3} thirst: {4} food: {5} size: {6}", fountain.LiquidName, liquidInfo.color, liquidInfo.proof, liquidInfo.full, liquidInfo.thirst, liquidInfo.food, liquidInfo.servingsize);
                else
                    sb.AppendFormatLine("Liquid type: {0} (not found in liquid table)", fountain.LiquidName);
            }
            //
            if (What is IItemFurniture furniture)
            {
                sb.AppendFormatLine("Actions: {0} Preposition: {1} MaxPeople: {2} HealBonus: {3} ResourceBonus: {4}", furniture.FurnitureActions, furniture.FurniturePlacePreposition, furniture.MaxPeople, furniture.HealBonus, furniture.ResourceBonus);
                List<ICharacter> people = furniture.People.ToList();
                if (people.Count == 0)
                    sb.AppendLine("None is using it");
                else
                {
                    sb.Append("People using it: ");
                    sb.Append(people.Select(x => x.DebugName).Aggregate((n, i) => n + "," + i));
                    sb.AppendLine();
                }
            }
            //
            if (What is IItemLight light)
                sb.AppendFormatLine("Time left: {0}", light.IsInfinite ? "Infinite" : (light.TimeLeft * 60).FormatDelay());
            //
            if (What is IItemMoney money)
                sb.AppendFormatLine("Silver: {0} Gold: {1}", money.SilverCoins, money.GoldCoins);
            //
            if (What is IItemPortal portal)
                sb.AppendFormatLine("Destination: {0} Flags: {1} Key: {2} Current charges: {3} Max charges: {4}", portal.Destination?.DebugName ?? "???", portal.PortalFlags, portal.KeyId, portal.CurrentChargeCount, portal.MaxChargeCount);
            //
            if (What is IItemShield shield)
                sb.AppendFormatLine("Armor: {0}", shield.Armor);
            //
            if (What is IItemWeapon weapon)
                sb.AppendFormatLine("Weapon type: {0}  {1}d{2} {3} {4} (base: {5}) {6}", weapon.Type, weapon.DiceCount, weapon.DiceValue, weapon.DamageType, weapon.WeaponFlags, weapon.BaseWeaponFlags, weapon.DamageNoun);

            // TODO: other item type
            //
            foreach (IAura aura in What.Auras)
                aura.Append(sb);
            //
            Actor.Send(sb);
        }
    }
}
