using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Guards.CharacterGuards;

public class RequiresAtLeastOneArgument : ActorGuards.RequiresAtLeastOneArgument, IGuard<ICharacter>
{
    public string? Guards(ICharacter character, IActionInput actionInput, IGameAction gameAction)
        => base.Guards(character, actionInput, gameAction);
}
