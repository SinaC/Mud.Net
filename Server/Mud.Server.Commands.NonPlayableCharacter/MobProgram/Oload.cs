using Microsoft.Extensions.Logging;
using Mud.Blueprints.Item;
using Mud.Common;
using Mud.Server.Commands.Character.Item;
using Mud.Server.Domain.Attributes;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.NonPlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Commands.NonPlayableCharacter.MobProgram;

[NonPlayableCharacterCommand("mpoload", "MobProgram", Hidden = true)]
[Help("Lets the mobile load an object")]
[Syntax("mob oload [vnum] {level} {room|wear}")]
public class Oload : WearCharacterGameActionBase<INonPlayableCharacter, INonPlayableCharacterGameActionInfo>
{
    protected override IGuard<INonPlayableCharacter>[] Guards => [new RequiresAtLeastOneArgument()];

    private IItemManager ItemManager { get; }

    public Oload(ILogger<Oload> logger, IItemManager itemManager)
        : base(logger)
    {
        ItemManager = itemManager;
    }

    private int BlueprintId { get; set; }
    private ItemBlueprintBase ItemBlueprint { get; set; } = default!;
    private int ItemLevel { get; set; }
    private bool EquipItem { get; set; }
    private bool LoadInRoom { get; set; }

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (!actionInput.Parameters[0].IsNumber)
            return BuildCommandSyntax();

        // blueprint
        BlueprintId = actionInput.Parameters[0].AsNumber;
        ItemBlueprint = ItemManager.GetItemBlueprint(BlueprintId)!;
        if (ItemBlueprint == null)
            return "No item with that id.";

        // level
        if (actionInput.Parameters.Length > 1)
        {
            if (!actionInput.Parameters[1].IsNumber)
                return "Item level must be a number.";
            ItemLevel = actionInput.Parameters[1].AsNumber;
            if (ItemLevel <= 0)
                return "Item level must be strictly positive.";
        }

        // wear|room
        if (actionInput.Parameters.Length > 2)
        {
            if (StringCompareHelpers.StringStartsWith("room", actionInput.Parameters[2].Value))
                LoadInRoom = true;
            else if (StringCompareHelpers.StringStartsWith("wear", actionInput.Parameters[2].Value))
                EquipItem = true;
            else
                return "You must specify room or wear";
        }

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        var container = ItemBlueprint.NoTake || LoadInRoom
            ? Actor.Room
            : Actor as IContainer;
        var item = ItemManager.AddItem(Guid.NewGuid(), ItemBlueprint, $"MPOload[{Actor.Name}]", container!);
        if (item == null)
        {
            Logger.LogError("MPOload: item with id {blueprintId} cannot be created", BlueprintId);
            return;
        }

        if (EquipItem)
            WearItem(item, true);
    }
}
