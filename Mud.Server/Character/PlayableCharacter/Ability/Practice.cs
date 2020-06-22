using Mud.Common;
using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Table;
using System;
using System.Linq;
using System.Text;

namespace Mud.Server.Character.PlayableCharacter.Ability
{
    [PlayableCharacterCommand("practice", "Ability", MinPosition = Positions.Sleeping)]
    [Syntax(
            "[cmd]",
            "[cmd] <ability>")]
    public class Practice : PlayableCharacterGameAction
    {
        private ITableValues TableValues { get; }

        public bool Display { get; protected set; }
        public IAbilityLearned AbilityLearned { get; protected set; }

        public Practice(ITableValues tableValues)
        {
            TableValues = tableValues;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            // display
            if (actionInput.Parameters.Length == 0)
            {
                Display = true;
                return null;
            }

            // practice
            INonPlayableCharacter practicer = Actor.Room.NonPlayableCharacters.FirstOrDefault(x => Actor.CanSee(x) && x.ActFlags.HasFlag(ActFlags.Practice));
            if (practicer == null)
                return "You can't do that here.";
            if (Actor.Practices < 0)
                return "You have no practice sessions left.";
            // search ability
            // TODO: search among already max percentage and display another message
            AbilityLearned = Actor.LearnedAbilities.FirstOrDefault(x => x.CanBePracticed(Actor) && StringCompareHelpers.StringStartsWith(x.Name, actionInput.Parameters[0].Value));
            if (AbilityLearned == null)
                return "You can't practice that.";
            if (AbilityLearned.Learned >= (Actor.Class?.MaxPracticePercentage ?? 50))
                return $"You are already learned at {AbilityLearned.Name}";
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            if (Display)
            {
                StringBuilder sb = PracticeAbilityTableGenerator.Value.Generate("Abilities", 3, Actor.LearnedAbilities.Where(x => x.CanBePracticed(Actor)).OrderBy(x => x.Level).ThenBy(x => x.Name));
                sb.AppendFormatLine("You have {0} practice sessions left.", Actor.Practices);
                Actor.Send(sb);
                return;
            }

            Actor.UpdateTrainsAndPractices(0, -1);
            int learned = Math.Max(1, TableValues.LearnBonus(Actor) / AbilityLearned.Rating);
            AbilityLearned.IncrementLearned(learned);
            int maxPractice = Actor.Class?.MaxPracticePercentage ?? 50;
            if (AbilityLearned.Learned < maxPractice)
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

        private static readonly Lazy<TableGenerator<IAbilityLearned>> PracticeAbilityTableGenerator = new Lazy<TableGenerator<IAbilityLearned>>(() =>
        {
            TableGenerator<IAbilityLearned> generator = new TableGenerator<IAbilityLearned>();
            generator.AddColumn("Name", 18, x => x.Name);
            generator.AddColumn("Pra%", 6, x => $"{x.Learned}%");
            return generator;
        });
    }
}
