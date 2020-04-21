using System.Linq;
using System.Text;
using Mud.Server.Common;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Admin
{
    public partial class Admin
    {
        [Command("delete", Category = "Misc", Priority = 999, NoShortcut = true)]
        protected override bool DoDelete(string rawParameters, params CommandParameter[] parameters)
        {
            Send("An admin cannot be deleted in game!!!");
            return true;
        }

        [Command("questdisplay", Category = "Misc")]
        protected virtual bool DoQuestDisplay(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Display quests for whom?");
                return true;
            }

            IPlayableCharacter whom = FindHelpers.FindByName(World.PlayableCharacters.Where(x => x.ImpersonatedBy != null), parameters[0]);
            if (whom == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return true;
            }

            if (!whom.Quests.Any())
            {
                Send($"No quest to display on {DisplayName}");
                return true;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Quests for {DisplayName}:");
            int id = 0;
            foreach (IQuest quest in whom.Quests)
            {
                BuildQuestInfo(sb, quest, id);
                id++;
            }
            Page(sb);
            return true;
        }

        [Command("questreset", Category = "Misc")]
        protected virtual bool DoQuestReset(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Display quests for whom?");
                return true;
            }

            IPlayableCharacter whom = FindHelpers.FindByName(World.PlayableCharacters.Where(x => x.ImpersonatedBy != null), parameters[0]);
            if (whom == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return true;
            }

            if (!whom.Quests.Any())
            {
                Send($"No quest to reset on {DisplayName}");
                return true;
            }

            foreach (IQuest quest in whom.Quests)
            {
                Send($"Resetting quest '{quest.Blueprint?.Title}' for '{whom.DisplayName}");
                whom.Send($"%y%The quest ''{quest.Blueprint?.Title}' has been reset.%x%");
                quest.Reset();
            }

            return true;
        }

        //
        private void BuildQuestInfo(StringBuilder sb, IQuest quest, int id)
        {
            sb.AppendFormat($"{id + 1,2}) {quest.Blueprint.Title}: {(quest.IsCompleted ? "%g%complete%x%" : "in progress")}");
            if (quest.Blueprint.TimeLimit > 0)
                sb.Append($" Time left : {StringHelpers.FormatDelay(quest.SecondsLeft)}");
            sb.AppendLine();
            foreach (IQuestObjective objective in quest.Objectives)
                sb.AppendFormatLine($"     {objective.CompletionState}");
        }
    }
}
