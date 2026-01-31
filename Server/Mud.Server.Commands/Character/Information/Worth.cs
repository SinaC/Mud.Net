using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.Commands.Character.Information;

[CharacterCommand("worth", "Information")]
public class Worth : CharacterGameAction
{
    protected override IGuard<ICharacter>[] Guards => [];

    public override void Execute(IActionInput actionInput)
    {
        Actor.Send("You have {0} gold coins and {1} silver coins.", Actor.GoldCoins, Actor.SilverCoins);
    }
}
