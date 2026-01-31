using Microsoft.Extensions.Options;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using Mud.Server.Options;
using Mud.Server.TableGenerator;

namespace Mud.Server.Commands.Character.Ability;

public abstract class AbilitiesCharacterGameActionBase<TCharacter, TCharacterGameActionInfo> : CharacterGameActionBase<TCharacter, TCharacterGameActionInfo>
    where TCharacter : class, ICharacter
    where TCharacterGameActionInfo : class, ICharacterGameActionInfo
{
    protected override IGuard<TCharacter>[] Guards => [];

    private int MaxLevel { get; }

    protected AbilitiesCharacterGameActionBase(IOptions<WorldOptions> worldOptions)
    {
        MaxLevel = worldOptions.Value.MaxLevel;
    }

    protected int MinLevelDisplay { get; set; }
    protected int MaxLevelDisplay { get; set; }

    protected abstract Func<AbilityTypes, bool> AbilityTypeFilterFunc { get; }
    protected abstract string Title { get; }

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
        {
            MinLevelDisplay = 1;
            MaxLevelDisplay = Actor.Level;
            return null;
        }
        else if (actionInput.Parameters[0].IsAll)
        {
            MinLevelDisplay = 1;
            MaxLevelDisplay = MaxLevel;
            return null;
        }
        else if (actionInput.Parameters[0].IsNumber)
        {
            if (actionInput.Parameters.Length > 1)
            {
                if (actionInput.Parameters[1].IsNumber)
                {
                    MinLevelDisplay = actionInput.Parameters[0].AsNumber;
                    MaxLevelDisplay = actionInput.Parameters[1].AsNumber;
                    return null;
                }
            }
            else
            {
                MinLevelDisplay = 1;
                MaxLevelDisplay = actionInput.Parameters[0].AsNumber;
                return null;
            }
        }

        return BuildCommandSyntax();
    }

    public override void Execute(IActionInput actionInput)
    {
        IEnumerable<IAbilityLearned> abilities = Actor.LearnedAbilities
            .Where(x => x.Level >= MinLevelDisplay && x.Level <= MaxLevelDisplay && AbilityTypeFilterFunc(x.AbilityUsage.AbilityDefinition.Type))
            .OrderBy(x => x.Level)
            .ThenBy(x => x.Name);

        var sb = TableGenerators.LearnedAbilitiesTableGenerator.Value.Generate(Title, abilities);
        Actor.Page(sb);
    }
}
