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

[NonPlayableCharacterCommand("mpgtransfer", "MobProgram", Hidden = true)]
[Help("Lets the mobile transfer all chars in same group as the victim.")]
[Syntax("mob gtransfer [victim] [location]")]
public class Gtransfer : NonPlayableCharacterGameAction
{
    protected override IGuard<INonPlayableCharacter>[] Guards => [new RequiresAtLeastTwoArguments()];

    private IRoomManager RoomManager { get; }
    private ICharacterManager CharacterManager { get; }
    private IItemManager ItemManager { get; }

    public Gtransfer(IRoomManager roomManager, ICharacterManager characterManager, IItemManager itemManager)
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

        Where = FindHelpers.FindLocation(RoomManager, CharacterManager, ItemManager, Actor, actionInput.Parameters[1])!;
        if (Where == null)
            return StringHelpers.LocationNotFound;
        if (Where.IsPrivate)
            return "That room is private right now.";

        var whom = FindHelpers.FindByName(Actor.Room.PlayableCharacters, actionInput.Parameters[0]);
        if (whom == null)
            return StringHelpers.CharacterNotFound;
        if (whom.Group != null)
            Whom = whom.Group.Members.ToArray();
        else
            Whom = [whom];

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
