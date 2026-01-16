using Mud.Server.Domain;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.Guards.CharacterGuards;

public class NoShapeshiftGuard : ICharacterGuard
{
    public string? Guards(ICharacter actor)
    {
        if (actor.Shape != Shapes.Normal)
            return "You must be in your normal shape to do that.";
        return null;
    }
}