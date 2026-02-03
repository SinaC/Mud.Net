namespace Mud.Server.AbilityGroup.Interfaces;

public interface IAbilityGroup
{
    string Name { get; }

    IEnumerable<string> AbilityGroups { get; }
    IEnumerable<string> Abilities { get; }
}
