using Mud.Common;
using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Quest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.Server.Character.PlayableCharacter.Quest
{
    [PlayableCharacterCommand("quest", "Quest", Priority = 1, MinPosition = Positions.Standing)]
    [Syntax(
            "[cmd]",
            "[cmd] <id>",
            "[cmd] abandon <id>",
            "[cmd] complete <id>",
            "[cmd] complete all",
            "[cmd] get <quest name>",
            "[cmd] get all",
            "[cmd] list")]
    public class Quest : PlayableCharacterGameAction
    {
        public enum Actions
        {
            DisplayAll,
            Display,
            Abandon,
            Complete,
            Get,
            List
        }

        public Actions Action { get; protected set; }
        public IQuest What { get; protected set; }
        public (string rawParameters, ICommandParameter[] parameters) Parameters { get; protected set; }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            // no param -> quest info
            if (actionInput.Parameters.Length == 0)
            {
                Action = Actions.DisplayAll;
                return null;
            }

            // quest id
            if (actionInput.Parameters[0].IsNumber)
            {
                int id = actionInput.Parameters[0].AsNumber;
                What = id > 0
                    ? Actor.Quests.ElementAtOrDefault(id - 1)
                    : null; // index starts at 0
                if (What == null)
                    return "No such quest.";
                Action = Actions.Display;
                return null;
            }

            // quest abandon id
            if ("abandon".StartsWith(actionInput.Parameters[0].Value))
            {
                Parameters = CommandHelpers.SkipParameters(actionInput.Parameters, 1);
                Action = Actions.Abandon;
                return null;
            }

            // quest complete id
            if ("complete".StartsWith(actionInput.Parameters[0].Value))
            {
                Parameters = CommandHelpers.SkipParameters(actionInput.Parameters, 1);
                Action = Actions.Complete;
                return null;
            }

            // quest get all|title
            if ("get".StartsWith(actionInput.Parameters[0].Value))
            {
                Parameters = CommandHelpers.SkipParameters(actionInput.Parameters, 1);
                Action = Actions.Get;
                return null;
            }

            // quest list
            if ("list".StartsWith(actionInput.Parameters[0].Value))
            {
                Parameters = CommandHelpers.SkipParameters(actionInput.Parameters, 1);
                Action = Actions.List;
                return null;
            }

            return BuildCommandSyntax();
        }

        public override void Execute(IActionInput actionInput)
        {
            switch (Action)
            {
                case Actions.DisplayAll:
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("Quests:");
                        if (!Actor.Quests.Any())
                            sb.AppendLine("None.");
                        else
                        {
                            int id = 0;
                            foreach (IQuest quest in Actor.Quests)
                            {
                                BuildQuestSummary(sb, quest, id);
                                id++;
                            }
                        }
                        Actor.Page(sb);
                        return;
                    }
                case Actions.Display:
                    {
                        StringBuilder sb = new StringBuilder();
                        BuildQuestSummary(sb, What, null);
                        Actor.Page(sb);
                        return;
                    }
                case Actions.Abandon:
                    {
                        QuestAbandon questAbandon = new QuestAbandon();
                        IActionInput questAbandonActionInput = new ActionInput(actionInput.GameActionInfo, Actor, null /*TODO*/, "questabandon", Parameters.rawParameters, Parameters.parameters);
                        string questAbandonGuards = questAbandon.Guards(questAbandonActionInput);
                        if (questAbandonGuards != null)
                            Actor.Send(questAbandonGuards);
                        else
                            questAbandon.Execute(questAbandonActionInput);
                        return;
                    }
                case Actions.Complete:
                    {
                        QuestComplete questComplete = new QuestComplete();
                        IActionInput questCompleteActionInput = new ActionInput(actionInput.GameActionInfo, Actor, null /*TODO*/, "questcomplete", Parameters.rawParameters, Parameters.parameters);
                        string questCompleteGuards = questComplete.Guards(questCompleteActionInput);
                        if (questCompleteGuards != null)
                            Actor.Send(questCompleteGuards);
                        else
                            questComplete.Execute(questCompleteActionInput);
                        return;
                    }
                case Actions.Get:
                    {
                        QuestGet questGet = new QuestGet();
                        IActionInput questGetActionInput = new ActionInput(actionInput.GameActionInfo, Actor, null /*TODO*/, "questget", Parameters.rawParameters, Parameters.parameters);
                        string questGetGuards = questGet.Guards(questGetActionInput);
                        if (questGetGuards != null)
                            Actor.Send(questGetGuards);
                        else
                            questGet.Execute(questGetActionInput);
                        return;
                    }
                case Actions.List:
                    {
                        QuestList questList = new QuestList();
                        IActionInput questListActionInput = new ActionInput(actionInput.GameActionInfo, Actor, null /*TODO*/, "questlist", Parameters.rawParameters, Parameters.parameters);
                        string questListGuards = questList.Guards(questListActionInput);
                        if (questListGuards != null)
                            Actor.Send(questListGuards);
                        else
                            questList.Execute(questListActionInput);
                        return;
                    }
            }
        }

        private void BuildQuestSummary(StringBuilder sb, IQuest quest, int? id)
        {
            // TODO: Table ?
            if (id >= 0)
                sb.Append($"{id + 1,2}) {quest.Blueprint.Title}: {(quest.IsCompleted ? "%g%complete%x%" : "in progress")}");
            else
                sb.Append($"{quest.Blueprint.Title}: {(quest.IsCompleted ? "%g%complete%x%" : "in progress")}");
            if (quest.Blueprint.TimeLimit > 0)
                sb.Append($" Time left: {(quest.PulseLeft / Pulse.PulsePerSeconds).FormatDelay()}");
            sb.AppendLine();
            if (!quest.IsCompleted)
                BuildQuestObjectives(sb, quest);
        }

        private void BuildQuestObjectives(StringBuilder sb, IQuest quest)
        {
            foreach (IQuestObjective objective in quest.Objectives)
            {
                // TODO: 2 columns ?
                if (objective.IsCompleted)
                    sb.AppendLine($"     %g%{objective.CompletionState}%x%");
                else
                    sb.AppendLine($"     {objective.CompletionState}");
            }
        }
    }
}
