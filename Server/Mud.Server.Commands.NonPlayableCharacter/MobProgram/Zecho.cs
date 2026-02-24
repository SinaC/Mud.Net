using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.NonPlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.NonPlayableCharacter.MobProgram;

[NonPlayableCharacterCommand("mpzecho", "MobProgram", Hidden = true)]
[Help("Prints the argument to all players in the same area as the mob")]
[Syntax("mob zecho [string]")]
public class Zecho : NonPlayableCharacterGameAction
{
    protected override IGuard<INonPlayableCharacter>[] Guards => [new RequiresAtLeastOneArgument()];

    public override void Execute(IActionInput actionInput)
    {
        // send message to every PC in area
        foreach (var pc in Actor.Room.Area.PlayableCharacters)
            pc.Send(actionInput.RawParameters);
    }
}
