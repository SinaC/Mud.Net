using Mud.Server.Common.Helpers;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.NonPlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Parser.Interfaces;

namespace Mud.Server.Commands.NonPlayableCharacter.MobProgram;

[NonPlayableCharacterCommand("mpforce", "MobProgram", Hidden = true)]
[Help(
@"Lets the mobile force someone to do something. Must be mortal level
and the all argument only affects those in the room with the mobile.")]
[Syntax("mob force [victim] [commands]")]
public class Force : NonPlayableCharacterGameAction
{
    protected override IGuard<INonPlayableCharacter>[] Guards => [new RequiresAtLeastTwoArguments()];

    private IParser Parser { get; }

    public Force(IParser parser)
    {
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

        // whom
        Whom = FindHelpers.Find(Actor.Room.People.Where(x => x != Actor), actionInput.Parameters[0]).ToArray();
        if (Whom.Length == 0)
            return StringHelpers.CharacterNotFound;

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
