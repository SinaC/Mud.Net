using Mud.Common;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using System.Text;

namespace Mud.Server.Common.Helpers;

public static class ItemsHelpers
{
    public static StringBuilder AppendItems(StringBuilder sb, IEnumerable<IItem> items, ICharacter actor, bool shortDisplay, bool displayNothing) // equivalent to act_info.C:show_list_to_char
    {
        var enumerable = items as IItem[] ?? items.ToArray();
        if (displayNothing && enumerable.Length == 0)
            sb.AppendLine("Nothing.");
        else
        {
            // Grouped by description
            foreach (var groupedFormattedItem in enumerable.Select(item => item.Append(new StringBuilder(), actor, shortDisplay).ToString()).GroupBy(x => x))
            {
                int count = groupedFormattedItem.Count();
                if (count > 1)
                    sb.AppendFormatLine("%W%({0,2})%x% {1}", count, groupedFormattedItem.Key);
                else
                    sb.AppendFormatLine("     {0}", groupedFormattedItem.Key);
            }
        }

        return sb;
    }
}
