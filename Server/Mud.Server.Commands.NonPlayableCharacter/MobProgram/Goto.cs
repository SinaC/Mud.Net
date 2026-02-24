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

[NonPlayableCharacterCommand("mpgoto", "MobProgram", Hidden = true)]
[Help("Lets the mobile goto any location it wishes that is not private.")]
[Syntax("mob goto [location]")]
public class Goto : NonPlayableCharacterGameAction
{
    protected override IGuard<INonPlayableCharacter>[] Guards => [new RequiresAtLeastOneArgument()];

    private IRoomManager RoomManager { get; }
    private ICharacterManager CharacterManager { get; }
    private IItemManager ItemManager { get; }

    public Goto(IRoomManager roomManager, ICharacterManager characterManager, IItemManager itemManager)
    {
        RoomManager = roomManager;
        CharacterManager = characterManager;
        ItemManager = itemManager;
    }

    private IRoom Where { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        Where = FindHelpers.FindLocation(RoomManager, CharacterManager, ItemManager, Actor, actionInput.Parameters[0])!;
        if (Where == null)
            return StringHelpers.LocationNotFound;
        if (Where.IsPrivate && Where.People.Count() > 1)
            return "That room is private right now.";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        if (Actor.Fighting != null)
            Actor.StopFighting(true);
        Actor.ChangeRoom(Where, true);
    }
}
