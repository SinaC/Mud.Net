using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Admin.Administration;

[AdminCommand("iload", "Admin", MustBeImpersonated = true)]
[Alias("oload")]
[Syntax("[cmd] <id>")]
public class Oload : AdminGameAction
{
    private IItemManager ItemManager { get; }
    private IWiznet Wiznet { get; }

    public Oload(IItemManager itemManager, IWiznet wiznet)
    {
        ItemManager = itemManager;
        Wiznet = wiznet;
    }

    protected int BlueprintId { get; set; }
    protected ItemBlueprintBase ItemBlueprint { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0 || !actionInput.Parameters[0].IsNumber)
            return BuildCommandSyntax();

        BlueprintId = actionInput.Parameters[0].AsNumber;

        ItemBlueprint = ItemManager.GetItemBlueprint(BlueprintId)!;
        if (ItemBlueprint == null)
            return "No item with that id.";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        var container = ItemBlueprint.NoTake
            ? Impersonating.Room
            : Actor.Impersonating as IContainer;
        var item = ItemManager.AddItem(Guid.NewGuid(), ItemBlueprint, container!);
        if (item == null)
        {
            Wiznet.Wiznet($"DoIload: item with id {BlueprintId} cannot be created", WiznetFlags.Bugs, AdminLevels.Implementor);
            Actor.Send("Item cannot be created.");
            return;
        }

        Wiznet.Wiznet($"{Actor.DisplayName} loads {item.DebugName}.", WiznetFlags.Load);

        Impersonating.Act(ActOptions.ToAll, "{0:N} {0:h} created {1}!", Actor.Impersonating!, item);
        Actor.Send("Ok.");
    }
}
