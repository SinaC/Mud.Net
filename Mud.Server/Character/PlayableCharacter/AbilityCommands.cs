using System;
using System.Linq;
using System.Text;
using Mud.Domain;
using Mud.Server.Abilities;
using Mud.Server.Common;
using Mud.Server.Input;

namespace Mud.Server.Character.PlayableCharacter
{
    public partial class PlayableCharacter
    {
        [Command("gain", "Ability")]
        [Syntax(
            "[cmd] list",
            "[cmd] skills|spells",
            "[cmd] convert|revert",
            "[cmd] <ability>")]
        protected virtual CommandExecutionResults DoGain(string rawParameters, params CommandParameter[] parameters)
        {
            INonPlayableCharacter trainer = Room.NonPlayableCharacters.FirstOrDefault(x => CanSee(x) && x.ActFlags.HasFlag(ActFlags.Gain));
            if (trainer == null)
            {
                Send("You can't do that here.");
                return CommandExecutionResults.TargetNotFound;
            }

            if (parameters.Length == 0)
            {
                Act(ActOptions.ToCharacter, "{0:N} tells you 'Pardon me?'", trainer);
                return CommandExecutionResults.SyntaxError;
            }

            // List
            if (StringCompareHelpers.StringStartsWith("list", parameters[0].Value))
            {
                StringBuilder sb = TrainAbilityTableGenerator.Value.Generate("Abilities",  3, KnownAbilities.Where(x => x.CanBeGained(this)).OrderBy(x => x.Level).ThenBy(x => x.Ability.Name));
                Send(sb);
                return CommandExecutionResults.Ok;
            }
            // skills + passives
            if (StringCompareHelpers.StringStartsWith("skills", parameters[0].Value))
            {
                StringBuilder sb = TrainAbilityTableGenerator.Value.Generate("Skills+Passives", 3, KnownAbilities.Where(x => (x.Ability.Kind == AbilityKinds.Skill || x.Ability.Kind == AbilityKinds.Passive) && x.CanBeGained(this)).OrderBy(x => x.Level).ThenBy(x => x.Ability.Name));
                Send(sb);
                return CommandExecutionResults.Ok;
            }
            // spells
            if (StringCompareHelpers.StringStartsWith("spells", parameters[0].Value))
            {
                StringBuilder sb = TrainAbilityTableGenerator.Value.Generate("Spells", 3, KnownAbilities.Where(x => x.Ability.Kind == AbilityKinds.Spell && x.CanBeGained(this)).OrderBy(x => x.Level).ThenBy(x => x.Ability.Name));
                Send(sb);
                return CommandExecutionResults.Ok;
            }
            // convert
            if (StringCompareHelpers.StringStartsWith("convert", parameters[0].Value))
            {
                if (Practices < 10)
                {
                    Act(ActOptions.ToCharacter, "{0:N} tells you 'You are not yet ready.'", trainer);
                    return CommandExecutionResults.InvalidTarget;
                }
                Practices -= 10;
                Trains++;
                Act(ActOptions.ToCharacter, "{0:N} helps you apply your practice to training", trainer);
            }
            // convert
            if (StringCompareHelpers.StringStartsWith("revert", parameters[0].Value))
            {
                if (Trains < 1)
                {
                    Act(ActOptions.ToCharacter, "{0:N} tells you 'You are not yet ready.'", trainer);
                    return CommandExecutionResults.InvalidTarget;
                }
                Practices += 10;
                Trains--;
                Act(ActOptions.ToCharacter, "{0:N} helps you apply your train to practicing", trainer);
            }
            // Gain ability
            // TODO: search among all abilities even if can't be learned, not yet be learned, already learned ?
            KnownAbility knownAbility = KnownAbilities.FirstOrDefault(x => x.CanBeGained(this) && StringCompareHelpers.StringStartsWith(x.Ability.Name, parameters[0].Value));
            if (knownAbility == null)
            {
                Act(ActOptions.ToCharacter, "{0:N} tells you 'This is beyond your powers.'", trainer);
                return CommandExecutionResults.TargetNotFound;
            }
            if (Trains < knownAbility.Rating)
            {
                Act(ActOptions.ToCharacter, "{0:N} tells you 'You are not yet ready for that skill.'", trainer);
                return CommandExecutionResults.InvalidTarget;
            }
            // Let's go
            knownAbility.Learned = 1;
            Trains -= knownAbility.Rating;
            Act(ActOptions.ToCharacter, "{0:N} trains you in the art of {1}", trainer, knownAbility.Ability.Name);

            return CommandExecutionResults.Ok;
        }

        [Command("practice", "Ability")]
        [Syntax(
            "[cmd]",
            "[cmd] <ability>")]
        protected virtual CommandExecutionResults DoPractice(string rawParameters, params CommandParameter[] parameters)
        {
            // list
            if (parameters.Length == 0) // no practicer needed to see list
            {
                StringBuilder sb = PracticeAbilityTableGenerator.Value.Generate("Abilities", 3, KnownAbilities.Where(x => x.CanBePracticed(this)).OrderBy(x => x.Level).ThenBy(x => x.Ability.Name));
                sb.AppendFormatLine("You have {0} practice sessions left.", Practices);
                Send(sb);
                return CommandExecutionResults.Ok;
            }
            // practice
            INonPlayableCharacter practicer = Room.NonPlayableCharacters.FirstOrDefault(x => CanSee(x) && x.ActFlags.HasFlag(ActFlags.Practice));
            if (practicer == null)
            {
                Send("You can't do that here.");
                return CommandExecutionResults.TargetNotFound;
            }
            if (Practices < 0)
            {
                Send("You have no practice sessions left.");
                return CommandExecutionResults.InvalidTarget;
            }
            // search ability
            // TODO: search among already max percentage and display another message
            KnownAbility knownAbility = KnownAbilities.FirstOrDefault(x => x.CanBePracticed(this) && StringCompareHelpers.StringStartsWith(x.Ability.Name, parameters[0].Value));
            if (knownAbility == null)
            {
                Send("You can't practice that.");
                return CommandExecutionResults.TargetNotFound;
            }
            if (knownAbility.Learned >= (Class?.MaxPracticePercentage ?? 50))
            {
                Send("You are already learned at {0}", knownAbility.Ability.Name);
                return CommandExecutionResults.InvalidTarget;
            }
            // let's go
            Practices--;
            int learned = Math.Max(1, TableValues.LearnBonus(this) / knownAbility.Rating);
            knownAbility.Learned += learned;
            int maxPractice = Class?.MaxPracticePercentage ?? 50;
            if (knownAbility.Learned < maxPractice)
            {
                Act(ActOptions.ToCharacter, "You practice {0}.", knownAbility.Ability.Name);
                Act(ActOptions.ToRoom, "{0:N} practice {1}.", this, knownAbility.Ability.Name);
            }
            else
            {
                Act(ActOptions.ToCharacter, "You are now learned at {0}.", knownAbility.Ability.Name);
                Act(ActOptions.ToRoom, "{0:N} is now learned at {1}.", this, knownAbility.Ability.Name);
            }

            return CommandExecutionResults.Ok;
        }

        private static readonly Lazy<TableGenerator<KnownAbility>> TrainAbilityTableGenerator = new Lazy<TableGenerator<KnownAbility>>(() =>
        {
            TableGenerator<KnownAbility> generator = new TableGenerator<KnownAbility>();
            generator.AddColumn("Name", 18, x => x.Ability.Name);
            generator.AddColumn("Lvl", 5, x => x.Level.ToString());
            generator.AddColumn("Cost", 5, x => x.Rating.ToString());
            return generator;
        });

        private static readonly Lazy<TableGenerator<KnownAbility>> PracticeAbilityTableGenerator = new Lazy<TableGenerator<KnownAbility>>(() =>
        {
            TableGenerator<KnownAbility> generator = new TableGenerator<KnownAbility>();
            generator.AddColumn("Name", 18, x => x.Ability.Name);
            generator.AddColumn("Pra%", 6, x => $"{x.Learned}%");
            return generator;
        });
    }
}
