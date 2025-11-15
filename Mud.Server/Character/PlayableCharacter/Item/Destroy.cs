using Mud.Domain;
using Mud.Logger;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Quest;

namespace Mud.Server.Character.PlayableCharacter.Item;

[PlayableCharacterCommand("destroy", "Item", Priority = 50, NoShortcut = true, MinPosition = Positions.Standing, NotInCombat = true)]
[Syntax("[cmd] <item>")]
public class Destroy : PlayableCharacterGameAction
{
    private IItemManager ItemManager { get; }

    public Destroy(IItemManager itemManager)
    {
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

        var item = FindHelpers.FindByName(Actor.Inventory.Where(x => Actor.CanSee(x)), actionInput.Parameters[0]);
        if (item == null)
            return StringHelpers.ItemInventoryNotFound;

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        // Remove from inventory
        What.ChangeContainer(null);
        // Update quest if needed
        if (What is IItemQuest itemQuest)
        {
            foreach (IQuest quest in Actor.Quests)
                quest.Update(itemQuest, true);
        }
        //
        Log.Default.WriteLine(LogLevels.Debug, "Manually destroying item {0} by {1}", What.DebugName, Actor.DebugName);
        Actor.Send($"You destroy {What.RelativeDisplayName(Actor)}.");

        ItemManager.RemoveItem(What);
        Actor.Recompute();
    }
}
