using Mud.Server.Domain;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Guards.CharacterGuards;

public class RequiresShapesGuard(Shapes[] shapes) : IGuard<ICharacter>
{
    public Shapes[] Shapes { get; } = shapes;

    public string? Guards(ICharacter character, IActionInput actionInput, IGameAction gameAction)
    {
        if (!Shapes.Contains(character.Shape))
        {
            if (Shapes.Length > 1)
                return $"You are not in {string.Join(" or ", Shapes.Select(x => x.ToString()))} shape";
            return $"You are not in {Shapes.Single()} shape";
        }
        return null;
    }
}