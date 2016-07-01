using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mud.POC
{
    //sb.AppendLine("+-------------------------------------------------+");
    //sb.AppendLine("| Abilities                                       |");
    //sb.AppendLine("+-----+-----------------------+----------+--------+");
    //sb.AppendLine("| Lvl | Name                  | Resource | Cost   |");
    //sb.AppendLine("+-----+-----------------------+----------+--------+");
    //List<AbilityAndLevel> abilities = KnownAbilities
    //    .Where(x => (x.Ability.Flags & AbilityFlags.CannotBeUsed) != AbilityFlags.CannotBeUsed && (displayAll || x.Level <= Level))
    //    .OrderBy(x => x.Level)
    //    .ThenBy(x => x.Ability.Name)
    //    .ToList();
    //foreach (AbilityAndLevel abilityAndLevel in abilities)
    //{
    //    int level = abilityAndLevel.Level;
    //    IAbility ability = abilityAndLevel.Ability;
    //    if ((ability.Flags & AbilityFlags.Passive) == AbilityFlags.Passive)
    //        sb.AppendFormatLine("| {0,3} | {1,21} |  %m%passive ability%x%  |", level, ability.Name, StringHelpers.ResourceColor(ability.ResourceKind), ability.CostAmount);
    //    else if (ability.CostType == AmountOperators.Percentage)
    //        sb.AppendFormatLine("| {0,3} | {1,21} | {2,14} | {3,5}% |", level, ability.Name, StringHelpers.ResourceColor(ability.ResourceKind), ability.CostAmount);
    //    else if (ability.CostType == AmountOperators.Fixed)
    //        sb.AppendFormatLine("| {0,3} | {1,21} | {2,14} | {3,6} |", level, ability.Name, StringHelpers.ResourceColor(ability.ResourceKind), ability.CostAmount);
    //    else
    //        sb.AppendFormatLine("| {0,3} | {1,21} | %W%free cost ability%x% |", level, ability.Name, ability.ResourceKind, ability.CostAmount, ability.CostType == AmountOperators.Percentage? "%" : " ");
    //}
    //sb.AppendLine("+-----+-----------------------+----------+--------+");

    // +-----------------------------------------+
    // | Title                                   |
    // +-------+-------------------+------+------+
    // | H1    | H2                | H3   | H4   |
    // +-------+-------------------+------+------+
    // |   v11 |               v12 |  v13 |  v14 |
    // |   v21 |               v22 | v23**|  v24 |    <-- additional format (replace trailing space)
    // |   v11 |               v12 |       v13+4 |    <-- merged cells
    // +-------+-------------------+------+------+

    public class TableGenerator<T>
    {
        private class Column
        {
            public string Header { get; }
            public int Width { get; }

            public Func<T, string> GetValueFunc { get; }
            public Func<T, int> GetMergeLengthFunc { get; } = t => 0;
            public Func<T, string> GetTrailingSpaceFunc { get; } = t => " ";

            public Column(string header, int width, Func<T, string> getValueFunc)
            {
                Header = header;
                Width = width;
                GetValueFunc = getValueFunc;
            }

            public Column(string header, int width, Func<T, string> getValueFunc, Func<T, int> getMergeLengthFunc)
                : this(header, width, getValueFunc)
            {
                GetMergeLengthFunc = getMergeLengthFunc;
            }

            public Column(string header, int width, Func<T, string> getValueFunc, Func<T, string> getTrailingSpaceFunc)
                : this(header, width, getValueFunc)
            {
                GetTrailingSpaceFunc = getTrailingSpaceFunc;
            }

            public Column(string header, int width, Func<T, string> getValueFunc, Func<T, int> getMergeLengthFunc, Func<T, string> getTrailingSpaceFunc)
                :this(header, width, getValueFunc)
            {
                GetMergeLengthFunc = getMergeLengthFunc;
                GetTrailingSpaceFunc = getTrailingSpaceFunc;
            }
        }

        private readonly string _title;
        private readonly List<Column> _columns;

        public TableGenerator(string title)
        {
            _title = title;
            _columns = new List<Column>();
        }

        public void AddColumn(string header, int width, Func<T, string> getValueFunc)
        {
            Column column = new Column(header, width, getValueFunc);
            _columns.Add(column);
        }

        public void AddColumn(string header, int width, Func<T, string> getValueFunc, Func<T, int> getMergeLengthFunc)
        {
            Column column = new Column(header, width, getValueFunc, getMergeLengthFunc);
            _columns.Add(column);
        }

        public void AddColumn(string header, int width, Func<T, string> getValueFunc, Func<T, string> getTrailingSpaceFunc)
        {
            Column column = new Column(header, width, getValueFunc, getTrailingSpaceFunc);
            _columns.Add(column);
        }

        public void AddColumn(string header, int width, Func<T, string> getValueFunc, Func<T, int> getMergeLengthFunc, Func<T, string> getTrailingSpaceFunc)
        {
            Column column = new Column(header, width, getValueFunc, getMergeLengthFunc, getTrailingSpaceFunc);
            _columns.Add(column);
        }

        public StringBuilder Generate(IEnumerable<T> items) // no merged cells
        {
            StringBuilder sb = new StringBuilder();

            int width = 1 + _columns.Sum(x => x.Width) + _columns.Count;

            // Header

            //-->
            // line before title
            sb.Append('+');
            for (int i = 1; i < width - 1; i++)
                sb.Append('-');
            sb.AppendLine("+");

            // title
            sb.Append("| ");
            sb.Append(_title);
            for (int i = 3 + _title.Length; i < width; i++)
                sb.Append(' ');
            sb.AppendLine("|");

            // line after title
            foreach (Column column in _columns)
            {
                sb.Append('+');
                for (int i = 0; i < column.Width; i++)
                    sb.Append('-');
            }
            sb.AppendLine("+");

            // column headers
            foreach (Column column in _columns)
            {
                sb.Append("| ");
                sb.Append(column.Header);
                for (int i = 1 + column.Header.Length; i < column.Width; i++)
                    sb.Append(' ');
            }
            sb.AppendLine("|");

            // line after column header
            foreach (Column column in _columns)
            {
                sb.Append('+');
                for (int i = 0; i < column.Width; i++)
                    sb.Append('-');
            }
            sb.AppendLine("+");
            //<-- can be precomputed

            // Values
            foreach (T item in items)
            {
                for (int index = 0; index < _columns.Count; index++)
                {
                    Column column = _columns[index];

                    string value = column.GetValueFunc(item);
                    string trailingSpace = column.GetTrailingSpaceFunc(item);
                    int columnWidth = column.Width;
                    int mergeLength = column.GetMergeLengthFunc(item);
                    if (mergeLength > 0)
                    {
                        // if cell is merged, increase articially column width
                        for (int i = index + 1; i < index + 1 + mergeLength; i++)
                            columnWidth += 1 + _columns[i].Width;
                        index += mergeLength;
                    }

                    sb.Append('|');
                    for (int i = 1 + value.Length + trailingSpace.Length - 1; i < columnWidth; i++)
                        sb.Append(' ');
                    sb.Append(value);
                    sb.Append(trailingSpace);
                }
                sb.AppendLine("|");
            }

            // line after values
            foreach (Column column in _columns)
            {
                sb.Append('+');
                for (int i = 0; i < column.Width; i++)
                    sb.Append('-');
            }
            sb.AppendLine("+");

            // TODO: ONLY FOR TEST PURPOSE
            Console.Write(sb.ToString());


            return sb;
        }
    }
}