using Mud.Domain;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Commands.Admin;

[AdminCommand("incarnate", "Admin"), CannotBeImpersonated]
[Syntax(
        "[cmd]",
        "[cmd] room <name|id>",
        "[cmd] item name",
        "[cmd] mob name")]
public class Incarnate : AdminGameAction
{
    private IRoomManager RoomManager { get; }
    private ICharacterManager CharacterManager { get; }
    private IItemManager ItemManager { get; }
    private IWiznet Wiznet { get; }

    public Incarnate(IRoomManager roomManager, ICharacterManager characterManager, IItemManager itemManager, IWiznet wiznet)
    {
        RoomManager = roomManager;
        CharacterManager = characterManager;
        ItemManager = itemManager;
        Wiznet = wiznet;
    }

    protected IEntity Target { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
        {
            if (Actor.Incarnating == null)
                return BuildCommandSyntax();
            Target = null!; // un-incarnate
            return null;
        }

        if (actionInput.Parameters.Length == 1)
            return BuildCommandSyntax();

        string kind = actionInput.Parameters[0].Value;
        if ("room".StartsWith(kind))
        {
            if (actionInput.Parameters[1].IsNumber)
            {
                int id = actionInput.Parameters[1].AsNumber;
                Target = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint.Id == id)!;
            }
            else
                Target = FindHelpers.FindByName(RoomManager.Rooms, actionInput.Parameters[1])!;
        }
        else if ("item".StartsWith(kind))
            Target = FindHelpers.FindByName(ItemManager.Items, actionInput.Parameters[1])!;
        else if ("mob".StartsWith(kind))
            Target = FindHelpers.FindByName(CharacterManager.Characters, actionInput.Parameters[1])!;
        if (Target == null)
            return "Target not found.";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        if (Target == null)
        {
            Wiznet.Log($"{Actor.DisplayName} stops incarnating {Actor.Incarnating?.DebugName}.", WiznetFlags.Incarnate);

            Actor.Send("%M%You stop incarnating %C%{0}%x%.", Actor.Incarnating?.DisplayName ?? "???");
            Actor.StopIncarnating();

            return;
        }

        if (Actor.Incarnating != null)
        {
            Actor.Send("%M%You stop incarnating %C%{0}%x%.", Actor.Incarnating?.DisplayName ?? "???");
            Actor.StopIncarnating();
        }

        var incarnated = Actor.StartIncarnating(Target);
        if (incarnated)
        {
            string msg = $"{Actor.DisplayName} starts incarnating {Target.DebugName}.";
            Wiznet.Log(msg, WiznetFlags.Incarnate);

            Actor.Send("%M%You start incarnating %C%{0}%x%.", Target.DisplayName);
        }
        else
            Actor.Send($"Something prevented you from being incarnating {Target.DisplayName}.");
    }
}
