using Mud.Server.CommandParser.Interfaces;
using Mud.Server.Domain.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.AdminGuards;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using System.Text;

namespace Mud.Server.Commands.Admin.Information;

[AdminCommand("ifind", "Information")]
[Alias("ofind")]
[Syntax("[cmd] <item>")]
public class Ifind : AdminGameAction
{
    protected override IGuard<IAdmin>[] Guards => [new RequiresAtLeastOneArgument()];

    private IItemManager ItemManager { get; }

    public Ifind(IItemManager itemManager)
    {
        ItemManager = itemManager;
    }

    private ICommandParameter Pattern { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        Pattern = actionInput.Parameters[0];

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        StringBuilder sb = new();
        sb.AppendLine($"Searching items '{Pattern.Value}'");
        List<IItem> items = FindHelpers.FindAllByName(ItemManager.Items, Pattern).OrderBy(x => x.Blueprint.Id).ToList();
        if (items.Count == 0)
            sb.AppendLine("No matches");
        else
        {
            sb.AppendLine("Id       DisplayName                    ContainedInto");
            sb.AppendLine("-----------------------------------------------------");
            foreach (var item in items)
                sb.AppendLine($"{item.Blueprint.Id,-8} {item.DisplayName,-30} {DisplayEntityAndContainer(item) ?? "(none)"}");
        }
        Actor.Page(sb);
    }

    private static string DisplayEntityAndContainer(IEntity entity)
    {
        if (entity == null)
            return "???";
        StringBuilder sb = new();
        sb.Append(entity.DebugName);
        // don't to anything if entity is IRoom
        if (entity is IItem item)
        {
            if (item.ContainedInto != null)
            {
                sb.Append(" in ");
                sb.Append('<');
                sb.Append(DisplayEntityAndContainer(item.ContainedInto));
                sb.Append('>');
            }
            else if (item.EquippedBy != null)
            {
                sb.Append(" equipped by ");
                sb.Append('<');
                sb.Append(DisplayEntityAndContainer(item.EquippedBy));
                sb.Append('>');
            }
            else
                sb.Append("Seems to be nowhere!!!");
        }

        if (entity is ICharacter character)
        {
            sb.Append('<');
            sb.Append(DisplayEntityAndContainer(character.Room));
            sb.Append('>');
        }
        return sb.ToString();
    }
}
