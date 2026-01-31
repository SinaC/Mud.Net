using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.Guards.PlayableCharacterGuards;

public class RequiresAtLeastOneArgument : CharacterGuards.RequiresAtLeastOneArgument, IGuard<IPlayableCharacter>
{
    public string? Guards(IPlayableCharacter character, IActionInput actionInput, IGameAction gameAction)
        => base.Guards(character, actionInput, gameAction);
}
