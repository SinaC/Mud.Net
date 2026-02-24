using Mud.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.NonPlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.NonPlayableCharacter.MobProgram;

[NonPlayableCharacterCommand("mpasound", "MobProgram", Hidden = true)]
[Syntax("mob asound [string]")]
[Help("Prints the argument to all the rooms aroud the mobile")]
public class Asound : NonPlayableCharacterGameAction
{
    protected override IGuard<INonPlayableCharacter>[] Guards => [new RequiresAtLeastOneArgument()];

    public override void Execute(IActionInput actionInput)
    {
        // send message to every PC room around
        var room = Actor.Room;
        foreach (var direction in Enum.GetValues<ExitDirections>())
        {
            var exit = room[direction];
            var destination = exit?.Destination;
            if (exit != null && destination != null)
            {
                foreach(var pc in destination.PlayableCharacters)
                    pc.Send(actionInput.RawParameters);
            }
        }
    }
}
