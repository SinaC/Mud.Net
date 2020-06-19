using Mud.Domain;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Quest;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mud.Server.Character.PlayableCharacter.Quest
{
    [PlayableCharacterCommand("qlist", "Quest", Priority = 5, MinPosition = Positions.Standing)]
    [PlayableCharacterCommand("questlist", "Quest", Priority = 5, MinPosition = Positions.Standing)]
    public class QuestList : PlayableCharacterGameAction
    {
        public List<QuestBlueprint> What { get; protected set; }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            What = new List<QuestBlueprint>();
            bool questGiverFound = false;
            bool questAvailable = false;
            foreach (var questGiver in Actor.Room.GetNonPlayableCharacters<CharacterQuestorBlueprint>())
            {
                if (questGiver.blueprint?.QuestBlueprints?.Any() == true)
                {
                    foreach (QuestBlueprint questBlueprint in GetAvailableQuestBlueprints(questGiver.blueprint))
                    {
                        What.Add(questBlueprint);
                        questAvailable = true;
                    }
                    questGiverFound = true;
                }
            }

            if (!questGiverFound)
                return "You cannot get any quest here.";

            if (!questAvailable)
                return "No quest available for the moment.";

            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Available quests:");
            foreach (QuestBlueprint questBlueprint in What)
                sb.AppendLine($"Quest '{questBlueprint.Title}' [lvl:{questBlueprint.Level}]");
            Actor.Page(sb);
        }

        private IEnumerable<QuestBlueprint> GetAvailableQuestBlueprints(CharacterQuestorBlueprint questGiverBlueprint) => questGiverBlueprint.QuestBlueprints.Where(x => Actor.Quests.All(y => y.Blueprint.Id != x.Id));
    }
}
