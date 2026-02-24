using Mud.Server.Common.Helpers;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.NonPlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Parser.Interfaces;

namespace Mud.Server.Commands.NonPlayableCharacter.MobProgram;

[NonPlayableCharacterCommand("mpat", "MobProgram", Hidden = true)]
[Syntax("mob at [location] [commands]")]
[Help("Lets the mobile do a command at another location.")]
public class At : NonPlayableCharacterGameAction
{
    protected override IGuard<INonPlayableCharacter>[] Guards => [new RequiresAtLeastTwoArguments()];

    private IRoomManager RoomManager { get; }
    private ICharacterManager CharacterManager { get; }
    private IItemManager ItemManager { get; }
    private IParser Parser { get; }

    public At(IRoomManager roomManager, ICharacterManager characterManager, IItemManager itemManager, IParser parser)
    {
        RoomManager = roomManager;
        CharacterManager = characterManager;
        ItemManager = itemManager;
        Parser = parser;
    }

    private IRoom Where { get; set; } = default!;
    private string What { get; set; } = default!;

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

        What = Parser.JoinParameters(actionInput.Parameters.Skip(1));
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        var originalRoom = Actor!.Room;
        // move to destination
        Actor.ChangeRoom(Where, false);

        // perform action
        var processed = Actor.ProcessInput(What);
        if (!processed)
            Actor.Send(StringHelpers.SomethingGoesWrong);

        // move back to original room
        if (!Actor.IsValid)
            return;
        Actor.ChangeRoom(originalRoom, false);
    }
}
