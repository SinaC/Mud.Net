using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.Information;

[CharacterCommand("worth", "Information")]
public class Worth : CharacterGameAction
{
    public override void Execute(IActionInput actionInput)
    {
        Actor.Send("You have {0} gold coins and {1} silver coins.", Actor.GoldCoins, Actor.SilverCoins);
    }
}
