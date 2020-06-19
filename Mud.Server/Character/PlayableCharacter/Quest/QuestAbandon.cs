using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Quest;
using System.Linq;

namespace Mud.Server.Character.PlayableCharacter.Quest
{
    [PlayableCharacterCommand("qabandon", "Quest", Priority = 3, MinPosition = Positions.Standing)]
    [PlayableCharacterCommand("questabandon", "Quest", Priority = 3, MinPosition = Positions.Standing)]
    [Syntax("[cmd] <id>")]
    public class QuestAbandon : PlayableCharacterGameAction
    {
        public IQuest What { get; protected set; }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
                return "Abandon which quest?";
            int id = actionInput.Parameters[0].AsNumber;
            IQuest quest = id > 0 
                ? Actor.Quests.ElementAtOrDefault(id - 1)
                : null;
            if (quest == null)
                return "No such quest.";
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            What.Abandon();
            Actor.RemoveQuest(What);

            Actor.Send("You abandon '{0}'!", What.Blueprint.Title);
        }
    }
}
