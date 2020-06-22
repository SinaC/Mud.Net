﻿using Mud.Common;
using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Quest;
using System.Linq;
using System.Text;

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
        private IGameActionManager GameActionManager { get; }

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

        public Quest(IGameActionManager gameActionManager)
        {
            GameActionManager = gameActionManager;
        }

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
                        string executionResults = GameActionManager.Execute<QuestAbandon, IPlayableCharacter>(Actor, "questabandon", Parameters.rawParameters, Parameters.parameters);
                        if (executionResults != null)
                            Actor.Send(executionResults);
                        return;
                    }
                case Actions.Complete:
                    {
                        string executionResults = GameActionManager.Execute<QuestComplete, IPlayableCharacter>(Actor, "questcomplete", Parameters.rawParameters, Parameters.parameters);
                        if (executionResults != null)
                            Actor.Send(executionResults);
                        return;
                    }
                case Actions.Get:
                    {
                        string executionResults = GameActionManager.Execute<QuestGet, IPlayableCharacter>(Actor, "questget", Parameters.rawParameters, Parameters.parameters);
                        if (executionResults != null)
                            Actor.Send(executionResults);
                        return;
                    }
                case Actions.List:
                    {
                        string executionResults = GameActionManager.Execute<QuestList, IPlayableCharacter>(Actor, "questlist", Parameters.rawParameters, Parameters.parameters);
                        if (executionResults != null)
                            Actor.Send(executionResults);
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
