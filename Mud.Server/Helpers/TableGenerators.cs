using Mud.Common;
using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Server.Common;
using Mud.Server.Input;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.Race;
using System;
using System.Linq;

namespace Mud.Server.Helpers
{
    public static class TableGenerators
    {
        public static readonly Lazy<TableGenerator<IAbilityLearned>> LearnedAbilitiesTableGenerator = new Lazy<TableGenerator<IAbilityLearned>>(() =>
        {
            // Merge resource and cost if free cost ability
            TableGenerator<IAbilityLearned> generator = new TableGenerator<IAbilityLearned>();
            generator.AddColumn("Lvl", 5, x => x.Level.ToString());
            generator.AddColumn("Name", 23, x => x.Name, new TableGenerator<IAbilityLearned>.ColumnOptions { AlignLeft = true });
            generator.AddColumn("Cost", 10, x =>
            {
                if (x.Learned == 0)
                    return "n/a";
                if (x.AbilityInfo.Type == AbilityTypes.Passive)
                    return "%m%passive%x%";
                if (x.CostAmountOperator == CostAmountOperators.Percentage)
                {
                    if (x.ResourceKind.HasValue)
                        return $"{x.CostAmount}% {x.ResourceKind.Value.ResourceColor()}";
                    else
                        return "???";
                }
                else if (x.CostAmountOperator == CostAmountOperators.Fixed)
                {
                    if (x.ResourceKind.HasValue)
                        return $"{x.CostAmount} {x.ResourceKind.Value.ResourceColor()}";
                    else
                        return "???";
                }
                else
                    return "%W%free cost ability%x%";
            });
            generator.AddColumn("Pra%", 6, 
                x =>
                {
                    if (x.Learned == 0)
                        return "n/a";
                    else
                        return $"{x.Learned}%";
                });
            generator.AddColumn("Type", 10, x => x.AbilityInfo.Type.ToString());
            generator.AddColumn("Cooldown", 10, x => x.AbilityInfo.Cooldown.HasValue ? x.AbilityInfo.Cooldown.Value.FormatDelayShort() : "---");
            return generator;
        });

        public static readonly Lazy<TableGenerator<IArea>> AreaTableGenerator = new Lazy<TableGenerator<IArea>>(() =>
        {
            TableGenerator<IArea> generator = new TableGenerator<IArea>();
            generator.AddColumn("Name", 30, area => area.DisplayName, new TableGenerator<IArea>.ColumnOptions { AlignLeft = true });
            generator.AddColumn("Builders", 15, area => area.Builders, new TableGenerator<IArea>.ColumnOptions { AlignLeft = true });
            generator.AddColumn("Credits", 45, area => area.Credits, new TableGenerator<IArea>.ColumnOptions { AlignLeft = true });
            return generator;
        });

        // Admin specific
        public static readonly Lazy<TableGenerator<IClass>> ClassTableGenerator = new Lazy<TableGenerator<IClass>>(() =>
        {
            TableGenerator<IClass> generator = new TableGenerator<IClass>();
            generator.AddColumn("Name", 20, x => x.DisplayName, new TableGenerator<IClass>.ColumnOptions { AlignLeft = true });
            generator.AddColumn("ShortName", 10, x => x.ShortName);
            generator.AddColumn("Resource(s)", 20, x => string.Join(",", (x.ResourceKinds ?? Enumerable.Empty<ResourceKinds>())?.Select(StringHelpers.ResourceColor)));
            generator.AddColumn("Prime attr", 12, x => x.PrimeAttribute.ShortName());
            generator.AddColumn("#Abilities", 12, x =>
            {
                int count = x.Abilities.Count();
                return count == 0
                    ? "---"
                    : count.ToString();
            });
            return generator;
        });

        public static readonly Lazy<TableGenerator<IPlayableRace>> PlayableRaceTableGenerator = new Lazy<TableGenerator<IPlayableRace>>(() =>
        {
            TableGenerator<IPlayableRace> generator = new TableGenerator<IPlayableRace>();
            generator.AddColumn("Name", 20, x => x.DisplayName, new TableGenerator<IPlayableRace>.ColumnOptions { AlignLeft = true });
            generator.AddColumn("ShortName", 10, x => x.ShortName);
            generator.AddColumn("Size", 10, x => x.Size.ToString());
            generator.AddColumn(BasicAttributes.Strength.ShortName(), 5, x => x.GetMaxAttribute(CharacterAttributes.Strength).ToString());
            generator.AddColumn(BasicAttributes.Intelligence.ShortName(), 5, x => x.GetMaxAttribute(CharacterAttributes.Intelligence).ToString());
            generator.AddColumn(BasicAttributes.Wisdom.ShortName(), 5, x => x.GetMaxAttribute(CharacterAttributes.Wisdom).ToString());
            generator.AddColumn(BasicAttributes.Dexterity.ShortName(), 5, x => x.GetMaxAttribute(CharacterAttributes.Dexterity).ToString());
            generator.AddColumn(BasicAttributes.Constitution.ShortName(), 5, x => x.GetMaxAttribute(CharacterAttributes.Constitution).ToString());
            generator.AddColumn("#Abilities", 12, x =>
            {
                int count = x.Abilities.Count();
                return count == 0
                    ? "---"
                    : count.ToString();
            });
            return generator;
        });

        public static readonly Lazy<TableGenerator<IArea>> FullInfoAreaTableGenerator = new Lazy<TableGenerator<IArea>>(() =>
        {
            TableGenerator<IArea> generator = new TableGenerator<IArea>();
            generator.AddColumn("Name", 30, area => area.DisplayName, new TableGenerator<IArea>.ColumnOptions { AlignLeft = true });
            generator.AddColumn("Builders", 15, area => area.Builders, new TableGenerator<IArea>.ColumnOptions { AlignLeft = true });
            generator.AddColumn("Credits", 45, area => area.Credits, new TableGenerator<IArea>.ColumnOptions { AlignLeft = true });
            generator.AddColumn("Ids", 16, area => $"{area.Rooms.Min(x => x.Blueprint.Id)}-{area.Rooms.Max(x => x.Blueprint.Id)}");
            return generator;
        });

        public static readonly Lazy<TableGenerator<IAbilityInfo>> FullInfoAbilityTableGenerator = new Lazy<TableGenerator<IAbilityInfo>>(() =>
        {
            // Merge resource and cost if free cost ability
            TableGenerator<IAbilityInfo> generator = new TableGenerator<IAbilityInfo>();
            generator.AddColumn("Name", 23, x => x.Name, new TableGenerator<IAbilityInfo>.ColumnOptions { AlignLeft = true });
            generator.AddColumn("Type", 9, x => x.Type.ToString());
            generator.AddColumn("GCD", 5, x => x.PulseWaitTime.ToString());
            generator.AddColumn("Cooldown", 10, x => x.Cooldown.HasValue ? x.Cooldown.Value.FormatDelayShort() : "---");
            generator.AddColumn("WearOff", 20, x => x.CharacterWearOffMessage?.ToString() ?? string.Empty);
            generator.AddColumn("ItemWearOff", 20, x => x.ItemWearOffMessage?.ToString() ?? string.Empty);
            generator.AddColumn("DispelRoom", 20, x => x.DispelRoomMessage?.ToString() ?? string.Empty);
            return generator;
        });

        public static readonly Lazy<TableGenerator<IAbilityUsage>> FullInfoAbilityUsageTableGenerator = new Lazy<TableGenerator<IAbilityUsage>>(() =>
        {
            // Merge resource and cost if free cost ability
            TableGenerator<IAbilityUsage> generator = new TableGenerator<IAbilityUsage>();
            generator.AddColumn("Lvl", 5, x => x.Level.ToString());
            generator.AddColumn("Name", 23, x => x.Name, new TableGenerator<IAbilityUsage>.ColumnOptions { AlignLeft = true });
            generator.AddColumn("Resource", 10,
                x =>
                {
                    if (x.AbilityInfo.Type == AbilityTypes.Passive)
                        return "%m%passive ability%x%";
                    if (x.CostAmountOperator == CostAmountOperators.Percentage || x.CostAmountOperator == CostAmountOperators.Fixed)
                    {
                        if (x.ResourceKind.HasValue)
                            return x.ResourceKind.Value.ResourceColor();
                        else
                            return "???";
                    }
                    return "%W%free cost ability%x%";
                },
                new TableGenerator<IAbilityUsage>.ColumnOptions
                {
                    GetMergeLengthFunc = x =>
                    {
                        if (x.AbilityInfo.Type == AbilityTypes.Passive)
                            return 1;
                        if (x.CostAmountOperator == CostAmountOperators.Percentage || x.CostAmountOperator == CostAmountOperators.Fixed)
                            return 0;
                        return 1;
                    }
                });
            generator.AddColumn("Cost", 8, x => x.CostAmount.ToString(),
                new TableGenerator<IAbilityUsage>.ColumnOptions
                {
                    GetTrailingSpaceFunc = x => x.CostAmountOperator == CostAmountOperators.Percentage ? "%" : " "
                });
            generator.AddColumn("Type", 10, x => x.AbilityInfo.Type.ToString());
            generator.AddColumn("Rating", 8, x => x.Rating.ToString());
            return generator;
        });

        private static string ConvertBool(bool value) => value ? "%y%yes%x%" : "no";

        private static string ConvertPriority(int priority) => priority != CommandAttribute.DefaultPriority ? $"%y%{priority}%x%" : $"{priority}";

        public static readonly Lazy<TableGenerator<CommandMethodInfo>> CommandMethodInfoTableGenerator = new Lazy<TableGenerator<CommandMethodInfo>>(() =>
        {
            TableGenerator<CommandMethodInfo> generator = new TableGenerator<CommandMethodInfo>();
            generator.AddColumn("Method", 20, x => x.MethodInfo.Name, new TableGenerator<CommandMethodInfo>.ColumnOptions { MergeIdenticalValue = true });
            generator.AddColumn("Command", 20, x => x.Attribute.Name, new TableGenerator<CommandMethodInfo>.ColumnOptions { AlignLeft = true });
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
