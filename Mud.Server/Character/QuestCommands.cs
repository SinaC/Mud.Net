using System.Linq;
using System.Text;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Common;
using Mud.Server.Input;

namespace Mud.Server.Character
{
    public partial class Character
    {
        [CharacterCommand("quest", Category = "Quest", Priority = 1)]
        protected virtual bool DoQuest(string rawParameters, params CommandParameter[] parameters)
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
                return true;
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
                    return true;
                }
                StringBuilder sb = new StringBuilder();
                BuildQuestSummary(sb, quest, null);
                Page(sb);
                return true;
            }

            // quest abandon id
            if ("abandon".StartsWith(parameters[0].Value))
            {
                var subCommandParameters = CommandHelpers.SkipParameters(parameters, 1);
                DoQuestAbandon(subCommandParameters.rawParameters, subCommandParameters.parameters);
                return true;
            }

            // quest complete id
            if ("complete".StartsWith(parameters[0].Value))
            {
                var subCommandParameters = CommandHelpers.SkipParameters(parameters, 1);
                DoQuestComplete(subCommandParameters.rawParameters, subCommandParameters.parameters);
                return true;
            }

            // quest get all|title
            if ("get".StartsWith(parameters[0].Value))
            {
                var subCommandParameters = CommandHelpers.SkipParameters(parameters, 1);
                DoQuestGet(subCommandParameters.rawParameters, subCommandParameters.parameters);
                return true;
            }

            // quest list
            if ("list".StartsWith(parameters[0].Value))
            {
                var subCommandParameters = CommandHelpers.SkipParameters(parameters, 1);
                DoQuestList(subCommandParameters.rawParameters, subCommandParameters.parameters);
                return true;
            }

            Send("Syntax: quest ");
            Send("        quest id");
            Send("        quest abandon id");
            Send("        quest complete id");
            Send("        quest get all|title");
            Send("        quest list");
            return true;
        }

        [CharacterCommand("qcomplete", Category = "Quest", Priority = 2)]
        [CharacterCommand("questcomplete", Category = "Quest", Priority = 2)]
        protected virtual bool DoQuestComplete(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Complete which quest?");
                return true;
            }
            int id = parameters[0].AsNumber;
            IQuest quest = id > 0 ? Quests.ElementAtOrDefault(id - 1) : null;
            if (quest == null)
            {
                Send("No such quest.");
                return true;
            }
            if (Room.People.All(x => x != quest.Giver))
            {
                Send($"You must be near {quest.Giver.DisplayName} to complete this quest.");
                return true;
            }
            CompleteQuest(quest);
            return true;
        }

        [CharacterCommand("qabandon", Category = "Quest", Priority = 3)]
        [CharacterCommand("questabandon", Category = "Quest", Priority = 3)]
        protected virtual bool DoQuestAbandon(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Abandon which quest?");
                return true;
            }
            int id = parameters[0].AsNumber;
            IQuest quest = id > 0 ? Quests.ElementAtOrDefault(id - 1) : null;
            if (quest == null)
            {
                Send("No such quest.");
                return true;
            }
            AbandonQuest(quest);
            return true;
        }

        [CharacterCommand("qget", Category = "Quest", Priority = 4)]
        [CharacterCommand("questget", Category = "Quest", Priority = 4)]
        protected virtual bool DoQuestGet(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Get which quest?");
                return true;
            }

            if (!Room.People.Any(x => x.Blueprint is CharacterQuestorBlueprint))
            {
                Send("You cannot get any quest here.");
                return true;
            }

            // get all
            if (parameters[0].IsAll)
            {
                bool found = false;
                // Search quest giver with wanted quest
                foreach (ICharacter questGiver in Room.People.Where(x => x.Blueprint is CharacterQuestorBlueprint))
                {
                    CharacterQuestorBlueprint questGiverBlueprint = questGiver.Blueprint as CharacterQuestorBlueprint;
                    if (questGiverBlueprint?.QuestBlueprints?.Any() == true)
                    {
                        foreach (QuestBlueprint questBlueprint in questGiverBlueprint.QuestBlueprints.Where(x => Quests.All(y => y.Blueprint.Id != x.Id))) // TODO: Add a method in CharacterQuestor to get available quests for a ICharacter
                        {
                            IQuest quest = new Quest.Quest(questBlueprint, this, questGiver);
                            AddQuest(quest);
                            //
                            Act(ActOptions.ToAll, "{0} get{0:v} quest '{1}'.", this, questBlueprint.Title);
                            found = true;
                        }
                    }
                }
                if (!found)
                    Send("You are already running all available quests here.");
                return true;
            }

            // get quest
            string questTitle = parameters[0].Value.ToLowerInvariant();
            // Search quest giver with wanted quest
            foreach (ICharacter questGiver in Room.People.Where(x => x.Blueprint is CharacterQuestorBlueprint))
            {
                CharacterQuestorBlueprint questGiverBlueprint = questGiver.Blueprint as CharacterQuestorBlueprint;
                if (questGiverBlueprint?.QuestBlueprints?.Any() == true)
                {
                    foreach (QuestBlueprint questBlueprint in questGiverBlueprint.QuestBlueprints.Where(x => Quests.All(y => y.Blueprint.Id != x.Id))) // TODO: Add a method in CharacterQuestor to get available quests for a ICharacter
                    {
                        if (questBlueprint.Title.ToLowerInvariant().StartsWith(questTitle))
                        {
                            IQuest quest = new Quest.Quest(questBlueprint, this, questGiver);
                            AddQuest(quest);
                            //
                            Act(ActOptions.ToAll, "{0} get{0:v} quest '{1}'.", this, questBlueprint.Title);
                            return true;
                        }
                    }
                }
            }
            //
            Send("No such quest can be get here.");
            return true;
        }

        [CharacterCommand("qlist", Category = "Quest", Priority = 5)]
        [CharacterCommand("questlist", Category = "Quest", Priority = 5)]
        protected virtual bool DoQuestList(string rawParameters, params CommandParameter[] parameters)
        {
            // Display quests available in this.Room
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Available quests:");
            bool found = false;
            foreach (ICharacter questGiver in Room.People.Where(x => x.Blueprint is CharacterQuestorBlueprint))
            {
                CharacterQuestorBlueprint questGiverBlueprint = questGiver.Blueprint as CharacterQuestorBlueprint;
                if (questGiverBlueprint?.QuestBlueprints?.Any() == true)
                {
                    // TODO: don't display if already on this quest
                    foreach (QuestBlueprint questBlueprint in questGiverBlueprint.QuestBlueprints)
                        sb.AppendLine($"Quest '{questBlueprint.Title}' [lvl:{questBlueprint.Level}]");
                    found = true;
                }
            }

            if (!found)
                Send("You cannot get any quest here.");
            else
                Page(sb);
            return true;
        }

        #region Helpers

        private void BuildQuestSummary(StringBuilder sb, IQuest quest, int? id)
        {
            if (id >= 0)
                sb.AppendFormatLine($"{id + 1,2}) {quest.Blueprint.Title}: {(quest.IsCompleted ? "%g%complete%x%" : "in progress")}");
            else
                sb.AppendFormatLine($"{quest.Blueprint.Title}: {(quest.IsCompleted ? "%g%complete%x%" : "in progress")}");
            if (!quest.IsCompleted)
                BuildQuestObjectives(sb, quest);
        }

        private void BuildQuestObjectives(StringBuilder sb, IQuest quest)
        {
            foreach (IQuestObjective objective in quest.Objectives)
            {
                // TODO: 2 columns ?
                if (objective.IsCompleted)
                    sb.AppendFormatLine($"     %g%{objective.CompletionState}%x%");
                else
                    sb.AppendFormatLine($"     {objective.CompletionState}");
            }
        }

        #endregion
    }
}
