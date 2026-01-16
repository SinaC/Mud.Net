namespace Mud.Server.Interfaces.AbilityGroup
{
    public interface IAbilityGroup
    {
        string Name { get; }

        IEnumerable<string> AbilityGroups { get; }
        IEnumerable<string> Abilities { get; }
    }
}
