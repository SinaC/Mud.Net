namespace Mud.Server.AbilityGroup.Interfaces;

public interface IAbilityGroupUsage
{
    string Name { get; }
    int Cost { get; }
    bool IsBasics { get; }
    IAbilityGroupDefinition AbilityGroupDefinition { get; }
}
