using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Quest;

namespace Mud.Server.Commands.Character.PlayableCharacter.Item;

[PlayableCharacterCommand("destroy", "Item", Priority = 999, NoShortcut = true), MinPosition(Positions.Standing), NotInCombat]
[Syntax("[cmd] <item>")]
public class Destroy : PlayableCharacterGameAction
{
    private ILogger<Destroy> Logger { get; }
    private IItemManager ItemManager { get; }

    public Destroy(ILogger<Destroy> logger, IItemManager itemManager)
    {
        Logger = logger;
        ItemManager = itemManager;
    }

    protected IItem What { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            return "Destroy what?";

        var item = FindHelpers.FindByName(Actor.Inventory.Where(Actor.CanSee), actionInput.Parameters[0]);
        if (item == null)
            return StringHelpers.ItemInventoryNotFound;

        What = item;
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        // Remove from inventory
        What.ChangeContainer(null);
        // Update quest if needed
        if (What is IItemQuest itemQuest)
        {
            foreach (IQuest quest in Actor.ActiveQuests)
                quest.Update(itemQuest, true);
        }
        //
        Logger.LogDebug("Manually destroying item {item} by {actor}", What.DebugName, Actor.DebugName);
        Actor.Send($"You destroy {What.RelativeDisplayName(Actor)}.");

        ItemManager.RemoveItem(What);
        Actor.Recompute();
    }
}
