using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Guards.PlayableCharacterGuards;

public class RequiresAtLeastOneArgument : CharacterGuards.RequiresAtLeastOneArgument, IGuard<IPlayableCharacter>
{
    public string? Guards(IPlayableCharacter character, IActionInput actionInput, IGameAction gameAction)
        => base.Guards(character, actionInput, gameAction);
}
