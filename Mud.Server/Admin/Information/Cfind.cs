using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mud.Server.Admin.Information
{
    [AdminCommand("cfind", "Information")]
    [AdminCommand("mfind", "Information")]
    [Syntax("[cmd] <character>")]
    public class Cfind : AdminGameAction
    {
        private ICharacterManager CharacterManager { get; }

        public ICommandParameter Pattern { get; protected set; }

        public Cfind(ICharacterManager characterManager)
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
            Pattern = actionInput.Parameters[0];
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Searching characters '{Pattern.Value}'");
            List<INonPlayableCharacter> characters = FindHelpers.FindAllByName(CharacterManager.NonPlayableCharacters, Pattern).OrderBy(x => x.Blueprint?.Id).ToList();
            if (characters.Count == 0)
                sb.AppendLine("No matches");
            else
            {
                sb.AppendLine("Id         DisplayName                    Room");
                foreach (INonPlayableCharacter character in characters)
                    sb.AppendLine($"{character.Blueprint?.Id.ToString() ?? "Player",-10} {character.DisplayName,-30} {character.Room?.DebugName ?? "none"}");
            }
            Actor.Page(sb);
        }
    }
}
