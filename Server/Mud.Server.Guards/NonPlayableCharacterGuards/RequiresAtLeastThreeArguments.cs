using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Guards.NonPlayableCharacterGuards;

public class RequiresAtLeastThreeArguments : ActorGuards.RequiresAtLeastThreeArguments, IGuard<INonPlayableCharacter>
{
    public string? Guards(INonPlayableCharacter player, IActionInput actionInput, IGameAction gameAction)
        => base.Guards(player, actionInput, gameAction);
}
