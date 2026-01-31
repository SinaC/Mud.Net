using Mud.Common;
using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.PlayableCharacterGuards;
using Mud.Server.Interfaces.AbilityGroup;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using Mud.Server.TableGenerator;
using System.Text;

namespace Mud.Server.Commands.Character.PlayableCharacter.Ability;

[PlayableCharacterCommand("groups", "Ability", Priority = 999)]
[Alias("info")]
[Syntax(
    "[cmd]",
    "[cmd] group name")]
public class Groups : PlayableCharacterGameAction
{
    protected override IGuard<IPlayableCharacter>[] Guards => [new RequiresMinPosition(Positions.Standing), new CannotBeInCombat()];

    private IAbilityGroupManager AbilityGroupManager { get; }

    public Groups(IAbilityGroupManager abilityGroupManager)
    {
        AbilityGroupManager = abilityGroupManager;
    }

    private bool Display { get; set; }
    private IAbilityGroupDefinition AbilityGroupDefinition { get; set; } = null!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        // display all ability groups
        if (actionInput.Parameters.Length == 0)
        {
            Display = true;
            return null;
        }

        // search ability group
        var abilityGroupDefinition = AbilityGroupManager.Search(actionInput.Parameters[0]);
        if (abilityGroupDefinition == null)
            return "No group of that name exist.";

        AbilityGroupDefinition = abilityGroupDefinition;
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        if (Display)
        {
            var sb = TableGenerators.AbilityGroupDefinitionTableGenerator.Value.Generate("Groups", new TableGeneratorOptions { HideHeaders = true }, AbilityGroupManager.AbilityGroups.OrderBy(x => x.Name));
            Actor.Page(sb);
        }
        else
        {
            var sb = new StringBuilder();
            sb.AppendLine($"%W%{AbilityGroupDefinition.Name.ToPascalCase()}%x%");
            if (AbilityGroupDefinition.Help != null)
            {
                sb.AppendLine(AbilityGroupDefinition.Help);
            }
            if (AbilityGroupDefinition.AbilityDefinitions.Any())
            {
                var abilities = TableGenerators.AbilityDefinitionTableGenerator.Value.Generate("Abilities", new TableGeneratorOptions { HideHeaders = true }, AbilityGroupDefinition.AbilityDefinitions);
                sb.Append(abilities);
            }
            if (AbilityGroupDefinition.AbilityGroupDefinitions.Any())
            {
                var subAbilityGroups = TableGenerators.AbilityGroupDefinitionTableGenerator.Value.Generate("Groups", new TableGeneratorOptions { HideHeaders = true }, AbilityGroupDefinition.AbilityGroupDefinitions);
                sb.Append(subAbilityGroups);
            }
            Actor.Page(sb);
        }
    }
}
