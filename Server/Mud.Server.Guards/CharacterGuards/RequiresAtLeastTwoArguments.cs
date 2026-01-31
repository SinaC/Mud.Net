using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.Guards.CharacterGuards;

public class RequiresAtLeastTwoArguments : ActorGuards.RequiresAtLeastTwoArguments, IGuard<ICharacter>
{
    public string? Guards(ICharacter character, IActionInput actionInput, IGameAction gameAction)
        => base.Guards(character, actionInput, gameAction);
}
