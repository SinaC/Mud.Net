using Microsoft.Extensions.Logging;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.NonPlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Commands.NonPlayableCharacter.MobProgram;

[NonPlayableCharacterCommand("mpjunk", "MobProgram", Hidden = true)]
[Help(
@"Lets the mobile destroy an object in its inventory
it can also destroy a worn object and it can destroy 
items using all.xxxxx or just plain all of them")]
[Syntax(
"mob junk all",
"mob junk [item]",
"mob junk all.[item]")]
public class Junk : NonPlayableCharacterGameAction
{
    private ILogger<Junk> Logger { get; }
    private IItemManager ItemManager { get; }

    protected override IGuard<INonPlayableCharacter>[] Guards => [new RequiresAtLeastOneArgument()];

    public Junk(ILogger<Junk> logger, IItemManager itemManager)
    {
        Logger = logger;
        ItemManager = itemManager;
    }

    private IItem[] WhatInInventory { get; set; } = default!;
    private IEquippedItem[] WhatInEquipment { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        // what
        var whatParameter = actionInput.Parameters[0];
        var whatInInventory = FindHelpers.Find(Actor.Inventory.Where(Actor.CanSee), whatParameter).ToArray();
        var whatInEquipment = FindHelpers.Find(Actor.Equipments.Where(x => x.Item is not null && Actor.CanSee(x.Item)), whatParameter).ToArray();
        if (whatInInventory.Length == 0 && whatInEquipment.Length == 0)
            return StringHelpers.ItemInventoryNotFound;
        WhatInInventory = whatInInventory;
        WhatInEquipment = whatInEquipment;

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        foreach (var item in WhatInInventory)
        {
            Logger.LogDebug("Manually destroying item {item} by {actor}", item.DebugName, Actor.DebugName);
            // Remove from inventory
            item.ChangeContainer(null);
            ItemManager.RemoveItem(item);
        }
        foreach (var equipedItem in WhatInEquipment.Where(x => x.Item != null))
        {
            Logger.LogDebug("Manually destroying equipped item {item} by {actor}", equipedItem.Item!.DebugName, Actor.DebugName);
            // Remove from equipment
            equipedItem.Item!.ChangeContainer(null);
            equipedItem.Item!.ChangeEquippedBy(null, false);
            equipedItem.Item = null;
            ItemManager.RemoveItem(equipedItem.Item!);
        }
        Actor.Recompute();
    }
}
