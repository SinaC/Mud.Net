using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using System;

namespace Mud.Server.Admin.Administration
{
    [AdminCommand("iload", "Admin", Priority = 10, MustBeImpersonated = true)]
    [AdminCommand("oload", "Admin", Priority = 10, MustBeImpersonated = true)]
    [Syntax("[cmd] <id>")]
    public class Oload : AdminGameAction
    {
        private IItemManager ItemManager { get; }
        private IWiznet Wiznet { get; }

        public int BlueprintId { get; protected set; }
        public ItemBlueprintBase ItemBlueprint { get; protected set; }

        public Oload(IItemManager itemManager, IWiznet wiznet)
        {
            ItemManager = itemManager;
            Wiznet = wiznet;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0 || !actionInput.Parameters[0].IsNumber)
                return BuildCommandSyntax();

            BlueprintId = actionInput.Parameters[0].AsNumber;

            ItemBlueprint = ItemManager.GetItemBlueprint(BlueprintId);
            if (ItemBlueprint == null)
                return "No item with that id.";

            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            IContainer container = ItemBlueprint.NoTake
                ? Impersonating.Room
                : Actor.Impersonating as IContainer;
            IItem item = ItemManager.AddItem(Guid.NewGuid(), ItemBlueprint, container);
            if (item == null)
            {
                Wiznet.Wiznet($"DoIload: item with id {BlueprintId} cannot be created", WiznetFlags.Bugs, AdminLevels.Implementor);
                Actor.Send("Item cannot be created.");
                return;
            }

            Wiznet.Wiznet($"{Actor.DisplayName} loads {item.DebugName}.", WiznetFlags.Load);

            Impersonating.Act(ActOptions.ToAll, "{0:N} {0:h} created {1}!", Actor.Impersonating, item);
            Actor.Send("Ok.");
        }
    }
}
