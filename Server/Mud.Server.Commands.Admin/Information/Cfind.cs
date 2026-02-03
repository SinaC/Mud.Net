using Mud.Server.Parser.Interfaces;
using Mud.Server.Domain.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.AdminGuards;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using System.Text;

namespace Mud.Server.Commands.Admin.Information;

[AdminCommand("cfind", "Information")]
[Alias("mfind")]
[Syntax("[cmd] <character>")]
public class Cfind : AdminGameAction
{
    protected override IGuard<IAdmin>[] Guards => [new RequiresAtLeastOneArgument()];

    private ICharacterManager CharacterManager { get; }

    public Cfind(ICharacterManager characterManager)
    {
        CharacterManager = characterManager;
    }

    private ICommandParameter Pattern { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

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
