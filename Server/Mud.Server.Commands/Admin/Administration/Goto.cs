using Mud.Server.Domain.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.AdminGuards;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using System.Text;

namespace Mud.Server.Commands.Admin.Administration;

[AdminCommand("goto", "Admin")]
[Syntax("[cmd] <location>")]
[Help(
@"[cmd] takes you to a location.  The location may be specified as the name
of a room, as the name of a mobile, or as the name of an object.

You may not [cmd] a room if it is PRIVATE and has two (or more) characters
already present, or if it is SOLITARY and has one (or more) characters
already present. Some other rooms are barred to players below a certain
god level.")]
public class Goto : AdminGameAction
{
    protected override IGuard<IAdmin>[] Guards => [new MustBeImpersonated(), new RequiresAtLeastOneArgument()];

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

        Where = FindHelpers.FindLocation(RoomManager, CharacterManager, ItemManager, Impersonating, actionInput.Parameters[0])!;
        if (Where == null)
            return "No such location.";
        if (Where.IsPrivate && Where.People.Count() > 1)
            return "That room is private right now.";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        if (Impersonating.Fighting != null)
            Impersonating.StopFighting(true);
        Impersonating.Act(Impersonating.Room.People.Where(x => x != Impersonating && x.CanSee(Impersonating)), "{0:N} leaves in a swirling mist.", Impersonating); // Don't display 'Someone leaves ...' if Impersonating is not visible
        Impersonating.ChangeRoom(Where, true);
        Impersonating.Act(Impersonating.Room.People.Where(x => x != Impersonating && x.CanSee(Impersonating)), "{0:N} appears in a swirling mist.", Impersonating);
        StringBuilder sb = new ();
        Impersonating.Room.Append(sb, Impersonating);
        Impersonating.Send(sb);
    }
}
