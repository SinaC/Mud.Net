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
using Mud.Blueprints.Character;

namespace Mud.Server.Commands.Admin.Information;

[AdminCommand("cfind", "Information")]
[Alias("mfind")]
[Syntax(
"[cmd] <character>",
"[cmd] <id>")]
public class Cfind : AdminGameAction
{
    protected override IGuard<IAdmin>[] Guards => [new RequiresAtLeastOneArgument()];

    private ICharacterManager CharacterManager { get; }

    public Cfind(ICharacterManager characterManager)
    {
        CharacterManager = characterManager;
    }

    private CharacterBlueprintBase? Blueprint { get; set; }
    private ICommandParameter? Pattern { get; set; }

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters[0].IsNumber)
        {
            var blueprintId = actionInput.Parameters[0].AsNumber;
            var blueprint = CharacterManager.GetCharacterBlueprint(blueprintId);
            if (blueprint == null)
                return "Not found.";
            Blueprint = blueprint;

            return null;
        }

        Pattern = actionInput.Parameters[0];

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        StringBuilder sb = new ();

        List<INonPlayableCharacter> npcs = null!;
        if (Blueprint is not null)
        {
            sb.AppendLine($"Searching characters id: '{Blueprint.Id}'");
            npcs = CharacterManager.NonPlayableCharacters.Where(x => x.Blueprint == Blueprint).ToList();
        }
        else if (Pattern is not null)
        {
            sb.AppendLine($"Searching characters '{Pattern.Value}'");
            npcs = FindHelpers.FindAllByName(CharacterManager.NonPlayableCharacters, Pattern).OrderBy(x => x.Blueprint.Id).ToList();
        }
        if (npcs is null || npcs.Count == 0)
            sb.AppendLine("No matches");
        else
        {
            sb.AppendLine("Id       DisplayName                    Room");
            sb.AppendLine("--------------------------------------------");
            foreach (var character in npcs)
                sb.AppendLine($"{character.Blueprint.Id.ToString() ?? "Player",-8} {character.DisplayName,-30} {character.Room?.DebugName ?? "none"}");
        }
        Actor.Page(sb);
    }
}
