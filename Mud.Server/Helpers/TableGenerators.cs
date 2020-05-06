using Mud.Domain;
using Mud.Server.Abilities;
using Mud.Server.Common;
using Mud.Server.Input;
using System;
using System.Linq;

namespace Mud.Server.Helpers
{
    public static class TableGenerators
    {
        public static readonly Lazy<TableGenerator<KnownAbility>> KnownAbilityTableGenerator = new Lazy<TableGenerator<KnownAbility>>(() =>
        {
            // Merge resource and cost if free cost ability
            TableGenerator<KnownAbility> generator = new TableGenerator<KnownAbility>();
            generator.AddColumn("Lvl", 5, x => x.Level.ToString());
            generator.AddColumn("Name", 23, x => x.Ability.Name);
            generator.AddColumn("Resource", 10,
                x =>
                {
                    if (x.Ability.Kind == AbilityKinds.Passive)
                        return "%m%passive ability%x%";
                    if (x.CostAmountOperator == CostAmountOperators.Percentage || x.CostAmountOperator == CostAmountOperators.Fixed)
                        return StringHelpers.ResourceColor(x.ResourceKind);
                    return "%W%free cost ability%x%";
                },
                new TableGenerator<KnownAbility>.ColumnOptions
                {
                    GetMergeLengthFunc = x =>
                    {
                        if (x.Ability.Kind == AbilityKinds.Passive)
                            return 1;
                        if (x.CostAmountOperator == CostAmountOperators.Percentage || x.CostAmountOperator == CostAmountOperators.Fixed)
                            return 0;
                        return 1;
                    }
                });
            generator.AddColumn("Cost", 8, x => x.CostAmount.ToString(),
                new TableGenerator<KnownAbility>.ColumnOptions
                {
                    GetTrailingSpaceFunc = x => x.CostAmountOperator == CostAmountOperators.Percentage ? "% " : " "
                });
            generator.AddColumn("%", 5, x => $"{x.Learned}%");
            generator.AddColumn("Type", 10, x => x.Ability.Kind.ToString());
            //TODO: generator.AddColumn("Cooldown", 10, x => x.Ability.Cooldown > 0 ? StringHelpers.FormatDelayShort(x.Ability.Cooldown) : "---");
            return generator;
        });

        public static readonly Lazy<TableGenerator<IArea>> AreaTableGenerator = new Lazy<TableGenerator<IArea>>(() =>
        {
            TableGenerator<IArea> generator = new TableGenerator<IArea>();
            generator.AddColumn("Name", 30, area => area.DisplayName, new TableGenerator<IArea>.ColumnOptions { AlignLeft = true });
            generator.AddColumn("Min", 5, area => area.MinLevel.ToString());
            generator.AddColumn("Max", 5, area => area.MaxLevel.ToString());
            generator.AddColumn("Builders", 15, area => area.Builders, new TableGenerator<IArea>.ColumnOptions { AlignLeft = true });
            generator.AddColumn("Credits", 45, area => area.Credits, new TableGenerator<IArea>.ColumnOptions { AlignLeft = true });
            return generator;
        });

        public static readonly Lazy<TableGenerator<IClass>> ClassTableGenerator = new Lazy<TableGenerator<IClass>>(() =>
        {
            TableGenerator<IClass> generator = new TableGenerator<IClass>();
            generator.AddColumn("Name", 15, x => x.Name);
            generator.AddColumn("ShortName", 10, x => x.ShortName);
            generator.AddColumn("DisplayName", 20, x => x.DisplayName);
            generator.AddColumn("Resource(s)", 30, x => string.Join(",", (x.ResourceKinds ?? Enumerable.Empty<ResourceKinds>())?.Select(StringHelpers.ResourceColor)));
            generator.AddColumn("#Abilities", 12, x =>
            {
                int count = x.Abilities.Count();
                return count == 0
                    ? "---"
                    : count.ToString();
            });
            return generator;
        });

        public static readonly Lazy<TableGenerator<IRace>> RaceTableGenerator = new Lazy<TableGenerator<IRace>>(() =>
        {
            TableGenerator<IRace> generator = new TableGenerator<IRace>();
            generator.AddColumn("Name", 15, x => x.Name);
            generator.AddColumn("ShortName", 10, x => x.ShortName);
            generator.AddColumn("DisplayName", 20, x => x.DisplayName);
            generator.AddColumn("#Abilities", 12, x =>
            {
                int count = x.Abilities.Count();
                return count == 0
                    ? "---"
                    : count.ToString();
            });
            return generator;
        });

        // Admin specific

        public static readonly Lazy<TableGenerator<IArea>> FullInfoAreaTableGenerator = new Lazy<TableGenerator<IArea>>(() =>
        {
            TableGenerator<IArea> generator = new TableGenerator<IArea>();
            generator.AddColumn("Name", 30, area => area.DisplayName, new TableGenerator<IArea>.ColumnOptions { AlignLeft = true });
            generator.AddColumn("Min", 5, area => area.MinLevel.ToString());
            generator.AddColumn("Max", 5, area => area.MaxLevel.ToString());
            generator.AddColumn("Builders", 15, area => area.Builders, new TableGenerator<IArea>.ColumnOptions { AlignLeft = true });
            generator.AddColumn("Credits", 45, area => area.Credits, new TableGenerator<IArea>.ColumnOptions { AlignLeft = true });
            generator.AddColumn("Ids", 16, area => $"{area.Rooms.Min(x => x.Blueprint.Id)}-{area.Rooms.Max(x => x.Blueprint.Id)}");
            return generator;
        });

        public static readonly Lazy<TableGenerator<IAbility>> FullInfoAbilityTableGenerator = new Lazy<TableGenerator<IAbility>>(() =>
        {
            // Merge resource and cost if free cost ability
            TableGenerator<IAbility> generator = new TableGenerator<IAbility>();
            generator.AddColumn("Id", 6, x => x.Id.ToString());
            generator.AddColumn("Name", 23, x => x.Name);
            generator.AddColumn("Type", 10, x => x.Kind.ToString());
            generator.AddColumn("Target", 15, x => ConvertAbilityTargets(x.Target));
            generator.AddColumn("GCD", 5, x => x.PulseWaitTime.ToString());
            generator.AddColumn("Flags", 20, x => x.AbilityFlags.ToString());
            generator.AddColumn("CharDispel", 20, x => x.CharacterDispelMessage?.ToString() ?? string.Empty);
            generator.AddColumn("ItemDispel", 20, x => x.ItemDispelMessage?.ToString() ?? string.Empty);
            return generator;
        });

        public static readonly Lazy<TableGenerator<AbilityUsage>> FullInfoAbilityUsageTableGenerator = new Lazy<TableGenerator<AbilityUsage>>(() =>
        {
            // Merge resource and cost if free cost ability
            TableGenerator<AbilityUsage> generator = new TableGenerator<AbilityUsage>();
            generator.AddColumn("Lvl", 5, x => x.Level.ToString());
            generator.AddColumn("Name", 23, x => x.Ability.Name);
            generator.AddColumn("Resource", 10,
                x =>
                {
                    if (x.Ability.Kind == AbilityKinds.Passive)
                        return "%m%passive ability%x%";
                    if (x.CostAmountOperator == CostAmountOperators.Percentage || x.CostAmountOperator == CostAmountOperators.Fixed)
                        return StringHelpers.ResourceColor(x.ResourceKind);
                    return "%W%free cost ability%x%";
                },
                new TableGenerator<AbilityUsage>.ColumnOptions
                {
                    GetMergeLengthFunc = x =>
                    {
                        if (x.Ability.Kind == AbilityKinds.Passive)
                            return 1;
                        if (x.CostAmountOperator == CostAmountOperators.Percentage || x.CostAmountOperator == CostAmountOperators.Fixed)
                            return 0;
                        return 1;
                    }
                });
            generator.AddColumn("Cost", 8, x => x.CostAmount.ToString(),
                new TableGenerator<AbilityUsage>.ColumnOptions
                {
                    GetTrailingSpaceFunc = x => x.CostAmountOperator == CostAmountOperators.Percentage ? "% " : " "
                });
            generator.AddColumn("Type", 10, x => x.Ability.Kind.ToString());
            generator.AddColumn("Diff", 5, x => x.DifficulityMultiplier.ToString());
            generator.AddColumn("Flags", 20, x => x.Ability.AbilityFlags.ToString());
            return generator;
        });

        private static string ConvertAbilityTargets(AbilityTargets target)
        {
            switch (target)
            {
                case AbilityTargets.None:return "";
                case AbilityTargets.CharacterOffensive: return "Off";
                case AbilityTargets.CharacterDefensive: return "Def";
                case AbilityTargets.CharacterSelf: return "Self";
                case AbilityTargets.ItemInventory: return "Item";
                case AbilityTargets.ItemHereOrCharacterOffensive: return "IOff";
                case AbilityTargets.ItemInventoryOrCharacterDefensive: return "IDef";
                case AbilityTargets.Custom: return "Custom";
                case AbilityTargets.OptionalItemInventory: return "Item?";
                case AbilityTargets.ArmorInventory: return "Armor";
                case AbilityTargets.WeaponInventory:return "Weapon";
                case AbilityTargets.CharacterFighting: return "Fighting";
                case AbilityTargets.CharacterWorldwide: return "World";
                default: return target.ToString();
            }
        }

        private static string ConvertBool(bool value) => value ? "%y%yes%x%" : "no";

        private static string ConvertPriority(int priority) => priority != CommandAttribute.DefaultPriority ? $"%y%{priority}%x%" : $"{priority}";

        public static readonly Lazy<TableGenerator<CommandMethodInfo>> CommandMethodInfoTableGenerator = new Lazy<TableGenerator<CommandMethodInfo>>(() =>
        {
            TableGenerator<CommandMethodInfo> generator = new TableGenerator<CommandMethodInfo>();
            generator.AddColumn("Method", 20, x => x.MethodInfo.Name, new TableGenerator<CommandMethodInfo>.ColumnOptions { MergeIdenticalValue = true });
            generator.AddColumn("Command", 20, x => x.Attribute.Name);
            generator.AddColumn("Categories", 20, x => string.Join(",", x.Attribute.Categories));
            generator.AddColumn("Prio", 5, x => ConvertPriority(x.Attribute.Priority));
            generator.AddColumn("S?", 5, x => ConvertBool(x.Attribute.NoShortcut));
            generator.AddColumn("H?", 5, x => ConvertBool(x.Attribute.Hidden));
            generator.AddColumn("F?", 5, x => ConvertBool(x.Attribute.AddCommandInParameters));
            generator.AddColumn("R?", 3, x => x.MethodInfo.ReturnType == typeof(CommandExecutionResults) ? "" : "%R%X%x%");
            return generator;
        });
    }
}
