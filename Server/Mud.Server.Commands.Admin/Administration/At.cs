using Mud.Server.Common.Helpers;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.AdminGuards;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Parser.Interfaces;

namespace Mud.Server.Commands.Admin.Administration;

[AdminCommand("at", "Admin")]
[Syntax("[cmd] <location> <command>")]
[Help(
@"[cmd] executes the given command (which may have arguments) at the given
location.  The location may be specified as a vnum, as the name of
a mobile, or as the name of an object.

At works by temporarily moving you to that location, executing the
command, and then moving you back (if the command didn't change your
location).")]
public class At : AdminGameAction
{
    protected override IGuard<IAdmin>[] Guards => [new MustBeImpersonated(), new RequiresAtLeastTwoArguments()];

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

        Where = FindHelpers.FindLocation(RoomManager, CharacterManager, ItemManager, Impersonating, actionInput.Parameters[0])!;
        if (Where == null)
            return StringHelpers.LocationNotFound;
        if (Where.IsPrivate && Where.People.Count() > 1)
            return "That room is private right now.";

        What = Parser.JoinParameters(actionInput.Parameters.Skip(1));
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        var originalRoom = Actor.Impersonating!.Room;
        // move to destination
        Actor.Impersonating.ChangeRoom(Where, false);

        // perform action
        var processed = Actor.ProcessInput(What);
        if (!processed)
            Actor.Send(StringHelpers.SomethingGoesWrong);

        // move back to original room
        if (Actor.Impersonating == null || !Actor.Impersonating.IsValid)
            return;
        Actor.Impersonating.ChangeRoom(originalRoom, false);
    }
}
