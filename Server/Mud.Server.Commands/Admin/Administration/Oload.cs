using Mud.Blueprints.Item;
using Mud.Domain;
using Mud.Flags;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Commands.Admin.Administration;

[AdminCommand("iload", "Admin"), MustBeImpersonated]
[Alias("oload")]
[Syntax("[cmd] <id>")]
[Help(
@"The [cmd] command is used to load new objects (use clone to 
duplicate strung items).  The vnums can be found with the vnum
command, or by stat'ing an existing object.

Load puts objects in inventory if they can be carried, otherwise they are
put in the room. Old format objects must be given a level argument to
determine their power, new format objects have a preset level that cannot
be changed without set.")]
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
        var item = ItemManager.AddItem(Guid.NewGuid(), ItemBlueprint, $"Oload[{Actor.Name}]", container!);
        if (item == null)
        {
            Wiznet.Log($"DoIload: item with id {BlueprintId} cannot be created", new WiznetFlags("Bugs"), AdminLevels.Implementor);
            Actor.Send("Item cannot be created.");
            return;
        }

        Wiznet.Log($"{Actor.DisplayName} loads {item.DebugName}.", new WiznetFlags("Load"));

        Impersonating.Act(ActOptions.ToAll, "{0:N} {0:h} created {1}!", Actor.Impersonating!, item);
        Actor.Send("Ok.");
    }
}
