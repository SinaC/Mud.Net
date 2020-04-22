using Mud.Domain;
using Mud.Server.Abilities;
using Mud.Server.Input;
using System;
using System.Linq;

namespace Mud.Server.Helpers
{
    public static class TableGenerators
    {
        public static readonly Lazy<TableGenerator<AbilityAndLevel>> AbilityAndLevelTableGenerator = new Lazy<TableGenerator<AbilityAndLevel>>(() =>
        {
            // Merge resource and cost if free cost ability
            TableGenerator<AbilityAndLevel> generator = new TableGenerator<AbilityAndLevel>();
            generator.AddColumn("Lvl", 5, x => x.Level.ToString());
            generator.AddColumn("Name", 23, x => x.Ability.Name);
            generator.AddColumn("Resource", 10,
                x =>
                {
                    if ((x.Ability.Flags & AbilityFlags.Passive) == AbilityFlags.Passive)
                        return "%m%passive ability%x%";
                    if (x.Ability.CostType == AmountOperators.Percentage || x.Ability.CostType == AmountOperators.Fixed)
                        return StringHelpers.ResourceColor(x.Ability.ResourceKind);
                    return "%W%free cost ability%x%";
                },
                new TableGenerator<AbilityAndLevel>.ColumnOptions
                {
                    GetMergeLengthFunc = x =>
                    {
                        if ((x.Ability.Flags & AbilityFlags.Passive) == AbilityFlags.Passive)
                            return 1;
                        if (x.Ability.CostType == AmountOperators.Percentage || x.Ability.CostType == AmountOperators.Fixed)
                            return 0;
                        return 1;
                    }
                });
            generator.AddColumn("Cost", 8, x => x.Ability.CostAmount.ToString(),
                new TableGenerator<AbilityAndLevel>.ColumnOptions
                {
                    GetTrailingSpaceFunc = x => x.Ability.CostType == AmountOperators.Percentage ? "% " : " "
                });
            generator.AddColumn("Type", 10, x => x.Ability.Kind.ToString());
            generator.AddColumn("Cooldown", 10, x => x.Ability.Cooldown > 0 ? StringHelpers.FormatDelayShort(x.Ability.Cooldown) : "---");
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
            generator.AddColumn("Ids", 16, area => $"{area.Rooms.Min(x => x.Blueprint.Id)}-{area.Rooms.Max(x => x.Blueprint.Id)}");
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

        public static readonly Lazy<TableGenerator<IAbility>> FullInfoAbilityTableGenerator = new Lazy<TableGenerator<IAbility>>(() =>
        {
            // Merge resource and cost if free cost ability
            TableGenerator<IAbility> generator = new TableGenerator<IAbility>();
            generator.AddColumn("Name", 23, x => x.Name);
            generator.AddColumn("Resource", 10,
                x =>
                {
                    if ((x.Flags & AbilityFlags.Passive) == AbilityFlags.Passive)
                        return "%m%passive ability%x%";
                    if (x.CostType == AmountOperators.Percentage || x.CostType == AmountOperators.Fixed)
                        return StringHelpers.ResourceColor(x.ResourceKind);
                    return "%W%free cost ability%x%";
                },
                new TableGenerator<IAbility>.ColumnOptions
                {
                    GetMergeLengthFunc = x =>
                    {
                        if ((x.Flags & AbilityFlags.Passive) == AbilityFlags.Passive)
                            return 1;
                        if (x.CostType == AmountOperators.Percentage || x.CostType == AmountOperators.Fixed)
                            return 0;
                        return 1;
                    }
                });
            generator.AddColumn("Cost", 8, x => x.CostAmount.ToString(),
                new TableGenerator<IAbility>.ColumnOptions
                {
                    GetTrailingSpaceFunc = x => x.CostType == AmountOperators.Percentage ? "% " : " "
                });
            generator.AddColumn("Type", 10, x => x.Kind.ToString());
            generator.AddColumn("Cooldown", 10, x => x.Cooldown > 0 ? StringHelpers.FormatDelayShort(x.Cooldown) : "---");
            generator.AddColumn("Flags", 20, x => x.Flags.ToString());
            return generator;
        });

        public static readonly Lazy<TableGenerator<AbilityAndLevel>> FullInfoAbilityAndLevelTableGenerator = new Lazy<TableGenerator<AbilityAndLevel>>(() =>
        {
            // Merge resource and cost if free cost ability
            TableGenerator<AbilityAndLevel> generator = new TableGenerator<AbilityAndLevel>();
            generator.AddColumn("Lvl", 5, x => x.Level.ToString());
            generator.AddColumn("Name", 23, x => x.Ability.Name);
            generator.AddColumn("Resource", 10,
                x =>
                {
                    if ((x.Ability.Flags & AbilityFlags.Passive) == AbilityFlags.Passive)
                        return "%m%passive ability%x%";
                    if (x.Ability.CostType == AmountOperators.Percentage || x.Ability.CostType == AmountOperators.Fixed)
                        return StringHelpers.ResourceColor(x.Ability.ResourceKind);
                    return "%W%free cost ability%x%";
                },
                new TableGenerator<AbilityAndLevel>.ColumnOptions
                {
                    GetMergeLengthFunc = x =>
                    {
                        if ((x.Ability.Flags & AbilityFlags.Passive) == AbilityFlags.Passive)
                            return 1;
                        if (x.Ability.CostType == AmountOperators.Percentage || x.Ability.CostType == AmountOperators.Fixed)
                            return 0;
                        return 1;
                    }
                });
            generator.AddColumn("Cost", 8, x => x.Ability.CostAmount.ToString(),
                new TableGenerator<AbilityAndLevel>.ColumnOptions
                {
                    GetTrailingSpaceFunc = x => x.Ability.CostType == AmountOperators.Percentage ? "% " : " "
                });
            generator.AddColumn("Type", 10, x => x.Ability.Kind.ToString());
            generator.AddColumn("Cooldown", 10, x => x.Ability.Cooldown > 0 ? StringHelpers.FormatDelayShort(x.Ability.Cooldown) : "---");
            generator.AddColumn("Flags", 20, x => x.Ability.Flags.ToString());
            return generator;
        });

        public static readonly Lazy<TableGenerator<CommandMethodInfo>> CommandMethodInfoTableGenerator = new Lazy<TableGenerator<CommandMethodInfo>>(() =>
        {
            TableGenerator<CommandMethodInfo> generator = new TableGenerator<CommandMethodInfo>();
            generator.AddColumn("Method", 20, x => x.MethodInfo.Name, new TableGenerator<CommandMethodInfo>.ColumnOptions { MergeIdenticalValue = true });
            generator.AddColumn("Command", 20, x => x.Attribute.Name);
            generator.AddColumn("Category", 15, x => x.Attribute.Category);
            generator.AddColumn("Prio", 5, x => x.Attribute.Priority.ToString());
            generator.AddColumn("S?", 5, x => x.Attribute.NoShortcut ? "yes" : "no");
            generator.AddColumn("H?", 5, x => x.Attribute.Hidden ? "yes" : "no");
            generator.AddColumn("F?", 5, x => x.Attribute.AddCommandInParameters ? "yes" : "no");
            generator.AddColumn("Type", 35, x => x.Attribute.GetType().Name);
            return generator;
        });
    }
}
