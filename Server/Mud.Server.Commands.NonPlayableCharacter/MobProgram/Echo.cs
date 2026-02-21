using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.NonPlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.NonPlayableCharacter.MobProgram;

[NonPlayableCharacterCommand("mpecho", "MobProgram", Hidden = true)]
[Syntax("mob echo [string]")]
[Help("Prints the message to the room at large")]
public class Echo : NonPlayableCharacterGameAction
{
    protected override IGuard<INonPlayableCharacter>[] Guards => [new RequiresAtLeastOneArgument()];

    public override void Execute(IActionInput actionInput)
    {
        // send message to every PC in the room
        foreach (var pc in Actor.Room.PlayableCharacters)
            pc.Send(actionInput.RawParameters);
    }
}
