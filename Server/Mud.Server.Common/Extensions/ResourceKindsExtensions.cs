using Mud.Domain;

namespace Mud.Server.Common.Extensions;

public static class ResourceKindsExtensions
{
    public static ResourceKinds[] MandatoryAvailableResources { get; } = [ResourceKinds.HitPoints, ResourceKinds.MovePoints];
    public static ResourceKinds[] DefaultAvailableResources { get; } = [ResourceKinds.HitPoints, ResourceKinds.MovePoints, ResourceKinds.Mana];

    public static bool IsMandatoryResource(this ResourceKinds resourceKinds)
        => MandatoryAvailableResources.Contains(resourceKinds);

    public static string DisplayName(this ResourceKinds resource)
        => resource switch
        {
            ResourceKinds.HitPoints => "Hp",
            ResourceKinds.MovePoints => "Move",
            ResourceKinds.Mana => "Mana",
            ResourceKinds.Psy => "Psy",
            ResourceKinds.Energy => "Energy",
            ResourceKinds.Rage => "Rage",
            ResourceKinds.Combo => "Combo",
            _ => resource.ToString(),
        };

    public static string ResourceColor(this ResourceKinds resource, bool shortDisplay)
    {
        if (shortDisplay)
            return resource switch
            {
                ResourceKinds.HitPoints => "%W%Hp%x%",
                ResourceKinds.MovePoints => "%W%Mv%x%",
                ResourceKinds.Mana => "%B%M%x%",
                ResourceKinds.Psy => "%M%P%x%",
                ResourceKinds.Energy => "%Y%E%x%",
                ResourceKinds.Rage => "%R%R%x%",
                ResourceKinds.Combo => "%G%C%x%",
                _ => resource.ToString(),
            };
        else
            return resource switch
            {
                ResourceKinds.HitPoints => "%W%Hp%x%",
                ResourceKinds.MovePoints => "%W%Move%x%",
                ResourceKinds.Mana => "%B%Mana%x%",
                ResourceKinds.Psy => "%M%Psy%x%",
                ResourceKinds.Energy => "%Y%Energy%x%",
                ResourceKinds.Rage => "%R%Rage%x%",
                ResourceKinds.Combo => "%G%Combo%x%",
                _ => resource.ToString(),
            };
    }
}
