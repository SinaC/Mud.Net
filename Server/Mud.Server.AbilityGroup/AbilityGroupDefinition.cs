using Mud.Server.Ability.Interfaces;
using Mud.Server.AbilityGroup.Interfaces;
using Mud.Server.Domain.Attributes;

namespace Mud.Server.Ability.AbilityGroup;

public class AbilityGroupDefinition : IAbilityGroupDefinition
{
    public string Name { get; }
    public string? Help { get; }
    public string? OneLineHelp { get; }

    public IEnumerable<IAbilityGroupDefinition> AbilityGroupDefinitions { get; }
    public IEnumerable<IAbilityDefinition> AbilityDefinitions { get; }


    public AbilityGroupDefinition(string name, IEnumerable<IAbilityGroupDefinition> abilityGroupDefinitions, IEnumerable<IAbilityDefinition> abilityDefinitions, HelpAttribute? helpAttribute, OneLineHelpAttribute? oneLineHelpAttribute)
    {
        Name = name;
        Help = helpAttribute?.Help;
        OneLineHelp = oneLineHelpAttribute?.OneLineHelp;
        AbilityGroupDefinitions = abilityGroupDefinitions;
        AbilityDefinitions = abilityDefinitions;
    }
}
