using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

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
