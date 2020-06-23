using Mud.Common;
using Mud.Domain;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Quest;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Quest;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mud.Server.Character.PlayableCharacter.Quest
{
    [PlayableCharacterCommand("questget", "Quest", Priority = 4, MinPosition = Positions.Standing)]
    [Alias("qget")]
    [Syntax(
            "[cmd] <quest name>",
            "[cmd] all",
            "[cmd] all.<quest name>")]
    public class QuestGet : PlayableCharacterGameAction
    {
        public List<(QuestBlueprint questBlueprint, INonPlayableCharacter questGiver)> What { get; protected set; }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
                return "Get which quest?";

            if (!Actor.Room.GetNonPlayableCharacters<CharacterQuestorBlueprint>().Any())
                return "You cannot get any quest here.";

            What = new List<(QuestBlueprint questBlueprint, INonPlayableCharacter questGiver)>();

            // get all
            if (actionInput.Parameters[0].IsAll)
            {
                Func<QuestBlueprint, bool> filterFunc;
                if (!string.IsNullOrWhiteSpace(actionInput.Parameters[0].Value))
                    filterFunc = x => StringCompareHelpers.StringStartsWith(x.Title, actionInput.Parameters[0].Value); 
                else
                    filterFunc = _ => true;

                foreach (var questGiver in Actor.Room.GetNonPlayableCharacters<CharacterQuestorBlueprint>())
                {
                    if (questGiver.blueprint?.QuestBlueprints?.Any() == true)
                    {
                        foreach (QuestBlueprint questBlueprint in GetAvailableQuestBlueprints(questGiver.blueprint).Where(filterFunc))
                            What.Add((questBlueprint, questGiver.character));
                    }
                }
                if (!What.Any())
                    Actor.Send("You are already running all available quests here.");
                return null;
            }

            // get quest (every quest starting with parameter)
            // Search quest giver with wanted quests
            foreach (var questGiver in Actor.Room.GetNonPlayableCharacters<CharacterQuestorBlueprint>())
            {
                if (questGiver.blueprint?.QuestBlueprints?.Any() == true)
                {
                    foreach (QuestBlueprint questBlueprint in GetAvailableQuestBlueprints(questGiver.blueprint))
                    {
                        if (StringCompareHelpers.StringStartsWith(questBlueprint.Title, actionInput.Parameters[0].Value))
                            What.Add((questBlueprint, questGiver.character));
                    }
                }
            }
            if (!What.Any())
                return "No such quest can be get here.";
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            foreach (var entry in What)
                GetQuest(entry.questBlueprint, entry.questGiver);
        }

        private IQuest GetQuest(QuestBlueprint questBlueprint, INonPlayableCharacter questGiver)
        {
            IQuest quest = new Mud.Server.Quest.Quest(questBlueprint, Actor, questGiver);
            Actor.AddQuest(quest);
            //
            Actor.Act(ActOptions.ToRoom, "{0} get{0:v} quest '{1}'.", Actor, questBlueprint.Title);
            if (questBlueprint.TimeLimit > 0)
                Actor.Send($"You get quest '{questBlueprint.Title}'. Better hurry, you have {questBlueprint.TimeLimit} minutes to complete this quest!");
            else
                Actor.Send($"You get quest '{questBlueprint.Title}'.");
            return quest;
        }

        private IEnumerable<QuestBlueprint> GetAvailableQuestBlueprints(CharacterQuestorBlueprint questGiverBlueprint) => questGiverBlueprint.QuestBlueprints.Where(x => Actor.Quests.All(y => y.Blueprint.Id != x.Id));
    }
}
