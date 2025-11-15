using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using System.Text;

namespace Mud.Server.Admin.Administration;

[AdminCommand("goto", "Admin", MustBeImpersonated = true)]
[Syntax("[cmd] <location>")]
public class Goto : AdminGameAction
{
    private IRoomManager RoomManager { get; }
    private ICharacterManager CharacterManager { get; }
    private IItemManager ItemManager { get; }

    public Goto(IRoomManager roomManager, ICharacterManager characterManager, IItemManager itemManager)
    {
        RoomManager = roomManager;
        CharacterManager = characterManager;
        ItemManager = itemManager;
    }

    protected IRoom Where { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            return BuildCommandSyntax();

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
