using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Common;
using Mud.Server.Input;

namespace Mud.Server.Character
{
    public partial class Character
    {
        [Command("quests", Category = "Quest", Priority = 1)]
        protected virtual bool DoQuests(string rawParameters, params CommandParameter[] parameters)
        {
            // TODO: quest <id>
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Quests:");
            if (!Quests.Any())
                sb.AppendLine("None.");
            else
            {
                int id = 1;
                foreach (IQuest quest in Quests)
                {
                    sb.AppendFormatLine($"{id,2}) {quest.Blueprint.Title}: {(quest.IsCompleted ? "complete" : "running")}"); // TODO: completion state + objectives remaining
                    foreach(QuestObjectiveBase objective in quest.Objectives)
                        if (objective.IsCompleted)
                            sb.AppendFormatLine($"     {objective.TargetName,-20}: complete");
                        else
                            sb.AppendFormatLine($"     {objective.TargetName,-20}: {objective.Count,3} / {objective.Total,3}");
                    id++;
                }
            }
            Page(sb);
            return true;
        }

        [Command("qcomplete", Category = "Quest", Priority = 2)]
        [Command("questcomplete", Category = "Quest", Priority = 2)]
        protected virtual bool DoQuestComplete(string rawParameters, params CommandParameter[] parameters)
        {
            // TODO
            throw new NotImplementedException();
        }

        [Command("qabandon", Category = "Quest", Priority = 3)]
        [Command("questabandon", Category = "Quest", Priority = 3)]
        protected virtual bool DoQuestAbandon(string rawParameters, params CommandParameter[] parameters)
        {
            // TODO
            throw new NotImplementedException();
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
    }
}
