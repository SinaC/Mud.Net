using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.PlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Quest;

namespace Mud.Server.Commands.PlayableCharacter.Item;

[PlayableCharacterCommand("destroy", "Item", Priority = 999, NoShortcut = true)]
[Syntax("[cmd] <item>")]
public class Destroy : PlayableCharacterGameAction
{
    protected override IGuard<IPlayableCharacter>[] Guards => [new RequiresMinPosition(Positions.Standing), new CannotBeInCombat(), new RequiresAtLeastOneArgument { Message = "Destroy what ?"}];

    private ILogger<Destroy> Logger { get; }
    private IItemManager ItemManager { get; }

    public Destroy(ILogger<Destroy> logger, IItemManager itemManager)
    {
        Logger = logger;
        ItemManager = itemManager;
    }

    private IItem What { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

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
