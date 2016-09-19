using System;
using System.Linq;
using System.Text;
using Mud.Server.Common;
using Mud.Server.Input;

namespace Mud.Server.Character
{
    public partial class Character
    {
        [Command("quests", Category = "Quest", Priority = 1)]
        protected virtual bool DoQuests(string rawParameters, params CommandParameter[] parameters)
        {
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
                        sb.Append(BuildQuestSummary(quest, id));
                        id++;
                    }
                }
                Page(sb);
            }
            else
            {
                int id = parameters[0].AsNumber;
                IQuest quest = id > 0 ? Quests.ElementAtOrDefault(id-1) : null; // index starts at 0
                if (quest == null)
                {
                    Send("No such quest.");
                    return true;
                }
                StringBuilder sb = new StringBuilder();
                sb.AppendFormatLine($"{quest.Blueprint.Title}: {(quest.IsCompleted ? "%g%complete%x%" : "in progress")}");
                sb.AppendLine(quest.Blueprint.Description);
                sb.Append(BuildQuestObjectives(quest));
                Page(sb);
            }
            return true;
        }

        [Command("qcomplete", Category = "Quest", Priority = 2)]
        [Command("questcomplete", Category = "Quest", Priority = 2)]
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

        [Command("qabandon", Category = "Quest", Priority = 3)]
        [Command("questabandon", Category = "Quest", Priority = 3)]
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

        [Command("qget", Category = "Quest", Priority = 4)]
        [Command("questget", Category = "Quest", Priority = 4)]
        protected virtual bool DoQuestGet(string rawParameters, params CommandParameter[] parameters)
        {
            // TODO
            throw new NotImplementedException();
        }

        [Command("qlist", Category = "Quest", Priority = 5)]
        [Command("questlist", Category = "Quest", Priority = 5)]
        protected virtual bool DoQuestList(string rawParameters, params CommandParameter[] parameters)
        {
            // TODO
            throw new NotImplementedException();
            // Display quest available in this.Room
        }

        #region Helpers

        private StringBuilder BuildQuestSummary(IQuest quest, int id)
        {
            StringBuilder sb = new StringBuilder();
            if (id >= 0)
                sb.AppendFormatLine($"{id + 1,2}) {quest.Blueprint.Title}: {(quest.IsCompleted ? "%g%complete%x%" : "in progress")}");
            else
                sb.AppendFormatLine($"{quest.Blueprint.Title}: {(quest.IsCompleted ? "%g%complete%x%" : "in progress")}");
            if (!quest.IsCompleted)
                sb.Append(BuildQuestObjectives(quest));
            return sb;
        }

        private StringBuilder BuildQuestObjectives(IQuest quest)
        {
            StringBuilder sb = new StringBuilder();
            foreach (QuestObjectiveBase objective in quest.Objectives)
                // TODO: 2 columns ?
                if (objective.IsCompleted)
                    sb.AppendFormatLine($"     %g%{objective.CompletionState}%x%");
                else
                    sb.AppendFormatLine($"     {objective.CompletionState}");
            return sb;
        }

        #endregion
    }
}
