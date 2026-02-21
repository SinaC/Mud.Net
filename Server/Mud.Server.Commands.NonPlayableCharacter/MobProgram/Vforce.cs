using Mud.Server.Common.Helpers;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.NonPlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Parser.Interfaces;

namespace Mud.Server.Commands.NonPlayableCharacter.MobProgram;

[NonPlayableCharacterCommand("mpvforce", "MobProgram", Hidden = true)]
[Help(
@"Forces all mobiles of certain vnum to do something (except ch)")]
[Syntax("mob vforce [vnum] [commands]")]
public class Vforce : NonPlayableCharacterGameAction
{
    protected override IGuard<INonPlayableCharacter>[] Guards => [new RequiresAtLeastTwoArguments()];

    private ICharacterManager CharacterManager { get; }
    private IParser Parser { get; }

    public Vforce(ICharacterManager characterManager, IParser parser)
    {
        CharacterManager = characterManager;
        Parser = parser;
    }

    private ICharacter[] Whom { get; set; } = default!;
    private string What { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters[1].Value == "delete" || actionInput.Parameters[1].Value == "deleteavatar")
            return "That will NOT be done.";

        // blueprint id
        if (!actionInput.Parameters[0].IsNumber)
            return BuildCommandSyntax();
        var blueprintId = actionInput.Parameters[0].AsNumber;
        var blueprint = CharacterManager.GetCharacterBlueprint(blueprintId);
        if (blueprint == null)
            return "No mob with that id.";

        // whom
        Whom =CharacterManager.NonPlayableCharacters.Where(x => x.Blueprint.Id == blueprintId && x != Actor).ToArray();
        if (Whom.Length == 0)
            return StringHelpers.NotFound;

        // what
        What = Parser.JoinParameters(actionInput.Parameters.Skip(1));
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        foreach (var victim in Whom)
            victim.ProcessInput(What);
    }
}
