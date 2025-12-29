using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.TableGenerator;

namespace Mud.Server.Commands.Character.Ability;

// TODO: level range
/*The skills, spells and powers commands are used to display your character's 
list of available skills (or spells or power, as the case may be).  They are 
listed in order of level, with mana/psp cost (for spells/powers) or percentage 
(for skills) listed where applicable.
Syntax:
 spells        will display spells you have from lvl 1 to your current lvl
 skills all    will display skills you have from lvl 1 to max level
 powers 50     will display powers you have from lvl 1 to lvl 50
 skills 3 60   will display skills you have from lvl 3 to lvl 60
 songs 50      will display songs you have from lvl 1 to lvl 50
*/
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
            .Where(x => (DisplayAll || (x.Level <= Actor.Level && x.Learned > 0)) && AbilityTypeFilterFunc(x.AbilityUsage.AbilityDefinition.Type))
            .OrderBy(x => x.Level)
            .ThenBy(x => x.Name);

        var sb = TableGenerators.LearnedAbilitiesTableGenerator.Value.Generate(Title, abilities);
        Actor.Page(sb);
    }
}
