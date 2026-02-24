using Mud.Server.Common.Helpers;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.NonPlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Parser.Interfaces;

namespace Mud.Server.Commands.NonPlayableCharacter.MobProgram;

[NonPlayableCharacterCommand("mpgforce", "MobProgram", Hidden = true)]
[Help("Lets the mobile force a group something.")]
[Syntax("mob gforce [victim] [commands]")]
public class Gforce : NonPlayableCharacterGameAction
{
    protected override IGuard<INonPlayableCharacter>[] Guards => [new RequiresAtLeastTwoArguments()];

    private IParser Parser { get; }

    public Gforce(IParser parser)
    {
        Parser = parser;
    }

    private IPlayableCharacter[] Whom { get; set; } = default!;
    private string What { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        var whom = FindHelpers.FindByName(Actor.Room.PlayableCharacters, actionInput.Parameters[0]);
        if (whom == null)
            return StringHelpers.CharacterNotFound;
        if (whom.Group != null)
            Whom = whom.Group.Members.ToArray();
        else
            Whom = [whom];

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
