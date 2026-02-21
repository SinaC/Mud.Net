using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Guards.NonPlayableCharacterGuards;

public class RequiresAtLeastOneArgument : CharacterGuards.RequiresAtLeastOneArgument, IGuard<INonPlayableCharacter>
{
    public string? Guards(INonPlayableCharacter character, IActionInput actionInput, IGameAction gameAction)
        => base.Guards(character, actionInput, gameAction);
}
