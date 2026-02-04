using Mud.Domain;
using Mud.Server.Ability.Interfaces;
using Mud.Server.AbilityGroup.Interfaces;
using Mud.Server.Class.Interfaces;
using Mud.Server.Domain;
using Mud.Server.Race.Interfaces;

namespace Mud.Server.Common.Extensions;

public static class ClassesExtensions
{
    public static string Name(this IEnumerable<IClass> classes)
        => classes.Any() ? string.Join(",", classes.Select(x => x.Name)) : "???";

    public static string DisplayName(this IEnumerable<IClass> classes)
        => classes.Any() ? string.Join(",", classes.Select(x => x.DisplayName)) : "???";

    public static string ShortName(this IEnumerable<IClass> classes)
    {
        var count = classes.Count();
        if (count == 0)
            return "???";
        else if (count == 1)
            return classes.Single().ShortName;
        return "MLT";
    }

    public static IEnumerable<ResourceKinds> CurrentResourceKinds(this IEnumerable<IClass> classes, Shapes shape)
        => classes.SelectMany(x => x.CurrentResourceKinds(shape)).Distinct();

    public static IEnumerable<ResourceKinds> ResourceKinds(this IEnumerable<IClass> classes)
        => classes.SelectMany(x => x.ResourceKinds).Distinct();

    // Get the lowest thac0_32 from all classes, then return the corresponding thac0
    public static (int thac0_00, int thac0_32) Thac0(this IEnumerable<IClass> classes)
        => classes.OrderBy(x => x.Thac0.thac0_32).First().Thac0;

    // Get all available abilities from all classes, removing duplicates by keeping the one with the lowest level, highest rating, highest min learned
    public static IEnumerable<IAbilityUsage> AvailableAbilities(this IEnumerable<IClass> classes)
        => classes.SelectMany(x => x.AvailableAbilities).GroupBy(x => x.AbilityDefinition).Select(x => x.OrderBy(x => x.Level).ThenByDescending(x => x.Rating).ThenByDescending(x => x.MinLearned).First());

    public static IEnumerable<IAbilityGroupUsage> AvailableAbilityGroups(this IEnumerable<IClass> classes)
        => classes.SelectMany(x => x.AvailableAbilityGroups).GroupBy(x => x.AbilityGroupDefinition).Select(x => x.OrderByDescending(x => x.Cost).First());

    public static IEnumerable<IAbilityGroupUsage> BasicAbilityGroups(this IEnumerable<IClass> classes)
        => classes.SelectMany(x => x.BasicAbilityGroups).GroupBy(x => x.AbilityGroupDefinition).Select(x => x.OrderByDescending(x => x.Cost).First());

    public static IEnumerable<BasicAttributes> PrimeAttributes(this IEnumerable<IClass> classes)
        => classes.Select(x => x.PrimeAttribute).Distinct();

    public static int MaxHitPointGainPerLevel(this IEnumerable<IClass> classes)
        => classes.Max(x => x.MaxHitPointGainPerLevel);

    public static int MinHitPointGainPerLevel(this IEnumerable<IClass> classes)
        => classes.Max(x => x.MinHitPointGainPerLevel);

    public static int ClassExperiencePercentageMultiplier(this IEnumerable<IClass> classes, IPlayableRace playableRace)
        => classes.Sum(playableRace.ClassExperiencePercentageMultiplier);

    public static int MaxPracticePercentage(this IEnumerable<IClass> classes)
        => classes.Max(x => x.MaxPracticePercentage);

    public static bool IncreasedManaGainWhenLeveling(this IEnumerable<IClass> classes)
        => classes.Any(x => x.IncreasedManaGainWhenLeveling);
}
