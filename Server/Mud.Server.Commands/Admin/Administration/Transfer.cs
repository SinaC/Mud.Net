using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.AdminGuards;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using System.Text;

namespace Mud.Server.Commands.Admin.Administration;

[AdminCommand("transfer", "Admin")]
[Alias("teleport")]
[Syntax(
        "[cmd] <character> (if impersonated)",
        "[cmd] <character> <location>")]
[Help(
@"[cmd] transfers the target character to your current location (default)
or to a specified location.")]
public class Transfer : AdminGameAction
{
    protected override IGuard<IAdmin>[] Guards => [new RequiresAtLeastOneArgument()];

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
    private ICharacter Whom { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (Actor.Impersonating == null && actionInput.Parameters.Length == 1)
            return "Transfer without specifying location can only be used when impersonating.";

        // TODO: IsAll ?

        if (Actor.Impersonating != null)
            Where = actionInput.Parameters.Length == 1
                ? Impersonating.Room
                : FindHelpers.FindLocation(RoomManager, CharacterManager, ItemManager, Actor.Impersonating, actionInput.Parameters[1])!;
        else
            Where = FindHelpers.FindLocation(RoomManager, CharacterManager, ItemManager, actionInput.Parameters[1])!;
        if (Where == null)
            return "No such location.";
        if (Where.IsPrivate)
            return "That room is private right now.";

        Whom = Actor.Impersonating != null
            ? FindHelpers.FindChararacterInWorld(CharacterManager, Actor.Impersonating, actionInput.Parameters[0])!
            : FindHelpers.FindByName(CharacterManager.Characters, actionInput.Parameters[0])!;
        if (Whom == null)
            return StringHelpers.CharacterNotFound;

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        if (Whom.Fighting != null)
            Whom.StopFighting(true);
        Whom.Act(ActOptions.ToRoom, "{0:N} disappears in a mushroom cloud.", Whom);
        Whom.ChangeRoom(Where, true);
        Whom.Act(ActOptions.ToRoom, "{0:N} appears from a puff of smoke.", Whom);
        if (Whom != Actor.Impersonating)
        {
            if (Actor.Impersonating != null)
                Whom.Act(ActOptions.ToCharacter, "{0:N} has transferred you.", Actor.Impersonating);
            else
                Whom.Act(ActOptions.ToCharacter, "Someone has transferred you.");
        }
        StringBuilder sb = new StringBuilder();
        Whom.Room.Append(sb, Whom);
        Whom.Send(sb);

        Actor.Send("Ok");
    }
}
