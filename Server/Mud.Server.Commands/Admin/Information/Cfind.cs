using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using System.Text;

namespace Mud.Server.Commands.Admin.Information;

[AdminCommand("cfind", "Information")]
[Alias("mfind")]
[Syntax("[cmd] <character>")]
public class Cfind : AdminGameAction
{
    private ICharacterManager CharacterManager { get; }

    public Cfind(ICharacterManager characterManager)
    {
        CharacterManager = characterManager;
    }

    protected ICommandParameter Pattern { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            return BuildCommandSyntax();

        Pattern = actionInput.Parameters[0];

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        StringBuilder sb = new ();
        sb.AppendLine($"Searching characters '{Pattern.Value}'");
        List<INonPlayableCharacter> characters = FindHelpers.FindAllByName(CharacterManager.NonPlayableCharacters, Pattern).OrderBy(x => x.Blueprint.Id).ToList();
        if (characters.Count == 0)
            sb.AppendLine("No matches");
        else
        {
            sb.AppendLine("Id       DisplayName                    Room");
            sb.AppendLine("--------------------------------------------");
            foreach (INonPlayableCharacter character in characters)
                sb.AppendLine($"{character.Blueprint.Id.ToString() ?? "Player",-8} {character.DisplayName,-30} {character.Room?.DebugName ?? "none"}");
        }
        Actor.Page(sb);
    }
}
