using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Common;
using Mud.Server.Helpers;
using Mud.Server.Input;
// ReSharper disable UnusedMember.Global

namespace Mud.Server.Character.PlayableCharacter
{
    public partial class PlayableCharacter
    {
        [PlayableCharacterCommand("quest", "Quest", Priority = 1)]
        [Syntax(
            "[cmd]",
            "[cmd] <id>",
            "[cmd] abandon <id>",
            "[cmd] get <quest name>",
            "[cmd] get all",
            "[cmd] list")]
        protected virtual CommandExecutionResults DoQuest(string rawParameters, params CommandParameter[] parameters)
        {
            // no param -> quest info
            if (parameters.Length == 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Quests:");
                if (!Quests.Any())
                    sb.AppendLine("None.");
                else
                {
                    int id = 0;
                    foreach (IQuest quest in Quests)
                    {
                        BuildQuestSummary(sb, quest, id);
                        id++;
                    }
                }
                Page(sb);
                return CommandExecutionResults.Ok;
            }

            // quest id
            if (parameters[0].IsNumber)
            {
                // quest id
                int id = parameters[0].AsNumber;
                IQuest quest = id > 0 ? Quests.ElementAtOrDefault(id - 1) : null; // index starts at 0
                if (quest == null)
                {
                    Send("No such quest.");
                    return CommandExecutionResults.InvalidParameter;
                }
                StringBuilder sb = new StringBuilder();
                BuildQuestSummary(sb, quest, null);
                Page(sb);
                return CommandExecutionResults.Ok;
            }

            // quest abandon id
            if ("abandon".StartsWith(parameters[0].Value))
            {
                var subCommandParameters = CommandHelpers.SkipParameters(parameters, 1);
                return DoQuestAbandon(subCommandParameters.rawParameters, subCommandParameters.parameters);
            }

            // quest complete id
            if ("complete".StartsWith(parameters[0].Value))
            {
                var subCommandParameters = CommandHelpers.SkipParameters(parameters, 1);
                return DoQuestComplete(subCommandParameters.rawParameters, subCommandParameters.parameters);
            }

            // quest get all|title
            if ("get".StartsWith(parameters[0].Value))
            {
                var subCommandParameters = CommandHelpers.SkipParameters(parameters, 1);
                return DoQuestGet(subCommandParameters.rawParameters, subCommandParameters.parameters);
            }

            // quest list
            if ("list".StartsWith(parameters[0].Value))
            {
                var subCommandParameters = CommandHelpers.SkipParameters(parameters, 1);
                return DoQuestList(subCommandParameters.rawParameters, subCommandParameters.parameters);
            }

            return CommandExecutionResults.SyntaxError;
        }

        [PlayableCharacterCommand("qcomplete", "Quest", Priority = 2)]
        [PlayableCharacterCommand("questcomplete", "Quest", Priority = 2)]
        [Syntax(
            "[cmd] <id>",
            "[cmd] all")]
        protected virtual CommandExecutionResults DoQuestComplete(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Complete which quest?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }
            // all
            if (parameters[0].IsAll)
            {
                IReadOnlyCollection<IQuest> clone = new ReadOnlyCollection<IQuest>(Quests.Where(x => x.IsCompleted).ToList());
                foreach (IQuest questToComplete in clone)
                {
                    if (Room.NonPlayableCharacters.Any(x => x == questToComplete.Giver))
                    {
                        questToComplete.Complete();
                        _quests.Remove(questToComplete);
                        Send("You complete '{0}' successfully.", questToComplete.Blueprint.Title);
                    }
                }
                return CommandExecutionResults.Ok;
            }
            // id
            int id = parameters[0].AsNumber;
            IQuest quest = id > 0 ? Quests.ElementAtOrDefault(id - 1) : null;
            if (quest == null)
            {
                Send("No such quest.");
                return CommandExecutionResults.InvalidParameter;
            }
            if (Room.NonPlayableCharacters.All(x => x != quest.Giver))
            {
                Send($"You must be near {quest.Giver.DisplayName} to complete this quest.");
                return CommandExecutionResults.NoExecution;
            }
            if (!quest.IsCompleted)
            {
                Send("Quest '{0}' is not finished!", quest.Blueprint.Title);
                return CommandExecutionResults.InvalidTarget;
            }
            //
            quest.Complete();
            _quests.Remove(quest);

            Send("You complete '{0}' successfully.", quest.Blueprint.Title);
            return CommandExecutionResults.Ok;
        }

        [PlayableCharacterCommand("qabandon", "Quest", Priority = 3)]
        [PlayableCharacterCommand("questabandon", "Quest", Priority = 3)]
        [Syntax("[cmd] <id>")]
        protected virtual CommandExecutionResults DoQuestAbandon(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Abandon which quest?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }
            int id = parameters[0].AsNumber;
            IQuest quest = id > 0 ? Quests.ElementAtOrDefault(id - 1) : null;
            if (quest == null)
            {
                Send("No such quest.");
                return CommandExecutionResults.InvalidParameter;
            }
            //
            quest.Abandon();
            _quests.Remove(quest);

            Send("You abandon '{0}'!", quest.Blueprint.Title);
            return CommandExecutionResults.Ok;
        }

        [PlayableCharacterCommand("qget", "Quest", Priority = 4)]
        [PlayableCharacterCommand("questget", "Quest", Priority = 4)]
        [Syntax(
            "[cmd] <quest name>",
            "[cmd] all")]
        protected virtual CommandExecutionResults DoQuestGet(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Get which quest?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }

            if (!Room.GetNonPlayableCharacters<CharacterQuestorBlueprint>().Any())
            {
                Send("You cannot get any quest here.");
                return CommandExecutionResults.NoExecution;
            }

            // get all
            if (parameters[0].IsAll)
            {
                bool found = false;
                foreach (var questGiver in Room.GetNonPlayableCharacters<CharacterQuestorBlueprint>())
                {
                    if (questGiver.blueprint?.QuestBlueprints?.Any() == true)
                    {
                        foreach (QuestBlueprint questBlueprint in GetAvailableQuestBlueprints(questGiver.blueprint))
                        {
                            GetQuest(questBlueprint, questGiver.character);
                            found = true;
                        }
                    }
                }
                if (!found)
                    Send("You are already running all available quests here.");
                return CommandExecutionResults.Ok;
            }

            // get quest
            string questTitle = parameters[0].Value.ToLowerInvariant();
            // Search quest giver with wanted quest
            foreach (var questGiver in Room.GetNonPlayableCharacters<CharacterQuestorBlueprint>())
            {
                if (questGiver.blueprint?.QuestBlueprints?.Any() == true)
                {
                    foreach (QuestBlueprint questBlueprint in GetAvailableQuestBlueprints(questGiver.blueprint))
                    {
                        if (questBlueprint.Title.ToLowerInvariant().StartsWith(questTitle))
                        {
                            GetQuest(questBlueprint, questGiver.character);
                            return CommandExecutionResults.Ok;
                        }
                    }
                }
            }
            //
            Send("No such quest can be get here.");
            return CommandExecutionResults.Ok;
        }

        [PlayableCharacterCommand("qlist", "Quest", Priority = 5)]
        [PlayableCharacterCommand("questlist", "Quest", Priority = 5)]
        protected virtual CommandExecutionResults DoQuestList(string rawParameters, params CommandParameter[] parameters)
        {
            // Display quests available in this.Room
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Available quests:");
            bool questGiverFound = false;
            bool questAvailable = false;
            foreach (var questGiver in Room.GetNonPlayableCharacters<CharacterQuestorBlueprint>())
            {
                if (questGiver.blueprint?.QuestBlueprints?.Any() == true)
                {
                    foreach (QuestBlueprint questBlueprint in GetAvailableQuestBlueprints(questGiver.blueprint))
                    {
                        sb.AppendLine($"Quest '{questBlueprint.Title}' [lvl:{questBlueprint.Level}]");
                        questAvailable = true;
                    }
                    questGiverFound = true;
                }
            }

            if (!questGiverFound)
            {
                Send("You cannot get any quest here.");
                return CommandExecutionResults.NoExecution;
            }

            if (!questAvailable)
                sb.AppendLine("No quest available for the moment.");
            Page(sb);
            return CommandExecutionResults.Ok;
        }

        #region Helpers

        private IQuest GetQuest(QuestBlueprint questBlueprint, INonPlayableCharacter questGiver)
        {
            IQuest quest = new Quest.Quest(questBlueprint, this, questGiver);
            AddQuest(quest);
            //
            Act(ActOptions.ToRoom, "{0} get{0:v} quest '{1}'.", this, questBlueprint.Title);
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (questBlueprint.TimeLimit > 0)
                Send($"You get quest '{questBlueprint.Title}'. Better hurry, you have {questBlueprint.TimeLimit} minutes to complete this quest!");
            else
                Send($"You get quest '{questBlueprint.Title}'.");
            return quest;
        }

        // TODO: Add a method in CharacterQuestor (to be developed) to get available quests for a ICharacter
        private IEnumerable<QuestBlueprint> GetAvailableQuestBlueprints(CharacterQuestorBlueprint questGiverBlueprint) => questGiverBlueprint.QuestBlueprints.Where(x => Quests.All(y => y.Blueprint.Id != x.Id));

        private void BuildQuestSummary(StringBuilder sb, IQuest quest, int? id)
        {
            // TODO: Table ?
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (id >= 0)
                sb.Append($"{id + 1,2}) {quest.Blueprint.Title}: {(quest.IsCompleted ? "%g%complete%x%" : "in progress")}");
            else
                sb.Append($"{quest.Blueprint.Title}: {(quest.IsCompleted ? "%g%complete%x%" : "in progress")}");
            if (quest.Blueprint.TimeLimit > 0)
                sb.Append($" Time left: {StringHelpers.FormatDelay(quest.PulseLeft / Pulse.PulsePerSeconds)}");
            sb.AppendLine();
            if (!quest.IsCompleted)
                BuildQuestObjectives(sb, quest);
        }

        private void BuildQuestObjectives(StringBuilder sb, IQuest quest)
        {
            foreach (IQuestObjective objective in quest.Objectives)
            {
                // TODO: 2 columns ?
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (objective.IsCompleted)
                    sb.AppendLine($"     %g%{objective.CompletionState}%x%");
                else
                    sb.AppendLine($"     {objective.CompletionState}");
            }
        }

        #endregion
    }
}
