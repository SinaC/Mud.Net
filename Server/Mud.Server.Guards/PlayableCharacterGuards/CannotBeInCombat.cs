using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.Guards.PlayableCharacterGuards;

public class CannotBeInCombat : CharacterGuards.CannotBeInCombat, IGuard<IPlayableCharacter>
{
    public string? Guards(IPlayableCharacter playableCharacter, IActionInput actionInput, IGameAction gameAction)
        => base.Guards(playableCharacter, actionInput, gameAction);
}