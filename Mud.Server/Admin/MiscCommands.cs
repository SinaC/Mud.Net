using System.Linq;
using System.Text;
using Mud.Common;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Quest;
// ReSharper disable UnusedMember.Global

namespace Mud.Server.Admin
{
    public partial class Admin
    {
        [AdminCommand("delete", "Misc", Priority = 999, NoShortcut = true)]
        protected override CommandExecutionResults DoDelete(string rawParameters, params ICommandParameter[] parameters)
        {
            Send("An admin cannot be deleted in game!!!");
            return CommandExecutionResults.NoExecution;
        }

        [AdminCommand("questdisplay", "Misc")]
        [Syntax("[cmd] <character>")]
        protected virtual CommandExecutionResults DoQuestDisplay(string rawParameters, params ICommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                return CommandExecutionResults.SyntaxError;

            IPlayableCharacter whom = FindHelpers.FindByName(World.PlayableCharacters.Where(x => x.ImpersonatedBy != null), parameters[0]);
            if (whom == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return CommandExecutionResults.TargetNotFound;
            }

            if (!whom.Quests.Any())
            {
                Send($"No quest to display on {DisplayName}");
                return CommandExecutionResults.Ok;
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
            return CommandExecutionResults.Ok;
        }

        [AdminCommand("questreset", "Misc")]
        [Syntax("[cmd] <character>")]
        protected virtual CommandExecutionResults DoQuestReset(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                return CommandExecutionResults.SyntaxError;

            IPlayableCharacter whom = FindHelpers.FindByName(World.PlayableCharacters.Where(x => x.ImpersonatedBy != null), parameters[0]);
            if (whom == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return CommandExecutionResults.TargetNotFound;
            }

            if (!whom.Quests.Any())
            {
                Send($"No quest to reset on {DisplayName}");
                return CommandExecutionResults.Ok;
            }

            foreach (IQuest quest in whom.Quests)
            {
                Send($"Resetting quest '{quest.Blueprint?.Title}' for '{whom.DisplayName}'");
                whom.Send($"%y%The quest ''{quest.Blueprint?.Title}' has been reset.%x%");
                quest.Reset();
            }

            return CommandExecutionResults.Ok;
        }

        //
        private void BuildQuestInfo(StringBuilder sb, IQuest quest, int id)
        {
            sb.AppendFormat($"{id + 1,2}) {quest.Blueprint.Title}: {(quest.IsCompleted ? "%g%complete%x%" : "in progress")}");
            if (quest.Blueprint.TimeLimit > 0)
                sb.Append($" Time left : {(quest.PulseLeft / Pulse.PulsePerSeconds).FormatDelay()}");
            sb.AppendLine();
            foreach (IQuestObjective objective in quest.Objectives)
                sb.AppendFormatLine($"     {objective.CompletionState}");
        }
    }
}
