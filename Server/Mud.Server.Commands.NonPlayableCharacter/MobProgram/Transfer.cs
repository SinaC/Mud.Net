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

[NonPlayableCharacterCommand("mptransfer", "MobProgram", Hidden = true)]
[Help(
@"Lets the mobile transfer people.  The 'all' argument transfers
everyone in the current room to the specified location")]
[Syntax("mob transfer [target|'all'] [location]")]
public class Transfer : NonPlayableCharacterGameAction
{
    protected override IGuard<INonPlayableCharacter>[] Guards => [new RequiresAtLeastTwoArguments()];

    private IRoomManager RoomManager { get; }
    private ICharacterManager CharacterManager { get; }
    private IItemManager ItemManager { get; }

    public Transfer(IRoomManager roomManager, ICharacterManager characterManager, IItemManager itemManager)
    {
        RoomManager = roomManager;
        CharacterManager = characterManager;
        ItemManager = itemManager;
    }

    private IRoom Where { get; set; } = default!;
    private IPlayableCharacter[] Whom { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        // where
        Where = FindHelpers.FindLocation(RoomManager, CharacterManager, ItemManager, Actor, actionInput.Parameters[1])!;
        if (Where == null)
            return StringHelpers.LocationNotFound;
        if (Where.IsPrivate)
            return "That room is private right now.";

        // whom
        Whom = FindHelpers.Find(Actor.Room.PlayableCharacters, actionInput.Parameters[0]).ToArray();
        if (Whom.Length == 0)
            return StringHelpers.CharacterNotFound;

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        foreach (var pc in Whom)
        {
            if (pc.Fighting != null)
                pc.StopFighting(true);
            pc.ChangeRoom(Where, false);
        }
        Where.Recompute();
    }
}
