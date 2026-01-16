using Mud.Common;
using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Table;
using Mud.Server.TableGenerator;

namespace Mud.Server.Commands.Character.PlayableCharacter.Ability;

[PlayableCharacterCommand("practice", "Ability"), MinPosition(Positions.Sleeping)]
[Syntax(
        "[cmd]",
        "[cmd] <ability>")]
[Help(
@"[cmd] without an argument tells you your current ability level in all
the skills and spells available to you.  You can check this anywhere.

[cmd] with an argument practice that skill or spell.  Your learning
percentage varies from 0% (unlearned) to a some maximum between 80% and 100%,
depending on your class.  You must be at a guild master to practice.

The higher your intelligence, the more you will learn at each practice
session.  The higher your wisdom, the more practice sessions you will
have each time you gain a level.  Unused sessions are saved until you
do use them.")]
public class Practice : PlayableCharacterGameAction
{
    private ITableValues TableValues { get; }

    public Practice(ITableValues tableValues)
    {
        TableValues = tableValues;
    }

    protected bool Display { get; set; }
    protected IAbilityLearned AbilityLearned { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        // display
        if (actionInput.Parameters.Length == 0)
        {
            Display = true;
            return null;
        }

        // practice
        var practicer = Actor.Room?.NonPlayableCharacters.FirstOrDefault(x => Actor.CanSee(x) && x.ActFlags.IsSet("Practice"));
        if (practicer == null)
            return "You can't do that here.";
        if (Actor.Practices < 0)
            return "You have no practice sessions left.";
        // search ability
        AbilityLearned = Actor.LearnedAbilities.FirstOrDefault(x => x.CanBePracticed(Actor) && StringCompareHelpers.StringStartsWith(x.Name, actionInput.Parameters[0].Value))!;
        if (AbilityLearned == null)
            return "You can't practice that.";
        if (AbilityLearned.Rating <= 0)
            return $"You cannot practice {AbilityLearned.Name}.";
        if (AbilityLearned.Learned >= (Actor.Class?.MaxPracticePercentage ?? 50))
            return $"You are already learned at {AbilityLearned.Name}";
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        if (Display)
        {
            var sb = PracticeAbilityTableGenerator.Value.Generate("Abilities", new TableGeneratorOptions { ColumnRepetionCount = 3 }, Actor.LearnedAbilities.Where(x => x.CanBePracticed(Actor)).OrderBy(x => x.Level).ThenBy(x => x.Name));
            sb.AppendFormatLine("You have {0} practice sessions left.", Actor.Practices);
            Actor.Send(sb);
            return;
        }

        Actor.UpdateTrainsAndPractices(0, -1);
        var learnedIncrement = Math.Min(75, AbilityLearned.Learned + (TableValues.LearnBonus(Actor) / AbilityLearned.Rating)); // cannot go higher than 75
        AbilityLearned.IncrementLearned(learnedIncrement);
        int maxLearned = Actor.Class?.MaxPracticePercentage ?? 50;
        if (AbilityLearned.Learned < maxLearned)
        {
            Actor.Act(ActOptions.ToCharacter, "You practice {0}.", AbilityLearned.Name);
            Actor.Act(ActOptions.ToRoom, "{0:N} practice {1}.", Actor, AbilityLearned.Name);
        }
        else
        {
            Actor.Act(ActOptions.ToCharacter, "You are now learned at {0}.", AbilityLearned.Name);
            Actor.Act(ActOptions.ToRoom, "{0:N} is now learned at {1}.", Actor, AbilityLearned.Name);
        }
    }

    private static readonly Lazy<TableGenerator<IAbilityLearned>> PracticeAbilityTableGenerator = new(() =>
    {
        TableGenerator<IAbilityLearned> generator = new();
        generator.AddColumn("Name", 18, x => x.Name.ToPascalCase());
        generator.AddColumn("Pra%", 6, x => $"{x.Learned}%");
        return generator;
    });
}
