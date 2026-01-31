using Mud.Server.Domain;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.Guards.CharacterGuards;

public class CannotBeShapeshift : IGuard<ICharacter>
{
    public string? Guards(ICharacter character, IActionInput actionInput, IGameAction gameAction)
    {
        if (character.Shape != Shapes.Normal)
            return "You must be in your normal shape to do that.";
        return null;
    }
}