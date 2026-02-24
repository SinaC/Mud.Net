using Mud.Server.Common.Helpers;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.NonPlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Commands.NonPlayableCharacter.MobProgram;

[NonPlayableCharacterCommand("mpotransfer", "MobProgram", Hidden = true)]
[Help(
@"Lets the mobile to transfer an object. The object must be in the same
room with the mobile.")]
[Syntax("mob otransfer [item name] [location]")]
public class Otransfer : NonPlayableCharacterGameAction
{
    protected override IGuard<INonPlayableCharacter>[] Guards => [new RequiresAtLeastTwoArguments()];

    private IRoomManager RoomManager { get; }
    private ICharacterManager CharacterManager { get; }
    private IItemManager ItemManager { get; }

    public Otransfer(IRoomManager roomManager, ICharacterManager characterManager, IItemManager itemManager)
    {
        RoomManager = roomManager;
        CharacterManager = characterManager;
        ItemManager = itemManager;
    }

    private IRoom Where { get; set; } = default!;
    private IItem What { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        var where = FindHelpers.FindLocation(RoomManager, CharacterManager, ItemManager, Actor, actionInput.Parameters[1])!;
        if (where == null)
            return StringHelpers.LocationNotFound;
        if (where.IsPrivate)
            return "That room is private right now.";
        Where = where;

        var what = FindHelpers.FindItemHere(Actor, actionInput.Parameters[0]);
        if (what == null)
            return StringHelpers.ItemNotFound;
        What = what;
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        What.ChangeEquippedBy(null, false);
        What.ChangeContainer(Where);

        Actor.Recompute();
        Actor.Room.Recompute();
        Where.Recompute();
    }
}
