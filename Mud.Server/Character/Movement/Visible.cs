using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.Movement
{
    [CharacterCommand("visible", "Movement", MinPosition = Positions.Sleeping)]
    [Syntax("[cmd]")]
    public class Visible : CharacterGameAction
    {
        public override void Execute(IActionInput actionInput)
        {
            Actor.RemoveBaseCharacterFlags("Invisible", "Sneak", "Hide");
            Actor.RemoveAuras(x => x.AbilityName == "Invisibility"
                             || x.AbilityName == "Sneak"
                             || x.AbilityName == "Hide", true);
            Actor.Send("You are now visible");
        }
    }
}
