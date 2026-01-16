using Mud.Server.Domain;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.Guards.CharacterGuards;

public class ShapesGuard(Shapes[] shapes) : ICharacterGuard
{
    public Shapes[] Shapes { get; } = shapes;

    public string? Guards(ICharacter actor)
    {
        if (!Shapes.Contains(actor.Shape))
        {
            if (Shapes.Length > 1)
                return $"You are not in {string.Join(" or ", Shapes.Select(x => x.ToString()))} shape";
            return $"You are not in {Shapes.Single()} shape";
        }
        return null;
    }
}