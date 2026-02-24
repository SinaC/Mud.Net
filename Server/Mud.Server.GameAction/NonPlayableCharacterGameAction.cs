using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.GameAction;

public abstract class NonPlayableCharacterGameAction : CharacterGameActionBase<INonPlayableCharacter, INonPlayableCharacterGameActionInfo>
{
    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (Actor.Leader != null || Actor.Master != null)
            return $"You have no free will!";

        return null;
    }
}
