using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.Server.Common;

namespace Mud.Server.Helpers
{
    // TODO: add title as Generator parameter
    // TODO: multi table
    // TODO: typed column:
    //   AddBooleanColumn: automatically convert to Yes/No
    //   AddIntColumn: automatically convert to string
    //   AddDelayColumn: automatically call StringHelpers.FormatDelay
    public class TableGenerator<T>
    {
        public static readonly Func<T, string> AlignLeftFunc = T => null;

        public class ColumnOptions
        {
            public bool MergeIdenticalValue { get; set; } = false;
            public bool AlignLeft { get; set; }
            public bool SeparatorAfterIdenticalValue { get; set; } = false;
            public Func<T, int> GetMergeLengthFunc { get; set; } = t => 0;
            public Func<T, string> GetTrailingSpaceFunc { get; set; } = t => " ";
        }

        private class Column
        {
            public string Header { get; set; }
            public int Width { get; set; }
            public Func<T,string> GetValueFunc { get; set; }

            public ColumnOptions Options { get; set; }
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
            AddColumn(header, width, getValueFunc, new ColumnOptions());
        }

        public void AddColumn(string header, int width, Func<T, string> getValueFunc, ColumnOptions options)
        {
            Column column = new Column { Header = header, Width = width, GetValueFunc = getValueFunc, Options = options};
            _columns.Add(column);
        }

        public int Width => 1 + _columns.Sum(x => x.Width) + _columns.Count;

        public StringBuilder GenerateWithPreHeaders(IEnumerable<T> items, IEnumerable<string> preHeaders) // TODO: will be replaced by multi-table
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();

            AddSeparatorLine(sb);
            foreach (string preHeader in preHeaders)
                AddPreHeaderLine(sb, preHeader);

            BuildTable(sb, items);

            return sb;
        }

        public StringBuilder Generate(IEnumerable<T> items)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();

            BuildTable(sb, items);

            return sb;
        }

        private void AddPreHeaderLine(StringBuilder sb, string content)
        {
            int width = Width;

            sb.Append("| ");
            sb.Append(content);
            int contentLengthNoColor = content.LengthNoColor();
            for (int i = 3 + contentLengthNoColor; i < width; i++)
                sb.Append(' ');
            sb.AppendLine("|");
        }

        private void AddSeparatorLine(StringBuilder sb)
        {
            int width = Width;

            sb.Append("+");
            for (int i = 1; i < width - 1; i++)
                sb.Append('-');
            sb.AppendLine("+");
        }

        private void AddSeparatorWithColumnsLine(StringBuilder sb)
        {
            foreach (Column column in _columns)
            {
                sb.Append('+');
                for (int i = 0; i < column.Width; i++)
                    sb.Append('-');
            }
            sb.AppendLine("+");
        }

        private void BuildTable(StringBuilder sb, IEnumerable<T> items)
        {
            int width = Width;

            //-->
            // line before title
            AddSeparatorLine(sb);

            // title
            sb.Append("| ");
            sb.Append(_title);
            for (int i = 3 + _title.LengthNoColor(); i < width; i++)
                sb.Append(' ');
            sb.AppendLine("|");

            // line after title
            AddSeparatorWithColumnsLine(sb);

            // column headers
            foreach (Column column in _columns)
            {
                sb.Append("| ");
                sb.Append(column.Header);
                for (int i = 1 + column.Header.LengthNoColor(); i < column.Width; i++)
                    sb.Append(' ');
            }
            sb.AppendLine("|");

            // line after column header
            AddSeparatorWithColumnsLine(sb);
            //<-- can be precomputed

            // Values
            string[] previousValues = new string[_columns.Count];
            foreach (T item in items)
            {
                // TODO: if hasToMergeIdenticalValue was true for any column and is false now and SeparatorAfterIdenticalValue is true, add separator
                for (int index = 0; index < _columns.Count; index++)
                {
                    Column column = _columns[index];

                    string value = column.GetValueFunc(item);
                    bool hasToMergeIdenticalValue = column.Options.MergeIdenticalValue && value == previousValues[index];
                    string trailingSpace = column.Options.AlignLeft 
                        ? AlignLeftFunc(item)
                        : column.Options.GetTrailingSpaceFunc(item);
                    int columnWidth = column.Width;
                    int mergeLength = column.Options.GetMergeLengthFunc(item);
                    if (mergeLength > 0)
                    {
                        // if cell is merged, increase artificially column width
                        for (int i = index + 1; i < index + 1 + mergeLength; i++)
                            columnWidth += 1 + _columns[i].Width;
                        //for (int i = 0; i < mergeLength; i++) // reset previous values
                        //    previousValues[index + i] = null;
                        index += mergeLength;
                    }

                    sb.Append('|');
                    if (trailingSpace == null) // if not trailing space, align on left
                    {
                        if (hasToMergeIdenticalValue)
                        {
                            sb.Append(' ', columnWidth);
                        }
                        else
                        {
                            sb.Append(' ');
                            sb.Append(value);
                            for (int i = value.LengthNoColor() - 1; i < columnWidth - 2; i++)
                                sb.Append(' ');
                        }
                    }
                    else // if trailing space, align on right
                    {
                        if (hasToMergeIdenticalValue)
                        {
                            sb.Append(' ', columnWidth);
                        }
                        else
                        {
                            for (int i = 1 + value.LengthNoColor() + trailingSpace.LengthNoColor() - 1; i < columnWidth; i++)
                                sb.Append(' ');
                            sb.Append(value);
                            sb.Append(trailingSpace);
                        }
                    }

                    // store previous value
                    previousValues[index] = value;
                }
                sb.AppendLine("|");
            }

            // line after values
            AddSeparatorWithColumnsLine(sb);

            // ONLY FOR TEST PURPOSE
            //Console.Write(sb.ToString());
        }
    }
}
