using Mud.Server.Common.Helpers;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.NonPlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Commands.NonPlayableCharacter.MobProgram;

[NonPlayableCharacterCommand("mpremove", "MobProgram", Hidden = true)]
[Help(
@"Lets the mobile to strip an object or all objects from the victim.
Useful for removing e.g. quest objects from a character.")]
[Syntax("mob remove [victim] [object vnum|'all']")]
public class Remove : NonPlayableCharacterGameAction
{
    protected override IGuard<INonPlayableCharacter>[] Guards => [new RequiresAtLeastTwoArguments()];

    private IItemManager ItemManager { get; }

    public Remove(IItemManager itemManager)
    {
        ItemManager = itemManager;
    }

    private ICharacter Whom { get; set; } = default!;
    private IItem[] What { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        // whom
        Whom = FindHelpers.FindByName(Actor.Room.People, actionInput.Parameters[0])!;

        // what
        if (actionInput.Parameters[1].IsNumber)
        {
            var blueprintId = actionInput.Parameters[1].AsNumber;
            What = Whom.Inventory.Where(x => x.Blueprint.Id == blueprintId).Concat(Whom.Equipments.Where(x => x.Item?.Blueprint.Id == blueprintId).Select(x => x.Item!)).ToArray();
        }
        else
            What = Whom.Inventory.Concat(Whom.Equipments.Where(x => x.Item is not null).Select(x => x.Item!)).ToArray();
        if (What.Length == 0)
            return StringHelpers.NotFound;

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        foreach (var item in What)
            ItemManager.RemoveItem(item);
    }
}
