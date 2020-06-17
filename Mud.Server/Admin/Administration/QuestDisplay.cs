using Mud.Common;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Quest;
using System.Linq;
using System.Text;

namespace Mud.Server.Admin.Administration
{
    [AdminCommand("questdisplay", "Misc")]
    [Syntax("[cmd] <character>")]
    public class QuestDisplay : AdminGameAction
    {
        private ICharacterManager CharacterManager { get; }

        public IPlayableCharacter Whom { get; protected set; }

        public QuestDisplay(ICharacterManager characterManager)
        {
            CharacterManager = characterManager;
        }


        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
                return BuildCommandSyntax();

            Whom = FindHelpers.FindByName(CharacterManager.PlayableCharacters.Where(x => x.ImpersonatedBy != null), actionInput.Parameters[0]);
            if (Whom == null)
                return StringHelpers.CharacterNotFound;

            if (!Whom.Quests.Any())
                return $"No quest to display on {Whom.DisplayName}";

            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Quests for {Whom.DisplayName}:");
            int id = 0;
            foreach (IQuest quest in Whom.Quests)
            {
                BuildQuestInfo(sb, quest, id);
                id++;
            }
            Actor.Page(sb);
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
