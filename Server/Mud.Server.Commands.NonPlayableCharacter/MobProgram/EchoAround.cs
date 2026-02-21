using Mud.Server.Common.Helpers;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.NonPlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.NonPlayableCharacter.MobProgram;

[NonPlayableCharacterCommand("mpechoaround", "MobProgram", Hidden = true)]
[Syntax("mob echoaround [victim] [string]")]
[Help("Prints the message to everyone in the room other than the mob and victim")]
public class EchoAround : NonPlayableCharacterGameAction
{
    protected override IGuard<INonPlayableCharacter>[] Guards => [new RequiresAtLeastOneArgument()];

    private ICharacter Whom { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        Whom = FindHelpers.FindByName(Actor.Room.People, actionInput.Parameters[0])!;
        if (Whom == null)
            return StringHelpers.CharacterNotFound;

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        // send the message to every PC in the room other than the mob and victim
        foreach (var pc in Actor.Room.PlayableCharacters.Where(x => x != Actor && x != Whom))
            pc.Send(actionInput.RawParameters);
    }
}
