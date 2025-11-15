using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.Ability;

public abstract class AbilitiesCharacterGameActionBase : CharacterGameAction
{
    protected bool DisplayAll { get; set; }

    protected abstract Func<AbilityTypes, bool> AbilityTypeFilterFunc { get; }
    protected abstract string Title { get; }

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length > 0 && actionInput.Parameters[0].IsAll)
            DisplayAll = true;
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        IEnumerable<IAbilityLearned> abilities = Actor.LearnedAbilities
            //.Where(x => (displayAll || x.Level <= Level) && (displayAll || x.Learned > 0) && filterOnAbilityKind(x.Ability.Kind))
            .Where(x => (DisplayAll || x.Level <= Actor.Level) && AbilityTypeFilterFunc(x.AbilityInfo.Type))
            .OrderBy(x => x.Level)
            .ThenBy(x => x.Name);

        var sb = TableGenerators.LearnedAbilitiesTableGenerator.Value.Generate("Abilities", abilities);
        Actor.Page(sb);
    }
}
