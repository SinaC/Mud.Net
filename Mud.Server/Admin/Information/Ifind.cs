using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mud.Server.Admin.Information
{
    [AdminCommand("ifind", "Information")]
    [Alias("ofind")]
    [Syntax("[cmd] <item>")]
    public class Ifind : AdminGameAction
    {
        private IItemManager ItemManager { get; }

        public ICommandParameter Pattern { get; protected set; }

        public Ifind(IItemManager itemManager)
        {
            ItemManager = itemManager;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;
            if (actionInput.Parameters.Length == 0)
                return BuildCommandSyntax();
            Pattern = actionInput.Parameters[0];
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Searching items '{Pattern.Value}'");
            List<IItem> items = FindHelpers.FindAllByName(ItemManager.Items, Pattern).OrderBy(x => x.Blueprint?.Id).ToList();
            if (items.Count == 0)
                sb.AppendLine("No matches");
            else
            {
                sb.AppendLine("Id         DisplayName                    ContainedInto");
                foreach (IItem item in items)
                    sb.AppendLine($"{item.Blueprint.Id,-10} {item.DisplayName,-30} {DisplayEntityAndContainer(item) ?? "(none)"}");
            }
            Actor.Page(sb);
        }

        private static string DisplayEntityAndContainer(IEntity entity)
        {
            if (entity == null)
                return "???";
            StringBuilder sb = new StringBuilder();
            sb.Append(entity.DebugName);
            // don't to anything if entity is IRoom
            if (entity is IItem item)
            {
                if (item.ContainedInto != null)
                {
                    sb.Append(" in ");
                    sb.Append("<");
                    sb.Append(DisplayEntityAndContainer(item.ContainedInto));
                    sb.Append(">");
                }
                else if (item.EquippedBy != null)
                {
                    sb.Append(" equipped by ");
                    sb.Append("<");
                    sb.Append(DisplayEntityAndContainer(item.EquippedBy));
                    sb.Append(">");
                }
                else
                    sb.Append("Seems to be nowhere!!!");
            }

            if (entity is ICharacter character)
            {
                sb.Append("<");
                sb.Append(DisplayEntityAndContainer(character.Room));
                sb.Append(">");
            }
            return sb.ToString();
        }
    }
}
