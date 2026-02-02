using Mud.Server.Domain;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

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