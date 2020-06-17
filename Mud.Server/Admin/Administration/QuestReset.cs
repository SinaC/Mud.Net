using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Quest;
using System.Linq;

namespace Mud.Server.Admin.Administration
{
    [AdminCommand("questreset", "Misc")]
    [Syntax("[cmd] <character>")]
    public class QuestReset : AdminGameAction
    {
        private ICharacterManager CharacterManager { get; }

        public IPlayableCharacter Whom { get; protected set; }

        public QuestReset(ICharacterManager characterManager)
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
                return $"No quest to reset on {Whom.DisplayName}";

            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            foreach (IQuest quest in Whom.Quests)
            {
                Actor.Send($"Resetting quest '{quest.Blueprint?.Title}' for '{Whom.DisplayName}'");
                Whom.Send($"%y%The quest ''{quest.Blueprint?.Title}' has been reset.%x%");
                quest.Reset();
            }

        }
    }
}
