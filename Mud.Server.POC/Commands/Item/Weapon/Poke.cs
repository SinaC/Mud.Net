using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.POC.Commands.Item.Weapon;

[Command("poke", "POC")]
public class Poke : GameActionBase<IItemWeapon, IGameActionInfo>
{
    public override void Execute(IActionInput actionInput)
    {
        var container = Actor.EquippedBy ?? Actor.ContainedInto;

        if (container is IRoom room)
        {
            Actor.Act(room.People, "{0} pokes everyone.", Actor);
        }
        else if (container is ICharacter character)
        {
            Actor.Act(character.Room.People, "{0} pokes {1}.", Actor, character);
        }
    }
}
