using Mud.Common;
using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.AbilityGroup;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Race;
using System.Text;

namespace Mud.Server.TableGenerator;

public static class TableGenerators
{
    public static readonly Lazy<TableGenerator<string>> UnavailableNamesTableGenerator = new(() =>
    {
        TableGenerator<string> generator = new();
        generator.AddColumn("Name", 20, x => x);
        return generator;
    } );

    public static readonly Lazy<TableGenerator<IAbilityLearned>> LearnedAbilitiesTableGenerator = new(() =>
    {
        TableGenerator<IAbilityLearned> generator = new();
        generator.AddColumn("Lvl", 5, x => x.Level.ToString());
        generator.AddColumn("Name", 23, x => x.Name.ToPascalCase(), new TableGenerator<IAbilityLearned>.ColumnOptions { AlignLeft = true });
        generator.AddColumn("Cost", 12, x =>
        {
            if (x.Learned == 0)
                return "n/a";
            if (x.AbilityDefinition.Type == AbilityTypes.Passive)
                return "%m%passive%x%";
            else if (x.AbilityDefinition.Type == AbilityTypes.Weapon)
                return "%y%weapon%x%";
            if (x.CostAmountOperator == CostAmountOperators.Percentage)
            {
                if (x.ResourceKind.HasValue)
                    return $"{x.CostAmount}% {x.ResourceKind.Value.ResourceColor()}";
                return "???";
            }
            else if (x.CostAmountOperator == CostAmountOperators.Fixed)
            {
                if (x.ResourceKind.HasValue)
                    return $"{x.CostAmount} {x.ResourceKind.Value.ResourceColor()}";
                return "???";
            }
            else if (x.CostAmountOperator == CostAmountOperators.All)
            {
                if (x.ResourceKind.HasValue)
                {
                    if (x.CostAmount > 0)
                        return $"min {x.CostAmount} {x.ResourceKind.Value.ResourceColor()}";
                    return $"all {x.ResourceKind.Value.ResourceColor()}";
                }
                return "???";
            }
            else if (x.CostAmountOperator == CostAmountOperators.None)
                return "%W%free%x%";
            return "???";
        });
        generator.AddColumn("Pra%", 6, 
            x =>
            {
                if (x.Learned == 0)
                    return "n/a";
                return $"{x.Learned}%";
            });
        generator.AddColumn("Type", 10, x => x.AbilityDefinition.Type.ToString());
        generator.AddColumn("Cooldown", 10, x => x.AbilityDefinition.CooldownInSeconds.HasValue ? x.AbilityDefinition.CooldownInSeconds.Value.FormatDelayShort() : "---");
        return generator;
    });

    public static readonly Lazy<TableGenerator<IArea>> FullInfoAreaTableGenerator = new(() =>
    {
        TableGenerator<IArea> generator = new();
        generator.AddColumn("Name", 30, area => area.DisplayName, new TableGenerator<IArea>.ColumnOptions { AlignLeft = true });
        generator.AddColumn("Builders", 15, area => area.Builders, new TableGenerator<IArea>.ColumnOptions { AlignLeft = true });
        generator.AddColumn("Credits", 45, area => area.Credits, new TableGenerator<IArea>.ColumnOptions { AlignLeft = true });
        generator.AddColumn("Ids", 16, area => $"{area.Rooms.Min(x => x.Blueprint.Id)}-{area.Rooms.Max(x => x.Blueprint.Id)}");
        return generator;
    });

    public static readonly Lazy<TableGenerator<IArea>> AreaTableGenerator = new(() =>
    {
        TableGenerator<IArea> generator = new();
        generator.AddColumn("Name", 30, area => area.DisplayName, new TableGenerator<IArea>.ColumnOptions { AlignLeft = true });
        generator.AddColumn("Builders", 15, area => area.Builders, new TableGenerator<IArea>.ColumnOptions { AlignLeft = true });
        generator.AddColumn("Credits", 45, area => area.Credits, new TableGenerator<IArea>.ColumnOptions { AlignLeft = true });
        return generator;
    });

    // Admin specific
    public static readonly Lazy<TableGenerator<IClass>> ClassTableGenerator = new(() =>
    {
        TableGenerator<IClass> generator = new();
        generator.AddColumn("Name", 20, x => x.DisplayName, new TableGenerator<IClass>.ColumnOptions { AlignLeft = true });
        generator.AddColumn("ShortName", 10, x => x.ShortName);
        generator.AddColumn("Resource(s)", 24, x => string.Join(",", (x.ResourceKinds ?? Enumerable.Empty<ResourceKinds>()).Select(StringHelpers.ResourceColor)));
        generator.AddColumn("Prime attr", 12, x => x.PrimeAttribute.ShortName());
        generator.AddColumn("#Abilities", 12, x =>
        {
            int count = x.AvailableAbilities.Count();
            return count == 0
                ? "---"
                : count.ToString();
        });
        return generator;
    });

    public static readonly Lazy<TableGenerator<IRace>> RaceTableGenerator = new(() =>
    {
        TableGenerator<IRace> generator = new();
        generator.AddColumn("Name", 16, x => x.DisplayName, new TableGenerator<IRace>.ColumnOptions { AlignLeft = true });
        generator.AddColumn("Size", 8, x => x.Size.ToString());
        return generator;
    });

    public static readonly Lazy<TableGenerator<IPlayableRace>> PlayableRaceTableGenerator = new(() =>
    {
        TableGenerator<IPlayableRace> generator = new();
        generator.AddColumn("Name", 16, x => x.DisplayName, new TableGenerator<IPlayableRace>.ColumnOptions { AlignLeft = true });
        generator.AddColumn("", 5, x => x.ShortName);
        generator.AddColumn("Size", 8, x => x.Size.ToString());
        generator.AddColumn(BasicAttributes.Strength.ShortName(), 8, x => DisplayPlayableRaceAttribute(x, BasicAttributes.Strength));
        generator.AddColumn(BasicAttributes.Intelligence.ShortName(), 8, x => DisplayPlayableRaceAttribute(x, BasicAttributes.Intelligence));
        generator.AddColumn(BasicAttributes.Wisdom.ShortName(), 8, x => DisplayPlayableRaceAttribute(x, BasicAttributes.Wisdom));
        generator.AddColumn(BasicAttributes.Dexterity.ShortName(), 8, x => DisplayPlayableRaceAttribute(x, BasicAttributes.Dexterity));
        generator.AddColumn(BasicAttributes.Constitution.ShortName(), 8, x => DisplayPlayableRaceAttribute(x, BasicAttributes.Constitution));
        generator.AddColumn("Affects", 10, x => x.CharacterFlags?.ToString() ?? "???");
        generator.AddColumn("Imm", 10, x => x.Immunities?.ToString() ?? "???");
        generator.AddColumn("Res", 10, x => x.Immunities?.ToString() ?? "???");
        generator.AddColumn("Vuln", 10, x => x.Immunities?.ToString() ?? "???");
        generator.AddColumn("#Abilities", 12, x =>
        {
            int count = x.Abilities.Count();
            return count == 0
                ? "---"
                : count.ToString();
        });
        return generator;
    });

    public static readonly Lazy<TableGenerator<IAbilityGroupUsage>> AbilityGroupUsageTableGenerator = new(() =>
    {
        TableGenerator<IAbilityGroupUsage> generator = new();
        generator.AddColumn("Name", 23, x => x.Name.ToPascalCase(), new TableGenerator<IAbilityGroupUsage>.ColumnOptions { AlignLeft = true });
        generator.AddColumn("Cost", 6, x => x.Cost.ToString());
        return generator;
    });

    public static readonly Lazy<TableGenerator<IAbilityGroupDefinition>> AbilityGroupDefinitionTableGenerator = new(() =>
    {
        TableGenerator<IAbilityGroupDefinition> generator = new();
        generator.AddColumn("Name", 23, x => x.Name.ToPascalCase(), new TableGenerator<IAbilityGroupDefinition>.ColumnOptions { AlignLeft = true });
        generator.AddColumn("", 65, x => x.OneLineHelp, new TableGenerator<IAbilityGroupDefinition>.ColumnOptions { AlignLeft = true });
        return generator;
    });

    public static readonly Lazy<TableGenerator<IAbilityDefinition>> AbilityDefinitionTableGenerator = new(() =>
    {
        TableGenerator<IAbilityDefinition> generator = new();
        generator.AddColumn("Name", 23, x => x.Name.ToPascalCase(), new TableGenerator<IAbilityDefinition>.ColumnOptions { AlignLeft = true });
        generator.AddColumn("", 65, x => x.OneLineHelp, new TableGenerator<IAbilityDefinition>.ColumnOptions { AlignLeft = true });
        return generator;
    });

    public static readonly Lazy<TableGenerator<IAbilityUsage>> AbilityUsageTableGenerator = new(() =>
    {
        TableGenerator<IAbilityUsage> generator = new();
        generator.AddColumn("Name", 18, x => x.Name.ToPascalCase());
        generator.AddColumn("Cost", 5, x => x.Rating.ToString());
        return generator;
    });

    public static readonly Lazy<TableGenerator<IAbilityDefinition>> FullInfoAbilityTableGenerator = new(() =>
    {
        TableGenerator<IAbilityDefinition> generator = new();
        generator.AddColumn("Name", 23, x => x.Name.ToPascalCase(), new TableGenerator<IAbilityDefinition>.ColumnOptions { AlignLeft = true });
        generator.AddColumn("Type", 9, x => x.Type.ToString());
        generator.AddColumn("GCD", 5, x => x.PulseWaitTime?.ToString() ?? "???");
        generator.AddColumn("Cooldown", 10, x => x.CooldownInSeconds.HasValue ? x.CooldownInSeconds.Value.FormatDelayShort() : "---");
        generator.AddColumn("WearOff", 20, x => x.CharacterWearOffMessage?.ToString() ?? string.Empty);
        generator.AddColumn("ItemWearOff", 20, x => x.ItemWearOffMessage?.ToString() ?? string.Empty);
        generator.AddColumn("DispelRoom", 20, x => x.DispelRoomMessage?.ToString() ?? string.Empty);
        generator.AddColumn("H", 4, x => ConvertBool(x.Help != null));
        return generator;
    });

    public static readonly Lazy<TableGenerator<IAbilityUsage>> FullInfoAbilityUsageTableGenerator = new(() =>
    {
        // Merge resource and cost if free cost ability
        TableGenerator<IAbilityUsage> generator = new();
        generator.AddColumn("Lvl", 5, x => x.Level.ToString());
        generator.AddColumn("Name", 35, x => x.Name.ToPascalCase(), new TableGenerator<IAbilityUsage>.ColumnOptions { AlignLeft = true });
        generator.AddColumn("Resource", 10,
            x =>
            {
                if (x.AbilityDefinition.Type == AbilityTypes.Passive)
                    return "%m%passive%x%";
                else if (x.AbilityDefinition.Type == AbilityTypes.Weapon)
                    return "%y%weapon%x%";
                if (x.CostAmountOperator == CostAmountOperators.Percentage || x.CostAmountOperator == CostAmountOperators.Fixed || x.CostAmountOperator == CostAmountOperators.All)
                {
                    if (x.ResourceKind.HasValue)
                        return x.ResourceKind.Value.ResourceColor();
                    else
                        return "???";
                }
                else if (x.CostAmountOperator == CostAmountOperators.None)
                    return "%W%free cost ability%x%";
                else
                    return "???";
            },
            new TableGenerator<IAbilityUsage>.ColumnOptions
            {
                GetMergeLengthFunc = x =>
                {
                    if (x.AbilityDefinition.Type == AbilityTypes.Passive || x.AbilityDefinition.Type == AbilityTypes.Weapon)
                        return 1;
                    if (x.CostAmountOperator == CostAmountOperators.Percentage || x.CostAmountOperator == CostAmountOperators.Fixed || x.CostAmountOperator == CostAmountOperators.All)
                        return 0;
                    return 1;
                }
            });
        generator.AddColumn("Cost", 8,
            x =>
            {
                if (x.CostAmountOperator == CostAmountOperators.All)
                {
                    if (x.CostAmount > 0)
                        return $"min {x.CostAmount}";
                    return "all";
                }
                return x.CostAmount.ToString();
            },
            new TableGenerator<IAbilityUsage>.ColumnOptions
            {
                GetTrailingSpaceFunc = x => x.CostAmountOperator == CostAmountOperators.Percentage ? "%" : " "
            });
        generator.AddColumn("Type", 10, x => x.AbilityDefinition.Type.ToString());
        generator.AddColumn("Rating", 8, x => x.Rating.ToString());
        generator.AddColumn("GCD", 5, x => x.AbilityDefinition.PulseWaitTime?.ToString());
        generator.AddColumn("CD", 4, x => x.AbilityDefinition.CooldownInSeconds?.ToString());
        return generator;
    });

    public static readonly Lazy<TableGenerator<IGameActionInfo>> GameActionInfoTableGenerator = new(() =>
    {
        TableGenerator<IGameActionInfo> generator = new();
        generator.AddColumn("Name", 20, x => x.Name.ToPascalCase(), new TableGenerator<IGameActionInfo>.ColumnOptions { AlignLeft = true });
        //generator.AddColumn("Names", 20, x => string.Join(",", x.Names), new TableGenerator<IGameActionInfo>.ColumnOptions { AlignLeft = true });
        generator.AddColumn("Categories", 20, x => string.Join(",", x.Categories));
        generator.AddColumn("Aliases", 20, x => string.Join(",", x.Aliases));
        generator.AddColumn("Prio", 5, x => ConvertPriority(x.Priority));
        generator.AddColumn("SHFN", 6, x => ConvertSHFN(x));
        //generator.AddColumn("Method", 50, x => GetMethodName(x));
        generator.AddColumn("Method", 50, x => x.CommandExecutionType.FullName ?? "???");
        return generator;
    });

    private static string DisplayPlayableRaceAttribute(IPlayableRace race, BasicAttributes attr)
        => $"{race.GetStartAttribute(attr)}->{race.GetMaxAttribute(attr)}";

    private static string ConvertBool(bool value) => value ? "%G%v%x%" : "x";

    private static string ConvertPriority(int priority) => priority != CommandAttribute.DefaultPriority ? $"%y%{priority}%x%" : $"{priority}";

    private static string ConvertSHFN(IGameActionInfo actionInfo)
    {
        StringBuilder sb = new();
        sb.Append(ConvertBool(actionInfo.NoShortcut));
        sb.Append(ConvertBool(actionInfo.Hidden));
        sb.Append(ConvertBool(actionInfo.AddCommandInParameters));
        sb.Append(ConvertBool(!string.IsNullOrWhiteSpace(actionInfo.Help)));
        return sb.ToString();
    }

    //private static string GetMethodName(IGameActionInfo gameActionInfo)
    //{
    //    //=> gameActionInfo.CommandExecutionType.BaseType.Name + "/" + gameActionInfo.CommandExecutionType.Name;
    //    Type baseType = gameActionInfo.CommandExecutionType.BaseType;
    //    while (true)
    //    {
    //        if (baseType == null)
    //            return gameActionInfo.CommandExecutionType.Name;
    //        if (!baseType.Name.Contains("Base"))
    //            break;
    //        baseType = baseType.BaseType;
    //    }
    //    return baseType.Name + "/" + gameActionInfo.CommandExecutionType.Name;
    //}
}
