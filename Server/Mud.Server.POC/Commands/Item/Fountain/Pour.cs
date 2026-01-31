using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.POC.Commands.Item.Fountain;

[ItemCommand("pour", "POC")]
public class Pour : ItemGameActionBase<IItemFountain, IItemGameActionInfo>
{
    protected override IGuard<IItemFountain>[] Guards => [];

    public override void Execute(IActionInput actionInput)
    {
        var container = Actor.EquippedBy ?? Actor.ContainedInto;

        if (container is IRoom room)
        {
            Actor.Act(room.People, "{0} pours {1} everywhere.", Actor, Actor.LiquidName ?? "???");
        }
        else if (container is ICharacter character)
        {
            Actor.Act(character.Room.People, "{0} pours {1} on {2}.", Actor, Actor.LiquidName ?? "???", character);
        }
    }
}
